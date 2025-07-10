using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;

[assembly: HostedService(
	DpsSilentScreenServiceTask.Code,
	"Denied Party Screening Silent Screen Service",
	"DPS",
	typeof(DpsSilentScreenServiceTask),
	IsMandatory = false,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "5minutes")]

namespace Enterprise.DeniedPartyScreening.ServiceTasks
{
	public class DpsSilentScreenServiceTask : ServiceProviderImpl
	{
		public const string Code = "DSS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		const string ErrorKey = "Error occurred when processing silent screening";
		protected DpsCandidateCreator candidateCreator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				try
				{
					candidateCreator = new DpsCandidateCreator();
					var factoryProvider = new BusinessObjectFactoryProvider();

					while (true)
					{
						youMustReactToThisToken.ThrowIfCancellationRequested();

						var silentScreeningData = GetSilentScreeningData();
						if (silentScreeningData.Any())
						{
							ServiceLogger?.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Denied Party Silent Screening batch has started, found {0} record(s).", silentScreeningData.Count));

							var headerPks = silentScreeningData.Where(x => x.Prefix == OrgHeaderSchema.Constants.Prefix).Select(x => x.PK).ToList();
							if (headerPks.Any())
							{
								ProcessSilentScreen<OrgHeader>(OrgHeaderSchema.PK, headerPks, factoryProvider.Current, youMustReactToThisToken);
							}

							var vesselPks = silentScreeningData.Select(x => x.PK).Except(headerPks).ToList();
							if (vesselPks.Any())
							{
								ProcessSilentScreen<RefVessel>(RefVesselSchema.PK, vesselPks, factoryProvider.Current, youMustReactToThisToken);
							}

							factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();

							if (headerPks.Any())
							{
								ProcessUpdateRelatedJobs<OrgHeader>(OrgHeaderSchema.PK, headerPks, factoryProvider.Current, youMustReactToThisToken);
							}

							if (vesselPks.Any())
							{
								ProcessUpdateRelatedJobs<RefVessel>(RefVesselSchema.PK, vesselPks, factoryProvider.Current, youMustReactToThisToken);
							}

							ServiceLogger?.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Denied Party Silent Screening batch has finished."));
						}
						else
						{
							break;
						}
					}
				}
				catch (WebException ex)
				{
					HttpDpsWebHelper.HandleWebException(ex, ServiceLogger);
				}
				catch (AggregateException aggEx)
				{
					foreach (var ex in aggEx.InnerExceptions)
					{
						HttpDpsWebHelper.HandleWebException((WebException)ex, ServiceLogger);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !(ex is OperationCanceledException))
				{
					if (!(ex is ZSaveConcurrencyException || ex is ZCannotSaveException || ex is ZSaveException || ex is AuthCertNotFoundException))
					{
						ErrorReporter.ReportOnce(ErrorKey, ex);
					}

					ServiceLogger?.Log(LogType.Error, ErrorKey, ex);
				}
			}
		}

		void ProcessUpdateRelatedJobs<TBizo>(SchemaPKColumn pkColumn, List<ZGuid> pks, BusinessObjectFactory factory, CancellationToken token) where TBizo : BusinessObject
		{
			var entities = factory.Load<TBizo>(new ZQuery(pkColumn, pks));

			foreach (var entity in entities)
			{
				ZString entityCode = new ZString();

				if (entity is OrgHeader orgHeader)
				{
					ScreeningStatusUpdater.UpdateRelatedJobs(orgHeader);
					entityCode = orgHeader.OH_Code;
				}

				if (entity is RefVessel refVessel)
				{
					ScreeningStatusUpdater.UpdateRelatedJobs(refVessel);
					entityCode = refVessel.RV_Code;
				}

				ServiceLogger?.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, $"{pkColumn.TableName}: {entityCode} performed update related jobs has finished."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service log message")]
		void ProcessSilentScreen<TBizo>(SchemaPKColumn pkColumn, List<ZGuid> pks, BusinessObjectFactory factory, CancellationToken token) where TBizo : BusinessObject
		{
			var entities = factory.Load<TBizo>(new ZQuery(pkColumn, pks));
			var tableName = pkColumn.TableName;

			foreach (var entity in entities)
			{
				token.ThrowIfCancellationRequested();

				var screeningPartyProvider = (IScreeningPartyProvider)entity;
				var previousScreeningStatus = screeningPartyProvider.ScreeningStatus;
				var requestHeaderAndResponse = GetRequestHeaderAndResultModel(entity);
				var model = requestHeaderAndResponse.ResultModel;

				if (model != null)
				{
					string logScreeningStatus;
					var matchConfidenceRating = string.Empty;
					var logClearedReason = string.Empty;

					if (model.ResponseWithScreeningParty.Response.ResponseCode == DpsResponseCode.Exception)
					{
						throw new Exception(string.Format(CultureInfo.InvariantCulture, "DPS.Response Code: {0}\nDPS.Response Message: {1}",
							model.ResponseWithScreeningParty.Response.ResponseCode, model.ResponseWithScreeningParty.Response.ExtraMessage));
					}
					else
					{
						if (model.ResponseWithScreeningParty.Response.ResponseCode == DpsResponseCode.Failed)
						{
							screeningPartyProvider.ScreeningStatus = ScreeningStatusesList.Codes.RequiresReview;
							logScreeningStatus = LogsScreeningStatus.ScreeningProcessError;
							logClearedReason = model.ResponseWithScreeningParty.Response.ExtraMessage;
						}
						else
						{
							var potentialMatch = model.PotentialMatchModels?.OrderByDescending(x => x.ScoreGrade).FirstOrDefault(x => !x.IsExcluded);
							if (potentialMatch != null && potentialMatch.ScoreGrade != ScoreGrades.Low)
							{
								screeningPartyProvider.ScreeningStatus = ScreeningStatusesList.Codes.RequiresReview;
								logScreeningStatus = LogsScreeningStatus.PotentialMatchesFound;
								matchConfidenceRating = potentialMatch.ScoreGrade == ScoreGrades.Medium ? Constants.ScreeningMatchConfidenceRating.Medium : Constants.ScreeningMatchConfidenceRating.High;
							}
							else
							{
								logScreeningStatus = LogsScreeningStatus.ScreenedClear;
								screeningPartyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
							}
						}

						var dpsMessage = factory.New<DpsEDIMessage>();
						dpsMessage.EM_MessageData = ZBlob.FromUTF8(
							JsonConvert.SerializeObject(
								new DpsEDIMessageCandidatesContent
								{
									ClientLicence = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier("-"),
									DatabaseType = GlbCompany.CurrentCompany.DatabaseType,
									ClientSpecifiedIdentifier = entity.PK.ToGuid(),
									EntityType = entity.TablePrefix,
									PersistentStatus = screeningPartyProvider.ScreeningStatus,
									ScreenTime = DateTime.UtcNow,
									DpsAddressCandidates = requestHeaderAndResponse.RequestHeader.DpsAddressCandidates,
									DpsCountryCandidates = requestHeaderAndResponse.RequestHeader.DpsCountryCandidates,
									DpsNameCandidates = requestHeaderAndResponse.RequestHeader.DpsNameCandidates,
									DpsRegistrationCodeCandidates = requestHeaderAndResponse.RequestHeader.DpsRegistrationCodeCandidates,
								},
								Formatting.None));

						DpsWorkflowTrackingEvent.AddNew(entity, previousScreeningStatus, screeningPartyProvider.ScreeningStatus, null, Constants.ScreeningType.Silent, matchConfidenceRating);
					}

					var sourceBizOs = new List<DpsSourceWithParties>() { new DpsSourceWithParties(entity, screeningPartyProvider.ScreeningParties) };
					DpsLog.AddNew(factory, entity, logScreeningStatus, logClearedReason, model.HighConfidenceResults, model.MediumConfidenceResults, model.LowConfidenceResultsCount, false, null, sourceBizOs);
				}

				ServiceLogger?.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "{0}: {1} silently screened|New Status is: {2}.|Response Code: {3}|Response Message: {4}", tableName,
					requestHeaderAndResponse.EntityCode, screeningPartyProvider.ScreeningStatus,
					requestHeaderAndResponse.ResultModel.ResponseWithScreeningParty.Response.ResponseCode,
					requestHeaderAndResponse.ResultModel.ResponseWithScreeningParty.Response.ExtraMessage));
			}
		}

		protected (DpsRequestHeaderWithAddressMatching RequestHeader, ScreenedPartyModel ResultModel, string EntityCode) GetRequestHeaderAndResultModel<TBizo>(TBizo entity) where TBizo : BusinessObject
		{
			var requestHeader = new DpsRequestHeaderWithAddressMatching();
			List<DpsResponseWithScreeningParty> responseWithParty;

			if (entity is OrgHeader header)
			{
				requestHeader = candidateCreator.NewRequestHeader(header);

				responseWithParty = new List<DpsResponseWithScreeningParty>()
				{
					new DpsResponseWithScreeningParty(new ScreeningParty(entity, header.TableName, header),
						HttpDpsWebHelper.PostScreenWithFailover(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestHeader))),
						requestHeader)
				};
				return (requestHeader, new DpsResultModel(responseWithParty, entity.Factory).ScreenedPartyModels.SingleOrDefault(), header.OH_Code);
			}

			if (entity is RefVessel vessel)
			{
				requestHeader = candidateCreator.NewRequestHeader(vessel);
				responseWithParty = new List<DpsResponseWithScreeningParty>()
				{
					new DpsResponseWithScreeningParty(new ScreeningParty(entity, vessel.TableName, vessel),
						HttpDpsWebHelper.PostScreenWithFailover(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestHeader))),
						requestHeader)
				};
				return (requestHeader, new DpsResultModel(responseWithParty, entity.Factory).ScreenedPartyModels.SingleOrDefault(), vessel.RV_Code);
			}

			return (null, null, null);
		}

		List<(ZGuid PK, string Prefix)> GetSilentScreeningData()
		{
			var result = DataUtils.GetDataTableFromQuery(Db.Connection, Script, ("@batchSize", SqlDbType.TinyInt, 3, (object)OrganisationsDataRegistry.Instance.DeniedPartySilentScreeningServiceTaskBatchSize.Value));
			return result.Rows.Cast<DataRow>().Select(x => (new ZGuid(x["PK"].ToString()), x["Prefix"].ToString())).ToList();
		}

		const string Script = @";WITH SilentScreeningData AS
(
	SELECT OH_PK PK, 'OH' Prefix FROM dbo.OrgHeader WHERE OH_ScreeningStatus IN ('NOT', 'UNK') and OH_IsActive = 1 and OH_Code <> 'UNMATCHED'
	UNION ALL
	SELECT RV_PK PK, 'RV' Prefix FROM dbo.RefVessel WHERE RV_ScreeningStatus IN ('NOT', 'UNK') AND RV_IsActive = 1
)

SELECT TOP (@batchSize) PK, Prefix FROM SilentScreeningData";
	}
}
