using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class QuotedBookingContextMenuManagerTest : TestCaseWithFactory
	{
		#region Selecting Creditor Menu Item
		public void TestShowMenu_OnlyJobCarrierIsBlank_DoNotShowMenu()
		{
			var (jobChargesZGrid, menuManager) = CreateWithTestValues(
				jobCreditor: Factory.NewWithValidTestData<OrgHeader>().PK,
				jobCarrier: ZGuid.Empty,
				possibleCreditors: new (ZGuid Carrier, ZGuid Creditor)[] {
					(ZGuid.BrettsGuid, ZGuid.BrettsGuid)
				});

			AssertEquals(false, menuManager.ShouldShowSelectingCreditorsForChargesMenuItem);
			var selectCreditorMenuItem = jobChargesZGrid.ContextMenu.MenuItems.FindByText("Select Creditor from Potential Carriers");
			AssertEquals(false, selectCreditorMenuItem.Visible);

			jobChargesZGrid.Dispose();
		}

		public void TestShowMenu_OnlyJobCreditorIsBlank_DoNotShowMenu()
		{
			var (jobChargesZGrid, menuManager) = CreateWithTestValues(
				jobCreditor: ZGuid.Empty,
				jobCarrier: Factory.NewWithValidTestData<OrgHeader>().PK,
				possibleCreditors: new (ZGuid Carrier, ZGuid Creditor)[] {
					(ZGuid.BrettsGuid, ZGuid.BrettsGuid)
				});

			AssertEquals(false, menuManager.ShouldShowSelectingCreditorsForChargesMenuItem);
			var selectCreditorMenuItem = jobChargesZGrid.ContextMenu.MenuItems.FindByText("Select Creditor from Potential Carriers");
			AssertEquals(false, selectCreditorMenuItem.Visible);

			jobChargesZGrid.Dispose();
		}

		public void TestShowMenu_OnlyPossibleCarriersAreEmpty_DoNotShowMenu()
		{
			var (jobChargesZGrid, menuManager) = CreateWithTestValues(
				jobCreditor: Factory.NewWithValidTestData<OrgHeader>().PK,
				jobCarrier: Factory.NewWithValidTestData<OrgHeader>().PK,
				possibleCreditors: System.Array.Empty<(ZGuid Carrier, ZGuid Creditor)>());

			AssertEquals(false, menuManager.ShouldShowSelectingCreditorsForChargesMenuItem);
			var selectCreditorMenuItem = jobChargesZGrid.ContextMenu.MenuItems.FindByText("Select Creditor from Potential Carriers");
			AssertEquals(false, selectCreditorMenuItem.Visible);

			jobChargesZGrid.Dispose();
		}

		public void TestShowMenu_JobCarrierAndCreditorNotBlank_AndPossibleCarriersNotEmpty_DoNotShowMenu()
		{
			var (jobChargesZGrid, menuManager) = CreateWithTestValues(
				jobCreditor: Factory.NewWithValidTestData<OrgHeader>().PK,
				jobCarrier: Factory.NewWithValidTestData<OrgHeader>().PK,
				possibleCreditors: new (ZGuid Carrier, ZGuid Creditor)[] {
					(ZGuid.BrettsGuid, ZGuid.BrettsGuid)
				});

			AssertEquals(false, menuManager.ShouldShowSelectingCreditorsForChargesMenuItem);
			var selectCreditorMenuItem = jobChargesZGrid.ContextMenu.MenuItems.FindByText("Select Creditor from Potential Carriers");
			AssertEquals(false, selectCreditorMenuItem.Visible);

			jobChargesZGrid.Dispose();
		}

		public void TestShowMenu_JobCarrierAndCreditorAreBlank_AndPossibleCarriersNotEmpty_ShowMenu()
		{
			var (jobChargesZGrid, menuManager) = CreateWithTestValues(
				jobCreditor: ZGuid.Empty,
				jobCarrier: ZGuid.Empty,
				possibleCreditors: new (ZGuid Carrier, ZGuid Creditor)[] {
					(ZGuid.BrettsGuid, ZGuid.BrettsGuid)
				});

			AssertEquals(true, menuManager.ShouldShowSelectingCreditorsForChargesMenuItem);
			var selectCreditorMenuItem = jobChargesZGrid.ContextMenu.MenuItems.FindByText("Select Creditor from Potential Carriers");
			AssertEquals(true, selectCreditorMenuItem.Visible);

			jobChargesZGrid.Dispose();
		}

		[GuiTest]
		public void TestMenuClick_WhenPossibleCarriers_HasRowSelected_ShowSelectionForm()
		{
			var (jobChargesZGrid, menuManager) = CreateWithTestValues(
				jobCreditor: Factory.NewWithValidTestData<OrgHeader>().PK,
				jobCarrier: Factory.NewWithValidTestData<OrgHeader>().PK,
				possibleCreditors: new (ZGuid Carrier, ZGuid Creditor)[] {
					(ZGuid.BrettsGuid, ZGuid.BrettsGuid)
				});

			var selectCreditorMenuItem = jobChargesZGrid.ContextMenu.MenuItems.FindByText("Select Creditor from Potential Carriers");

			using var form = new ZForm();
			form.Controls.Add(jobChargesZGrid);

			// there's a selection
			menuManager.CurrentJobCharge = new Moq.Mock<ICharge>().Object;

			var formWasSeen = false;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (form is JobChargePossibleCarrierSelectionForm selectForm)
				{
					formWasSeen = true;
				}
			});

			UnitTestUserNotification.Instance.ClearMessages();
			selectCreditorMenuItem.PerformClick();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, formWasSeen);

			jobChargesZGrid.Dispose();
		}

		[GuiTest]
		public void TestMenuClick_WhenPossibleCarriers_NoSelectedRows_ShowErrorMessage()
		{
			var (jobChargesZGrid, menuManager) = CreateWithTestValues(
				jobCreditor: Factory.NewWithValidTestData<OrgHeader>().PK,
				jobCarrier: Factory.NewWithValidTestData<OrgHeader>().PK,
				possibleCreditors: new (ZGuid Carrier, ZGuid Creditor)[] {
					(ZGuid.BrettsGuid, ZGuid.BrettsGuid)
				});

			AssertNull("There's no selection", menuManager.CurrentJobCharge);

			var selectCreditorMenuItem = jobChargesZGrid.ContextMenu.MenuItems.FindByText("Select Creditor from Potential Carriers");
			var formWasSeen = false;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (form is JobChargePossibleCarrierSelectionForm selectForm)
				{
					formWasSeen = true;
				}
			});

			selectCreditorMenuItem.PerformClick();
			AssertEquals(false, formWasSeen);
			AssertEquals("Please select a row to make a selection.", UnitTestUserNotification.Instance.LastMessage.Text);

			jobChargesZGrid.Dispose();
		}

		(ZGrid Grid, QuotedBookingContextMenuManager Manager) CreateWithTestValues(
			ZGuid jobCreditor,
			ZGuid jobCarrier,
			(ZGuid Carrier, ZGuid Creditor)[] possibleCreditors)
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			quotedBooking.Creditor = jobCreditor;
			quotedBooking.OH_Carrier = jobCarrier;
			foreach (var tuple in possibleCreditors)
			{
				var possibleCarrier = quotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew();
				possibleCarrier.TTC_OH_Carrier = tuple.Carrier;
				possibleCarrier.TTC_OH_Creditor = tuple.Creditor;
			}

			// Add menu after the setup so that its default visibility is set.
			var jobChargesZGrid = new ZGrid();
			var menuManager = new QuotedBookingContextMenuManager();
			((IQuotedBookingContextMenuManager)menuManager).AddMenuToCharges(jobChargesZGrid, quotedBooking);

			return (jobChargesZGrid, menuManager);
		}

		#endregion
	}
}
