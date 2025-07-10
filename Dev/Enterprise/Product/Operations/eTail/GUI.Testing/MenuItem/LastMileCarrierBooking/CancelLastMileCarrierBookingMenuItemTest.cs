using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.eTail.GUI.Testing
{
	public class CancelLastMileCarrierBookingMenuItemTest : BaseLastMileCarrierBookingMenuItemTest
	{
		public void TestVisiblity()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			Factory.Save();

			var menuItem = new CancelLastMileCarrierBookingMenuItem(() => consignment);
			menuItem.UpdateVisibilityAndCaption();

			AssertEquals("Cancel LMC Caption: Hidden", false, menuItem.Visible);

			item1.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);
			Factory.Save();
			menuItem.UpdateVisibilityAndCaption();

			AssertEquals("Cancel LMC Caption: Shown", true, menuItem.Visible);

			item2.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);
			Factory.Save();
			menuItem.UpdateVisibilityAndCaption();

			AssertEquals("Cancel LMC Caption: Shown", true, menuItem.Visible);
		}

		public void TestMenuItemAction_WhenCarrierBookingAgentIsNotConfigured_ThenDisplayWarning()
		{
			var lastMileCarrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OH_LastMileCarrier = lastMileCarrierOrg.PK;
			consignment.HVC_OH_LastMileCarrierBookingAgent = ZGuid.Empty;
			consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
			var item = consignment.Items.AddNew();
			Factory.Save();

			var cancelLastMileCarrierBookingMenuItem = new CancelLastMileCarrierBookingMenuItem(() => consignment);
			cancelLastMileCarrierBookingMenuItem.PerformClick();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			CombineAssertions(() =>
			{
				AssertContains("An error message should appear", "Carrier Booking Agent, Last Mile Carrier or Destination Depot have not been configured.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("The Cancel LMC Booking item selection form should not be shown", ZFormModaliser.LastFormShownDialogForTest);
			});
		}

		public void TestMenuItemAction_WhenLastMileCarrierIsNotConfigured_ThenDisplayWarning()
		{
			var bookingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OH_LastMileCarrier = ZGuid.Empty;
			consignment.HVC_OH_LastMileCarrierBookingAgent = bookingAgentOrg.PK;
			var item = consignment.Items.AddNew();
			Factory.Save();

			var cancelLastMileCarrierBookingMenuItem = new CancelLastMileCarrierBookingMenuItem(() => consignment);
			cancelLastMileCarrierBookingMenuItem.PerformClick();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			CombineAssertions(() =>
			{
				AssertContains("An error message should appear", "Carrier Booking Agent, Last Mile Carrier or Destination Depot have not been configured.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("The Cancel LMC Booking item selection form should not be shown", ZFormModaliser.LastFormShownDialogForTest);
			});
		}

		public void TestMenuItemAction_WhenConsignmentLMCAndBookingAgentArePopulated_OpensCancelLMCBookingItemSelectionForm()
		{
			var bookingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var lastMileCarrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OH_LastMileCarrierBookingAgent = bookingAgentOrg.PK;
			consignment.HVC_OH_LastMileCarrier = lastMileCarrierOrg.PK;
			consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
			consignment.Items.AddNew();
			Factory.Save();

			var cancelLastMileCarrierBookingMenuItem = new CancelLastMileCarrierBookingMenuItem(() => consignment);
			cancelLastMileCarrierBookingMenuItem.PerformClick();

			AssertEquals("The Cancel LMC Booking item selection form should be shown", typeof(HVLVCancelLastMileCarrierSelectItemForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestMenuItemAction_ExceptionOccured_ErrorMessageShown()
		{
			var bookingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var lastMileCarrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OH_LastMileCarrierBookingAgent = bookingAgentOrg.PK;
			consignment.HVC_OH_LastMileCarrier = lastMileCarrierOrg.PK;
			consignment.HVC_OA_DestinationDepot = destinationDepot.PK;

			var itemCausingException = consignment.Items.AddNew();
			itemCausingException.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.CancelBooking(itemCausingException.PK.ToGuid()))
				.Throws(new InvalidOperationException("Boom!"));

			AssertCancelLMCBookingResult(mockProvider.Object, itemCausingException, errorExpected: true, "Boom!");
		}

		public void TestMenuItemAction_BookingFailed_ErrorMessageShown()
		{
			var bookingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var lastMileCarrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OH_LastMileCarrierBookingAgent = bookingAgentOrg.PK;
			consignment.HVC_OH_LastMileCarrier = lastMileCarrierOrg.PK;
			consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
			var unbookableItem = consignment.Items.AddNew();
			unbookableItem.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);

			var responseCollection = CreateMockLMCBookingResponseCollection(new[]
			{
				CreateMockLMCBookingResponse(successful: false, "I'm hungry2")
			});

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.CancelBooking(unbookableItem.PK.ToGuid()))
				.Returns(responseCollection);

			AssertCancelLMCBookingResult(mockProvider.Object, unbookableItem, errorExpected: true, "I'm hungry2");
		}

		public void TestMenuItemAction_BookingSuccessful()
		{
			var bookingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var lastMileCarrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OH_LastMileCarrierBookingAgent = bookingAgentOrg.PK;
			consignment.HVC_OH_LastMileCarrier = lastMileCarrierOrg.PK;
			consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
			var item = consignment.Items.AddNew();
			item.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);

			var responseCollection = CreateMockLMCBookingResponseCollection(new[]
			{
				CreateMockLMCBookingResponse(successful: true, string.Empty)
			});

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.CancelBooking(item.PK.ToGuid()))
				.Returns(responseCollection);

			AssertCancelLMCBookingResult(mockProvider.Object, item, errorExpected: false);
		}

		public void TestMenuItemAction_BookingSuccessful_DeleteLabel()
		{
			var bookingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var lastMileCarrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OH_LastMileCarrierBookingAgent = bookingAgentOrg.PK;
			consignment.HVC_OH_LastMileCarrier = lastMileCarrierOrg.PK;
			consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
			var item = consignment.Items.AddNew();
			item.HVI_ItemId = "testItemID001";

			item.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);
			var labelFile = consignment.DocManagerInfo().AddFileOrDocument(ZBlob.FromAscii("Shawn"), "testItemID001.pdf", "PDF");

			var responseCollection = CreateMockLMCBookingResponseCollection(new[]
			{
				CreateMockLMCBookingResponse(successful: true, string.Empty)
			});

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.CancelBooking(item.PK.ToGuid()))
				.Returns(responseCollection);

			PerformLMCBookingAction(mockProvider.Object, new CancelLastMileCarrierBookingMenuItem(() => item.Consignment));
			Assert("Label was deleted", labelFile.IsDeleted);
		}

		void AssertCancelLMCBookingResult(ILastMileCarrierBookingService testBookingProvider, HVLVItem item, bool errorExpected, string errorMessageExpected = null)
		{
			PerformLMCBookingAction(testBookingProvider, new CancelLastMileCarrierBookingMenuItem(() => item.Consignment));

			if (errorExpected)
			{
				AssertEquals("Last shown message:", errorMessageExpected, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Last message should be an Error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
			else
			{
				AssertEquals("Last shown message:", "Last Mile Carrier Booking(s) have been canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Last message should be Information", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		ILastMileCarrierBookingResponse CreateMockLMCBookingResponse(bool successful, string error)
		{
			var response = new Mock<ILastMileCarrierBookingResponse>();
			response.SetupGet(mock => mock.ErrorMessage).Returns(error);
			response.SetupGet(mock => mock.Successful).Returns(successful);
			return response.Object;
		}
	}
}
