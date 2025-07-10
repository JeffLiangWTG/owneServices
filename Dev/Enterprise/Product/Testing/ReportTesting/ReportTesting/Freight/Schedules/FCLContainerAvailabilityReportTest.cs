using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.ReportTesting.Freight.Schedules
{
	[TemplateName("FCL Container Availability")]
	[CountryCode(Core.Constants.CountryCodes.Australia)]
	public class FCLContainerAvailabilityReportTest : TemplateTestCase
	{
		protected override void SetUp()
		{
			JobVoyage voyage = CreateVoyage();
			CreateDeclarationWithContainer(voyage);
			CreateConsolWithContainer(voyage);
			Factory.Save();
			base.SetUp();
		}

		JobVoyage CreateVoyage()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "MYPKG";
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			JobSailing sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			return voyage;
		}

		BaseJobDeclaration CreateDeclarationWithContainer(JobVoyage voyage)
		{
			BaseJobDeclaration declaration = voyage.Factory.New<BaseJobDeclaration>();
			declaration.JE_VesselName = "Vessel";
			declaration.JE_VoyageFlightNo = "Voyage";
			declaration.JE_RL_NKFinalDestination = "AUSYD";

			BaseCusContainer cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "CusContainer";
			return declaration;
		}

		ForwardingConsol CreateConsolWithContainer(JobVoyage voyage)
		{
			ForwardingConsol consol = voyage.Factory.New<ForwardingConsol>();
			Transport consolTransport = consol.Transports.AddNew();
			consolTransport.JW_JX = voyage.Sailings[0].PK;

			ForwardingContainer consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = "ConsolContainer";
			return consol;
		}
	}

	public class FCLContainerAvailabilityReportTestMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Freight.Module.BookingReports(); }
		}

		public override string MenuName
		{
			get { return "FCL Container Availability"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report displays Availability and Storage details for containers not picked up from the wharf, for a specified date range, Port or CTO.  Container Availability and Storage dates and times entered on the Sailing Schedule appear on the Consol Routing, these dates determine the Storage charges on overdue containers.
Use this report to monitor containers waiting to be picked up from the CTO.
The report may be grouped on CTO or storage Date, filters include Availability Date Range, ETA Date Range, Discharge Port and CTO.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new FCLContainerAvailabilityReportTest();
		}
	}
}
