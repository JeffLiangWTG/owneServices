namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;
	using Enterprise.ReportTesting;

	public class TestImportDeliveryAndContainerReturnMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Container - Import Full Delivery & Empty Return Master Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This is a report identifying the delivery of FULL containers to Consignee/Importer (or Arrival Depot for Groupage Containers) AND return of the unpacked empty container to the Container Yard.
It analyses Containers on FORWARDING Shipments/Consols and Customs Declarations.
It is an IMPORT container report that tracks FCL, BCN and GRP containers.

Note: This report includes both FORWARDING and STANDALONE DECLARATION records";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new Freight.Forwarding.TestImportDeliveryAndContainerReturnTemplate();
		}
	}
}
