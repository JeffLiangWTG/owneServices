namespace Enterprise.ReportTesting.Freight.Orders
{
	using Enterprise.Freight.Forwarding.Orders.Module;

	[TemplateName("Client - Order Status Summary Report")]
	public class TestClientOrderStatusSummaryReport : TemplateTestCase
	{
	}

	public class OrderStatusSummaryMenuSetupTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new OrdersReportModule(); }
		}

		public override string MenuName
		{
			get { return @"Client - Order Status Summary"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestClientOrderStatusSummaryReport();
		}

		public override string Hint
		{
			get
			{
				return

@"The Client Order Status Summary Report shows order header details for a selected Client.  
For each order the report will list selected order header details. 

Shipping details listed in the report will come from the shipment attached to an order. When there is no shipment details will be drawn from the 'Planning' tab of the Order.

The actual columns displayed in this report can be customized for each or your clients through your column selections on the 'Columns' filter tab.  On this same tab, you can save a set of 'default columns' for a particular client. This 'default' layout will automatically be used the next time you run the report for that same client.  You can change the column settings for a client simply by re-selecting columns and re-saving the default layout for that client.";
			}
		}
	}
}
