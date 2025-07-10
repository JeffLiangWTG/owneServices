namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Air Consol MAWB Allocations by Carrier")]
	public class TestAirConsolMAWBAllocationsbyCarrier : TemplateTestCase
	{
	}

	public class TestAirConsolMAWBAllocationsbyCarrierMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Air Consol MAWB Allocations by Carrier"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to generate a list of Air Consol MAWB allocations by Carrier.
Consols are selected based on their First Load Port ETD date.
After nominating an ETD date range the report will list MAWB details by Carrier. 
You can further limit the report to only listing MAWB stock for First Load Ports in a nominated Port or Country/Region.
Optional columns in this report allow you to include Consol Pre-Allocation and Actual Shipment Volume, Weight and Chargeable units.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAirConsolMAWBAllocationsbyCarrier();
		}
	}
}
