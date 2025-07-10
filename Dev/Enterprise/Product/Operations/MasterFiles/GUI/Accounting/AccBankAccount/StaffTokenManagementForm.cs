using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class StaffTokenManagementForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public StaffTokenManagementForm()
		{
			InitializeComponent();
		}

		public StaffTokenManagementForm(AccBankAccount bo)
			: base(bo)
		{
		}

		AccBankAccount BankAccount => BusinessEntity as AccBankAccount;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisclaimerMessage.Text = Res.GetString("6af3e280-0239-4be2-a95e-a57b625ad5f5", @"Please add the details of any person who you want to be able request FX quotes and book FX transactions with {0} from the Payments Processing Module.
Please note each user must first be added as an authorized user on your organization’s {0} account.
When you add a user to your E-Payment Account below, they will be asked to authorize the connection of your E-payment Account and {0} account using their {0} user name and password.
Once a user’s status below is listed as ""Authorized"" they will be able to request FX quotes and book FX transactions with {0} from the Payment Processing module.
Click ""Learn More"" for more information about {0} and CargoWise Global Integrated Payments.", BankAccount.AB_PaymentProvider);
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			if (BankAccount.EPaymentStaffTokenCollection.HasChanges)
			{
				var staffTokens = BankAccount.EPaymentStaffTokenCollection.OfType<AccEPaymentStaffToken>();
				if (staffTokens.Any(x => x.HasErrors))
				{
					Globals.Message.ShowError(Res.GetString("7DAA072C-2997-49D2-8B87-40C4A53D3AB7", "Some records have errors, please fix the errors before close the form"));
					return;
				}
				var result = Globals.Message.Show(Res.GetString("C29E5A60-EA96-4CCE-8A7D-A2FE1B9A21F1", "There are unsaved changes, do you want to save them first before close the form?"), Res.GetString("12CFA0F6-7B8D-4807-AADC-3F57CEE65F45", "Warning"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					try
					{
						BusinessEntity.Factory.Save();
						Close();
					}
					catch (ZCannotSaveException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
				else if (result == DialogResult.No)
				{
					Close();
				}
			}
			else
			{
				Close();
			}
		}

		void AuthorizeButton_Click(object sender, EventArgs e)
		{
			var selectedStaffToken = staffTokenGrid.GetCurrent() as AccEPaymentStaffToken;
			if (selectedStaffToken == null)
			{
				Globals.Message.ShowError(Res.GetString("61B04C75-F727-4E9E-AA01-B36E7E14E5BE", "Cannot authorize, please choose a record first"));
				return;
			}
			if (BankAccount.EPaymentStaffTokenCollection.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("131A064F-81A6-4A00-9836-C33A5739BF77", "Please save the form before start the authorization"));
				return;
			}
			if (!GlbStaff.CurrentUser.GS_Code.Equals(selectedStaffToken.TK_GS_NKStaffCode))
			{
				Globals.Message.ShowError(Res.GetString("F871BFC3-11A9-48AA-95A3-F5185B4B4881", "Users can only authorize their own accounts"));
				return;
			}

			var authorizeURL = selectedStaffToken.PrepareOAuthURL();
			try
			{
				selectedStaffToken.ResetToPendingStatus();
				BusinessEntity.Factory.Save();
				staffTokenGrid.Refresh();
				WebUrlLauncher.Launch(authorizeURL);
			}
			catch (ZCannotSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void saveButton_Click(object sender, EventArgs e)
		{
			var staffTokens = BankAccount.EPaymentStaffTokenCollection.OfType<AccEPaymentStaffToken>();
			if (staffTokens.Any(x => x.HasErrors))
			{
				Globals.Message.ShowError(Res.GetString("C16F8DEC-95D6-42C7-8805-F067B3813B58", "Some records have errors, please fix the errors before save."));
				return;
			}
			try
			{
				BusinessEntity.Factory.Save();
				staffTokenGrid.Refresh();
			}
			catch (ZCannotSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void refreshButton_Click(object sender, EventArgs e)
		{
			BankAccount.EPaymentStaffTokenCollection.Reload(true, true);
			staffTokenGrid.Refresh();
		}

		void LearnMoreButton_Click(object sender, EventArgs e)
		{
			EPaymentUrlLauncher.LaunchEPaymentProductMarketingURL();
		}
	}
}
