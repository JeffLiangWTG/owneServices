using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrganisationTaxRateFileImportForm : ZForm
	{
		public OrganisationTaxRateFileImportForm()
			: base(new OrganisationTaxRateFileImport())
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		OrganisationTaxRateFileImport OrganisationTaxRateFileImportBusinessEntity => (OrganisationTaxRateFileImport)base.BusinessEntity;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Event Handlers

		void ImportButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog.DefaultExt = (NoResString)"txt";
			OpenFileDialog.Filter = (NoResString)"Text Files (*.txt)|*.txt|All files (*.*)|*.*";
			OpenFileDialog.FilterIndex = 0;
			OpenFileDialog.RestoreDirectory = true;

			if (ZFormModaliser.ShowCommonDialogWithoutDispose(OpenFileDialog) == DialogResult.OK)
			{
				FileNameTextBox.Text = OpenFileDialog.UnmappedFileName;
				var notifications = new NotificationBuffer();

				var importer = ObjectFactory.Get<IOrganisationTaxRateFileImportFileDataImporter>();
				importer.ImportData(OpenFileDialog.ForceLocalFile(), notifications, OrganisationTaxRateFileImportBusinessEntity);

				if (notifications.HasErrors)
				{
					Globals.Message.ShowError(notifications.AsString);
				}
				else if (notifications.HasWarnings)
				{
					Globals.Message.ShowWarning(notifications.AsString);
				}

				ImportButton.Enabled = false;
				TaxConfigurationCodeAndDescriptionZGuidDropEdit.Enabled = false;
				RateSourceDropEdit.Enabled = false;
			}
		}

		#endregion

		public override string FormVerb => string.Empty;
		protected override bool AllowNew => false;
	}
}
