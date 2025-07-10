using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DDRBankAccountDisclaimerControl : ZUserControl
	{
		public DDRBankAccountDisclaimerControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisclaimerMessageLabel.Text = Res.GetString("C1DA3762-710B-4119-93D4-9721BC8CC94A", @"You have selected an ABA file format for this bank account.
In order for your DDR file to be accepted by the bank, please ensure you have recorded the ""User ID No"" and that you have set the ""Abbreviation"" to the approved code for the financial institution.");
		}

		public static DialogDefaultContext DialogDefaultContext
		{
			get
			{
				return new DialogDefaultContext(new ZGuid("0247AACB-3416-4F56-A8C5-AFA8E80F63E2"),
												ResString.GetMultilingualString("001FE0BC-B5E5-4D31-B1B4-2ED77AC335F3", "Save DDR Account"),
												null,
												ZMessageBoxIcon.Information,
												showCheckboxOnly: true,
												checkBoxCaption: Res.GetData("2315EB59-6FB0-4068-86A9-703E82F8922B", "Do not show this message again"));
			}
		}
	}
}
