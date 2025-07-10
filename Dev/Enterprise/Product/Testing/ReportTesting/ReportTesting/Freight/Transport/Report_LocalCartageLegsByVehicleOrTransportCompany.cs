namespace Enterprise.ReportTesting.Freight
{
	using Enterprise.Freight.Module;

	[TemplateName("Local Transport Legs By Vehicle or Transport Company")]
	public class TestReport_LocalTransportLegsByVehicleOrTransportCompanyTemplate : TemplateTestCase
	{
	}

	public class TestReport_LocalTransportLegsByVehicleOrTransportCompanyTemplateMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new TransportReports(); }
		}

		public override string MenuName
		{
			get { return "Port Transport Legs By Vehicle or Transport Company"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report itemizes the transport legs of each Vehicle or Port Transport company recorded on Port Transport jobs.
Reading Down the Report, results are grouped and listed by Vehicle / Transport company. 
All Columns are configurable. This means columns can be included / excluded from the final report by the user.
Reading Across the Report, columns will report details about each leg or the Port Transport Job the leg is attached to.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestReport_LocalTransportLegsByVehicleOrTransportCompanyTemplate();
		}
	}
}
