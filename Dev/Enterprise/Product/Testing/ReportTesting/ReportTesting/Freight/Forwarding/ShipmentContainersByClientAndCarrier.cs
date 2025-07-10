namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Shipment Containers by Client and Carrier")]
	public class TestShipmentContainersByClientAndCarrierTemplate : TemplateTestCase
	{
	}

	public class TestShipmentContainersByClientAndCarrier : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return @"Shipment Containers By Client and Carrier"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use the Shipment Containers by Client and Carrier to generate a list of FCL and BCN containers by client. The report will lists FCL and BCN containers attached to shipments where the nominated client/s are Consignor/Exporter or Consignee/Importer.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestShipmentContainersByClientAndCarrierTemplate();
		}
	}
}
