using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgSecurityProfileUpdateForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		protected OrgSecurityProfileUpdateForm()
		{
			InitializeComponent();
		}

		public OrgSecurityProfileUpdateForm(OrgSecurityProfileUpdater updater, bool shouldShowConfirmation = false) : base(updater)
		{
			ShouldShowConfirmation = shouldShowConfirmation;
		}

		public static readonly Overridable<bool> CustomerSelfManagementEnabled = new Overridable<bool>();

		readonly bool ShouldShowConfirmation;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!CustomerSelfManagementEnabled.Value)
			{
				ShouldApplyGrantedCheckBox.Visible = false;
				ShouldApplyCustomerSelfManagementCheckBox.Visible = false;
				ShouldApplyGrantedCheckBox.Checked = true;
				ShouldApplyCustomerSelfManagementCheckBox.Checked = false;
				BusinessEntity.HasChanges = false;
			}
		}

		void UpdateButton_Click(object sender, EventArgs e)
		{
			var updater = (OrgSecurityProfileUpdater)BusinessEntity;
			updater.RunPreSaveValidation();

			if (updater.HasErrors)
			{
				ShowErrorsDialog();
				return;
			}

			if (ShouldShowConfirmation)
			{
				if (Globals.Message.ShowConfirmation(
							Res.GetString("e2489151-1495-48b0-be6a-8a0c764dc778",
@"This can be a time-consuming process.
This process cannot be reversed.
If this process is run during production hours on large systems it can potentially slow operational processing.
The safest approach on a larger heavily used system is to run this process out of business hours."),
					Res.GetString("d3cf3f0b-85e5-4228-a8e5-aeb08592ade6", "Apply Security Profile"),
							Res.GetString("c49473fe-6941-4e3b-9db3-72cb48dcb05d", "Please type the following to continue:"),
							Res.GetString("7d409bdc-d6ee-492a-b7ea-83db75ccc795", "Yes"),
							MessageBoxIcon.Warning,
							ConfirmationMessageLayout.AllInOneLine
						) != DialogResult.OK)
				{
					Close();
					return;
				}
			}

			var affectedOrgs = 0;
			var hasConcurrencyException = false;
			using (var progressForm = new ProgressForm())
			{
				try
				{
					progressForm.Cancelled += (_, x_) => updater.Cancel();
					updater.OrgHeaderProcessing += (_, evt) =>
					{
						if (!progressForm.Visible)
						{
							progressForm.ShowModalTo(this);
						}

						progressForm.SetStatusAndPercentComplete(evt.Status, evt.PercentComplete);
					};

					affectedOrgs = updater.Update();
				}
				catch (ZSaveConcurrencyException)
				{
					hasConcurrencyException = true;
				}
				finally
				{
					progressForm.Close();
					Close();
				}
			}

			if (hasConcurrencyException)
			{
				Globals.Message.ShowError(Res.GetString("7d6cd8aa-8686-4ec8-b4d0-55f6c111f1f2", "While you have been working with this form, another user has made changes affecting the update process. Please try again."));
			}

			if (ShouldShowConfirmation)
			{
				Globals.Message.ShowInformation(Res.GetString("84e08371-2735-420b-82fc-128aed9cee3a", "{0} organization(s) have been updated.", affectedOrgs.ToString(CultureInfo.InvariantCulture)));
			}
		}

		void CancelFormButton_Click(object sender, EventArgs e) => Close();

		public override string FormVerb => string.Empty;
	}
}
