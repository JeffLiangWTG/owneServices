using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed partial class AllocationAdjustmentDialog : ZChildForm
	{
		public AllocationAdjustmentDialog(AllocationAdjustmentDetails details)
			: base(details)
		{
			InitializeComponent();

			DialogResult = DialogResult.Cancel;
			messageBodyLabel.Text =
				Res.GetString("AllocationAdjustmentDialog|WarningMessage", "The current shipment will not fit within the over allocation limits. To continue saving you can increase the over allocation percentage from {0}% to {1}% to cater for this shipment or you can cancel the save and put the shipment on another sailing. If you want to increase the over allocation percentage you will need authorization from someone with permissions to adjust the allocations.",
				details.OldOverAllocationPercent, details.NewOverAllocationPercent);
		}

		public static bool ConfirmAdjustAllocations(AllocationUsageSet usageSet, AllocationUsage required)
		{
			bool result;

			AllocationUsage totalRequired = AllocationUsage.Sum(new AllocationUsage[] { usageSet.Used, required });
			AllocationAdjustmentDetails details = new AllocationAdjustmentDetails(usageSet.Allocation, totalRequired);

			result = (ZFormModaliser.ShowDialogAndDispose(new AllocationAdjustmentDialog(details)) == DialogResult.OK);
			if (result)
			{
				details.AcceptNewOverAllocation();
			}

			return result;
		}

		#region Implementation

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("af6bbe60-20fc-49cd-99c3-f2f8dc113e61", "allocation"), Res.GetString("abf5ebcf-cb6d-4770-8d23-7d32fddaa05c", "adjust"), Res.GetString("92395616-f692-4035-90bd-71a263c1ed53", "adjusted"), includeIgnoreOption);
		}

		AllocationAdjustmentDetails Details
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AllocationAdjustmentDetails)BusinessEntity; }
		}

		bool CheckCredentials()
		{
			SecurityCore security = Details.UserSecurity;
			bool result = false;

			if (security == null)
			{
				Globals.Message.ShowError(Res.GetString("0a1606ec-cc0d-4f92-a60f-70551d67c366", "Invalid username / password."));
			}
			else if (!security.SailingScheduleAllocationEdit.IsAllowed)
			{
				Globals.Message.ShowError(Res.GetString("d4a96e97-0626-428e-9bef-924d73223ecd", "{0} is not authorized to adjust the allocations.", Details.Login));
			}
			else
			{
				result = true;
			}

			return result;
		}

		void ValidateAndAdjust()
		{
			BusinessEntity.RunPreSaveValidation();

			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
			}
			else if (!CheckCredentials())
			{
				Details.Password = "";
			}
			else
			{
				Details.Password = "";
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		#endregion

		#region Events

		void OkZButton_Click(object sender, EventArgs e)
		{
			ValidateAndAdjust();
		}

		void CancelZButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.Agency.GUI
{
	partial class AllocationAdjustmentDialog
	{
		public void ClickOkForTesting()
		{
			okButton.PerformClick();
		}
	}
}

#endregion



#endif
#endregion
