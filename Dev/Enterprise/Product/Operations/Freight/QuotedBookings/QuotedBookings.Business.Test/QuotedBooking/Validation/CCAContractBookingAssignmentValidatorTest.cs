using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	sealed class CCAContractBookingAssignmentValidatorTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			var result = validator.IsAllowedToAllocateToContract(quotedBooking, contract, out var notification);

			AssertEquals(result, true);
			AssertEquals(notification, null);
		}

		public void TestCheckContract_StartDate()
		{
			quotedBooking.ETD = new ZDateTime(2020, 3, 1);
			contract.RCT_StartDate = new ZDate(2022, 4, 2);

			var result = validator.IsAllowedToAllocateToContract(quotedBooking, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Contract Start Date is later than the Quoted Booking's ETD.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("Start Date (02-Apr-22) of Carrier Contract DORSIA is later than the ETD (01-Mar-20) of this Booking. Booking departure should be within the validity period of the selected Carrier Contract.", notification.Message);
		}

		public void TestCheckContract_EndDate()
		{
			quotedBooking.ETD = new ZDateTime(2020, 3, 1);
			contract.RCT_StartDate = new ZDate(2020, 1, 2);
			contract.RCT_EndDate = new ZDate(2020, 2, 12);

			var result = validator.IsAllowedToAllocateToContract(quotedBooking, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Contract Expiry Date is earlier than the Quoted Booking's ETD.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("Expiry Date (12-Feb-20) of Carrier Contract DORSIA is earlier than the ETD (01-Mar-20) of this Booking. Booking departure should be within the validity period of the selected Carrier Contract.", notification.Message);
		}

		public void TestCheckContainerType()
		{
			var refCont = Factory.New<RefContainer>();
			refCont.RC_ContainerType = "LAA";

			var container = Factory.New<IForwardingContainer>();
			container.JC_RC = refCont.PK;

			contract.RCT_ContainerType = "BOO";
			quotedBooking.QuotedBookingContainers.Add((CargoWise.EntityFramework.BusinessObject)container);

			var result = validator.IsAllowedToAllocateToContract(quotedBooking, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Ref Container Type and Contract Container Type are different.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("NOT all Containers on Booking PAULALLEN share the same Container Type (BOO) of the selected Carrier Contract DORSIA. Booking Container(s) and selected Carrier Contract should share the same Container Type.", notification.Message);
		}

		public void TestCheckAllowHazardousCommodities()
		{
			contract.RCT_AllowHazardousCommodities = false;

			var refCommodityCode = Factory.New<RefCommodityCode>();
			refCommodityCode.RH_IsHazardous = true;
			refCommodityCode.RH_Code = "NOX";

			var container = Factory.New<IForwardingContainer>();
			container.JC_RH_NKContainerCommodityCode = refCommodityCode.RH_Code;
			container.JC_ContainerNum = "VERYNICE";

			quotedBooking.QuotedBookingContainers.Add((CargoWise.EntityFramework.BusinessObject)container);

			var result = validator.IsAllowedToAllocateToContract(quotedBooking, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Booking is not hazardous but has hazardous container.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("Hazardous Commodities are not allowed for Carrier Contract DORSIA but Container VERYNICE has Commodity NOX with 'Is this Commodity Hazardous' checked. Only Booking(s) and Container(s) without Hazardous Commodities can be allocated.", notification.Message);
		}

		public void TestCheckBookingNamedAccounts()
		{
			var namedAccount = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount);

			var result = validator.IsAllowedToAllocateToContract(quotedBooking, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows warning when Quoted Booking doesn't have matching Named Account with Contract.", CargoWise.ComponentModel.NotificationType.Warning, notification.Type);
			AssertEquals("At least one of the Booking clients should match a Named Account of Carrier Contract DORSIA (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).", notification.Message);
		}

		public void TestCheckTransportMode()
		{
			quotedBooking.TransportMode = "AIR";

			var result = validator.IsAllowedToAllocateToContract(quotedBooking, contract, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Contract's Transport Mode does not match the Booking's.", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("Mode 'LSE' of this Booking is NOT under or aligned with the Transport mode 'SEA' of Carrier Contract for allocation.", notification.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			validator = new CCAContractBookingAssignmentValidator();

			contract = Factory.NewWithValidTestData<CarrierContractForUtilizationSimulation>();
			contract.RCT_ContractNumber = "DORSIA";

			quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.JS_UniqueConsignRef = "PAULALLEN";
			quotedBooking.TransportMode = "SEA";
		}

		CCAContractBookingAssignmentValidator validator;
		CarrierContractForUtilizationSimulation contract;
		QuotedBooking quotedBooking;
	}
}
