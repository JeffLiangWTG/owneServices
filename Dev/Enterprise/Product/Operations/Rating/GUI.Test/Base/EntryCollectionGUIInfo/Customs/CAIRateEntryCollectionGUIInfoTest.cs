using System;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	public sealed class CAIRateEntryCollectionGUIInfoTest : AIRRateEntryCollectionGUIInfoTest
	{
		protected override Type GUIInfoType => typeof(CAIRateEntryCollectionGUIInfo);

		protected override string RateCategory => RatingConstants.RateCategory.CAI;

		public override void TestClientRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(ClientRate), false);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.TI_OriginLRC}
{RateEntry.Schema.TI_DestinationLRC}
{RateEntry.Schema.TI_IsCrossTrade}
{RateEntry.Schema.TI_ViaLRC}
{RateEntry.Schema.TI_PlannedLoadLRC}
{RateEntry.Schema.TI_PlannedDischargeLRC}
{RateEntry.Schema.TI_RateOrigin}
{RateEntry.Schema.TI_RateDestination}
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_PaymentTerm}
{RateEntry.Schema.TI_OH_Supplier}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TransportProviderCarrierCode}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_PL_NKCarrierServiceLevel}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityDescription}
{RateEntry.Schema.CommodityLocalCode}
{RateEntry.Schema.TI_OH_ControllingCustomer}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.Unit}
{RateEntry.Schema.TI_TransitTime}
{RateEntry.Schema.TI_Frequency}
{RateEntry.Schema.TI_FrequencyUnit}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.TI_ContractNumber}
{RateEntry.Schema.TI_CartagePickupAddressPostCode}
{RateEntry.Schema.TI_CartageDeliveryAddressPostCode}
{RateEntry.Schema.IsPublished}
{RateEntry.Schema.TI_AircraftType}
{RateEntry.Schema.TI_FMCTariffID}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}

		public override void TestCompanyTariffColumns()
		{
			var info = GetInfos(RateCategory, typeof(CompanyTariff), false);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.TI_OriginLRC}
{RateEntry.Schema.TI_DestinationLRC}
{RateEntry.Schema.TI_IsCrossTrade}
{RateEntry.Schema.TI_ViaLRC}
{RateEntry.Schema.TI_PlannedLoadLRC}
{RateEntry.Schema.TI_PlannedDischargeLRC}
{RateEntry.Schema.TI_RateOrigin}
{RateEntry.Schema.TI_RateDestination}
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_PaymentTerm}
{RateEntry.Schema.TI_OH_Supplier}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TransportProviderCarrierCode}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_PL_NKCarrierServiceLevel}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityDescription}
{RateEntry.Schema.CommodityLocalCode}
{RateEntry.Schema.TI_OH_ControllingCustomer}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.Unit}
{RateEntry.Schema.TI_TransitTime}
{RateEntry.Schema.TI_Frequency}
{RateEntry.Schema.TI_FrequencyUnit}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.TI_ContractNumber}
{RateEntry.Schema.TI_CartagePickupAddressPostCode}
{RateEntry.Schema.TI_CartageDeliveryAddressPostCode}
{RateEntry.Schema.IsPublished}
{RateEntry.Schema.TI_AircraftType}
{RateEntry.Schema.TI_FMCTariffID}
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
{RateEntry.Schema.TI_PlannedLoadLRC}
{RateEntry.Schema.TI_PlannedDischargeLRC}
{RateEntry.Schema.TI_RateOrigin}
{RateEntry.Schema.TI_RateDestination}
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_PaymentTerm}
{RateEntry.Schema.TI_OH_Supplier}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TransportProviderCarrierCode}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_PL_NKCarrierServiceLevel}
{RateEntry.Schema.TI_RH_NKCommodityCode}
{RateEntry.Schema.CommodityDescription}
{RateEntry.Schema.CommodityLocalCode}
{RateEntry.Schema.TI_OH_ControllingCustomer}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.Unit}
{RateEntry.Schema.TI_TransitTime}
{RateEntry.Schema.TI_Frequency}
{RateEntry.Schema.TI_FrequencyUnit}
{RateEntry.Schema.TI_DataChecked}
{RateEntry.Schema.TI_QuotePageIncoTerm}
{RateEntry.Schema.TI_ContractNumber}
{RateEntry.Schema.TI_CartagePickupAddressPostCode}
{RateEntry.Schema.TI_CartageDeliveryAddressPostCode}
{RateEntry.Schema.TI_AircraftType}
{RateEntry.Schema.TI_FMCTariffID}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}
	}
}
