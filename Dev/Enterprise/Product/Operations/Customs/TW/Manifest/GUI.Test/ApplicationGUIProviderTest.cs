using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using AsycudaBill = Enterprise.Customs.TW.Manifest.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override void AssertGetContainersGridColumnsOrder(string[] columnsOrder)
		{
			CombineAssertions(() =>
			{
				AssertEquals(6, columnsOrder.Length);
				AssertContainsExactElementsInExactOrder(
					new[]
					{
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_ContainerNumber,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_RC_ContainerType,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_EmptyFullIndicator,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal1,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal2,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal3
					},
					columnsOrder);
			});
		}

		protected override void AssertGetContainersGridColumnsWidth(IReadOnlyDictionary<string, int> columnsWidth)
		{
			CombineAssertions(() =>
			{
				AssertEquals(6, columnsWidth.Count);
				AssertContainsExactElementsInExactOrder(
					new[]
					{
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_ContainerNumber,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_EmptyFullIndicator,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_RC_ContainerType,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal1,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal2,
					ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal3
					},
					columnsWidth.Keys);
				AssertContainsExactElementsInExactOrder(new[] { 130, 130, 130, 125, 125, 125 }, columnsWidth.Values);
			});
		}

		protected override void AssertGetContainersGridColumnVisibility(IReadOnlyDictionary<bool, string[]> columnVisibility)
		{
			CombineAssertions(() =>
			{
				AssertEquals(2, columnVisibility.Count);
				AssertContainsExactElementsInExactOrder(new[] { true, false }, columnVisibility.Keys);

				if (columnVisibility.TryGetValue(true, out string[] visibleColumns))
				{
					AssertContainsExactElementsInAnyOrder(
						new string[]
						{
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal2,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal3
						},
						visibleColumns);
				}

				if (columnVisibility.TryGetValue(false, out string[] invisibleColumns))
				{
					AssertContainsExactElementsInAnyOrder(
						new string[]
						{
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealType1,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealingPartyType,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealingPartyName,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_NumberOfPackages,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_CommodityCode,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_GoodsWeight,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_GoodsWeightUQ,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_StowageLocation,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal1UnloadingState,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal2UnloadingState,
						ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal3UnloadingState,
						},
						invisibleColumns);
				}
			});
		}

		public void TestBillsGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions(() =>
				{
					Assert("ABL_RL_NKFinalDestination IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKFinalDestination).IsVisible);
					Assert("ABL_BillNumber IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BillNumber).IsVisible);
					Assert("ABL_Remarks IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Remarks).IsVisible);
					Assert("ABL_UCRNumber IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_UCRNumber).IsVisible);
					Assert("ABL_ManifestQty IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ManifestQty).IsVisible);
					Assert("ABL_ManifestUQ IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ManifestUQ).IsVisible);
					Assert("ABL_GrossWeight IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GrossWeight).IsVisible);
					Assert("ABL_GrossWeightUQ IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GrossWeightUQ).IsVisible);
					Assert("ABL_Volume IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Volume).IsVisible);
					Assert("ABL_VolumeUQ IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_VolumeUQ).IsVisible);
					Assert("ABL_MarksAndNumbers IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_MarksAndNumbers).IsVisible);

					Assert("ABL_SequenceNumber IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SequenceNumber).IsUnavailable);
					Assert("ABL_BolType IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BolType).IsUnavailable);
					Assert("ABL_RL_NKOrigin IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKOrigin).IsUnavailable);
					Assert("ABL_CarrierReference IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CarrierReference).IsUnavailable);
					Assert("ABL_GoodsDescription IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GoodsDescription).IsUnavailable);
					Assert("ABL_CustomsValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CustomsValue).IsUnavailable);
					Assert("ABL_RX_NKCustomsValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency).IsUnavailable);
					Assert("ABL_FreightValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_FreightValue).IsUnavailable);
					Assert("ABL_RX_NKFreightValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency).IsUnavailable);
					Assert("ABL_InsuranceValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_InsuranceValue).IsUnavailable);
					Assert("ABL_RX_NKInsuranceValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency).IsUnavailable);
					Assert("ABL_TransportValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_TransportValue).IsUnavailable);
					Assert("ABL_RX_NKTransportValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency).IsUnavailable);
					Assert("ABL_PrepaidCollect IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_PrepaidCollect).IsUnavailable);
					Assert("ABL_CargoStatus IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CargoStatus).IsUnavailable);
					Assert("ShipperOrgPK IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ShipperOrgPK).IsUnavailable);
					Assert("ABL_OA_Shipper IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OA_Shipper).IsUnavailable);
					Assert("ABL_ShipperName IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperName).IsUnavailable);
					Assert("ABL_ShipperStreet1 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperStreet1).IsUnavailable);
					Assert("ABL_ShipperStreet2 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperStreet2).IsUnavailable);
					Assert("ABL_ShipperCity IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperCity).IsUnavailable);
					Assert("ABL_ShipperState IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperState).IsUnavailable);
					Assert("ABL_ShipperPostcode IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperPostcode).IsUnavailable);
					Assert("ABL_RN_NKShipperCountry IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RN_NKShipperCountry).IsUnavailable);
					Assert("ConsigneeOrgPK IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ConsigneeOrgPK).IsUnavailable);
					Assert("ABL_OA_Consignee IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OA_Consignee).IsUnavailable);
					Assert("ABL_ConsigneeName IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeName).IsUnavailable);
					Assert("ABL_ConsigneeStreet1 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeStreet1).IsUnavailable);
					Assert("ABL_ConsigneeStreet2 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeStreet2).IsUnavailable);
					Assert("ABL_ConsigneeCity IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeCity).IsUnavailable);
					Assert("ABL_ConsigneeState IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeState).IsUnavailable);
					Assert("ABL_ConsigneePostcode IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneePostcode).IsUnavailable);
					Assert("ABL_RN_NKConsigneeCountry IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RN_NKConsigneeCountry).IsUnavailable);
					Assert("ABL_ConsigneePhone IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneePhone).IsUnavailable);
					Assert("NotifyPartyOrgPK IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.NotifyPartyOrgPK).IsUnavailable);
					Assert("ABL_OA_NotifyParty IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OA_NotifyParty).IsUnavailable);
					Assert("ABL_NotifyPartyName IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyName).IsUnavailable);
					Assert("ABL_NotifyPartyStreet1 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyStreet1).IsUnavailable);
					Assert("ABL_NotifyPartyStreet2 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyStreet2).IsUnavailable);
					Assert("ABL_NotifyPartyCity IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyCity).IsUnavailable);
					Assert("ABL_NotifyPartyState IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyState).IsUnavailable);
					Assert("ABL_NotifyPartyPostcode IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyPostcode).IsUnavailable);
					Assert("ABL_RN_NKNotifyPartyCountry IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RN_NKNotifyPartyCountry).IsUnavailable);
					Assert("ABL_NotifyPartyPhone IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyPhone).IsUnavailable);
					Assert("DiscountValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.DiscountValue).IsUnavailable);
					Assert("DiscountValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.DiscountValueCurrency).IsUnavailable);
					Assert("OtherChargesValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.OtherChargesValue).IsUnavailable);
					Assert("OtherChargesValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.OtherChargesValueCurrency).IsUnavailable);
					Assert("CustomsJobNumber IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.CustomsJobNumber).IsUnavailable);
				});
			}
		}

		public void TestBillsGridExtraColumns()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions(() =>
				{
					Assert("ABL_RL_NKPortOfLoading", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKPortOfLoading).IsVisible);
					Assert("ABL_LocationInformation", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_LocationInformation).IsVisible);
					Assert("ABL_GoodsLocation", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GoodsLocation).IsVisible);
					Assert("ABL_ShipmentType", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipmentType).IsVisible);
					Assert("GoodsDescription", billsGrid.GetColumnStyle(AsycudaBill.Schema.GoodsDescription).IsVisible);
					Assert("ABL_SplitQuantity", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SplitQuantity).IsVisible);
					Assert("ABL_SplitQuantityUQ", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SplitQuantityUQ).IsVisible);
					Assert("CustomsEntryNumber", billsGrid.GetColumnStyle(AsycudaBill.Schema.CustomsEntryNumber).IsVisible);
					Assert("ABL_BillStatus", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BillStatus).IsVisible);
					Assert("ABL_MessageStatus", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_MessageStatus).IsVisible);
					Assert("ABL_BillStatusDescription", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BillStatusDescription).IsVisible);
					Assert("StatusDescription", billsGrid.GetColumnStyle(AsycudaBill.Schema.StatusDescription).IsVisible);
				});
			}
		}

		protected override void AssertGetBillsGridColumnsWidth(IReadOnlyDictionary<string, int> columnsWidth)
		{
			AssertEquals(4, columnsWidth.Count);
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					AsycudaBill.Schema.ABL_Remarks,
					AsycudaBill.Schema.ABL_ManifestQty,
					AsycudaBill.Schema.ABL_GrossWeight,
					AsycudaBill.Schema.ABL_Volume
				},
				columnsWidth.Keys);
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					157,
					70,
					70,
					70
				},
				columnsWidth.Values);
		}

		public void TestMessagesGridExtraColumns()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var messagesTabPage = mainTabControl.FindSingle<ZTabPage>("mainTabControl_TabPage_MessagesUserControl");
				mainTabControl.SelectedTab = messagesTabPage;
				var messagesGrid = messagesTabPage.FindSingle<ZGrid>("messagesGrid");

				CombineAssertions(() =>
				{
					Assert("EM_ApplicationCode", !messagesGrid.GetColumnStyle(AsycudaMessage.Schema.EM_ApplicationCode).IsVisible);
					Assert("EM_MessageOwner", !messagesGrid.GetColumnStyle(AsycudaMessage.Schema.EM_MessageOwner).IsVisible);
					Assert("EM_SendWithMessageErrors", !messagesGrid.GetColumnStyle(AsycudaMessage.Schema.EM_SendWithMessageErrors).IsVisible);
					Assert("SystemCreateTimeUtcPlusEight", messagesGrid.GetColumnStyle("SystemCreateTimeUtcPlusEight").IsVisible);
					Assert("EM_SystemLastEditTimeUtc", !messagesGrid.GetColumnStyle(AsycudaMessage.Schema.EM_SystemLastEditTimeUtc).IsVisible);
					Assert("SystemLastEditTimeUtcPlusEight", messagesGrid.GetColumnStyle("SystemLastEditTimeUtcPlusEight").IsVisible);
					Assert("EM_SystemLastEditUser", messagesGrid.GetColumnStyle(AsycudaMessage.Schema.EM_SystemLastEditUser).IsVisible);
					Assert("InterchangeApplicationCode", messagesGrid.GetColumnStyle("InterchangeApplicationCode").IsVisible);
					Assert("InterchangeSender", !messagesGrid.GetColumnStyle("InterchangeSender").IsVisible);
					Assert("InterchangeReceiver", !messagesGrid.GetColumnStyle("InterchangeReceiver").IsVisible);
					Assert("InterchangeStatus", !messagesGrid.GetColumnStyle("InterchangeStatus").IsVisible);
					Assert("InterchangeNumber", messagesGrid.GetColumnStyle("InterchangeNumber").IsVisible);
					Assert("InterchangeDeliveredTime", messagesGrid.GetColumnStyle("InterchangeDeliveredTime").IsVisible);
					Assert("InterchangeCreateTime", !messagesGrid.GetColumnStyle("InterchangeCreateTime").IsVisible);
					Assert("InterchangeCreateTimePlusEight", !messagesGrid.GetColumnStyle("InterchangeCreateTimePlusEight").IsVisible);
					Assert("InterchangeCreateUser", !messagesGrid.GetColumnStyle("InterchangeCreateUser").IsVisible);
					Assert("InterchangeLastEditTime", !messagesGrid.GetColumnStyle("InterchangeLastEditTime").IsVisible);
					Assert("InterchangeLastEditTimePlusEight", !messagesGrid.GetColumnStyle("InterchangeLastEditTimePlusEight").IsVisible);
					Assert("InterchangeLastEditUser", !messagesGrid.GetColumnStyle("InterchangeLastEditUser").IsVisible);
				});
			}
		}

		protected override void AssertGetMessagesGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			CombineAssertions(() =>
			{
				AssertEquals(19, columnInfos.Length);
				AssertEquals("EM_ApplicationCode", Enterprise.Customs.TW.Manifest.Business.AsycudaMessage.Schema.EM_ApplicationCode, columnInfos[0].ColumnName);
				AssertEquals("EM_MessageOwner", Enterprise.Customs.TW.Manifest.Business.AsycudaMessage.Schema.EM_MessageOwner, columnInfos[1].ColumnName);
				AssertEquals("EM_SendWithMessageErrors", Enterprise.Customs.TW.Manifest.Business.AsycudaMessage.Schema.EM_SendWithMessageErrors, columnInfos[2].ColumnName);
				AssertEquals("SystemCreateTimeUtcPlusEight", "SystemCreateTimeUtcPlusEight", columnInfos[3].ColumnName);
				AssertEquals("EM_SystemLastEditTimeUtc", "EM_SystemLastEditTimeUtc", columnInfos[4].ColumnName);
				AssertEquals("SystemLastEditTimeUtcPlusEight", "SystemLastEditTimeUtcPlusEight", columnInfos[5].ColumnName);
				AssertEquals("EM_SystemLastEditUser", Enterprise.Customs.TW.Manifest.Business.AsycudaMessage.Schema.EM_SystemLastEditUser, columnInfos[6].ColumnName);
				AssertEquals("InterchangeApplicationCode", "InterchangeApplicationCode", columnInfos[7].ColumnName);
				AssertEquals("InterchangeSender", "InterchangeSender", columnInfos[8].ColumnName);
				AssertEquals("InterchangeReceiver", "InterchangeReceiver", columnInfos[9].ColumnName);
				AssertEquals("InterchangeStatus", "InterchangeStatus", columnInfos[10].ColumnName);
				AssertEquals("InterchangeNumber", "InterchangeNumber", columnInfos[11].ColumnName);
				AssertEquals("InterchangeDeliveredTime", "InterchangeDeliveredTime", columnInfos[12].ColumnName);
				AssertEquals("InterchangeCreateTime", "InterchangeCreateTime", columnInfos[13].ColumnName);
				AssertEquals("InterchangeCreateTimePlusEight", "InterchangeCreateTimePlusEight", columnInfos[14].ColumnName);
				AssertEquals("InterchangeCreateUser", "InterchangeCreateUser", columnInfos[15].ColumnName);
				AssertEquals("InterchangeLastEditTime", "InterchangeLastEditTime", columnInfos[16].ColumnName);
				AssertEquals("InterchangeLastEditTimePlusEight", "InterchangeLastEditTimePlusEight", columnInfos[17].ColumnName);
				AssertEquals("InterchangeLastEditUser", "InterchangeLastEditUser", columnInfos[18].ColumnName);
			});
		}

		protected override void AssertGetMessagesGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals(1, columnAvailability.Count);
			AssertContainsExactElementsInExactOrder(new[] { false }, columnAvailability.Keys);
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					AsycudaMessage.Schema.EM_MessageDateTime,
					TWMessage.Schema.InterchangeeHubID
				},
				columnAvailability.Values.FirstOrDefault());
		}

		protected override void AssertGetMessagesGridColumnVisible(IReadOnlyDictionary<bool, string[]> columnVisible)
		{
			AssertEquals(2, columnVisible.Count);
			AssertContainsExactElementsInExactOrder(new[] { true, false }, columnVisible.Keys);
			AssertContainsExactElementsInExactOrder(new[] { AsycudaMessage.Schema.EM_MessageSubType }, columnVisible.Values.ElementAt(0));
			AssertContainsExactElementsInExactOrder(new[] { AsycudaMessage.Schema.EM_SystemCreateTimeUtc }, columnVisible.Values.ElementAt(1));
		}

		protected override void AssertGetMessagesGridColumnsWidth(IReadOnlyDictionary<string, int> columnsWidth)
		{
			AssertEquals(3, columnsWidth.Count);
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					AsycudaMessage.Schema.EM_MessageSubType,
					AsycudaMessage.Schema.EM_SystemCreateTimeUtc,
					AsycudaMessage.Schema.EM_SystemCreateUser
				},
				columnsWidth.Keys);
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					130,
					140,
					140
				},
				columnsWidth.Values);
		}

		protected override void AssertGetBillsGridColumnVisiblilityOnValueChanged(IReadOnlyDictionary<string, bool> columnsVisiblility, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			AssertEquals(1, columnsVisiblility.Count);
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					AsycudaBill.Schema.BagNumber
				},
				columnsVisiblility.Keys);
		}

		protected override Dictionary<string, ControlReference[]> GetManifestControlGroups()
		{
			var common = CommonManifestControlBag.Instance;
			var twManifestControlBag = TWManifestControlBag.Instance;
			var groups = new Dictionary<string, ControlReference[]>();
			groups.Add("First Columns", new[]
			{
				common.TransportModeDropEdit,
				common.CustomsOfficeDropEdit,
				common.ManifestNumberFromMasterBillTextBox,
				twManifestControlBag.GoodsLocationCodeFindBox,
				common.ShortEstArrivalDateEdit
			});
			groups.Add("Second Columns", new[]
			{
				common.VoyageFlightTextBox,
				common.VesselCodeFindBox,
				common.LloydsNumberTextBox,
				common.RadioCallSignTextBox,
				common.VehicleRegistrationTextBox
			});
			groups.Add("Third Columns", new[]
			{
				common.CarrierAddressControl,
				common.CarrierCodeTextBox,
				common.DeconsolidateAddressControl
			});
			return groups;
		}

		public void TestBagNumberColumnIsOnlyVisibleWhenTransportTypeAir()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				manifest.AMA_TransportMode = ZString.Empty;
				Assert("BagNumber", !billsGrid.GetColumnStyle("BagNumber").IsVisible);

				manifest.AMA_TransportMode = "SEA";
				Assert("BagNumber", !billsGrid.GetColumnStyle("BagNumber").IsVisible);

				manifest.AMA_TransportMode = "AIR";
				Assert("BagNumber", billsGrid.GetColumnStyle("BagNumber").IsVisible);
			}
		}

		public void TestAMA_VesselNameInfoValueChanged()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "VESSEL1";
			vessel1.RV_LloydsNumber = "9832343";
			vessel1.RV_RadioCallSign = "CALLME1";
			vessel1.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "VESSEL2";
			vessel2.RV_LloydsNumber = "0943454";
			vessel2.RV_RadioCallSign = "CALLME2";
			vessel2.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;
			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Name = "VESSEL3";
			vessel3.RV_LloydsNumber = "1054565";
			vessel3.RV_RadioCallSign = "CALLME3";
			vessel3.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MAN";
			header.AMA_ApplicationCode = RefCusCodeListTypes.Codes.NVC;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			using (var form = new ManifestForm(header))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				header.AMA_VesselNameInfo.Value = new ZString("VESSEL1");
				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("AMA_VesselName", "VESSEL1", header.AMA_VesselName);
					AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
					AssertEquals("AMA_RadioCallSign", "CALLME1", header.AMA_RadioCallSign);
					AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Taiwan, header.AMA_RN_NKConveyanceNationality);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				header.AMA_VesselNameInfo.Value = new ZString("VESSEL2");
				CombineAssertions(() =>
				{
					AssertEquals("The selected Vessel has a different IMO Number (0943454). Do you want to replace the current value (9832343)?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("AMA_VesselName", "VESSEL2", header.AMA_VesselName);
					AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
					AssertEquals("AMA_RadioCallSign", "CALLME1", header.AMA_RadioCallSign);
					AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Taiwan, header.AMA_RN_NKConveyanceNationality);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				header.AMA_VesselNameInfo.Value = new ZString("VESSEL3");
				CombineAssertions(() =>
				{
					AssertEquals("The selected Vessel has a different IMO Number (1054565). Do you want to replace the current value (9832343)?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("AMA_VesselName", "VESSEL3", header.AMA_VesselName);
					AssertEquals("AMA_LloydsNumber", "1054565", header.AMA_LloydsNumber);
					AssertEquals("AMA_RadioCallSign", "CALLME3", header.AMA_RadioCallSign);
					AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Taiwan, header.AMA_RN_NKConveyanceNationality);
				});
			}
		}

		public override void TestVesselCodeFindBox_PopupSelected()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";
			vessel.RV_LloydsNumber = "9832343";
			vessel.RV_RadioCallSign = "CALLME";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;
			Factory.Save();
			var header = CreateNewManifest();
			var selectedEventArgs = new ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs(new BusinessObject[] { vessel });
			ApplicationGUIProvider.GetApplicationGuiProvider(header).VesselCodeFindBox_PopupSelected(header, null, selectedEventArgs);
			CombineAssertions(() =>
			{
				AssertEquals("AMA_VesselName", ZString.Empty, header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", ZString.Empty, header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", ZString.Empty, header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", ZString.Empty, header.AMA_RN_NKConveyanceNationality);
			});
		}

		public void TestManifestLayoutType()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertType<TWManifestLayouts>(provider.GetManifestLayout());
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[] {
				"ABL_RL_NKPortOfLoading",
				"ABL_LocationInformation",
				"ABL_GoodsLocation",
				"ABL_ShipmentType",
				"GoodsDescription",
				"BagNumber",
				"ABL_SplitQuantity",
				"ABL_SplitQuantityUQ",
				"CustomsEntryNumber",
				"ABL_BillStatus",
				"ABL_BillStatusDescription",
				"ABL_MessageStatus",
				"StatusDescription",
			}, columnInfos.Select(s => s.ColumnName));
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaContainerBillLinkUserControl) };

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(TWBillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(TWBillPartiesLayouts);

		protected override int MaxColumnsOfManifestLayout => 3;

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MAN";
			header.AMA_ApplicationCode = RefCusCodeListTypes.Codes.NVC;
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = Enterprise.Customs.ASYCUDA.Business.AsycudaBill.ChildBolCode;
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "HELLO";
			_ = pack.PackedItemForTesting();
			return header;
		}
	}
}
