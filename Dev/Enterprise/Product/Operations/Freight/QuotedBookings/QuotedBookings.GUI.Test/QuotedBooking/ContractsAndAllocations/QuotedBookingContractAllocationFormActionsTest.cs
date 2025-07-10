using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	sealed class QuotedBookingContractAllocationFormActionsTest : TestCaseWithFactory
	{
		public void TestAllocateToContract()
		{
			var serviceProvider = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider.OH_Code = "MAELI_WW";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;
			contract.RCT_OH = serviceProvider.PK;

			var serviceProvider2 = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider2.OH_Code = "SHREK1ONVHS";

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.CarrierContractNumber = "LUMOS";
			quotedBooking.AllocationLinePK = ZGuid.NewZGuid();
			quotedBooking.OH_Carrier = serviceProvider2.PK;

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RCA_AllocationLine = quotedBooking.AllocationLinePK;

			var formActions = new QuotedBookingContractAllocationFormActions(quotedBooking) as IRatingContractSimulationFormActions;

			var didAllocate = formActions.TryAllocateToContract(null);
			Assert("Allocation should terminate if contract is null.", !didAllocate);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			didAllocate = formActions.TryAllocateToContract(contract);
			Assert("Allocation should terminate if Quoted Booking's Carrier is not empty and user selects 'No'.", !didAllocate);

			var expectedMessage = $"Service Provider MAELI_WW of Contract SHREK123 to be allocated is different from the Carrier SHREK1ONVHS on Booking with Quote.\r\n\r\nDo you want to override the Carrier on the Booking with Quote with MAELI_WW?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			didAllocate = formActions.TryAllocateToContract(contract);
			Assert("Allocation should proceed if Quoted Booking's Carrier is not empty and user selects 'Yes'.", didAllocate);
			AssertEquals("Quoted Booking should have been allocated to Contract.", contract.RCT_OH, quotedBooking.OH_Carrier);
			AssertEquals("Quoted Booking should have been allocated to Contract.", contract.RCT_ContractNumber, quotedBooking.CarrierContractNumber);
			AssertEquals("Quoted Booking should have had its allocation route cleared.", ZGuid.Empty, quotedBooking.AllocationLinePK);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			container.JC_RCA_AllocationLine = ZGuid.NewZGuid();
			formActions = new QuotedBookingContractAllocationFormActions(container);
			formActions.TryAllocateToContract(contract);
			AssertEquals("Container should have had its allocation route cleared.", ZGuid.Empty, container.JC_RCA_AllocationLine);
		}

		public void TestAllocateToAllocationRoute()
		{
			var serviceProvider = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider.OH_Code = "MAELI_WW";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "SHREK123";
			contract.RCT_ContractType = RatingContractTypes.Provider;
			contract.RCT_OH = serviceProvider.PK;

			var serviceProvider2 = Factory.NewWithValidTestData<OrgHeader>();
			serviceProvider2.OH_Code = "SHREK1ONVHS";

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.CarrierContractNumber = "LUMOS";
			quotedBooking.AllocationLinePK = ZGuid.NewZGuid();
			quotedBooking.OH_Carrier = serviceProvider2.PK;

			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_AllocationLineID = "0001";

			var formActions = new QuotedBookingContractAllocationFormActions(quotedBooking) as IRatingContractSimulationFormActions;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var didAllocate = formActions.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation should terminate if Quoted Booking's Carrier is not empty and user selects 'No'.", !didAllocate);

			var expectedMessage = $"Service Provider MAELI_WW of Contract SHREK123 and Allocation Route 0001 is different from the Carrier SHREK1ONVHS on Booking with Quote.\r\n\r\nDo you want to override the Carrier on the Booking with Quote with MAELI_WW?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			didAllocate = formActions.TryAllocateToAllocationRoute(allocationRoute);
			Assert("Allocation should proceed if Quoted Booking's Carrier is not empty and user selects 'Yes'.", didAllocate);
			AssertEquals("Quoted Booking should have been allocated to Contract.", contract.RCT_OH, quotedBooking.OH_Carrier);
			AssertEquals("Quoted Booking should have been allocated to Contract.", contract.RCT_ContractNumber, quotedBooking.CarrierContractNumber);
			AssertEquals("Quoted Booking should have been allocated to Route.", allocationRoute.PK, quotedBooking.AllocationLinePK);

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			formActions = new QuotedBookingContractAllocationFormActions(container);
			formActions.TryAllocateToAllocationRoute(allocationRoute);
			AssertEquals("Container shuold have been allocated to Route.", allocationRoute.PK, container.JC_RCA_AllocationLine);
		}
	}
}
