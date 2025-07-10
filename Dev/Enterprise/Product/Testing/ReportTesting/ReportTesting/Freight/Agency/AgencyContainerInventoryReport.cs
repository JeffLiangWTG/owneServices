namespace Enterprise.ReportTesting.Freight.Agency
{
	using Enterprise.Freight.Agency.Module;
	using Enterprise.ZArchitecture.Modules;

	[TemplateName("Agency Container Inventory Report")]
	internal sealed class AgencyContainerInventoryReport : TemplateTestCase
	{
	}

	internal sealed class AgencyContainerInventoryReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				return
					"This report allows monitoring of current container stock on land.\r\n" +
					"\r\n" +
					"The report provides container counts by:\r\n" +
					" - Location Category (e.g. yard, wharf, with consignor or consignee)\r\n" +
					" - Geographic Location\r\n" +
					" - Quality & Condition at Yard\r\n" +
					" - Full/Empty status at Depot or Wharf\r\n" +
					" - Import/Export status at Depot or Wharf";
			}
		}

		public override string MenuName
		{
			get { return "Container Inventory"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AgencyContainerInventoryReport();
		}
	}
}
