using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class AllocateNumberForm : ZChildForm
	{
		public AllocateNumberForm(AllocateNumber allocateNumber)
			: base(allocateNumber)
		{
		}

		public override string FormCaption
		{
			get { return formCaption; }
		}
		string formCaption = "Allocate Entry Number";

		public override string FormVerb
		{
			get { return ""; }
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
		}

		internal void SetCaptions(CargoWise.Types.ZString numberType)
		{
			EntryNumberTextBox.CaptionResourceString = Res.GetData("c93902da-3b76-4ca5-8a86-9a1d0f85835e", "Number");

			switch (numberType)
			{
				case CusEntryNumberTypes.UnitedStates.Protest:
					formCaption = "Allocate CBP Assigned Number";
					OKButton.Text = "Allocate CBP Number";
					AllocateNumberGroupBox.Text = "CBP Assigned Number";
					DescriptionLabel.Text = "Enter the number, click 'Allocate CBP Number' and CBP Number will be allocated for this job.";
					break;
			}
		}
	}
}
