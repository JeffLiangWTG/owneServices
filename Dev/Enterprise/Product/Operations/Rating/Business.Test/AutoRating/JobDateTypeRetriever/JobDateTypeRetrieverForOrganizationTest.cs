using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.AutoRating.Testing
{
	public class JobDateTypeRetrieverForOrganizationTest : TestCaseWithFactory
	{
		#region Organization

		#region Organization > Autorating Date Filtering

		public void TestGetRankedJobDateTypesFromRatingCriteria_Organization_AutoratingDateFiltering_Default()
			=> TestGetRankedJobDateTypesByRatingCriteria_Organization_AutoratingDateFiltering
			(
				autoratingDateFiltering: RatingDateFilterTypes.Codes.Default,
				expectedJobDateTypes: new[] { "------DEP" }
			);

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_AutoratingDateFiltering_Standard()
			=> TestGetRankedJobDateTypesByRatingCriteria_Organization_AutoratingDateFiltering
			(
				autoratingDateFiltering: RatingDateFilterTypes.Codes.Standard,
				expectedJobDateTypes: new[] { "------DEP" }
			);

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_AutoratingDateFiltering_Arrival()
			=> TestGetRankedJobDateTypesByRatingCriteria_Organization_AutoratingDateFiltering
			(
				autoratingDateFiltering: RatingDateFilterTypes.Codes.Arrival,
				expectedJobDateTypes: new[] { "------ARV" }
			);

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_AutoratingDateFiltering_Departure()
			=> TestGetRankedJobDateTypesByRatingCriteria_Organization_AutoratingDateFiltering
			(
				autoratingDateFiltering: RatingDateFilterTypes.Codes.Departure,
				expectedJobDateTypes: new[] { "------DEP" }
			);

		void TestGetRankedJobDateTypesByRatingCriteria_Organization_AutoratingDateFiltering(string autoratingDateFiltering, string[] expectedJobDateTypes)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = autoratingDateFiltering;

			var criteria = new TestRatingCriteria();
			criteria.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			criteria.FreightMode = FreightMode.AIR;
			criteria.JobDirection = Directions.Export;
			criteria.Carrier = carrier;

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes);
		}

		#endregion

		#region Organization > Autorating Date Filtering = Custom

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_JobType()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);
			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.GatewayConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Export, freightMode: FreightMode.SEA, containerMode: ContainerModes.FCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "FCN-EXP-SEA-CST-FCL--ARV" });
		}

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_JobType_ALL()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, "ALL", FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Export, freightMode: FreightMode.SEA, containerMode: ContainerModes.FCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "ALL-EXP-SEA-CST-FCL--ARV" });
		}

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_Direction()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);
			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Import, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Import, freightMode: FreightMode.SEA, containerMode: ContainerModes.FCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "FCN-IMP-SEA-CST-FCL--ARV" });
		}

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_Direction_ALL()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.All, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Import, freightMode: FreightMode.SEA, containerMode: ContainerModes.FCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "FCN-ALL-SEA-CST-FCL--ARV" });
		}

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_RateMode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.AIR, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);
			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Export, freightMode: FreightMode.SEA, containerMode: ContainerModes.FCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "FCN-EXP-SEA-CST-FCL--ARV" });
		}

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_RateMode_ALL()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.ALL, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Export, freightMode: FreightMode.SEA, containerMode: ContainerModes.FCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "FCN-EXP-ALL-CST-FCL--ARV" });
		}

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_Container()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);
			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.LCL, "", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Export, freightMode: FreightMode.SEA, containerMode: ContainerModes.LCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "FCN-EXP-SEA-CST-LCL--ARV" });
		}

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_Container_ALL()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.All, "", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Export, freightMode: FreightMode.SEA, containerMode: ContainerModes.LCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "FCN-EXP-SEA-CST-ALL--ARV" });
		}

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_Location()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "AUSYD", JobDateTypes.Codes.ArrivalDate);
			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "USLAX", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Export, freightMode: FreightMode.SEA, containerMode: ContainerModes.FCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "FCN-EXP-SEA-CST-FCL-USLAX-ARV" });
		}

		public void TestGetRankedJobDateTypesByRatingCriteria_Organization_Custom_Location_ALL()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "", JobDateTypes.Codes.ArrivalDate);
			TestHelper.CreateOrganizationRatingDateConfig(carrier, ChargeCodeGroupList.Codes.Freight, JobInvoicingConsumerTypes.ForwardingConsolCode, FreightShipmentDirection.Code.Export, RateMode.SEA, JobRateTypes.Codes.Cost, ContainerModes.FCL, "USLAX", JobDateTypes.Codes.ArrivalDate);

			var criteria = TestHelper.CreateRatingCriteria("AUSYD", "USLAX", consumerType: JobInvoicingConsumerTypes.ForwardingConsol, directions: Directions.Export, freightMode: FreightMode.SEA, containerMode: ContainerModes.FCL, carrier: carrier);

			AssertRankedJobDateTypes(ChargeCodeGroupList.Codes.Freight, criteria, expectedJobDateTypes: new[] { "FCN-EXP-SEA-CST-FCL-USLAX-ARV", "FCN-EXP-SEA-CST-FCL--ARV" });
		}

		#endregion

		#endregion

		#region Implementation

		TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		void AssertRankedJobDateTypes(string chargeGroup, RatingCriteria criteria, string[] expectedJobDateTypes)
		{
			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				var ratingHeaderMock = new Mock<IRatingHeader>();
				ratingHeaderMock.SetupGet(x => x.TH_RateType).Returns(RatingConstants.RatingHeaderTypes.Costing);
				ratingHeaderMock.SetupGet(x => x.Header).Returns(criteria.Carrier);

				var rateEntryMock = new Mock<IRateEntry>();
				rateEntryMock.SetupGet(x => x.ParentRatingHeader).Returns(ratingHeaderMock.Object);

				var rateLineMock = new Mock<IRateLine>();
				rateLineMock.SetupGet(x => x.ParentRateEntry).Returns(rateEntryMock.Object);

				var rankedJobDateTypes = criteria.GetRankedDateTypes_ExposedForTest(rateLineMock.Object, chargeGroup, isCosting: true);

				AssertContainsExactElementsInAnyOrder
				(
					$"RatingCriteria: {criteria.ConsumerType}-{criteria.FreightMode}-{criteria.ContainerMode}-{criteria.JobDirection}",
					expected: expectedJobDateTypes,
					actual: rankedJobDateTypes.Select(autoRateDate => $"{autoRateDate.JobType}-{autoRateDate.DirectionCode}-{autoRateDate.Mode}-{autoRateDate.RateType}-{autoRateDate.ContainerMode}-{autoRateDate.Location}-{autoRateDate.DateType}")
				);
			}
		}

		#endregion
	}
}
