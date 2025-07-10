using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using SharedCustoms = Enterprise.Customs.Business;
using SupervisorOverrides = Enterprise.Customs.US.Business.SupervisorOverrides;

namespace Enterprise.Customs.US.GUI
{
	public class ShipmentAndBrokerageCommon : BaseShipmentAndBrokerageCommon
	{
		public ShipmentAndBrokerageCommon(SharedCustoms.BaseJobDeclaration declaration)
			: base(declaration)
		{
		}

		public override SharedCustoms.SupervisorOverrides GetSupervisorOverrides()
		{
			return new SupervisorOverrides(declaration, SharedCustoms.SupervisorOverridesContext.SavingDeclaration);
		}

		public override ContinueWithSave IsSupervisorApproved()
		{
			var result = ContinueWithSave.Yes;
			var declaration = (JobDeclaration)this.declaration;

			if (declaration != null)
			{
				if (declaration.HasDeactivatedAESEntryOriginalRejected())
				{
					string warningMessage = "The changes you have made caused a rejected AES to be deactivated. The deactivated entries could be viewed on the grid under Customs Declarations > Messages > Shipper’s Export Declarations by right-clicking in the grid and selecting an option ‘Show Deactivated Entries’  Are you sure you wish to continue to save?";
					if (Globals.Message.Show(warningMessage, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
					{
						result = ContinueWithSave.No;
					}
				}

				if (result == ContinueWithSave.Yes && (declaration.IsImport || declaration.IsExWarehouse))
				{
					declaration.RecalculateReconIndicators();
					result = base.IsSupervisorApproved();
				}

				if (result == ContinueWithSave.Yes)
				{
					CheckExportEntriesToWithdraw(declaration);
					new STUSender(declaration).Send();
				}
			}

			return result;
		}

		void CheckExportEntriesToWithdraw(JobDeclaration declaration)
		{
			if (declaration != null && declaration.IsExport)
			{
				if (declaration.HasAESTIRMessageThatNeedsToBeWithdrawn)
				{
					if (Globals.Message.Show(NeedToWithdrawAESEntry, "Deactivated Entry Needs To Be Withdrawn", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						AESMessageHandler.SendMessage(declaration, null);
					}
				}
			}
		}

		internal const string NeedToWithdrawAESEntry = "Some changes were made which has caused an entry, which has already been lodged, to be deactivated.\r\nThis entry can be viewed under Messages > Shipper's Export Declarations Entries.\r\nWould you like to withdraw such entries now?";
	}
}
