namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Import Container Delivery & Return")]
	public class TestImportDeliveryAndContainerReturnTemplate : TemplateTestCase
	{
	}

	public class TestImportDeliveryAndContainerReturnReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Container - Import Full Delivery & Empty Return Master Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This is a report identifying 
   a) delivery of FULL containers to Consignee or Arrival Depot
   AND
   b) return of the unpacked empty container to the Container Yard.

   It analyses Containers on FORWARDING Shipments/Consols and Customs Declarations.

   It is an IMPORT container report.
   It evaluates FCL, BCN and GRP containers.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestImportDeliveryAndContainerReturnTemplate();
		}
	}
}
