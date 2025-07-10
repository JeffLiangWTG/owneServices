using System;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	public sealed class SCORateEntryCollectionGUIInfoTest : BaseRateEntryCollectionGUIInfoTest
	{
		protected override Type GUIInfoType => typeof(SCORateEntryCollectionGUIInfo);
		protected override string RateCategory => RatingConstants.RateCategory.SCO;

		public override void TestClientRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(ClientRate), false);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.TI_OriginLRC}
{RateEntry.Schema.TI_DestinationLRC}
{RateEntry.Schema.TI_ViaLRC}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_IsNonOperatedReefer}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityDescription}
{RateEntry.Schema.CommodityLocalCode}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.Unit}
{RateEntry.Schema.TI_TransitTime}
{RateEntry.Schema.TI_Frequency}
{RateEntry.Schema.TI_FrequencyUnit}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.TI_ContractNumber}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_OH_Supplier}
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
{RateEntry.Schema.TI_OriginLRC}
{RateEntry.Schema.TI_DestinationLRC}
{RateEntry.Schema.TI_ViaLRC}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_IsNonOperatedReefer}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityDescription}
{RateEntry.Schema.CommodityLocalCode}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.Unit}
{RateEntry.Schema.TI_TransitTime}
{RateEntry.Schema.TI_Frequency}
{RateEntry.Schema.TI_FrequencyUnit}
{RateEntry.Schema.TI_DataChecked}
{RateEntry.Schema.TI_QuotePageIncoTerm}
{RateEntry.Schema.TI_ContractNumber}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_OH_Supplier}
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
{RateEntry.Schema.TI_OriginLRC}
{RateEntry.Schema.TI_DestinationLRC}
{RateEntry.Schema.TI_ViaLRC}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_IsNonOperatedReefer}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityDescription}
{RateEntry.Schema.CommodityLocalCode}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.Unit}
{RateEntry.Schema.TI_TransitTime}
{RateEntry.Schema.TI_Frequency}
{RateEntry.Schema.TI_FrequencyUnit}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.TI_ContractNumber}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_OH_Supplier}
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
