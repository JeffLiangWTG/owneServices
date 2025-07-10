using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class DpsResultsManager
	{
		public static (List<DpsResultStatus> ResultStatuses, List<BusinessObject> ChangedBizOs, bool IsNewDpsForm) ProcessResults(List<DpsResponseWithScreeningParty> responses, List<DpsSourceWithParties> sourceBizOs, bool standAlone, BusinessObjectFactory factory, bool forceReScreen, bool forceAllLists = false, DpsImageSources? entityTypeIconOverride = null)
		{
			Argument.NotNull(factory, nameof(factory));

			bool isNewDpsForm = false;
			string errorMessage = null;
			var successfulResponse = new List<DpsResponseWithScreeningParty>();
			var otherParties = new List<ScreeningParty>();

			foreach (var response in responses)
			{
				if (response.Response != null)
				{
					if (response.Response.ResponseCode != DpsResponseCode.Successful)
					{
						errorMessage += response.Response.ExtraMessage + System.Environment.NewLine;
					}
					else
					{
						successfulResponse.Add(response);
					}
				}
				else
				{
					otherParties.Add(response.ScreeningParty);
				}
			}

			if (string.IsNullOrEmpty(errorMessage))
			{
				var resultViewModel = new DpsResultWinModel(new DpsResultModel(successfulResponse, factory, forceAllLists), standAlone) as IDpsResult;
				if (resultViewModel.AllPartiesClear)
				{
					if (standAlone)
					{
						Globals.Message.ShowInformation(Res.GetString("81CFDBB9-7799-43BF-A805-4A68C721639B", "No denied party matching info."));
					}
					else if (resultViewModel.AllScreenedParties.Any())
					{
						using (var summaryForm = new SummaryForm(resultViewModel.AllScreenedParties.ToList()))
						{
							ZFormModaliser.ShowDialogWithoutDispose(summaryForm);
						}
					}
				}
				else
				{
					if (standAlone && entityTypeIconOverride != null)
					{
						resultViewModel.AllScreenedParties.ForEach(u =>
						{
							u.EntityTypeIcon = entityTypeIconOverride.Value;
						});
					}
					ZForm resultsForm;
					var canShowNewDpsForm = HVLVDataRegistry.Instance.HVLVEnablePartyScreening.Value.EnableNewDPSResultForm &&
						(successfulResponse.Any(r => r.ScreeningParty.Parent.TableName == HVLVConsignmentSchema.Constants.TableName)
						|| successfulResponse.Any(r => r.ScreeningParty.Parent is IForwardingConsol consol && consol.Shipments.Any(x => x.JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValue))
						|| successfulResponse.Any(r => r.ScreeningParty.Parent.TableName == HVLVBookingHeaderSchema.Constants.TableName));

					if (canShowNewDpsForm)
					{
						isNewDpsForm = true;
						resultsForm = Activator.CreateInstance(ObjectFactory.GetType<IHVLVDpsResultForm>(), successfulResponse, factory) as ZForm;
					}
					else
					{
						resultsForm = new DpsResultWinform((DpsResultWinModel)resultViewModel);
					}

					using (resultsForm)
					{
						ZFormModaliser.ShowDialogWithoutDispose(resultsForm);
					}

					if (!standAlone && !isNewDpsForm)
					{
						using (var summaryForm = new SummaryForm(resultViewModel.AllScreenedParties.ToList()))
						{
							ZFormModaliser.ShowDialogWithoutDispose(summaryForm);
						}
					}
				}

				if (!standAlone && !isNewDpsForm)
				{
					var resultStatusAndDecisions = CreateResultStatusAndRecordDecisions(factory, resultViewModel.AllScreenedParties.Cast<IDeniedPartyResultItemV4>().ToList(), otherParties, sourceBizOs, forceReScreen, resultViewModel.CredentialOverride);
					return (resultStatusAndDecisions.ResultStatuses, resultStatusAndDecisions.BizObjects, false);
				}
			}
			else
			{
				Globals.Message.ShowError(errorMessage);
			}

			return (new List<DpsResultStatus>(), new List<BusinessObject>(), isNewDpsForm);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		internal static (List<DpsResultStatus> ResultStatuses, List<BusinessObject> BizObjects) CreateResultStatusAndRecordDecisions(BusinessObjectFactory factory, List<IDeniedPartyResultItemV4> resultItems, List<ScreeningParty> otherResultItems, List<DpsSourceWithParties> sourceBizOs, bool forceReScreen, string credentialOverride)
		{
			var resultStatuses = new List<DpsResultStatus>();
			var bizObjects = new List<BusinessObject>();
			var cancelledBizObjects = resultItems.Where(x => x.NewScreeningStatus == ScreeningStatusesList.Codes.Canceled).Select(x => x.ScreenedEntity);
			var cancelledBizObjectsHashSet = cancelledBizObjects.Select(x => x.PK).ToHashSet();

			foreach (var item in resultItems)
			{
				if (item.NewScreeningStatus != ScreeningStatusesList.Codes.Canceled)
				{
					if (item.ScreenedEntity.TablePrefix == OrgHeaderSchema.Constants.Prefix ||
						item.ScreenedEntity.TablePrefix == RefVesselSchema.Constants.Prefix)
					{
						var dpsMessage = factory.New<DpsEDIMessage>();
						dpsMessage.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(new DpsEDIMessageCandidatesContent
						{
							ClientLicence = DpsConfigurationDataHelper.NewServiceConfig().LicenceCode,
							DatabaseType = GlbCompany.CurrentCompany.DatabaseType,
							ClientSpecifiedIdentifier = item.ScreenedEntity.PK.ToGuid(),
							EntityType = item.ScreenedEntity.TablePrefix,
							PersistentStatus = item.NewScreeningStatus,
							ScreenTime = DateTime.UtcNow,
							DpsAddressCandidates = item.RequestHeaderWithAddressMatching.DpsAddressCandidates,
							DpsCountryCandidates = item.RequestHeaderWithAddressMatching.DpsCountryCandidates,
							DpsNameCandidates = item.RequestHeaderWithAddressMatching.DpsNameCandidates,
							DpsRegistrationCodeCandidates = item.RequestHeaderWithAddressMatching.DpsRegistrationCodeCandidates,
						},
						Formatting.None));
					}

					if (item.ScreenedEntity.TablePrefix == HVLVConsignmentSchema.Constants.Prefix ||
						item.Parents.Any(b => b.TablePrefix == HVLVConsignmentSchema.Constants.Prefix))
					{
						if (!cancelledBizObjectsHashSet.Contains(item.ScreenedEntity.PK))
						{
							AddDpsResultStatuses(item, bizObjects, resultStatuses, item.Parents.Except(cancelledBizObjects));
						}
					}
					else
					{
						AddDpsResultStatuses(item, bizObjects, resultStatuses, item.Parents);
					}
				}

				if (item.ScreenedEntity.TablePrefix != HVLVConsignmentSchema.Constants.Prefix)
				{
					var logStatus = item.ScreenedEntity is RefCountry && item.NewScreeningStatus == ScreeningStatusesList.Codes.Matched ? DpsLog.GetCountrySanctionsLogStatus(item.NewScreeningStatus) : DpsLog.GetStatus(item.NewScreeningStatus);

					DpsLog.AddNew(factory, logStatus, item, sourceBizOs, forceReScreen, credentialOverride);
				}
			}

			foreach (var resultStatus in
				from otherItem in otherResultItems
				from parent in otherItem.Parents.Where(parent => parent is IScreeningPartyProvider provider && provider.ScreeningStatus != ScreeningStatusesList.Codes.JobCleared)
				select new DpsResultStatus(parent, otherItem.CurrentScreeningStatus))
			{
				AddStatus(resultStatuses, resultStatus);
			}

			return (resultStatuses, bizObjects);
		}

		internal static void AddDpsResultStatuses(IDeniedPartyResultItemV4 item, List<BusinessObject> bizObjects, List<DpsResultStatus> resultStatuses, IEnumerable<BusinessObject> parents)
		{
			if (item.CurrentScreeningStatus != item.NewScreeningStatus)
			{
				bizObjects.Add(item.ScreenedEntity);
			}

			var newResultStatus = new DpsResultStatus(item.ScreenedEntity, item.NewScreeningStatus);
			AddStatus(resultStatuses, newResultStatus);

			foreach (BusinessObject parent in parents)
			{
				newResultStatus = new DpsResultStatus(parent, item.NewScreeningStatus);
				AddStatus(resultStatuses, newResultStatus);
			}
		}

		internal static void AddStatus(List<DpsResultStatus> resultStatuses, DpsResultStatus addResult)
		{
			if (string.IsNullOrEmpty(addResult.Status))
			{
				return;
			}

			var statusResultFoundAndUpdated = false;
			foreach (var result in resultStatuses)
			{
				if (result.ScreeningEntity.PK == addResult.ScreeningEntity.PK)
				{
					if (addResult.Status == ScreeningStatusesList.Codes.Unknown &&
						(result.Status == ScreeningStatusesList.Codes.Clear || result.Status == ScreeningStatusesList.Codes.PermanentClear))
					{
						result.Status = ScreeningStatusesList.Codes.Unknown;
					}
					else if (addResult.Status == ScreeningStatusesList.Codes.Matched)
					{
						result.Status = ScreeningStatusesList.Codes.Matched;
					}
					else if (addResult.Status == ScreeningStatusesList.Codes.Clear && result.Status == ScreeningStatusesList.Codes.PermanentClear)
					{
						result.Status = ScreeningStatusesList.Codes.Clear;
					}

					statusResultFoundAndUpdated = true;
					break;
				}
			}

			if (!statusResultFoundAndUpdated)
			{
				resultStatuses.Add(addResult);
			}
		}
	}
}
