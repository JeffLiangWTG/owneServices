using System;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	public sealed class TWURateEntryCollectionGUIInfoTest : BaseRateEntryCollectionGUIInfoTest
	{
		protected override Type GUIInfoType => typeof(TWURateEntryCollectionGUIInfo);

		protected override string RateCategory => RatingConstants.RateCategory.TWU;

		public override void TestClientRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(ClientRate), false);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.AllWarehouses}
{RateEntry.Schema.TI_WW_Warehouse}
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityDescription}
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
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityDescription}
{RateEntry.Schema.CommodityLocalCode}
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
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityDescription}
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
