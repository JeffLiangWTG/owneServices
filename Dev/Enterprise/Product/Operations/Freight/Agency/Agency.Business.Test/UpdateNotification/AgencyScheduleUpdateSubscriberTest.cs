using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyScheduleUpdateSubscriberTest : ScheduleUpdateSubscriberTest<AgencyScheduleUpdateSubscriber>
	{
		[ShipmentDateUpdateConfiguration(Value = true)]
		public void TestATDChanged_Enabled()
		{
			var providerMock = new Mock<IScheduleUpdateQueryProvider>();
			var providerDelegateMock = new Mock<GetValueDelegate<IScheduleUpdateQueryProvider>>();

			IScheduleUpdateQueryProvider provider = providerMock.Object;
			GetValueDelegate<IScheduleUpdateQueryProvider> providerDelegate = providerDelegateMock.Object;

			ZDateTime today = ZDateTime.Today;
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			BillOfLading bill1 = Factory.New<BillOfLading>();
			bill1.JS_UniqueConsignRef = "Bill1";
			bill1.JS_JX = sailing.PK;
			bill1.JS_ShippedOnBoardDate = today.AddDays(-3);
			bill1.JS_HouseBillIssueDate = ZDateTime.Empty;
			BillOfLading bill2 = Factory.New<BillOfLading>();
			bill2.JS_UniqueConsignRef = "Bill2";
			bill2.JS_JX = sailing.PK;
			bill2.JS_ShippedOnBoardDate = today.AddDays(-2);
			bill2.JS_HouseBillIssueDate = today.AddDays(-3);
			BillOfLading bill3 = Factory.New<BillOfLading>();
			bill3.JS_UniqueConsignRef = "Bill3";
			bill3.JS_JX = sailing.PK;
			bill3.JS_ShippedOnBoardDate = ZDateTime.Empty;
			bill3.JS_HouseBillIssueDate = today.AddDays(-2);
			BillOfLading bill4 = Factory.New<BillOfLading>();
			bill4.JS_UniqueConsignRef = "Bill4";
			bill4.JS_JX = sailing.PK;
			bill4.JS_ShippedOnBoardDate = ZDateTime.Empty;
			bill4.JS_HouseBillIssueDate = ZDateTime.Empty;
			ScheduleUpdateQueryProviderFactory.Set(Factory, providerDelegate);

			providerDelegateMock
				.Setup(m => m.Invoke())
				.Returns(provider);

			providerMock
				.SetupGet(m => m.ShouldUpdateAgencyShipmentDatesFromATD)
				.Returns(true);

			origin.JA_A_DEP = today;

			providerMock
				.VerifyGet(m => m.ShouldUpdateAgencyShipmentDatesFromATD, Times.AtLeastOnce);

			CombineAssertions(delegate
			{
				AssertEquals("bill1.JS_ShippedOnBoardDate", today.AddDays(-3), bill1.JS_ShippedOnBoardDate);
				AssertEquals("bill2.JS_ShippedOnBoardDate", today.AddDays(-2), bill2.JS_ShippedOnBoardDate);
				AssertEquals("bill3.JS_ShippedOnBoardDate", today, bill3.JS_ShippedOnBoardDate);
				AssertEquals("bill4.JS_ShippedOnBoardDate", today, bill4.JS_ShippedOnBoardDate);
				AssertEquals("bill1.JS_HouseBillIssueDate", today, bill1.JS_HouseBillIssueDate);
				AssertEquals("bill2.JS_HouseBillIssueDate", today.AddDays(-3), bill2.JS_HouseBillIssueDate);
				AssertEquals("bill3.JS_HouseBillIssueDate", today.AddDays(-2), bill3.JS_HouseBillIssueDate);
				AssertEquals("bill4.JS_HouseBillIssueDate", today, bill4.JS_HouseBillIssueDate);
			});

			providerDelegateMock.Invocations.Clear();
			providerMock.Invocations.Clear();

			providerDelegateMock
				.Setup(m => m.Invoke())
				.Returns(provider);

			origin.JA_A_DEP = today.AddDays(-1);

			CombineAssertions(delegate
			{
				AssertEquals("bill1.JS_ShippedOnBoardDate", today.AddDays(-3), bill1.JS_ShippedOnBoardDate);
				AssertEquals("bill2.JS_ShippedOnBoardDate", today.AddDays(-2), bill2.JS_ShippedOnBoardDate);
				AssertEquals("bill3.JS_ShippedOnBoardDate", today, bill3.JS_ShippedOnBoardDate);
				AssertEquals("bill4.JS_ShippedOnBoardDate", today, bill4.JS_ShippedOnBoardDate);
				AssertEquals("bill1.JS_HouseBillIssueDate", today, bill1.JS_HouseBillIssueDate);
				AssertEquals("bill2.JS_HouseBillIssueDate", today.AddDays(-3), bill2.JS_HouseBillIssueDate);
				AssertEquals("bill3.JS_HouseBillIssueDate", today.AddDays(-2), bill3.JS_HouseBillIssueDate);
				AssertEquals("bill4.JS_HouseBillIssueDate", today, bill4.JS_HouseBillIssueDate);
			});
		}

		[ShipmentDateUpdateConfiguration(Value = true)]
		public void TestATDChanged_UpdatesShipmentsFromAnotherVoyagesWithSameVesselVoyageCombination()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier4 = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("Visund", "123", carrier1.PK, "AUSYD", "NZAKL");
			var voyage2 = helper.CreateSeaVoyage("Visund", "123", carrier2.PK, "AUSYD", "NZAKL");
			var voyage3 = helper.CreateSeaVoyage("Visund", "123", carrier3.PK, "AUSYD", "NZAKL");
			var voyage4 = helper.CreateSeaVoyage("Visund", "123", carrier4.PK, "AUMEL", "NZAKL");
			var bill1 = Factory.New<BillOfLading>();
			bill1.JS_JX = voyage1.Sailings[0].PK;
			var bill2 = Factory.New<BillOfLading>();
			bill2.JS_JX = voyage2.Sailings[0].PK;
			var bill3 = Factory.New<BillOfLading>();
			bill3.JS_JX = voyage3.Sailings[0].PK;
			var bill4 = Factory.New<BillOfLading>();
			bill4.JS_JX = voyage4.Sailings[0].PK;
			// To temporary suppress update triggered by changed ATD
			AgencyRegistry.Instance.UpdateShipmentDatesFromSailing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var origin = voyage1.Origins[0];
			origin.JA_A_DEP = new ZDateTime(2013, 5, 1);
			AgencyRegistry.Instance.UpdateShipmentDatesFromSailing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var providerMock = new Mock<IScheduleUpdateQueryProvider>();
			var scheduleUpdateServicesMock = new Mock<IScheduleUpdateServices>();

			var provider = providerMock.Object;
			providerMock.
				SetupGet(p => p.ShouldUpdateAgencyShipmentDatesFromATD)
				.Returns(true);

			var scheduleUpdateServices = scheduleUpdateServicesMock.Object;
			scheduleUpdateServicesMock
				.SetupGet(m => m.QueryProvider)
				.Returns(provider);

			new AgencyScheduleUpdateSubscriber().ATDChanged(scheduleUpdateServices, origin, ZDateTime.Empty);
			AssertEquals(new ZDateTime(2013, 5, 1), bill2.JS_HouseBillIssueDate);
			AssertEquals(new ZDateTime(2013, 5, 1), bill3.JS_HouseBillIssueDate);
			AssertEquals("Matched by voyage-vessel-carrier, but does not have required origin port", true, bill4.JS_HouseBillIssueDate.IsEmpty);
		}

		[ShipmentDateUpdateConfiguration(Value = false)]
		public void TestATDChanged_Dissabled()
		{
			var providerMock = new Mock<IScheduleUpdateQueryProvider>();
			var providerDelegateMock = new Mock<GetValueDelegate<IScheduleUpdateQueryProvider>>();

			IScheduleUpdateQueryProvider provider = providerMock.Object;
			GetValueDelegate<IScheduleUpdateQueryProvider> providerDelegate = providerDelegateMock.Object;
			ZDateTime today = ZDateTime.Today;
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			BillOfLading bill1 = Factory.New<BillOfLading>();
			bill1.JS_UniqueConsignRef = "Bill1";
			bill1.JS_JX = sailing.PK;
			bill1.JS_ShippedOnBoardDate = today.AddDays(-3);
			bill1.JS_HouseBillIssueDate = ZDateTime.Empty;
			BillOfLading bill2 = Factory.New<BillOfLading>();
			bill2.JS_UniqueConsignRef = "Bill2";
			bill2.JS_JX = sailing.PK;
			bill2.JS_ShippedOnBoardDate = today.AddDays(-2);
			bill2.JS_HouseBillIssueDate = today.AddDays(-3);
			BillOfLading bill3 = Factory.New<BillOfLading>();
			bill3.JS_UniqueConsignRef = "Bill3";
			bill3.JS_JX = sailing.PK;
			bill3.JS_ShippedOnBoardDate = ZDateTime.Empty;
			bill3.JS_HouseBillIssueDate = today.AddDays(-2);
			BillOfLading bill4 = Factory.New<BillOfLading>();
			bill4.JS_UniqueConsignRef = "Bill4";
			bill4.JS_JX = sailing.PK;
			bill4.JS_ShippedOnBoardDate = ZDateTime.Empty;
			bill4.JS_HouseBillIssueDate = ZDateTime.Empty;
			ScheduleUpdateQueryProviderFactory.Set(Factory, providerDelegate);

			providerDelegateMock
				.Setup(m => m.Invoke())
				.Returns(provider);

			origin.JA_A_DEP = today;

			CombineAssertions(delegate
			{
				AssertEquals("bill1.JS_ShippedOnBoardDate", today.AddDays(-3), bill1.JS_ShippedOnBoardDate);
				AssertEquals("bill2.JS_ShippedOnBoardDate", today.AddDays(-2), bill2.JS_ShippedOnBoardDate);
				AssertEquals("bill3.JS_ShippedOnBoardDate", ZDateTime.Empty, bill3.JS_ShippedOnBoardDate);
				AssertEquals("bill4.JS_ShippedOnBoardDate", ZDateTime.Empty, bill4.JS_ShippedOnBoardDate);
				AssertEquals("bill1.JS_HouseBillIssueDate", ZDateTime.Empty, bill1.JS_HouseBillIssueDate);
				AssertEquals("bill2.JS_HouseBillIssueDate", today.AddDays(-3), bill2.JS_HouseBillIssueDate);
				AssertEquals("bill3.JS_HouseBillIssueDate", today.AddDays(-2), bill3.JS_HouseBillIssueDate);
				AssertEquals("bill4.JS_HouseBillIssueDate", ZDateTime.Empty, bill4.JS_HouseBillIssueDate);
			});
		}
	}
}
