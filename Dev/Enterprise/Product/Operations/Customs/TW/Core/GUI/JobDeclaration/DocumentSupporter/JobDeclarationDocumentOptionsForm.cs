using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class JobDeclarationDocumentOptionsForm : ZChildForm
	{
		public JobDeclarationDocumentOptionsForm(bool isExport, JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig)
			: base(jobDeclarationDocumentAddressConfig)
		{
			ImportAddressGroupBox.Visible = !isExport;
			ExportAddressGroupBox.Visible = isExport;
			CellSettingGroupBox.Visible = !jobDeclarationDocumentAddressConfig.HideCustomizeSectionBodyRow;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormHeading => Res.GetString("DCAE0DAD-D11C-4B26-9938-4A409B9598DD", "Customs Declaration Options");

		void DeliverButton_Click(object sender, System.EventArgs e)
		{
			var jobDeclarationSadDocumentSupporter = CurrentDataItem as JobDeclarationDocumentAddressConfig;
			jobDeclarationSadDocumentSupporter.Validation.ValidateAll();
			if (jobDeclarationSadDocumentSupporter.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}
	}
}
