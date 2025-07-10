namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;
	using Enterprise.ReportTesting;

	public class TestShipmentContainersByClientAndCarrierMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get
			{
				return @"Shipment Containers By Client and Carrier";
			}
		}

		public override string Hint
		{
			get
			{
				return @"Use the Shipment Containers by Client and Carrier to generate a list of FCL and BCN containers by client. 
The report will lists FCL and BCN containers attached to shipments and declarations where the nominated client/s are Consignor/Exporter or Consignee/Importer.

Note: This report includes both FORWARDING SHIPMENT and STANDALONE DECLARATION records";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new Freight.Forwarding.TestShipmentContainersByClientAndCarrierTemplate();
		}
	}
}
