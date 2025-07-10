using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Newtonsoft.Json;

namespace Enterprise.DeniedPartyScreening.GUI
{
	static class DpsScreeningStatusOverrider
	{
		internal static void AddActionsMenuItem(DeniedPartyScreeningActionsProvider provider)
		{
			if (provider.IsFormActionsMenuItem && (provider.GetFormIdentifier == AutoRefVessel.Schema.TableName || provider.GetFormIdentifier == AutoOrgHeader.Schema.TableName))
			{
				Argument.NotNull(provider.ParentForm, "Organization or Vessel parent form");

				MenuItem overrideScreeningStatus = new ZMenuItem(ResString.GetMultilingualString("9E990AB5-0FA5-4410-825C-DFDC92B34A4D", "Override Screening Status"));

				var parentBizO = (provider.ParentForm.BusinessEntity as BusinessObject);

				overrideScreeningStatus.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("99CC5814-C391-4F64-A0C0-5878C401B8A9", "Update Status to Permanent Clear"), delegate
				{
					if (ShouldUpdateScreeningStatusToClear(parentBizO))
					{
						UpdateScreeningStatus(parentBizO, ScreeningStatusesList.Codes.PermanentClear, DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear);
					}
				}));

				overrideScreeningStatus.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("183C87E4-E305-4C14-AF37-B69AA970AB8B", "Reset Status to Unknown"), delegate
				{
					if (ShouldResetScreeningStatusToUnknwon(parentBizO))
					{
						UpdateScreeningStatus(parentBizO, ScreeningStatusesList.Codes.Unknown, DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges);
					}
				}));

				ZFormMenuStrategy.AddActionsMenuItem(provider.ParentForm, overrideScreeningStatus);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		static void UpdateScreeningStatus(BusinessObject parentBizO, string newStatus, string logStatus)
		{
			var entityProvider = (IDpsEntityProvider)parentBizO;
			entityProvider.ScreeningStatus = newStatus;

			var dpsMessage = parentBizO.Factory.New<DpsEDIMessage>();
			dpsMessage.EM_MessageData = ZBlob.FromUTF8(
				JsonConvert.SerializeObject(
					new DpsEDIMessageCandidatesContent
					{
						ClientLicence = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier("-"),
						DatabaseType = GlbCompany.CurrentCompany.DatabaseType,
						ClientSpecifiedIdentifier = parentBizO.PK.ToGuid(),
						EntityType = parentBizO.TablePrefix,
						PersistentStatus = entityProvider.ScreeningStatus,
						ScreenTime = DateTime.UtcNow,
						DpsAddressCandidates = CreateRequestHeader(parentBizO).DpsAddressCandidates,
						DpsCountryCandidates = CreateRequestHeader(parentBizO).DpsCountryCandidates,
						DpsNameCandidates = CreateRequestHeader(parentBizO).DpsNameCandidates,
						DpsRegistrationCodeCandidates = CreateRequestHeader(parentBizO).DpsRegistrationCodeCandidates
					},
					Formatting.None));

			DpsWorkflowTrackingEvent.AddNew(parentBizO, entityProvider.OriginalScreeningStatus, entityProvider.ScreeningStatus);

			DpsLog.AddNewWithDefaultValues(parentBizO.Factory, parentBizO, logStatus, Res.GetString("4542A8E2-4E8D-4AAB-93AB-C5B42397F49E", "Override Screening Status"));

			DpsLog.SaveWithExceptionHandler(parentBizO.Factory);
		}

		static DpsRequestHeaderWithAddressMatching CreateRequestHeader(BusinessObject parentBizO)
		{
			var creator = new DpsCandidateCreator();
			DpsRequestHeaderWithAddressMatching requestHeader;

			if (parentBizO is OrgHeader orgHeader)
			{
				requestHeader = creator.NewRequestHeader(orgHeader);
			}
			else
			{
				requestHeader = creator.NewRequestHeader((RefVessel)parentBizO);
			}

			return requestHeader;
		}

		static bool ShouldOverrideScreeningStatus(BusinessObject parentBizO, string overrideStatus)
		{
			var result = false;
			var entityProvider = (IDpsEntityProvider)parentBizO;
			var isActive = parentBizO is ICancellable cancellable && !cancellable.IsCancelled;

			if (parentBizO.HasChanges)
			{
				Globals.Message.Show(Res.GetString("27907EDB-32E7-4442-92A0-CF962627B13D", "Please save the form before overriding the screening status"));
			}
			else if (isActive && entityProvider.ScreeningStatus == overrideStatus)
			{
				Globals.Message.ShowWarning(Res.GetString("816A76D3-9C06-4CEA-BBD1-03BD05A24DA5", "The {0} screening status is already ({1})", parentBizO.HumanReadableName, entityProvider.ScreeningStatus));
			}
			else if (!DeniedPartyScreeningHelper.IsInactiveEntityWithShowMessage(entityProvider, isActive) &&
				!(DeniedPartyScreeningHelper.IsSystemDefinedUnmatchedOrganizationWithShowMessage(entityProvider)))
			{
				result = true;
			}

			return result;
		}

		static bool ShouldUpdateScreeningStatusToClear(BusinessObject parentBizO)
		{
			return ShouldOverrideScreeningStatus(parentBizO, ScreeningStatusesList.Codes.PermanentClear) &&
				DpsSecurityRights.IsGrantedOverrideScreeningStatusUpdateToClear() &&
				DeniedPartyScreeningCompliance.HasAcknowledgedRisk(Res.GetString("D9BB1828-D382-45C4-B6EB-2E44BB4B0196", @"You are about to change the screening status of {0} to Override Permanent Clear.

Doing so will exclude this record from Denied Party Screening processes. This records status will not be reset when data on the record is updated or when denied party lists are amended.

This operation must only be done by staff that understand the impacts it will have on their compliance process", parentBizO.HumanReadableName));
		}

		static bool ShouldResetScreeningStatusToUnknwon(BusinessObject parentBizO)
		{
			var entityProvider = (IDpsEntityProvider)parentBizO;
			var result = ShouldOverrideScreeningStatus(parentBizO, ScreeningStatusesList.Codes.Unknown);

			if (result)
			{
				if (entityProvider.ScreeningStatus != ScreeningStatusesList.Codes.PermanentClear)
				{
					Globals.Message.ShowWarning(Res.GetString("D7D50E28-03FC-4E0C-8549-E0DA4D23BC34", "Cannot reset screening status to Unknown. The screening status must be override Clear"));
					result = false;
				}
				else
				{
					result = DpsSecurityRights.IsGrantedOverrideScreeningStatusUpdateToUnknown() &&
						DeniedPartyScreeningCompliance.HasAcknowledgedRisk(Res.GetString("B108B4C8-C15E-4399-8A40-AAE47A25F884", @"You are about to reset the {0} status to Unknown.

Doing so will reset the overridden screening status

This operation must only be done by staff that understand the impacts it will have on their compliance process", parentBizO.HumanReadableName));
				}
			}

			return result;
		}
	}
}
