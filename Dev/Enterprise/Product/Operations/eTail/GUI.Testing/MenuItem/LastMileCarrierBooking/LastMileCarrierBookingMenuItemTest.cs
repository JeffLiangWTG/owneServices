using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.eTail.GUI.Testing
{
	public class LastMileCarrierBookingMenuItemTest : BaseLastMileCarrierBookingMenuItemTest
	{
		public void TestVisibility()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			Factory.Save();

			var menuItem = new LastMileCarrierBookingMenuItem(() => consignment);
			menuItem.UpdateVisibilityAndCaption();

			AssertEquals("LMC Booking Caption: Shown", true, menuItem.Visible);

			item1.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);
			Factory.Save();
			menuItem.UpdateVisibilityAndCaption();

			AssertEquals("LMC Booking Caption: Shown", true, menuItem.Visible);

			item2.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);
			Factory.Save();
			menuItem.UpdateVisibilityAndCaption();

			AssertEquals("LMC Booking Caption: Hidden", false, menuItem.Visible);
		}

		public void TestMenuItemAction_ShowInformation_WhenExceptionTypeIsConfigurationErrorsException()
		{
			var lastMileCarrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			AssertNull("Precondition : last mile carrier booking agent doesn't config RTUS so it will throw ConfigurationErrorsException", ObjectFactory.Get<ITransportRegistry>().GetRTUSOption(lastMileCarrierBookingAgent.PK.ToGuid()));

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_OH_LastMileCarrierBookingAgent = lastMileCarrierBookingAgent.PK;

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;

			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			consignment.HVC_OA_DestinationDepot = destinationDepot.PK;

			consignment.Items.AddNew();
			Factory.Save();

			using (var form = new HVLVConsignmentForm(consignment))
			{
				form.Show();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((selectItemForm) =>
				{
					var consignmentTreeView = ((HVLVBookLastMileCarrierSelectItemForm)selectItemForm).Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;

					consignmentTreeView.Nodes[0].Nodes[0].Checked = true;
				});

				var menuItemLMCBooking = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.OfType<LastMileCarrierBookingMenuItem>().Single();

				menuItemLMCBooking.PerformClick();

				Assert("Expected Error when RTUS isn't configured", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Expected the following error message", "RTUS is required for making Carrier Booking. Please raise an eRequest (CR9) for RTUS configuration", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemAction_WhenCarrierBookingAgentCarrierAndDestinationDepotHaveNotConfigured_ThenDisplayWarning()
		{
			var lastMileCarrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OH_LastMileCarrier = ZGuid.Empty;
			consignment.HVC_OH_LastMileCarrierBookingAgent = ZGuid.Empty;
			consignment.HVC_OA_DestinationDepot = ZGuid.Empty;

			var item = consignment.Items.AddNew();
			Factory.Save();

			var lastMileCarrierBookingMenuItem = new LastMileCarrierBookingMenuItem(() => consignment);
			lastMileCarrierBookingMenuItem.PerformClick();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			CombineAssertions(() =>
			{
				AssertContains("An error message should appear", "Carrier Booking Agent, Last Mile Carrier or Destination Depot have not been configured.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("The Book LMC item selection form should not be shown", ZFormModaliser.LastFormShownDialogForTest);
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

			var lastMileCarrierBookingMenuItem = new LastMileCarrierBookingMenuItem(() => consignment);
			lastMileCarrierBookingMenuItem.PerformClick();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			CombineAssertions(() =>
			{
				AssertContains("An error message should appear", "Carrier Booking Agent, Last Mile Carrier or Destination Depot have not been configured.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("The Book LMC item selection form should not be shown", ZFormModaliser.LastFormShownDialogForTest);
			});
		}

		public void TestMenuItemAction_WhenConsignmentLMCABookingAgentAndDestinationDepotArePopulated_OpensLMCBookingItemSelectionForm()
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

			var lastMileCarrierBookingMenuItem = new LastMileCarrierBookingMenuItem(() => consignment);
			lastMileCarrierBookingMenuItem.PerformClick();

			AssertEquals("The Book LMC item selection form should be shown", typeof(HVLVBookLastMileCarrierSelectItemForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
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

			var itemCausingException = Factory.New<HVLVItem>();
			itemCausingException.HVI_HVC_Consignment = consignment.PK;

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(itemCausingException.PK.ToGuid(), true))
				.Throws(new InvalidOperationException("Boom!"));

			AssertLMCBookingResult(mockProvider.Object, itemCausingException, errorExpected: true, "Boom!");
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
			var unbookableItem = Factory.New<HVLVItem>();
			unbookableItem.HVI_HVC_Consignment = consignment.PK;

			var responseCollection = CreateMockLMCBookingResponseCollection(new[]
			{
				CreateMockLMCBookingResponse(successful: false, "I'm hungry2", "PDF", null)
			});

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(unbookableItem.PK.ToGuid(), true))
				.Returns(responseCollection);

			AssertLMCBookingResult(mockProvider.Object, unbookableItem, errorExpected: true, "I'm hungry2");
		}

		public void TestMenuItemAction_BookingSuccessful_ShowDocDeliveryForm()
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

			var testData = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm a label and I'm sticky"));
			var responseCollection = CreateMockLMCBookingResponseCollection(new[]
			{
				CreateMockLMCBookingResponse(successful: true, error: string.Empty, "PDF", testData)
			});

			var mockProvider = new Mock<ILastMileCarrierBookingService>();
			mockProvider.Setup(m => m.BookLastMileCarrier(item.PK.ToGuid(), true))
				.Returns(responseCollection);

			AssertLMCBookingResult(mockProvider.Object, item, errorExpected: false);
			AssertEquals("DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().Name);
		}

		void AssertLMCBookingResult(ILastMileCarrierBookingService testBookingProvider, HVLVItem item, bool errorExpected, string errorMessageExpected = null)
		{
			PerformLMCBookingAction(testBookingProvider, new LastMileCarrierBookingMenuItem(() => item.Consignment));

			if (errorExpected)
			{
				AssertEquals("Last shown message:", errorMessageExpected, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Last message should be an Error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		ILastMileCarrierBookingResponse CreateMockLMCBookingResponse(bool successful, string error, string fileType, ReadOnlyCollection<byte> binaryData)
		{
			var response = new Mock<ILastMileCarrierBookingResponse>();
			response.SetupGet(mock => mock.FileType).Returns(fileType);
			response.SetupGet(mock => mock.ErrorMessage).Returns(error);
			response.SetupGet(mock => mock.Successful).Returns(successful);
			response.SetupGet(mock => mock.BinaryData).Returns(binaryData);
			return response.Object;
		}
	}
}
