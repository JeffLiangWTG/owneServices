using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.US.GUI.Testing
{
	abstract class ZPGAFormBasherAbstractTest<TPGAUserControl> : ZFormBasherTest where TPGAUserControl : UserControl, new()
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = GetDeclaration();
			GetPGABusinessObject(declaration.InvoiceLines[0]);
			Factory.Save();

			return new ACEPGATestForm<TPGAUserControl>(declaration, BindMember);
		}

		protected abstract string BindMember { get; }

		protected abstract BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine);

		protected JobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			return declaration;
		}
	}

	sealed class ACEPGATestForm<TPGAUserControl> : ZForm
		where TPGAUserControl : UserControl, new()
	{
		public ACEPGATestForm(JobDeclaration declaration, string bindMember)
			: base(declaration)
		{
			this.bindMember = bindMember;
			InitializeComponent();
		}
		readonly string bindMember;
		UserControl userControl;

		new void InitializeComponent()
		{
			base.InitializeComponent();

			userControl = new TPGAUserControl();
			BindingSource.SetBindingMember(userControl, bindMember);
			userControl.Dock = DockStyle.Fill;
			Size = new System.Drawing.Size(1024, 680);

			Controls.Add(userControl);
			DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			DataSourceTypeName = "Enterprise.Customs.US.Business.JobDeclaration";
			Name = "PGAUserControl";
		}
	}
}
