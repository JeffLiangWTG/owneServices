using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		public void TestGetNewAsycudaItemSelectionDialogCore()
		{
			var header = CreateNewManifest();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ASYCUDA.Business.ISelectionItem>();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);

			void AssertSelectionDialogType<T>(string subMessageType)
			{
				var messageChooser = header.GetNewMessageChooser(items, subMessageType, false);
				using (var dialog = provider.GetNewAsycudaItemSelectionDialog(messageChooser, string.Empty, subMessageType))
				{
					AssertType<T>($"Should be {typeof(T).Name} when the sub message type is {subMessageType}.", dialog);
				}
			}

			AssertSelectionDialogType<AIMBillsSelectionDialog>(AIMMessageSubTypes.FRI);
			AssertSelectionDialogType<AIMBillsSelectionDialog>(AIMMessageSubTypes.FXI);
			AssertSelectionDialogType<AIMBillsSelectionDialog>(AIMMessageSubTypes.FRC);
			AssertSelectionDialogType<AIMBillsSelectionDialog>(AIMMessageSubTypes.FXC);
			AssertSelectionDialogType<AIMBillsSelectionDialog>(AIMMessageSubTypes.FRX);
			AssertSelectionDialogType<AIMBillsSelectionDialog>(AIMMessageSubTypes.FXX);

			AssertSelectionDialogType<StatusQueryBillSelectionDialog>(AIMMessageSubTypes.FSQ);

			AssertSelectionDialogType<ASYCUDA.GUI.AsycudaItemSelectionDialog>(string.Empty);
			AssertSelectionDialogType<ASYCUDA.GUI.AsycudaItemSelectionDialog>("Invalid Sub MessageType");
		}

		public void TestResetMessageStatusBillsGridMenuItem()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			manifest.AMA_RL_NKPortOfLoading = "AUSYD";
			manifest.AMA_RL_NKPortOfDischarge = "USLAX";
			var bill1 = manifest.Bills.AddNew();
			bill1.ABL_BillNumber = "HB1";
			bill1.ABL_MessageStatus = string.Empty;
			var bill2 = manifest.Bills.AddNew();
			bill2.ABL_BillNumber = "HB2";
			bill2.ABL_MessageStatus = EDIMessage.Status.Sent;
			Factory.Save();

			using (var form = new ASYCUDA.GUI.ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				asycudaManifestUserControl.SelectAndShowBill(bill1.PK);
				billsGrid.SelectSingleElementByPK(bill1.PK);

				var resetMessageStatusMenuItem = billsGrid.ContextMenu.MenuItems.FindByText("Reset Message Status");
				billsGrid.ContextMenu.DoPopup();
				AssertEquals(true, resetMessageStatusMenuItem.Visible);
				AssertEquals("Menu is disabled when the selected bills message status is not SNT", false, resetMessageStatusMenuItem.Enabled);

				bill1.ABL_MessageStatus = EDIMessage.Status.Sent;
				billsGrid.ContextMenu.DoPopup();
				AssertEquals(true, resetMessageStatusMenuItem.Visible);
				AssertEquals("Menu is enabled when the selected bills message status is SNT", true, resetMessageStatusMenuItem.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				resetMessageStatusMenuItem.PerformClick();

				AssertEquals("There are changes in this form. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Neither Bill is reset", EDIMessage.Status.Sent, bill1.ABL_MessageStatus);
				AssertEquals("Neither Bill is reset", EDIMessage.Status.Sent, bill2.ABL_MessageStatus);
				AssertEquals("update is not saved since there are outstanding edits and we said No", true, bill1.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				resetMessageStatusMenuItem.PerformClick();

				AssertEquals("There are changes in this form. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Just the selected Bill is reset", string.Empty, bill1.ABL_MessageStatus);
				AssertEquals("Just the selected Bill is reset", EDIMessage.Status.Sent, bill2.ABL_MessageStatus);
				AssertEquals("update is saved since we said Yes", false, bill1.HasChanges);

				bill1.ABL_MessageStatus = EDIMessage.Status.Sent;
				bill2.ABL_MessageStatus = EDIMessage.Status.Sent;
				Factory.Save();
				billsGrid.SelectAllElements();
				billsGrid.ContextMenu.DoPopup();
				AssertEquals(true, resetMessageStatusMenuItem.Visible);
				AssertEquals("Menu is enabled when any of the selected bills message status is SNT", true, resetMessageStatusMenuItem.Enabled);

				resetMessageStatusMenuItem.PerformClick();
				AssertEquals("Both Bills are reset when both are selected", string.Empty, bill1.ABL_MessageStatus);
				AssertEquals("Both Bills are reset when both are selected", string.Empty, bill2.ABL_MessageStatus);
				AssertEquals("update is saved since no other outstanding edits", false, bill1.HasChanges || bill2.HasChanges);

				bill1.ABL_MessageStatus = EDIMessage.Status.Cancelled;
				bill2.ABL_MessageStatus = EDIMessage.Status.Sent;
				Factory.Save();
				billsGrid.SelectAllElements();
				AssertEquals(bill1, billsGrid.GetCurrent());

				billsGrid.ContextMenu.DoPopup();
				AssertEquals(true, resetMessageStatusMenuItem.Visible);
				AssertEquals("Menu is enabled when any of the selected bills message status is SNT", true, resetMessageStatusMenuItem.Enabled);

				resetMessageStatusMenuItem.PerformClick();
				AssertEquals("Only the Bill with message status SNT is reset when both are selected", EDIMessage.Status.Cancelled, bill1.ABL_MessageStatus);
				AssertEquals("Only the Bill with message status SNT is reset when both are selected", string.Empty, bill2.ABL_MessageStatus);

				bill1.ABL_MessageStatus = EDIMessage.Status.Error;
				bill2.ABL_MessageStatus = EDIMessage.Status.Error;
				Factory.Save();
				billsGrid.SelectAllElements();
				billsGrid.ContextMenu.DoPopup();
				AssertEquals(true, resetMessageStatusMenuItem.Visible);
				AssertEquals("Menu is enabled when any of the selected bills message status is ERR", true, resetMessageStatusMenuItem.Enabled);
				resetMessageStatusMenuItem.PerformClick();
				AssertEquals("Both Bill with message status ERR is reset.", string.Empty, bill1.ABL_MessageStatus);
				AssertEquals("Both Bill with message status ERR is reset.", string.Empty, bill2.ABL_MessageStatus);
			}
		}

		public void TestArrivalTab()
		{
			var manifest = CreateNewManifest();

			using (var form = new ASYCUDA.GUI.ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var arrivalsTab = asycudaManifestUserControl.FindSingle<ZTabPage>("arrivalsTabPage");
				Assert("arrivalTab TabVisible", arrivalsTab.TabVisible);
				mainTabControl.SelectedTab = arrivalsTab;

				var arrivalsGroupbox = arrivalsTab.FindSingle<ZGroupBox>("groupboxForArrivals");
				AssertEquals("Arrivals", arrivalsGroupbox.Text);

				var arrivalHeadersGrid = arrivalsGroupbox.FindSingle<ZGrid>("arrivalHeadersGrid");
				var flightNoColumn = arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_VoyageFlightNo);
				AssertEquals(System.Windows.Forms.CharacterCasing.Upper, flightNoColumn.CharacterCasing);
				var referenceColumn = arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_Reference) as ZTextBoxColumnStyleInfo;
				AssertEquals(System.Windows.Forms.CharacterCasing.Upper, referenceColumn.CharacterCasing);
				var etaAtDischargePortColumn = arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_ETAAtDischargePort);
				AssertEquals("EtaAtDischargePort column is unavailable", true, etaAtDischargePortColumn.IsUnavailable);
				var etaColumn = arrivalHeadersGrid.GetColumnStyle("ETAAtDischargePortForShortFormat") as ZDateEditColumnStyleInfo;
				AssertEquals(ZDateTimePickerFormat.Short, etaColumn.DateTimeFormat);
			}
		}

		public void TestArrivalDetailsTab()
		{
			var manifest = CreateNewManifest();

			using (var form = new ASYCUDA.GUI.ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var arrivalsTab = asycudaManifestUserControl.FindSingle<ZTabPage>("arrivalsTabPage");
				mainTabControl.SelectedTab = arrivalsTab;

				var arrivalDetailsTabControl = arrivalsTab.FindSingle<ZTabControl>("arrivalDetailsTabControl");
				var arrivalDetailsTab = arrivalDetailsTabControl.FindSingle<ZTabPage>("arrivalDetailsTabPage");
				Assert("arrivalDetailsTab TabVisible", arrivalDetailsTab.TabVisible);
				arrivalDetailsTabControl.SelectedTab = arrivalDetailsTab;

				var arrivalDetailsGrid = arrivalDetailsTab.FindSingle<ZGrid>("arrivalDetailsGrid");
				var lineBillNoColumn = arrivalDetailsGrid.GetColumnStyle("ATL_BillNumber");
				Assert(lineBillNoColumn.IsVisible);
				var lineQuantityColumn = arrivalDetailsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaArrivalLine.Schema.ATL_Quantity);
				Assert(lineQuantityColumn.IsVisible);
				var lineCargoStatusColumn = arrivalDetailsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaArrivalLine.Schema.ATL_CargoStatus);
				Assert(lineCargoStatusColumn.IsVisible);
				var lineReferenceColumn = arrivalDetailsGrid.GetColumnStyle(ASYCUDA.Business.AsycudaArrivalLine.Schema.ATL_Reference);
				Assert(lineReferenceColumn.IsVisible);
			}
		}

		public void TestArrivalHeadersGridColumnAvailability()
		{
			var manifest = CreateNewManifest();

			using (var form = new ASYCUDA.GUI.ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var arrivalsTab = asycudaManifestUserControl.FindSingle<ZTabPage>("arrivalsTabPage");
				mainTabControl.SelectedTab = arrivalsTab;

				var arrivalHeadersGrid = arrivalsTab.FindSingle<ZGrid>("arrivalHeadersGrid");
				AssertEquals("ATH_VoyageFlightNo IsVisible", true, arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_VoyageFlightNo).IsVisible);
				AssertEquals("ETAAtDischargePortForShortFormat IsVisible", true, arrivalHeadersGrid.GetColumnStyle("ETAAtDischargePortForShortFormat").IsVisible);
				AssertEquals("ATH_Reference IsVisible", true, arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_Reference).IsVisible);
				AssertEquals("ATH_ETAAtDischargePort IsUnavailable", true, arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_ETAAtDischargePort).IsUnavailable);
				AssertEquals("ATH_ArrivalSequence IsUnavailable", true, arrivalHeadersGrid.GetColumnStyle(AsycudaArrivalHeader.Schema.ATH_ArrivalSequence).IsUnavailable);
			}
		}

		public void TestCustomizeBillsGrid()
		{
			var manifest = CreateNewManifest();
			var bill1 = manifest.Bills.AddNew();

			using (var form = new ASYCUDA.GUI.ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				var columnInfo = billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKOrigin);
				AssertEquals("Origin Caption", "Flight Origin", columnInfo.CaptionResourceString.Caption);
				AssertEquals("Origin Width", 80, columnInfo.Width);
			}
		}

		public void TestBillsGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			var bill1 = manifest.Bills.AddNew();

			using (var form = new ASYCUDA.GUI.ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions(() =>
				{
					AssertEquals("ABL_SequenceNumber IsVisible", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SequenceNumber).IsVisible);
					AssertEquals("ABL_BillNumber IsVisible", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BillNumber).IsVisible);
					AssertEquals("ABL_RL_NKOrigin IsVisible", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKOrigin).IsVisible);
					AssertEquals("ABL_RL_NKFinalDestination IsVisible", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKFinalDestination).IsVisible);
					AssertEquals("ABL_GoodsDescription IsVisible", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GoodsDescription).IsVisible);
					AssertEquals("ABL_ManifestQty IsVisible", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ManifestQty).IsVisible);

					AssertEquals("CustomsJobNumber IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.CustomsJobNumber).IsUnavailable);
					AssertEquals("ABL_CustomsValue IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CustomsValue).IsUnavailable);
					AssertEquals("ABL_RX_NKCustomsValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency).IsUnavailable);
					AssertEquals("DiscountValue IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.DiscountValue).IsUnavailable);
					AssertEquals("DiscountValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.DiscountValueCurrency).IsUnavailable);
					AssertEquals("ABL_FreightValue IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_FreightValue).IsUnavailable);
					AssertEquals("ABL_RX_NKFreightValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency).IsUnavailable);
					AssertEquals("ABL_TransportValue IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_TransportValue).IsUnavailable);
					AssertEquals("ABL_RX_NKTransportValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency).IsUnavailable);
					AssertEquals("ABL_InsuranceValue IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_InsuranceValue).IsUnavailable);
					AssertEquals("ABL_RX_NKInsuranceValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency).IsUnavailable);
					AssertEquals("OtherChargesValue IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.OtherChargesValue).IsUnavailable);
					AssertEquals("OtherChargesValueCurrency IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.OtherChargesValueCurrency).IsUnavailable);
					AssertEquals("ABL_PrepaidCollect IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_PrepaidCollect).IsUnavailable);
					AssertEquals("ABL_UCRNumber IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_UCRNumber).IsUnavailable);
					AssertEquals("ABL_Volume IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Volume).IsUnavailable);
					AssertEquals("ABL_VolumeUQ IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_VolumeUQ).IsUnavailable);
					AssertEquals("ABL_MarksAndNumbers IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_MarksAndNumbers).IsUnavailable);
					AssertEquals("ABL_Remarks IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Remarks).IsUnavailable);
					AssertEquals("ABL_CargoStatus IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CargoStatus).IsUnavailable);
					AssertEquals("ABL_CarrierReference IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CarrierReference).IsUnavailable);
					AssertEquals("ABL_ManifestUQ IsUnavailable", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ManifestUQ).IsUnavailable);
				});
			}
		}

		public void TestBillsGridExtraColumns()
		{
			var manifest = CreateNewManifest();
			var bill1 = manifest.Bills.AddNew();

			using (var form = new ASYCUDA.GUI.ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions(() =>
				{
					AssertEquals("Goods Value", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GoodsValue).IsVisible);
					AssertEquals("Goods Currency", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency).IsVisible);
					AssertEquals("ABL_MessageStatus Not Visible", false, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_MessageStatus).IsVisible);
					AssertEquals("ABL_MessageStatus Is Available", false, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_MessageStatus).IsUnavailable);
					AssertEquals("ABL_BillStatus Not Visible", false, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BillStatus).IsVisible);
					AssertEquals("ABL_BillStatus Is Available", false, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BillStatus).IsUnavailable);
					AssertEquals("Entry Number Type Is Available", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.CustomsEntryNumberType).IsVisible);
					AssertEquals("Entry Number Is Available", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.CustomsEntryNumber).IsVisible);
					AssertEquals("GoodsOrigin Is Visible", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.GoodsOrigin).IsVisible);
					AssertEquals("Tariff Is Visible", true, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Tariff).IsVisible);
				});
			}
		}

		public void TestArrivalTransfersTab()
		{
			var manifest = CreateNewManifest();

			using (var form = new ASYCUDA.GUI.ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var arrivalsTab = asycudaManifestUserControl.FindSingle<ZTabPage>("arrivalsTabPage");
				mainTabControl.SelectedTab = arrivalsTab;

				var arrivalDetailsTabControl = arrivalsTab.FindSingle<ZTabControl>("arrivalDetailsTabControl");
				var transfersTab = arrivalDetailsTabControl.FindSingle<ZTabPage>("transfersTabPage");
				Assert("transfersTab TabVisible", transfersTab.TabVisible);
				arrivalDetailsTabControl.SelectedTab = transfersTab;

				var transferHeadersGrid = transfersTab.FindSingle<ZGrid>("transferHeadersGrid");
				var destinationPortColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_RL_NKDestinationPortCode);
				AssertType<ZCodeFindBoxColumnStyleInfo>(destinationPortColumn);
				Assert(destinationPortColumn.IsVisible);
				var transferTypeColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_TransferType);
				AssertType<ZDropEditColumnStyleInfo>(transferTypeColumn);
				Assert(transferTypeColumn.IsVisible);
				var carrierAddressColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_OA_Carrier);
				AssertType<ZGuidDropEditColumnStyleInfo>(carrierAddressColumn);
				Assert(carrierAddressColumn.IsVisible);
				var carrierIDColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_CarrierID);
				AssertType<ZTextBoxColumnStyleInfo>(carrierIDColumn);
				Assert(carrierIDColumn.IsVisible);
				var onwardCarrierColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_OnwardCarrier);
				AssertType<ZCodeFindBoxColumnStyleInfo>(onwardCarrierColumn);
				Assert(onwardCarrierColumn.IsVisible);
				var destinationWarehouseColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_OA_DestinationWarehouse);
				AssertType<ZGuidDropEditColumnStyleInfo>(destinationWarehouseColumn);
				Assert(destinationWarehouseColumn.IsVisible);
				var destinationWarehouseIDColumn = transferHeadersGrid.GetColumnStyle(AsycudaTransferHeader.Schema.ATF_DestinationWarehouseID);
				AssertType<ZTextBoxColumnStyleInfo>(destinationWarehouseIDColumn);
				Assert(destinationWarehouseIDColumn.IsVisible);
			}
		}

		public void TestArrivalTransferDetails()
		{
			var manifest = CreateNewManifest();

			using (var form = new ASYCUDA.GUI.ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<ASYCUDA.GUI.AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var arrivalsTab = asycudaManifestUserControl.FindSingle<ZTabPage>("arrivalsTabPage");
				mainTabControl.SelectedTab = arrivalsTab;

				var arrivalDetailsTabControl = arrivalsTab.FindSingle<ZTabControl>("arrivalDetailsTabControl");
				var transfersTab = arrivalDetailsTabControl.FindSingle<ZTabPage>("transfersTabPage");
				Assert("transfersTab TabVisible", transfersTab.TabVisible);
				arrivalDetailsTabControl.SelectedTab = transfersTab;

				var transferDetailsGroupbox = transfersTab.FindSingle<ZGroupBox>("transferDetailsGroupbox");
				AssertEquals("Transfer Details", transferDetailsGroupbox.Text);

				Assert(transferDetailsGroupbox.FindSingle<ZCodeFindBox>("DestinationPortCodeFindBox").Visible);
				Assert(transferDetailsGroupbox.FindSingle<ZDropEdit>("TransferTypeDropEdit").Visible);
				Assert(transferDetailsGroupbox.FindSingle<ZAddressControl>("CarrierAddressControl").Visible);
				Assert(transferDetailsGroupbox.FindSingle<ZTextBox>("CarrierIDTextBox").Visible);
				Assert(transferDetailsGroupbox.FindSingle<ZCodeFindBox>("OnwardCarrierCodeFindBox").Visible);
				Assert(transferDetailsGroupbox.FindSingle<ZAddressControl>("DestinationWarehouseAddressControl").Visible);
				Assert(transferDetailsGroupbox.FindSingle<ZTextBox>("DestinationWarehouseIDTextBox").Visible);

				var transferBillsGrid = transferDetailsGroupbox.FindSingle<ZGrid>("transferBillsGrid");
				var billNumberColumn = transferBillsGrid.GetColumnStyle(AsycudaTransferBill.Schema.ATB_BillNumber);
				AssertType<ZDropEditColumnStyleInfo>(billNumberColumn);
				Assert(billNumberColumn.IsVisible);
				var inBondNumberColumn = transferBillsGrid.GetColumnStyle(AsycudaTransferBill.Schema.InBondNumber);
				AssertType<ZTextBoxColumnStyleInfo>(inBondNumberColumn);
				Assert(inBondNumberColumn.IsVisible);
				var messageStatusColumn = transferBillsGrid.GetColumnStyle(AsycudaTransferBill.Schema.ATB_MessageStatus);
				AssertType<ZTextBoxColumnStyleInfo>(messageStatusColumn);
				Assert(messageStatusColumn.IsVisible);
				var messageStatusDescriptionColumn = transferBillsGrid.GetColumnStyle(AsycudaTransferBill.Schema.ATB_MessageStatusDescription);
				AssertType<ZTextBoxColumnStyleInfo>(messageStatusDescriptionColumn);
				Assert(messageStatusDescriptionColumn.IsVisible);

				var inbondAllocationControl = transferDetailsGroupbox.FindSingle<TransferBillInBondNumberAllocationUserControl>("TransferBillInBondNumberAllocationUserControl");
				Assert(inbondAllocationControl.Visible);
			}
		}

		protected override void AssertGetArrivalHeadersGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals(1, columnAvailability.Count);
		}

		protected override void AssertGetArrivalLinesGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals(1, columnAvailability.Count);
		}

		protected override void AssertGetTransferBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals(2, columnInfos.Length);
			AssertEquals(AsycudaTransferBill.Schema.InBondNumber, columnInfos[0].ColumnName);
			AssertEquals(AsycudaTransferBill.Schema.ATB_MessageStatusDescription, columnInfos[1].ColumnName);
		}

		protected override void AssertGetTransferBillsGridColumnsOrder(string[] columnsOrder)
		{
			var expectedOrder = new string[]
			{
				AsycudaTransferBill.Schema.ATB_BillNumber,
				AsycudaTransferBill.Schema.InBondNumber,
				AsycudaTransferBill.Schema.ATB_MessageStatus,
				AsycudaTransferBill.Schema.ATB_MessageStatusDescription
			};

			AssertContainsExactElementsInExactOrder(expectedOrder, columnsOrder);
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[]
				{
					"ABL_GoodsValue",
					"ABL_RX_NKGoodsValueCurrency",
					"ABL_MessageStatus",
					"ABL_BillStatus",
					"CustomsEntryNumberType",
					"CustomsEntryNumber",
					"GoodsOrigin",
					"ABL_Tariff"
				}, columnInfos.Select(s => s.ColumnName));
		}

		protected override Type ExpectedTransferBillCountrySpecificUserControlType => typeof(TransferBillInBondNumberAllocationUserControl);

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(ACEBillLayouts);

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = base.CreateNewManifest();

			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			return header;
		}
	}
}
