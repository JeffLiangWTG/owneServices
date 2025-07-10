using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingContainerFilterStripBusinessObject))]
	public class TrackingContainerFilterStripBusinessObjectTest2 : FilterStripBusinessObjectTestCase
	{
		#region Test Availablility Date Filter

		public void TestAvailableDateFilterNoSailing()
		{
			TrackingConsol consol = Factory.NewWithValidTestData<TrackingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";

			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			container.JC_FCLAvailable = ZDateTime.Today;
			consol.Containers.Add(container);

			AssertNull("Prequisite", consol.Schedule);

			Factory.Save();

			Assert("Prequisite", consol.IsInDatabase);
			Assert("Prequisite", container.IsInDatabase);

			AssertAvailabilityDate((date) => container.JC_FCLAvailable = date);
		}

		[TestDate(1978, 10, 11, 00, 22, 0)]
		public void TestAvailableDateFilterNoSailingNearMidnight()
		{
			TestAvailableDateFilterNoSailing();
		}

		public void TestAvailableDateFilterWithSailing()
		{
			var consol = Factory.NewWithValidTestData<TrackingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "AUSYD";

			AssertNotEquals("Prequisite", 0, consol.Transports.Count);

			var transport = consol.Transports[0];
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			transport.JW_VoyageFlight = "1111";

			consol.VoyDestination.JB_AvailabilityDate = ZDateTime.Today;

			AssertNotNull("Prequisite", transport.Sailing);
			AssertNotNull("Prequisite", consol.Voyage);
			AssertNotNull("Prequisite", consol.Schedule);
			AssertEquals("Prequisite", ZDateTime.Today, consol.Schedule.Destination.JB_AvailabilityDate);

			var container = Factory.NewWithValidTestData<TrackingContainer>();
			consol.Containers.Add(container);
			container.JC_OverrideFCLAvailableStorage = false;

			Factory.Save();

			Assert("Prequisite", consol.IsInDatabase);
			Assert("Prequisite", transport.IsInDatabase);
			Assert("Prequisite", container.IsInDatabase);

			AssertAvailabilityDate((date) => consol.VoyDestination.JB_AvailabilityDate = date);
		}

		public void TestAvailableDateFilterWithCustomDeclaration()
		{
			var date = ZDateTime.Today;

			var container = Factory.New<CommonContainer>();
			container.JC_ContainerNotes = "Container1";
			container.JC_ContainerNum = "cn123456";
			container.JC_SealNum = "sn123456";
			container.JC_AdditionalSealNum = "sa123456";
			container.JC_Additional2SealNum = "sb123456";
			container.JC_DepartureSlotReference = "ds123456";
			container.JC_ArrivalSlotReference = "as123456";

			container.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Empty;
			container.JC_FCLAvailable = date;
			container.JC_ArrivalCTOStorageStartDate = date;
			container.JC_FCLWharfGateIn = date;
			container.JC_FCLWharfGateOut = date;
			container.JC_ContainerYardEmptyReturnGateIn = date;
			container.JC_ContainerYardEmptyPickupGateOut = date;
			container.JC_FCLUnloadFromVessel = date;
			container.JC_FCLOnBoardVessel = date;

			var decl = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();

			var cusContainer = ((BusinessObjectCollection)decl["CusContainers"]).AddNew();
			cusContainer[CusContainerSchema.Constants.CO_JC] = container.PK;

			decl[JobDeclarationSchema.Constants.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			decl[JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading] = "AUBNE";
			decl[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = "AUSYD";

			container.JC_OverrideFCLAvailableStorage = false;

			var destination = Factory.NewWithValidTestData<VoyageDestination>();
			destination.JB_AvailabilityDate = date;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.JX_JB = destination.PK;

			var transport = (Transport)((BusinessObjectCollection)decl["Transports"]).AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			Factory.Save();

			Assert("Prequisite", decl.IsInDatabase);
			Assert("Prequisite", transport.IsInDatabase);
			Assert("Prequisite", cusContainer.IsInDatabase);

			var availableFilter = (ModuleDateFilter)TestFilterStrip["Available"];
			availableFilter.IsActive = true;
			availableFilter.PropertySearch = "Date range";

			availableFilter.Property1 = date;
			availableFilter.Property2 = date.AddDays(1);

			AssertEquals(true, Factory.Load<TrackingContainer>(TestFilterStrip.Filter).Length > 0);
		}

		void AssertAvailabilityDate(Action<DateTime> dateUpdater)
		{
			ModuleLocationFilter loadDischargeFilter = ((ModuleLocationFilter)TestFilterStrip["Load / Discharge"]);
			loadDischargeFilter.IsActive = true;
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUSYD";

			AssertEquals(TestFilterStrip.Filter.LiteralTextSqlFormatted, 1, Factory.Load<TrackingContainer>(TestFilterStrip.Filter).Length);

			ModuleDateFilter availableFilter = (ModuleDateFilter)TestFilterStrip["Available"];
			availableFilter.IsActive = true;
			availableFilter.PropertySearch = "This Week";

			AssertEquals(String.Format("Filter\n{0}\ndid not yeld correct results", TestFilterStrip.Filter.LiteralTextSqlFormatted),
				1, Factory.Load<TrackingContainer>(TestFilterStrip.Filter).Length);

			availableFilter.PropertySearch = "Last Week";
			AssertEquals(String.Format("Filter\n{0}\ndid not yeld correct results", TestFilterStrip.Filter.LiteralTextSqlFormatted),
				0, Factory.Load<TrackingContainer>(TestFilterStrip.Filter).Length);

			availableFilter.PropertySearch = "This Week";
			loadDischargeFilter.Property2 = "AUBNE";
			AssertEquals(String.Format("Filter\n{0}\ndid not yeld correct results", TestFilterStrip.Filter.LiteralTextSqlFormatted),
				0, Factory.Load<TrackingContainer>(TestFilterStrip.Filter).Length);

			dateUpdater(DateTime.Today.AddDays(-1));
			Factory.Save();

			AssertEquals("Prequisite", "AUBNE", loadDischargeFilter.Property2);
			availableFilter.PropertySearch = "Next Week";
			AssertEquals(String.Format("Filter\n{0}\ndid not yeld correct results", TestFilterStrip.Filter.LiteralTextSqlFormatted),
				0, Factory.Load<TrackingContainer>(TestFilterStrip.Filter).Length);

			loadDischargeFilter.Property2 = "AUSYD";
			AssertEquals(String.Format("Filter\n{0}\ndid not yeld correct results", TestFilterStrip.Filter.LiteralTextSqlFormatted),
				0, Factory.Load<TrackingContainer>(TestFilterStrip.Filter).Length);
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			TestFilterStrip = (TrackingContainerFilterStripBusinessObject)GetNewBusinessObject();
			TestFilterStrip.CurrentOrg = header.PK;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TrackingContainerFilterStripBusinessObject();
		}

		TrackingContainerFilterStripBusinessObject TestFilterStrip;

		#endregion
	}
}
