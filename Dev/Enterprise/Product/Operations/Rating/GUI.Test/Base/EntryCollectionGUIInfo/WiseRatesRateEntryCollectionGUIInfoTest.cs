using System;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	public sealed class WiseRatesRateEntryCollectionGUIInfoTest : BaseRateEntryCollectionGUIInfoTest
	{
		protected override Type GUIInfoType => typeof(WiseRatesRateEntryCollectionGUIInfo);
		protected override string RateCategory => "WiseRates";

		public override void TestClientRateColumns()
		{
			Assert("Rates Service is Costing Only", true);
		}

		public override void TestQuotationRateColumns()
		{
			Assert("Rates Service is Costing Only", true);
		}

		public override void TestTransportModeFilterAvailability()
		{
			Assert("Not Applicable", true);
		}

		public override void TestSupplierFilterAvailabilityOnClientRate()
		{
			Assert("Not Applicable", true);
		}

		public override void TestSupplierFilterAvailabilityOnCompanyTariff()
		{
			Assert("Not Applicable", true);
		}

		public override void TestSupplierFilterAvailabilityOnQuote()
		{
			Assert("Not Applicable", true);
		}

		public override void TestStandardCostingRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(Costing), false, true);
			var expected = $@"
{RateEntry.Schema.TI_RateCategory}
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_OriginLRC}
{RateEntry.Schema.TI_DestinationLRC}
{RateEntry.Schema.TI_ViaLRC}
{nameof(WiseEntryView.CarrierCode)}
{RateEntry.Schema.TI_ContractNumber}
CGReference
RateProvider
{RateEntry.Schema.TI_OH_ControllingCustomer}
{RateEntry.Schema.TI_OH_Consignor}
{RateEntry.Schema.TI_OH_Consignee}
{RateEntry.Schema.TI_RS_NKServiceLevel_NI}
{RateEntry.Schema.TI_PL_NKCarrierServiceLevel}
{nameof(WiseEntryView.CommodityGroup)}
{nameof(WiseEntryView.ProductName)}
{nameof(WiseEntryView.Commodities)}
{nameof(WiseEntryView.TI_RC)}
{nameof(WiseEntryView.ContainerQuality)}
{nameof(WiseEntryView.ContainerPayloadWeightForBinding)}
{nameof(WiseEntryView.ContainerPayloadVolumeForBinding)}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
AllInCost
FreightRatePerChargeableUnit
UniversalCarrierServiceLevel
CargoguideProductCode
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expected);
		}
	}
}
