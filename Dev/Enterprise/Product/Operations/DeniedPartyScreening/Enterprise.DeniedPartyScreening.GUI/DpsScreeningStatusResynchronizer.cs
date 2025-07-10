using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class DpsScreeningStatusResynchronizer
	{
		public static void SaveWithVerbose(bool verboseMode, BusinessObject sourceBizO)
		{
			if (sourceBizO is IScreeningPartyProvider partyProvider)
			{
				var oldScreeningStatus = partyProvider.ScreeningStatus;
				var newScreeningStatus = ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(partyProvider);

				DpsWorkflowTrackingEvent.AddNew(sourceBizO, oldScreeningStatus, newScreeningStatus, null, verboseMode ? ScreeningType.Manual : ScreeningType.Resynchronize, null);

				if (verboseMode)
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(
						() => { sourceBizO.Factory.Save(); },
						() =>
						{
							newScreeningStatus = ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(partyProvider);
							if (!ComplianceRiskHelper.CheckIfComplianceRiskEnabled(sourceBizO))
							{
								sourceBizO.GetLogs().LogsNotInDB.FirstOrDefault(log => log.SL_SE_NKEvent == AutoEvents.DeniedPartyStatusUpdated.Code)?.Delete();
							}
							DpsWorkflowTrackingEvent.AddNew(sourceBizO, oldScreeningStatus, newScreeningStatus);
						});
				}
				else
				{
					try
					{
						sourceBizO.Factory.Save();
					}
					catch (ZSaveConcurrencyException)
					{
						//ZSaveConcurrencyException is possible but we don't need to handle it or let users know in non-verbose mode. Just let users resynchronize manually or reload the form to resynchronize again.
					}
				}
			}
		}

		public static void SynchronizeScreeningStatusToDeclarationsAndSave(bool verboseMode, BusinessObject bizO)
		{
			if (bizO is IForwardingShipment forwardingShipment)
			{
				var needToSave = false;
				foreach (var declaration in forwardingShipment.Declarations)
				{
					if (declaration.JE_ScreeningStatus != forwardingShipment.JS_ScreeningStatus)
					{
						declaration.JE_ScreeningStatus = forwardingShipment.JS_ScreeningStatus;
						needToSave = true;
					}
				}

				if (needToSave)
				{
					if (bizO is IShouldUpdateScreeningStatus shipment)
					{
						shipment.ShouldUpdateScreeningStatus = false;
					}

					if (verboseMode)
					{
						ZExceptionReporting.ProcessWithSaveExceptionHandling(() => { bizO.Factory.Save(); }, null);
					}
					else
					{
						try
						{
							bizO.Factory.Save();
						}
						catch (ZSaveConcurrencyException)
						{
							//ZSaveConcurrencyException is possible but we don't need to handle it or let users know in non-verbose mode. Just let users resynchronize manually or reload the form to resynchronize again.
							return;
						}
					}
				}
			}
		}
	}
}
