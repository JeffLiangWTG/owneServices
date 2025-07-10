using Enterprise.DocumentEngine.Testing;

namespace Enterprise.ReportTesting.Freight.Schedules
{
	[TemplateName("One Stop Import Arrival Report")]
	[CountryCode(Core.Constants.CountryCodes.Australia)]
	public class OneStopVesselArrivalTemplate : TemplateTestCase
	{
	}

	public class OneStopVesselArrivalReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Freight.Module.BookingReports(); }
		}

		public override string MenuName
		{
			get { return "1-Stop Vessel Arrival Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"1-Stop data can be seen through Operations > Schedules > Sailing Schedule Feed.
This report details Discharge Port information for five AU ports and one NZ port.
Drawn from 1-Stop data, it is detailing information for Vessels discharging in Australia or New Zeland.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OneStopVesselArrivalTemplate();
		}
	}
}
