using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class NewAWBForm : ZChildForm
	{
		public NewAWBForm(JobMawb mawb, IMAWBAllocationParent mawbParent = null)
			: base(mawb)
		{
			InitializeComponent();
			Init();
			MawbParent = mawbParent;
			mawb.ShouldValidateAllocation = true;
		}

		readonly IMAWBAllocationParent MawbParent;

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = ContinueWithSave.Yes;

			DuplicateJobMawbs duplicateJobMawbs = new DuplicateJobMawbs((JobMawb)BusinessEntity);
			if (duplicateJobMawbs.Count > 0 || IsDuplicateMAWB())
			{
				result = ContinueWithSave.No;
				Globals.Message.ShowError(Res.GetString("68cf4ccc-843f-4e9f-a18f-92f292f1f3bb", "The specified MAWB already exists."), Res.GetString("1f87bce4-e605-4b39-bc16-8727bcd1d2c4", "Error"));
			}

			return result;
		}

		bool IsDuplicateMAWB()
		{
			var consol = MawbParent as ForwardingConsol;

			if (consol != null)
			{
				var mawb = JM_Airline3DigitPrefixBoundTextBox.Text + JM_MAWBBoundTextBox.Text;
				return MasterBillValidator.IsDuplicate(consol,
					consol.JK_SystemCreateTimeUtc.IsValid ? consol.JK_SystemCreateTimeUtc.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value) : ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value),
					consol.JK_SystemCreateTimeUtc.IsValid ? consol.JK_SystemCreateTimeUtc.AddMonths(FreightDataRegistry.Instance.MAWBRecyclePeriod.Value) : ZDateTime.Empty,
					mawb);
			}

			return false;
		}

		void Init()
		{
			this.JM_GBBoundGuidFindBox.ReadOnly = !FreightDataRegistry.Instance.AllowMatchingOfMAWBsFromOtherBranches.Value;
		}

		#region Actions

		void CnclButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			ContinueWithSave result;

			try
			{
				result = ValidateAndSave();
			}
			catch (ZSaveException ex)
			{
				result = ContinueWithSave.No;
				HandleSaveException(ex);
			}

			if (result == ContinueWithSave.Yes)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		#endregion
	}
}
