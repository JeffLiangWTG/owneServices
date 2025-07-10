namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Shipment Registration and Customs Entries Report")]
	public class TestShipmentRegistrationandCustomsEntriesReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	public class TestShipmentRegistrationandCustomsEntriesMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get
			{
				return @"Shipment Registration & Customs Entries";
			}
		}

		public override string Hint
		{
			get
			{
				return @"The Shipment & Declarations Count by Date Report provides a count of shipments registered and declaration jobs created on a particular day. 
It provides total counts by day of shipments and customs jobs registered each day.  

Note: This report includes both FORWARDING SHIPMENT and STANDALONE DECLARATION records";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestShipmentRegistrationandCustomsEntriesReport();
		}
	}
}
