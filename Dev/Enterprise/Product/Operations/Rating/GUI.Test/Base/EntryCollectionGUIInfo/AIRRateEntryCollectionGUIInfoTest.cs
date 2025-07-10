using System;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	public class AIRRateEntryCollectionGUIInfoTest : BaseRateEntryCollectionGUIInfoTest
	{
		protected override Type GUIInfoType => typeof(AIRRateEntryCollectionGUIInfo);
		protected override string RateCategory => RatingConstants.RateCategory.AIR;

		// check CAIRateEntryCollectionGUIInfoTest.cs for updating its tests too
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
{RateEntry.Schema.TI_HBLDeliveryMode}
{RateEntry.Schema.TI_FMCTariffID}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}

		// check CAIRateEntryCollectionGUIInfoTest.cs for updating its tests too
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
{RateEntry.Schema.TI_HBLDeliveryMode}
{RateEntry.Schema.TI_FMCTariffID}
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
{RateEntry.Schema.TI_IsCrossTrade}
{RateEntry.Schema.TI_ViaLRC}
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_RX_NKCurrency}
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
{RateEntry.Schema.TI_ContractNumberLinked}
{RateEntry.Schema.TI_CartagePickupAddressPostCode}
{RateEntry.Schema.TI_CartageDeliveryAddressPostCode}
{RateEntry.Schema.IsPublished}
{RateEntry.Schema.TI_IsTact}
{RateEntry.Schema.TI_IsExcludedFromAutoRating}
{RateEntry.Schema.TI_AircraftType}
{RateEntry.Schema.TI_ShipmentConsolidationStatus}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}

		public void TestColumnsWithNonStandardCosting()
		{
			var info = GetInfos(RateCategory, typeof(Costing), false, false);

			var expectedForNonSupportUser = $@"
{RateEntry.Schema.TI_OriginLRC}
{RateEntry.Schema.TI_DestinationLRC}
{RateEntry.Schema.TI_IsCrossTrade}
{RateEntry.Schema.TI_ViaLRC}
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_RX_NKCurrency}
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
{RateEntry.Schema.TI_ContractNumberLinked}
{RateEntry.Schema.TI_CartagePickupAddressPostCode}
{RateEntry.Schema.TI_CartageDeliveryAddressPostCode}
{RateEntry.Schema.IsPublished}
{RateEntry.Schema.TI_AircraftType}
{RateEntry.Schema.TI_ShipmentConsolidationStatus}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}

		// check CAIRateEntryCollectionGUIInfoTest.cs for updating its tests too
		public virtual void TestCompanyTariffColumns()
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
{RateEntry.Schema.TI_HBLDeliveryMode}
{RateEntry.Schema.TI_FMCTariffID}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}

		public void TestIntercompanyTariffColumns()
		{
			var info = GetInfos(RateCategory, typeof(IntercompanyTariff), isGlobal: true);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.TI_OriginLRC}
{RateEntry.Schema.TI_DestinationLRC}
{RateEntry.Schema.TI_PlannedLoadLRC}
{RateEntry.Schema.TI_PlannedDischargeLRC}
{RateEntry.Schema.TI_RateOrigin}
{RateEntry.Schema.TI_RateDestination}
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_RX_NKCurrency}
{RateEntry.Schema.TI_PaymentTerm}
{RateEntry.Schema.TI_OH_TransportProvider}
{RateEntry.Schema.TransportProviderCarrierCode}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_PL_NKCarrierServiceLevel}
{RateEntry.Schema.TI_RS_NKGatewayServiceLevel}
{RateEntry.Schema.TI_RS_NKShipmentGatewayServiceLevel}
{RateEntry.Schema.TI_GatewayAgentType}
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
{RateEntry.Schema.TI_CartagePickupAddressPostCode}
{RateEntry.Schema.TI_CartageDeliveryAddressPostCode}
{RateEntry.Schema.TI_AircraftType}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}
	}
}
