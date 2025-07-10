using System;
using System.Collections.Generic;
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
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.DeniedPartyScreening.ServiceTasks.DpsRescreeningServiceTask.Code,
	"Denied Party Screening - Re-screening Advice Manager",
	"DPS",
	typeof(Enterprise.DeniedPartyScreening.ServiceTasks.DpsRescreeningServiceTask),
	MinimumPeriod = "5minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour")]

namespace Enterprise.DeniedPartyScreening.ServiceTasks
{
	public class DpsRescreeningServiceTask : ServiceProviderImpl
	{
		public const string Code = "DPR";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		const string ErrorKey = "Error occurred when processing rescreening";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public override void RunTask(CancellationToken token)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var factoryProvider = new BusinessObjectFactoryProvider();
				var serviceTaskHelper = new DpsServiceTaskHelper(Code);

				var totalResetEntities = 0;
				var rescreenAdvices = new List<DeniedPartyRescreenAdvice>();
				var featureControlHelper = new DpsFeatureControlHelper();
				var addressMatchingFlags = featureControlHelper.GetFeatureControlAddressOnlyFlags;
				try
				{
					var exclusionList = GetExclusionList(factoryProvider.Current);
					var orgThresholds = OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.Value;
					var personThresholds = OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForPersons.Value;
					var vesselThresholds = OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForVessels.Value;
					var request = new PendingRescreenAdviceRequestWithThresholdAndAddressMatching
					{
						ExcludedListCodes = exclusionList,
						OrgNameMediumConfidenceThreshold = orgThresholds.MediumThreshold,
						OrgNameHighConfidenceThreshold = orgThresholds.HighThreshold,
						PersonNameMediumConfidenceThreshold = personThresholds.MediumThreshold,
						PersonNameHighConfidenceThreshold = personThresholds.HighThreshold,
						VesselNameMediumConfidenceThreshold = vesselThresholds.MediumThreshold,
						VesselNameHighConfidenceThreshold = vesselThresholds.HighThreshold,
						IsAddressOnlyScreeningIncluded = addressMatchingFlags.isAddressOnlyScreeningIncluded,
						IsAllAddressesIncluded = addressMatchingFlags.isAllAddressesIncluded,
						AddressMatchingLevel = addressMatchingFlags.matchingLevel,
					};
					var requestBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
					rescreenAdvices = HttpDpsWebHelper.GetRescreenAdvices(requestBytes);
					if (rescreenAdvices.Any())
					{
						ServiceLogger?.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Received {0} Rescreening advice(s)", rescreenAdvices.Count));
						var batchSize = OrganisationsDataRegistry.Instance.DeniedPartyRescreeningServiceTaskBatchSize.Value;
						for (var i = 0; i < rescreenAdvices.Count; i += batchSize)
						{
							var resetOrgs = 0;
							var resetVessels = 0;
							token.ThrowIfCancellationRequested();

							var batchAdvices = rescreenAdvices.GetRange(i, Math.Min(batchSize, rescreenAdvices.Count - i));
							ServiceLogger?.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Rescreen batch has started processing {0} record(s).", batchAdvices.Count));
							var orgsInBatch = batchAdvices.Where(advice => advice.TypeOfEntity.Trim().Equals(OrgHeaderSchema.Constants.Prefix, StringComparison.OrdinalIgnoreCase)).ToList();
							var vesselsInBatch = batchAdvices.Where(advice => advice.TypeOfEntity.Trim().Equals(RefVesselSchema.Constants.Prefix, StringComparison.OrdinalIgnoreCase)).ToList();
							if (orgsInBatch.Any())
							{
								resetOrgs = ProcessEntities<OrgHeader>(orgsInBatch, OrgHeaderSchema.PK, OrgHeaderSchema.OH_ScreeningStatus, factoryProvider, token);
								totalResetEntities += resetOrgs;
							}
							if (vesselsInBatch.Any())
							{
								resetVessels = ProcessEntities<RefVessel>(vesselsInBatch, RefVesselSchema.PK, RefVesselSchema.RV_ScreeningStatus, factoryProvider, token);
								totalResetEntities += resetVessels;
							}

							ServiceLogger?.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Rescreen batch processed: Reset {0} Org and {1} Vessel record(s).", resetOrgs, resetVessels));
						}
						ServiceLogger?.Log(LogType.Information, FormattableString.Invariant($"Denied Party Rescreen Advice service task completed successfully: Processed {rescreenAdvices.Count} rescreen advice(s), reset {totalResetEntities} record(s)."));
					}
					else
					{
						ServiceLogger?.Log(LogType.Debug, FormattableString.Invariant($"Received 0 Rescreening advice(s)"));
					}
					serviceTaskHelper.UpdateLastSuccessfulRuntime();
				}
				catch (WebException ex)
				{
					HttpDpsWebHelper.HandleWebException(ex, ServiceLogger, serviceTaskHelper);
					ServiceLogger?.Log(LogType.Error, FormattableString.Invariant($"Denied Party Rescreen Advice service task terminated with an error: Reset {totalResetEntities} record(s)."));
				}
				catch (Exception exception) when (!exception.IsCriticalException() && !(exception is OperationCanceledException))
				{
					if (exception is not AuthCertNotFoundException)
					{
						ErrorReporter.ReportOnce(ErrorKey, exception);
					}
					ServiceLogger?.Log(LogType.Error, ErrorKey, exception);
					ServiceLogger?.Log(LogType.Error, FormattableString.Invariant($"Denied Party Rescreen Advice service task terminated with an error: Reset {totalResetEntities} record(s)."));
				}
			}
		}

		IEnumerable<string> GetExclusionList(BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(RefComplianceList));
			query.AddToFilter(RefComplianceListSchema.RCL_IsActive, true);
			query.AddToFilter(RefComplianceListSchema.RCL_IsExcluded, true);

			return factory.Load<RefComplianceList>(query).Select(x => x.RCL_ListCode.ToString()).ToHashSet().ToList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Need transaction for both bizos and Client table so use direct DB connection., Could safely use direct DB connection")]

#if DEBUG
		protected virtual
#endif
		int ProcessEntities<TBizo>(List<DeniedPartyRescreenAdvice> batchAdvices, SchemaPKColumn pkColumn, SchemaStringColumn screeningStatusColumn, BusinessObjectFactoryProvider factoryProvider, CancellationToken token) where TBizo : BusinessObject
		{
			var relevantQuery = new ZQuery(pkColumn, batchAdvices.Select(advice => advice.ClientSpecifiedIdentifier).Distinct()).AddToFilter(screeningStatusColumn, ScreeningStatusesList.Codes.Clear);
			var entities = factoryProvider.Current.Load<TBizo>(relevantQuery);

			var validBatchAdviceItemPks = batchAdvices.Where(x => entities.Select(y => y.PK).Contains(x.ClientSpecifiedIdentifier)).Select(x => x.AdviceItemPk).ToList();
			var invalidBatchAdviceItemPks = batchAdvices.Select(x => x.AdviceItemPk).Except(validBatchAdviceItemPks).ToList();
			if (invalidBatchAdviceItemPks.Count > 0)
			{
				HttpDpsWebHelper.ConfirmRescreenAdvices(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(invalidBatchAdviceItemPks)));
				ServiceLogger?.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "The following {0} record(s) will not be reset to (REQ) Requires Review: [{1}]. This is because the record is no longer clear, the record is deactivated or the record does not exist.", pkColumn.TableName, string.Join(",", invalidBatchAdviceItemPks)));
			}

			if (entities.Length > 0)
			{
				using (var manager = Db.Connection.BeginTransactionWithManager())
				{
					foreach (var entity in entities)
					{
						token.ThrowIfCancellationRequested();

						((IScreeningStatusProvider)entity).ScreeningStatus = ScreeningStatusesList.Codes.RequiresReview;
						DpsWorkflowTrackingEvent.AddNew(entity, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.RequiresReview, null, Constants.ScreeningType.RescreenAdvice, null);

						CreateScreeningStatus(entity.PK, entity.TablePrefix, factoryProvider.Current);
						ServiceLogger?.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Processing {0} - {1}.", entity.TableName, entity.PK));
					}

					factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
					HttpDpsWebHelper.ConfirmRescreenAdvices(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(validBatchAdviceItemPks)));
					manager.CommitTransaction();
				}
			}
			return entities.Length;
		}

		void CreateScreeningStatus(ZGuid bizOPk, string bizOType, BusinessObjectFactory factory)
		{
			var status = factory.New<StmEntityScreeningLog>();
			status.PJ_ParentID = bizOPk;
			status.PJ_ParentTableCode = bizOType;
			status.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.InvalidatedByContentUpdate;
			status.PJ_SourceID = bizOPk;
			status.PJ_SourceTableCode = bizOType;
		}
	}
}
