using System;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	public sealed class TRWRateEntryCollectionGUIInfoTest : BaseRateEntryCollectionGUIInfoTest
	{
		protected override Type GUIInfoType => typeof(TRWRateEntryCollectionGUIInfo);
		protected override string RateCategory => RatingConstants.RateCategory.TRW;

		public override void TestClientRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(ClientRate), false);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.AllWarehouses}
{RateEntry.Schema.TI_WW_Warehouse}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityLocalCode}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.IsPublished}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}

		public override void TestQuotationRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(Quote), false);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.AllWarehouses}
{RateEntry.Schema.TI_WW_Warehouse}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityLocalCode}
{RateEntry.Schema.TI_DataChecked}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}

		public override void TestStandardCostingRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(Costing), false, true);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.AllWarehouses}
{RateEntry.Schema.TI_WW_Warehouse}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityLocalCode}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.IsPublished}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}
	}
}
