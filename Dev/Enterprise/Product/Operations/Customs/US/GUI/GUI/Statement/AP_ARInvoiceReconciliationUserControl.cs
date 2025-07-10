using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class AP_ARInvoiceReconciliationUserControl : ZUserControl
	{
		public AP_ARInvoiceReconciliationUserControl()
		{
			InitializeComponent();
		}

		void AccIntegrationButton_Click(object sender, System.EventArgs e)
		{
			CusStatementHeader statement = (CusStatementHeader)DataSource;

			new AccIntegrationHandler().PerformIntegration(statement, (ZForm)ParentForm);
		}
	}
}
