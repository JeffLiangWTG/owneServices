using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class Phase5GoodsItemExportToOpenTabUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var exportToOpenSplitContainer = control.ExportToOpenSplitContainer;
				AssertEquals("ExportToOpenSplitContainer.Orientation", Orientation.Horizontal, exportToOpenSplitContainer.Orientation);
				AssertEquals("ExportToOpenSplitContainer.SplitterDistance", 206, exportToOpenSplitContainer.SplitterDistance);
			});
		}

		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
		}

		public void TestDeclarationTypeDropEdit()
		{
			var declarationTypeDropEdit = control.DeclarationTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", declarationTypeDropEdit);
				AssertEquals("GetBindingMember", "ExportToOpenList.CSI_Procedure", declarationTypeDropEdit.GetBindingMember());
			});
		}

		public void TestDeclarationNoTextBox()
		{
			var declarationNoTextBox = control.DeclarationNoTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", declarationNoTextBox);
				AssertEquals("GetBindingMember", "ExportToOpenList.CSI_ReferenceNumber", declarationNoTextBox.GetBindingMember());
			});
		}

		public void TestPartialCheckBox()
		{
			var partialCheckBox = control.PartialCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", partialCheckBox);
				AssertEquals("GetBindingMember", "ExportToOpenList.IsPartial", partialCheckBox.GetBindingMember());
			});
		}

		public void TestDeclarationLineNoCalcEdit()
		{
			var declarationLineNoCalcEdit = control.DeclarationLineNoCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", declarationLineNoCalcEdit);
				AssertEquals("GetBindingMember", "ExportToOpenList.CSI_ItemNumber", declarationLineNoCalcEdit.GetBindingMember());
			});
		}

		public void TestWrapperCheckBox()
		{
			var wrapperCheckBox = control.WrapperCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", wrapperCheckBox);
				AssertEquals("GetBindingMember", "ExportToOpenList.IsWrapper", wrapperCheckBox.GetBindingMember());
			});
		}

		public void TestConsignorIdTextBox()
		{
			var consignorIdTextBox = control.ConsignorIdTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", consignorIdTextBox);
				AssertEquals("GetBindingMember", "ExportToOpenList.CSI_ReferenceNumber2", consignorIdTextBox.GetBindingMember());
			});
		}

		public void TestAvaialbleColumnsForPreviousDocuments()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var bills = header.MovementHeader.Header.Bills.AddNew();
			var goodsitem = bills.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				using (var form = new ZForm(goodsitem))
				using (var control = new Phase5GoodsItemExportToOpenTabUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var exportToOpenGrid = (ZGrid)control.Controls.Find("ExportToOpenGrid", true).Single();
					AssertNotNull(FindColumnByName(exportToOpenGrid, NctsExportToOpen.Schema.CSI_Procedure));
					AssertNotNull(FindColumnByName(exportToOpenGrid, NctsExportToOpen.Schema.IsPartial));
					AssertNotNull(FindColumnByName(exportToOpenGrid, NctsExportToOpen.Schema.CSI_ReferenceNumber));
					AssertNotNull(FindColumnByName(exportToOpenGrid, NctsExportToOpen.Schema.CSI_ItemNumber));
					AssertNotNull(FindColumnByName(exportToOpenGrid, NctsExportToOpen.Schema.IsWrapper));
					AssertNotNull(FindColumnByName(exportToOpenGrid, NctsExportToOpen.Schema.CSI_ReferenceNumber2));
				}
			});
		}

		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName) => grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);

		protected override void SetUp()
		{
			base.SetUp();
			control = new Phase5GoodsItemExportToOpenTabUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		Phase5GoodsItemExportToOpenTabUserControl control;
	}
}
