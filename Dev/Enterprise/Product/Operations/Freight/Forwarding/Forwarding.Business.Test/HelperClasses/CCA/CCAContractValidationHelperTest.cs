using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CCAContractValidationHelperTest : TestCaseWithFactory
	{
		#region Consol

		public void TestIsETDBeforeContractStart()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			contract.RCT_StartDate = new ZDate(2022, 4, 2);
			AssertEquals("Consol ETD is before Contract Start Date", true, CCAContractValidationHelper.IsETDBeforeContractStart(contract, consol));

			transport.JW_ETD = new ZDateTime(2022, 4, 3);
			AssertEquals("Consol ETD is after Contract Start Date", false, CCAContractValidationHelper.IsETDBeforeContractStart(contract, consol));
		}

		public void TestIsETDAfterContractExpiry()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			contract.RCT_StartDate = new ZDate(2020, 1, 2);
			contract.RCT_EndDate = new ZDate(2020, 2, 12);
			AssertEquals("Consol ETD is after Contract Expiry Date.", true, CCAContractValidationHelper.IsETDAfterContractExpiry(contract, consol));

			transport.JW_ETD = new ZDateTime(2020, 1, 4);
			AssertEquals("Consol ETD is before Contract Expiry Date.", false, CCAContractValidationHelper.IsETDAfterContractExpiry(contract, consol));
		}

		public void TestIsConsolInvalidForContractNamedAccounts()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "GINNY";

			var namedAccount = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount);
			contract.RCT_OH = carrier.PK;

			var shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			AssertEquals("Contract has Named Accounts not specified on Consol's Shipments", true, CCAContractValidationHelper.IsConsolInvalidForContractNamedAccounts(contract, consol));

			shipment.ConsigneePK = namedAccount.PK;
			AssertEquals("Contract has Named Accounts that are specified on Consol's Shipments", false, CCAContractValidationHelper.IsConsolInvalidForContractNamedAccounts(contract, consol));
		}

		public void TestAreAnyContainerTypesInvalidForContract()
		{
			var container = Factory.New<ForwardingContainer>();
			var refCont = Factory.New<RefContainer>();
			refCont.RC_ContainerType = "LAA";
			contract.RCT_ContainerType = "BOO";
			container.JC_RC = refCont.PK;
			consol.Containers.Add(container);

			CCAContractValidationHelper.AreAnyContainerTypesInvalidForContract(contract, consol);
			AssertEquals("Container Types should not match.", true, CCAContractValidationHelper.AreAnyContainerTypesInvalidForContract(contract, consol));

			refCont.RC_ContainerType = "BOO";
			AssertEquals("Container Types should match (BOO).", false, CCAContractValidationHelper.AreAnyContainerTypesInvalidForContract(contract, consol));
		}

		public void TestIsConsolInvalidForNonHazardousContract()
		{
			contract.RCT_AllowHazardousCommodities = false;
			consol.JK_IsHazardous = true;

			AssertEquals("Hazardous Consol should be invalid for Non Hazardous Contract.", true, CCAContractValidationHelper.IsConsolInvalidForNonHazardousContract(contract, consol));

			consol.JK_IsHazardous = false;
			AssertEquals("Non Hazardous Consol should be valid for Non Hazardous Contract.", false, CCAContractValidationHelper.IsConsolInvalidForNonHazardousContract(contract, consol));

			consol.JK_IsHazardous = true;
			contract.RCT_AllowHazardousCommodities = true;
			AssertEquals("Hazardous Consol should be valid for Hazardous Contract.", false, CCAContractValidationHelper.IsConsolInvalidForNonHazardousContract(contract, consol));
		}

		public void TestAreAnyContainerCommoditiesInvalidForNonHazardousContract()
		{
			contract.RCT_AllowHazardousCommodities = false;
			consol.JK_IsHazardous = false;

			var refCommodityCode = Factory.New<RefCommodityCode>();
			refCommodityCode.RH_IsHazardous = true;
			refCommodityCode.RH_Code = "NOX";

			var container = Factory.New<ForwardingContainer>();
			container.JC_RH_NKContainerCommodityCode = refCommodityCode.RH_Code;
			container.JC_ContainerNum = "IMPERIO";
			consol.Containers.Add(container);

			AssertEquals("Hazardous Container Commodity should be invalid on Non Hazardous Contract.", true, CCAContractValidationHelper.AreAnyContainerCommoditiesInvalidForNonHazardousContract(contract, consol));

			refCommodityCode.RH_IsHazardous = false;

			AssertEquals("Non Hazardous Container Commodity should be valid on Non Hazardous Contract.", false, CCAContractValidationHelper.AreAnyContainerCommoditiesInvalidForNonHazardousContract(contract, consol));
		}

		public void TestIsConsolValidForContractValidDateRanges_NoRouteSets()
		{
			contract.RCT_TransportMode = Constants.TransportModes.Sea;
			contract.RCT_StartDate = ZDate.Today;
			contract.RCT_EndDate = ZDate.Empty;

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var modeValidLeg = consol.Transports[0];
			modeValidLeg.JW_TransportMode = Constants.TransportModes.Sea;
			modeValidLeg.JW_ETD = contract.RCT_StartDate.AddDays(-5);

			var dateValidLeg = consol.Transports.AddNew();
			dateValidLeg.JW_TransportMode = Constants.TransportModes.Air;
			dateValidLeg.JW_ETD = contract.RCT_StartDate.AddDays(5);

			Assert("Precondition", consol.Transports.All(t => ((Transport)t).RouteSetNumber == 0));
			AssertEquals(
				"When no route sets, legs should be evaluated individually, and none match",
				false,
				CCAContractValidationHelper.IsConsolValidForContractValidDateRanges(contract, consol));

			var modeAndDateValidLeg = consol.Transports.AddNew();
			modeAndDateValidLeg.JW_TransportMode = Constants.TransportModes.Sea;
			modeAndDateValidLeg.JW_ETD = contract.RCT_StartDate.AddDays(5);

			Assert("Precondition", consol.Transports.All(t => ((Transport)t).RouteSetNumber == 0));
			AssertEquals(
				"When no route sets, legs should be evaluated individually, and one should match",
				true,
				CCAContractValidationHelper.IsConsolValidForContractValidDateRanges(contract, consol));
		}

		public void TestIsConsolValidForContractValidDateRanges_ShouldNotConsiderTime()
		{
			contract.RCT_TransportMode = Constants.TransportModes.Sea;
			contract.RCT_StartDate = new ZDate(2001, 9, 10);
			contract.RCT_EndDate = new ZDate(2001, 9, 12);

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var modeAndDateValidLeg = consol.Transports[0];
			modeAndDateValidLeg.JW_TransportMode = Constants.TransportModes.Sea;
			modeAndDateValidLeg.JW_ETD = new ZDateTime(2001, 9, 12);

			Assert("Should be valid since same ETD as end date", CCAContractValidationHelper.IsConsolValidForContractValidDateRanges(contract, consol));

			modeAndDateValidLeg.JW_ETD = new ZDateTime(2001, 9, 12, 0, 0, 1);
			Assert("Time should not be considered when validating date ranges", CCAContractValidationHelper.IsConsolValidForContractValidDateRanges(contract, consol));
		}

		public void TestIsConsolValidForContractValidDateRanges_WithRouteSets()
		{
			contract.RCT_TransportMode = Constants.TransportModes.Sea;
			contract.RCT_StartDate = ZDate.Today;
			contract.RCT_EndDate = ZDate.Empty;

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var modeValidLeg = consol.Transports[0];
			modeValidLeg.JW_TransportMode = Constants.TransportModes.Sea;
			modeValidLeg.JW_ETD = contract.RCT_StartDate.AddDays(-5);
			modeValidLeg.JW_RL_NKLoadPort = "AUSYD";
			modeValidLeg.JW_RL_NKDiscPort = "AUPER";

			AssertEquals("Precondition", 1, modeValidLeg.RouteSetNumber);
			AssertEquals(
				"Route set does not contain date valid leg",
				false,
				CCAContractValidationHelper.IsConsolValidForContractValidDateRanges(contract, consol));

			var dateValidLeg = consol.Transports.AddNew();
			dateValidLeg.JW_TransportMode = Constants.TransportModes.Rail;
			dateValidLeg.JW_ETD = contract.RCT_StartDate.AddDays(5);
			dateValidLeg.JW_RL_NKLoadPort = "AUPER";
			dateValidLeg.JW_RL_NKDiscPort = "AUMEL";

			Assert("Precondition", consol.Transports.All(t => ((Transport)t).RouteSetNumber == 1));
			AssertEquals(
				"Route set contains a date valid leg and a mode valid leg",
				true,
				CCAContractValidationHelper.IsConsolValidForContractValidDateRanges(contract, consol));
		}

		#endregion

		#region QuotedBooking

		public void TestIsQuotedBookingValidForContractMultipleNamedAccountClients_WithLinkedClient()
		{
			var namedAccount1 = Factory.NewWithValidTestData<OrgHeader>();
			var namedAccount2 = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount1);
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount2);

			var clientAddress = Factory.NewWithValidTestData<OrgAddress>();
			clientAddress.OA_OH = namedAccount1.PK;
			bookingMock.Setup(booking => booking.Consignor).Returns(namedAccount1);

			AssertEquals(
				"Can allocate to Contract with multiple Named Account Clients, one matching Client",
				true,
				CCAContractValidationHelper.IsBookingValidForContractNamedAccounts(contract, bookingMock.Object));
		}

		public void TestIsQuotedBookingValidForContractMultipleNamedAccountClients_WithLinkedControllingCustomer()
		{
			var namedAccount1 = Factory.NewWithValidTestData<OrgHeader>();
			var namedAccount2 = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount1);
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount2);

			bookingMock.Setup(booking => booking.ControllingCustomer).Returns(namedAccount1);

			AssertEquals(
				"Can allocate to Contract with multiple Named Account Clients, one matching Controlling Agent",
				true,
				CCAContractValidationHelper.IsBookingValidForContractNamedAccounts(contract, bookingMock.Object));
		}

		public void TestIsQuotedBookingValidForContractMultipleNamedAccountClients_WithLinkedConsignor()
		{
			var namedAccount1 = Factory.NewWithValidTestData<OrgHeader>();
			var namedAccount2 = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount1);
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount2);

			bookingMock.Setup(booking => booking.Consignor).Returns(namedAccount1);

			AssertEquals(
				"Can allocate to Contract with multiple Named Account Clients, one matching Consignor",
				true,
				CCAContractValidationHelper.IsBookingValidForContractNamedAccounts(contract, bookingMock.Object));
		}

		public void TestIsQuotedBookingValidForContractMultipleNamedAccountClients_WithLinkedConsignee()
		{
			var namedAccount1 = Factory.NewWithValidTestData<OrgHeader>();
			var namedAccount2 = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount1);
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount2);

			bookingMock.Setup(booking => booking.Consignee).Returns(namedAccount1);

			AssertEquals(
				"Can allocate to Contract with multiple Named Account Clients, one matching Consignee",
				true,
				CCAContractValidationHelper.IsBookingValidForContractNamedAccounts(contract, bookingMock.Object));
		}

		public void TestIsQuotedBookingValidForContractMultipleNamedAccountClients_WithNoLinkedAddresses()
		{
			var namedAccount1 = Factory.NewWithValidTestData<OrgHeader>();
			var namedAccount2 = Factory.NewWithValidTestData<OrgHeader>();
			var namedAccount3 = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount1);
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount2);
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount3);

			AssertEquals(
				"Cannot allocate to Contract with multiple Named Account Clients but none matching",
				false,
				CCAContractValidationHelper.IsBookingValidForContractNamedAccounts(contract, bookingMock.Object));
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "R2D2";
			allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_AllocationLineID = "STUPEFY";
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "EXPELLIARMUS";
			bookingMock = new Mock<IQuotedBooking>();
			var booking = Factory.NewWithValidTestData<ForwardingShipment>();
			booking.JS_UniqueConsignRef = "PAULALLEN";
			bookingMock.Setup(booking => booking.ForwardingShipment).Returns(booking);
			bookingMock.Setup(booking => booking.TransportMode).Returns("SEA");
		}

		RatingContract contract;
		IRatingContractAllocationLine allocationRoute;
		ForwardingConsol consol;
		Mock<IQuotedBooking> bookingMock;
	}
}
