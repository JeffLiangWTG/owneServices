namespace Enterprise.ReportTesting.Freight.Schedules
{
	[TemplateName("Route Planning")]
	public class RoutePlanningTemplate : TemplateTestCase
	{
	}

	public class RoutePlanningReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Freight.Module.BookingReports(); }
		}

		public override string MenuName
		{
			get { return "Sea Freight - Route Planning"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report is a route planning tool.  

It will detail all the possible routes between a given origin and destination within a given period. 

The routes are derived by combining existing consecutive sailings such that the first sailing departs from the origin and the last sailing arrives at the destination within the given period.

Routes can be refined by detailing the maximum number of days in transit, the maximum number of vessel\Voyage changes, the maximum number of sailing legs and whether or not multiple carriers can be used.

For each route Ports of call, arrival and departure time, vessel\Voyage, length of leg, days between vessel\voyage change and carrier are detailed.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new RoutePlanningTemplate();
		}
	}
}
