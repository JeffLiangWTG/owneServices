using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.eTail.GUI.Testing
{
	class HVLVItemLinesUserControlTest : TestCaseWithFactory
	{
		public void TestPriceProperties_DecimalsAlwaysBeTwo_ItemLineGrid()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				var itemLinesGridColumnStyles = form.ItemLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var customsValueColumn = itemLinesGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == nameof(HVLVItemLine.HVS_CustomsValue)) as ZCalcEditColumnStyleInfo;
				var intrinsicValueColumn = itemLinesGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == nameof(HVLVItemLine.HVS_IntrinsicValue)) as ZCalcEditColumnStyleInfo;

				CombineAssertions("Decimals of itemLine's price properties always be two", () =>
				{
					AssertEquals("customs value column in grid", 2, customsValueColumn.Decimals);
					AssertEquals("intrinsic value column in grid", 2, intrinsicValueColumn.Decimals);
				});
			}
		}

		public void TestCharacterCasing_ForItemLineDropEditColumnsAndUnitColumns_ShouldAlwaysBeUpper()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				form.Show();

				var itemLinesGridColumnStyles = form.ItemLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>();

				CombineAssertions("Item Line grid columns should have correct casing", () =>
				{
					var columnCasing = itemLinesGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == "HVS_WeightUnit").CharacterCasing;
					AssertEquals($"HVS_WeightUnit should always be Upper Case", CharacterCasing.Upper, columnCasing);
				});
			}
		}

		public void TestExpectedColumns()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				var columns = form.ItemLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>();

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"HVS_RN_NKOriginCountryCode",
					"HVS_CC_Lookup",
					"ShipmentOriginCountryCode",
					"HVS_ProductCode",
					"ShipmentDestinationCountryCode",
					"HVS_FormattedOriginTariff",
					"HVS_FormattedDestinationTariff",
					"HVS_ItemURL",
					"HVS_GoodsDescription",
					"HVS_OriginGoodsDescription",
					"HVS_Quantity",
					"HVS_NetWeight",
					"HVS_GrossWeight",
					"HVS_WeightUnit",
					"HVS_CustomsValue",
					"HVS_IntrinsicValue"
				}, columns.Select(x => x.ColumnName));
			}
		}
	}
}
