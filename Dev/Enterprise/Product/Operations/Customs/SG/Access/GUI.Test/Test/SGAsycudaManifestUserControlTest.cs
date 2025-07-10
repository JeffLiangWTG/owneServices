using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using MessageStatusCodeList = Enterprise.Customs.ASYCUDA.Business.MessageStatusCodeList;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	partial class ApplicationGUIProviderTest
	{
		public void TestAmendBillDetailsMenuItem()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			header.AMA_ManifestType = "MGE";
			AssertEquals(true, header.LockedBills);

			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			bill.ABL_ShipmentType = Universal.Helper.ShipmentTypeList.Codes.Export22;

			var sgPackedItem = pack.PackedItem;
			sgPackedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;

			using (var form = new ASYCUDA.GUI.ManifestForm(header))
			using (var control = new ASYCUDA.GUI.AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabControl = (ZTabControl)control.Controls.Find("mainTabControl", false).Single();
				tabControl.SelectTab("billsAndPacksTabPage");

				var grid = (ZGrid)control.Controls.Find("BillsGrid", true).Single();
				control.SelectAndShowBill(bill.PK);
				AssertEquals(bill, grid.ListManager.GetCurrent());
				var sgPackCustomsEntryNumber = sgPackedItem.CustomsEntryNumbers.AddNew();
				sgPackCustomsEntryNumber.CE_EntryType = "ASY";
				sgPackCustomsEntryNumber.CE_EntryNum = "123";
				sgPackedItem.API_PackStatus = "REJ";
				var initialMsgStatus = sgPackedItem.API_MessageStatus;
				var amendBillDetailsMenuItem = grid.ContextMenu.MenuItems.FindByText("Amend Bill Details");

				grid.ContextMenu.DoPopup();
				AssertEquals(true, amendBillDetailsMenuItem.Visible);

				sgPackedItem.API_MessageStatus = MessageStatusCodeList.Codes.Updated;

				grid.ContextMenu.DoPopup();
				AssertEquals(true, amendBillDetailsMenuItem.Visible);

				sgPackedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
				grid.ContextMenu.DoPopup();
				AssertEquals(true, amendBillDetailsMenuItem.Visible);

				sgPackedItem.API_PackStatus = "CAN";
				grid.ContextMenu.DoPopup();
				AssertEquals(true, amendBillDetailsMenuItem.Visible);

				sgPackedItem.API_PackStatus = "REJ";
				grid.ContextMenu.DoPopup();
				AssertEquals(true, amendBillDetailsMenuItem.Visible);

				sgPackCustomsEntryNumber.CE_EntryNum = "";
				grid.ContextMenu.DoPopup();
				AssertEquals(false, amendBillDetailsMenuItem.Visible);

				var sgBillCustomsEntryNumber = bill.CustomsEntryNumbers.AddNew();
				sgBillCustomsEntryNumber.CE_EntryType = "ASY";
				sgBillCustomsEntryNumber.CE_EntryNum = "123";
				grid.ContextMenu.DoPopup();
				AssertEquals(true, amendBillDetailsMenuItem.Visible);

				grid.ContextMenu.DoPopup();
				AssertEquals(true, bill.ReadOnly);

				amendBillDetailsMenuItem.PerformClick();
				AssertEquals(false, bill.ReadOnly);
				AssertEquals(MessageStatusCodeList.Codes.Updated, sgPackedItem.API_MessageStatus);

				grid.ContextMenu.DoPopup();
				AssertEquals(true, amendBillDetailsMenuItem.Visible);
			}
		}

		public void TestBillsGridColumnNamesInSortOrder()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Air;

			var bill = header.Bills.AddNew();

			using (var form = new ASYCUDA.GUI.ManifestForm(header))
			using (var control = new ASYCUDA.GUI.AsycudaManifestUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var columnStyles = ((ZGrid)control.Controls.Find("BillsGrid", true).First()).ColumnStyles;
				var expectedList = GetExpectedBillsGridColumnNamesInSortOrderList();
				var length = expectedList.Length;
				Assert("packsGrid.ColumnStyles.Count must have at least " + length.ToString(), length <= columnStyles.Count);
				CombineAssertions(() =>
				{
					for (int i = 0; i < expectedList.Length; i++)
					{
						var columnInfo = columnStyles[i] as ZGridColumnInfo;
						AssertNotNull(columnInfo);
						var expectedColumnName = expectedList[i];
						AssertEquals(i.ToString() + " Expected", expectedColumnName, columnInfo.ColumnName);
					}
				});
			}
		}

		string[] GetExpectedBillsGridColumnNamesInSortOrderList()
		{
			return new string[]
			{
				AsycudaBill.Schema.ABL_BillNumber,
				AsycudaBill.Schema.ABL_BolType,
				AsycudaBill.Schema.ABL_RL_NKOrigin,
				AsycudaBill.Schema.ABL_RL_NKFinalDestination,
				AsycudaBill.Schema.ABL_GoodsDescription,
				AsycudaBill.Schema.ABL_ManifestQty,
				AsycudaBill.Schema.ABL_ManifestUQ,
				AsycudaBill.Schema.ABL_GrossWeight,
				AsycudaBill.Schema.ABL_GrossWeightUQ,
				AsycudaBill.Schema.ABL_Volume,
				AsycudaBill.Schema.ABL_VolumeUQ,
				AsycudaBill.Schema.ABL_MarksAndNumbers,
				AsycudaBill.Schema.ABL_Remarks,
				AsycudaBill.Schema.ABL_UCRNumber,
				AsycudaBill.Schema.CustomsJobNumber,
				AsycudaBill.Schema.CycleDate,
				AsycudaBill.Schema.CycleNumber,
				AsycudaBill.Schema.ABL_MessageStatus,
				AsycudaBill.Schema.ABL_BillStatus,
			};
		}
	}
}
