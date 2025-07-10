using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DeniedPartyScreening.GUI
{
	class DpsMarkJobScreeningStatusClear
	{
		public void AddActionsMenuItem(DeniedPartyScreeningActionsProvider provider)
		{
			Argument.NotNull(provider.ParentIBusiness, nameof(provider.ParentIBusiness), "Should have valid business entity");

			if (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.Value)
			{
				ZFormMenuStrategy.AddActionsMenuItem(provider.ParentForm, new ZMenuItem(ResString.GetMultilingualString("66431CC6-BA8C-42E7-A467-3DA9A9BD3BF2", "Mark as Job Clear"), delegate
				{
					ExecuteScreeningStatusUpdate((BusinessObject)provider.ParentIBusiness, provider.ParentForm);
				}));
			}
		}

		bool ValidateExecutionProcess(BusinessObject bizO)
		{
			var result = false;
			var statusProvider = (bizO as IScreeningStatusProvider)?.ScreeningStatus ?? ZString.Empty;

			if (bizO.HasChanges || !bizO.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("7DDE54FA-8A47-4D44-BEC6-BB2513A03C3C", "Please save the form before marking as job clear."));
			}
			else if (statusProvider == ScreeningStatusesList.Codes.Clear || statusProvider == ScreeningStatusesList.Codes.JobCleared)
			{
				Globals.Message.Show(Res.GetString("E223E88D-989A-47C2-9172-667D03C41987", "There is no need to Mark as Job Clear when screening status is \"CLR\" or \"JCL\"."));
			}
			else if (DpsSecurityRights.IsGrantedJobLevelClearanceWithShowError())
			{
				result = true;
			}

			return result;
		}

		void ExecuteScreeningStatusUpdate(BusinessObject bizO, ZForm parentForm)
		{
			if (ValidateExecutionProcess(bizO))
			{
				var jobClearModel = new DpsMarkJobClearConfirmationModel()
				{
					JobID = (bizO as IDeniedPartyProvider).ReferenceId,
					RelatedJobsIDNotJCLOrCLR = GetRelatedJobToMarkClear(bizO)
				};
				var jobClearForm = new DpsMarkJobClearConfirmationForm(jobClearModel);

				if (ZFormModaliser.ShowDialogAndDispose(jobClearForm, parentForm) == DialogResult.OK)
				{
					ScreeningStatusUpdater.ApplyJobClearStatus(bizO, GetJobMarkClearedReason(jobClearModel));
				}
			}
		}

		string GetJobMarkClearedReason(DpsMarkJobClearConfirmationModel model)
		{
			var clearedReasonComponents = new[]
			{
				model.Code, new ZString(model.JobClearingReasonList[model.Code]?.Description), model.Reason
			};
			var clearedReason = string.Join(", ", clearedReasonComponents.Where(r => !string.IsNullOrWhiteSpace(r)));

			return clearedReason;
		}

		List<string> GetRelatedJobToMarkClear(BusinessObject bizO)
		{
			var relatedJobs = new List<string>();

			if (bizO is IForwardingConsol consol && consol.Shipments.Any())
			{
				consol.Shipments.ForEach(shipment => AddRelatedJobToMarkClear((BusinessObject)shipment, relatedJobs));
			}
			else if (bizO is IForwardingShipment shipment && shipment.CoLoadShipments.Any())
			{
				shipment.CoLoadShipments.ForEach(childBizO => AddRelatedJobToMarkClear((BusinessObject)childBizO, relatedJobs));
			}

			return relatedJobs.Distinct().ToList();
		}

		void AddRelatedJobToMarkClear(BusinessObject bizO, List<string> relatedJobs)
		{
			var statusProvider = (IScreeningStatusProvider)bizO;

			if (statusProvider.ScreeningStatus != ScreeningStatusesList.Codes.Clear && statusProvider.ScreeningStatus != ScreeningStatusesList.Codes.JobCleared && bizO is IForwardingShipment shipment)
			{
				relatedJobs.Add(shipment.JS_UniqueConsignRef);

				if (shipment.IsMasterShipmentRepresentingAllChildShipments)
				{
					shipment.CoLoadShipments.ToList().ForEach(childShipment => AddRelatedJobToMarkClear((BusinessObject)childShipment, relatedJobs));
				}
			}
		}
	}
}
