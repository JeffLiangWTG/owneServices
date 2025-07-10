using Enterprise.Freight.Forwarding.Orders.Module;

namespace Enterprise.ReportTesting.Freight.Orders
{
	[TemplateName("Client - Order and Supplier Booking Status Report")]
	public class TestOrderAndSupplierBookingStatusReport : TemplateTestCase
	{
	}

	public class OrderAndSupplierBookingStatusReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new OrdersReportModule(); }
		}

		public override string MenuName
		{
			get { return @"Client - Order and Supplier Booking Status"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestOrderAndSupplierBookingStatusReport();
		}

		public override string Hint => "";
	}
}
