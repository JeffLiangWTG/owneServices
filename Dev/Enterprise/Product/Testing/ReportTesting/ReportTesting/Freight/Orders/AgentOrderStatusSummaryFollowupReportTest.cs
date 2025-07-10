namespace Enterprise.ReportTesting.Freight.Orders
{
	using Enterprise.Freight.Forwarding.Orders.Module;

	[TemplateName("Agent Order Status Summary Followup Report")]
	public class TestAgentOrderStatusSummaryFollowupReport : TemplateTestCase
	{
	}

	public class AgentOrderStatusSummaryFollowupMenuSetupTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new OrdersReportModule(); }
		}

		public override string MenuName
		{
			get { return @"Agent Order Status Summary Followup Report"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAgentOrderStatusSummaryFollowupReport();
		}

		public override string Hint
		{
			get
			{
				return

@"The Agent Order Status Summary Follow Up Report shows order header details for a selected Agent.  
This report will list all orders where the selected agent is either the Sending or Receiving agent on the order.
For each order the report will list selected order header details. 

Shipping details listed in the report will come from the shipment or standalone declaration attached to an order. 
When there is no shipment/declaration, details will be drawn from the 'Planning' tab of the Order.

The actual columns displayed in this report can be customized for each agent by saving a 'Column Customization' for your agent.";
			}
		}
	}
}
