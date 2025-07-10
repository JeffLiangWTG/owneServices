
namespace Enterprise.Customs.GUI
{
	public partial class InvoiceImporterForm : ZArchitecture.GUI.DataTransferForm
	{
		public InvoiceImporterForm()
		{
			InitializeComponent();
			this.fFormHeading = FormCaption;
		}

		public bool SkipUnknownSupplierRecords
		{
			get { return SkipUnkSupplierCheckBox.Checked; }
			set { SkipUnkSupplierCheckBox.Checked = value; }
		}

		#region Form Overrides

		public override string FormCaption
		{
			get { return Res.GetString("2fb301ed-62b7-4935-8a23-c1ee8ce8ab20", "Invoice Data Importer"); }
		}

		public override void FinishProcess()
		{
			base.FinishProcess();
			this.SkipUnkSupplierCheckBox.Enabled = false;
		}

		#endregion
	}
}
