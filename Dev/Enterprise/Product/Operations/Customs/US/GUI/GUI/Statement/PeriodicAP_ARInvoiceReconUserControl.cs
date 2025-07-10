using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Statement
{
	public partial class PeriodicAP_ARInvoiceReconUserControl : ZUserControl
	{
		public PeriodicAP_ARInvoiceReconUserControl()
		{
			InitializeComponent();
		}

		void AccIntegrationButton_Click(object sender, EventArgs e)
		{
			new AccIntegrationHandler().PerformIntegration((CusStatementHeader)DataSource, (ZForm)ParentForm);
		}
	}
}
