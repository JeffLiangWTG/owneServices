namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Shipment Delivery Legs - Proof of Delivery Report")]
	public class TestShipmentDeliveryLegsProofOfDeliveryReport : TemplateTestCase
	{
	}

	public class TestShipmentDeliveryLegsProofOfDeliveryReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Shipment Delivery Legs - Proof of Delivery Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to manage the recording and reporting of proof of delivery of cargo to your clients. 
To meet your reporting obligations back to your clients, you can use this report you can produce a list of shipments with Proof of Delivery recorded on the local transport delivery legs. 
Alternatively, you can also run this report and list local transport delivery legs with NO proof of delivery details. When run in this way you can use the report as a tool to follow up your local cartage contractors for details on proof of delivery.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestShipmentDeliveryLegsProofOfDeliveryReport();
		}
	}
}
