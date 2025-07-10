using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTradePeriod))]
	sealed class OrgTradePeriodTest : EnterpriseBusinessObjectTestCase
	{
		#region PAS_RepeatsMnth

		public void TestPAS_RepeatsMnth_RefreshesBinding()
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			AssertSettingPropertyRefreshesBinding<ZDecimal>(
				period.PAS_RepeatsMnthInfo,
				1m,
				2m,
				sales.OW_Calc_TotalAnnualChargeableInfo);

			AssertSettingPropertyRefreshesBinding<ZDecimal>(
				period.PAS_RepeatsMnthInfo,
				1m,
				2m,
				sales.OW_Calc_TotalAnnualCountInfo);

			AssertSettingPropertyRefreshesBinding<ZDecimal>(
				period.PAS_RepeatsMnthInfo,
				1m,
				2m,
				sales.OW_Calc_TotalAnnualPalletCountInfo);

			AssertSettingPropertyRefreshesBinding<ZDecimal>(
				period.PAS_RepeatsMnthInfo,
				1m,
				2m,
				sales.OW_Calc_TotalAnnualTEUInfo);

			AssertSettingPropertyRefreshesBinding<ZDecimal>(
				period.PAS_RepeatsMnthInfo,
				1m,
				2m,
				sales.OW_Calc_TotalAnnualVolumeInfo);

			AssertSettingPropertyRefreshesBinding<ZDecimal>(
				period.PAS_RepeatsMnthInfo,
				1m,
				2m,
				sales.OW_Calc_TotalAnnualWeightInfo);
		}

		#endregion

		#region PAS_PalletCount

		public void TestPAS_PalletCount_RefreshesBinding()
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;
			AssertSettingPropertyRefreshesBinding<ZInt>(period.PAS_PalletCountInfo, 1, 2, sales.OW_Calc_TotalAnnualPalletCountInfo);
		}

		#endregion

		#region PAS_Weight

		public void TestPAS_Weight_RefreshesBinding()
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;
			AssertSettingPropertyRefreshesBinding<ZDecimal>(period.PAS_WeightInfo, 1m, 2m, sales.OW_Calc_TotalAnnualWeightInfo);
		}

		#endregion

		#region PAS_Volume

		public void TestPAS_Volume_RefreshesBinding()
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;
			AssertSettingPropertyRefreshesBinding<ZDecimal>(period.PAS_VolumeInfo, 1m, 2m, sales.OW_Calc_TotalAnnualVolumeInfo);
		}

		#endregion

		#region PAS_Chargeable

		public void TestPAS_Chargeable_UpdatedOnWeightOrVolumeChange()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = Constants.TransportModes.Sea;

			var period = tradeDetail.CurrentProspectPeriod;

			period.PAS_Volume = 1;
			AssertEquals(1m, period.PAS_Chargeable);

			period.PAS_Weight = 2000m;
			AssertEquals(2m, period.PAS_Chargeable);
		}

		public void TestPAS_Chargeable_UpdatedOnWeightOrVolumeUnitChange()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = Constants.TransportModes.Sea;

			var period = tradeDetail.CurrentProspectPeriod;

			period.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			period.PAS_Volume = 1;
			AssertEquals(1m, period.PAS_Chargeable);

			period.PAS_VolumeUQ = Constants.Volume.CubicDecimetres;
			AssertEquals(0.001m, period.PAS_Chargeable);

			period.PAS_WeightUQ = Constants.Weight.Kilograms;
			period.PAS_Weight = 2000m;
			AssertEquals(2m, period.PAS_Chargeable);

			period.PAS_WeightUQ = Constants.Weight.Tonnes;
			AssertEquals(2000m, period.PAS_Chargeable);
		}

		public void TestPAS_Chargeable_CannotSetNegative()
		{
			var sales = Factory.NewWithValidTestData<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			period.PAS_Chargeable = -5;

			AssertEquals(-5m, period.PAS_Chargeable);

			AssertHasErrors(period.PAS_ChargeableInfo);
		}

		public void TestPAS_Calc_ChargeableUQ()
		{
			var sales = Factory.NewWithValidTestData<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			AssertEquals(Constants.Weight.Kilograms, period.PAS_Calc_ChargeableUQ);

			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			tradeDetail.PA_TradeMode = Constants.TransportModes.Sea;
			AssertEquals(Constants.Volume.CubicMetres, period.PAS_Calc_ChargeableUQ);

			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;
			AssertEquals(Constants.Weight.Kilograms, period.PAS_Calc_ChargeableUQ);
		}

		#endregion

		#region PAS_EstimatedProfit

		public void TestPAS_EstimatedProfit_UpdateProspectPeriods()
		{
			var salesItem = Helper.NewOrgSales(Helper.NewOrgHeader(), Helper.NewOrgHeader(), Helper.NewOrgHeader());
			var tradeDetail = salesItem.TradeDetails.AddNew();

			tradeDetail.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 2, 14);
			tradeDetail.ProspectPeriodStart = new ZDate(2018, 2, 1);
			tradeDetail.ProspectPeriodEnd = new ZDate(2018, 4, 1);
			tradeDetail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;

			var period = tradeDetail.CurrentProspectPeriod;
			period.PAS_EstimatedProfit = 1200m;

			AssertEquals(12, tradeDetail.ProspectPeriods.Count);

			foreach (var prospectPeriod in tradeDetail.ProspectPeriods)
			{
				AssertEquals(100m, prospectPeriod.PAS_EstimatedProfit);
			}

			tradeDetail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			foreach (var prospectPeriod in tradeDetail.ProspectPeriods)
			{
				AssertEquals(1200m, prospectPeriod.PAS_EstimatedProfit);
			}

			period.PAS_EstimatedProfit = 250m;
			foreach (var prospectPeriod in tradeDetail.ProspectPeriods)
			{
				AssertEquals(250m, prospectPeriod.PAS_EstimatedProfit);
			}

			period.PAS_EstimatedProfit = 60m;
			tradeDetail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			foreach (var prospectPeriod in tradeDetail.ProspectPeriods)
			{
				AssertEquals(260m, prospectPeriod.PAS_EstimatedProfit);
			}
		}

		public void TestPAS_EstimatedProfit_FowardingShipments()
		{
			var shipment = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = shipment.Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			tradeDetail.PA_TradeMode = Constants.TransportModes.Sea;
			tradeDetail.PA_TradeType = Constants.ContainerModes.FCL;
			period.PAS_RateOffered = 10;
			period.PAS_Units = 75;
			AssertEquals(750m, period.PAS_EstimatedProfit);
			AssertEquals(Constants.PkgUnit.Container, period.EstimatedProfitPerUnitType);

			tradeDetail.PA_TradeType = Constants.ContainerModes.BreakBulk;
			period.PAS_RateOffered = 8;
			AssertEquals(0m, period.PAS_EstimatedProfit);
			period.PAS_Chargeable = 50;
			AssertEquals(400m, period.PAS_EstimatedProfit);
		}

		public void TestPAS_EstimatedProfit_LinerAgency()
		{
			var linerAgency = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.LinerAgency);
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = linerAgency.Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			tradeDetail.PA_TradeType = Constants.ContainerModes.FCL;
			period.PAS_RateOffered = 10;
			period.PAS_Units = 75;
			AssertEquals(750m, period.PAS_EstimatedProfit);
			AssertEquals(Constants.PkgUnit.Container, period.EstimatedProfitPerUnitType);

			tradeDetail.PA_TradeType = Constants.ContainerModes.BreakBulk;
			period.PAS_RateOffered = 8;
			AssertEquals(0m, period.PAS_EstimatedProfit);
			period.PAS_Chargeable = 50;
			AssertEquals(400m, period.PAS_EstimatedProfit);

			tradeDetail.PA_TradeType = Constants.ContainerModes.LCL;
			AssertEquals(Constants.Volume.CubicMetres, period.EstimatedProfitPerUnitType);
		}

		public void TestPAS_EstimatedProfit_CustomsBrokerage()
		{
			var customsBrokerage = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = customsBrokerage.Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			period.PAS_RateOffered = 125;
			AssertEquals(125m, period.PAS_EstimatedProfit);
			AssertEquals(ZString.Empty, period.EstimatedProfitPerUnitType);
		}

		public void TestPAS_EstimatedProfit_Warehouse()
		{
			var warehouse = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = warehouse.Identifier;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;

			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			period.PAS_PalletCount = 10;
			period.PAS_RateOffered = 10;
			AssertEquals(100m, period.PAS_EstimatedProfit);
			AssertEquals(Constants.PkgUnit.Pallet, period.EstimatedProfitPerUnitType);

			period.PAS_RateOffered = 8;
			AssertEquals(80m, period.PAS_EstimatedProfit);
			period.PAS_PalletCount = 20;
			AssertEquals(160m, period.PAS_EstimatedProfit);
		}

		#endregion

		#region PAS_Units

		public void TestPAS_UnitsValue()
		{
			var container = Factory.New<RefContainer>();
			container.RC_Code = "XXX";
			container.RC_TEU = 2;
			Factory.Save();

			var tradeDetail = Factory.New<OrgTradeDetail>();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;
			AssertEquals(ZLong.Zero, period.PAS_Units);

			prospect.PAP_RC_NKContainer = "ZZZ";
			AssertEquals(ZLong.Zero, period.PAS_Units);

			prospect.PAP_RC_NKContainer = "XXX";
			AssertEquals(1L, period.PAS_Units);
		}

		public void TestPAS_Units_ReadOnly_Forwarding()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			tradeDetail.PA_TradeType = Constants.ContainerModes.LCL;
			AssertEquals(true, period.PAS_UnitsInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.FCL;
			AssertEquals(false, period.PAS_UnitsInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(false, period.PAS_UnitsInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.ULD;
			AssertEquals(false, period.PAS_UnitsInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.FTL;
			AssertEquals(false, period.PAS_UnitsInfo.ReadOnly);
		}

		public void TestPAS_Units_ReadOnly_Transport()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			tradeDetail.PA_TradeType = Constants.ContainerModes.LTL;
			AssertEquals(true, period.PAS_UnitsInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.FTL;
			AssertEquals(false, period.PAS_UnitsInfo.ReadOnly);
		}

		public void TestPAS_Units_ReadOnly_LinerAgency()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.LinerAgency).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			tradeDetail.PA_TradeType = Constants.ContainerModes.BreakBulk;
			AssertEquals(true, period.PAS_UnitsInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.FCL;
			AssertEquals(false, period.PAS_UnitsInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(false, period.PAS_UnitsInfo.ReadOnly);
		}

		#endregion

		#region PAS_TEUQuantity

		public void TestPA_TEUQuantity_UpdatedAccordingToContainerTypeAndUnits()
		{
			var container = Factory.New<RefContainer>();
			container.RC_Code = "XXX";
			container.RC_TEU = 2;

			Factory.Save();

			var orgSales = Factory.New<OrgSales>();
			var tradeDetail = orgSales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			period.PAS_Units = 1;
			prospect.PAP_RC_NKContainer = "";
			AssertEquals((ZDecimal)0, period.PAS_TEUQuantity);

			prospect.PAP_RC_NKContainer = "XXX";
			AssertEquals((ZDecimal)2, period.PAS_TEUQuantity);

			period.PAS_Units = 2;
			AssertEquals((ZDecimal)4, period.PAS_TEUQuantity);
		}

		public void TestPAS_TEUQuantity_ReadOnly_Forwarding()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			prospect.PAP_RC_NKContainer = "";

			tradeDetail.PA_TradeType = Constants.ContainerModes.LCL;
			AssertEquals(true, period.PAS_TEUQuantityInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.FCL;
			AssertEquals(false, period.PAS_TEUQuantityInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(false, period.PAS_TEUQuantityInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.ULD;
			AssertEquals(false, period.PAS_TEUQuantityInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.FTL;
			AssertEquals(false, period.PAS_TEUQuantityInfo.ReadOnly);

			prospect.PAP_RC_NKContainer = "XXX";
			AssertEquals(true, period.PAS_TEUQuantityInfo.ReadOnly);
		}

		public void TestPAS_TEUQuantity_ReadOnly_Transport()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			prospect.PAP_RC_NKContainer = "";

			tradeDetail.PA_TradeType = Constants.ContainerModes.LTL;
			AssertEquals(true, period.PAS_TEUQuantityInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.FTL;
			AssertEquals(false, period.PAS_TEUQuantityInfo.ReadOnly);

			prospect.PAP_RC_NKContainer = "XXX";
			AssertEquals(true, period.PAS_TEUQuantityInfo.ReadOnly);
		}

		public void TestPAS_TEUQuantity_ReadOnly_LinerAgency()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.LinerAgency).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			prospect.PAP_RC_NKContainer = "";

			tradeDetail.PA_TradeType = Constants.ContainerModes.BreakBulk;
			AssertEquals(true, period.PAS_TEUQuantityInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.FCL;
			AssertEquals(false, period.PAS_TEUQuantityInfo.ReadOnly);

			tradeDetail.PA_TradeType = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(false, period.PAS_TEUQuantityInfo.ReadOnly);

			prospect.PAP_RC_NKContainer = "XXX";
			AssertEquals(true, period.PAS_TEUQuantityInfo.ReadOnly);
		}

		#endregion

		#region Currency

		public void TestPAS_RX_NKCurrency()
		{
			var salesItem = Helper.NewOrgSales(Helper.NewOrgHeader(), Helper.NewOrgHeader(), Helper.NewOrgHeader());
			var tradeDetail = salesItem.TradeDetails.AddNew();

			tradeDetail.ProspectPeriodStart = new ZDate(2018, 2, 1);
			tradeDetail.ProspectPeriodEnd = new ZDate(2018, 4, 1);

			AssertEquals(12, tradeDetail.ProspectPeriods.Count);

			var period = tradeDetail.CurrentProspectPeriod;
			period.PAS_RX_NKCurrency = "AUD";

			foreach (var prospectPeriod in tradeDetail.ProspectPeriods)
			{
				AssertEquals("AUD", prospectPeriod.PAS_RX_NKCurrency);
			}

			period.PAS_RX_NKCurrency = "USD";
			foreach (var prospectPeriod in tradeDetail.ProspectPeriods)
			{
				AssertEquals("USD", prospectPeriod.PAS_RX_NKCurrency);
			}
		}

		public void TestSetDefaultCurrencyWithNoCurrency()
		{
			var salesItem = Helper.NewOrgSales(Helper.NewOrgHeader(), Helper.NewOrgHeader(), Helper.NewOrgHeader());
			salesItem.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "DEHAM", "RL").PK;
			salesItem.OW_OriginTableCode = "RL";
			salesItem.Origin.Country.RN_RX_NKLocalCurrency = ZString.Empty;

			var tradeDetail = salesItem.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;

			var period = tradeDetail.CurrentProspectPeriod;
			AssertEquals("", period.PAS_RX_NKCurrency);
		}

		public void TestSetDefaultCurrency()
		{
			var salesItem = Helper.NewOrgSales(Helper.NewOrgHeader(), Helper.NewOrgHeader(), Helper.NewOrgHeader());
			salesItem.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "DEHAM", "RL").PK;
			salesItem.OW_OriginTableCode = "RL";
			salesItem.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", "RL").PK;
			salesItem.OW_DestinationTableCode = "RL";

			var tradeDetail = salesItem.TradeDetails.AddNew();
			var period = tradeDetail.CurrentProspectPeriod;

			salesItem.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;
			AssertEquals("EUR", period.PAS_RX_NKCurrency);

			tradeDetail.PA_TradeMode = Constants.TransportModes.Sea;
			AssertEquals("USD", period.PAS_RX_NKCurrency);

			salesItem.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage).Identifier;
			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;
			AssertEquals("AUD", period.PAS_RX_NKCurrency);

			salesItem.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			tradeDetail.PA_TradeMode = Constants.TransportModes.Sea;
			AssertEquals("USD", period.PAS_RX_NKCurrency);

			Factory.Save();

			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;
			AssertEquals("USD", period.PAS_RX_NKCurrency);

			tradeDetail.PA_TradeMode = Constants.TransportModes.Sea;
			AssertEquals("USD", period.PAS_RX_NKCurrency);

			period.PAS_RX_NKCurrency = ZString.Empty;
			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;
			AssertEquals("EUR", period.PAS_RX_NKCurrency);
		}

		#endregion

		void AssertSettingPropertyRefreshesBinding<T>(ZPropertyInfo propertyInfoToSet, T oldValue, T newValue, ZPropertyInfo expectedPropertyInfoToRefreshBinding) where T : IZType
		{
			var refreshed = false;
			propertyInfoToSet.Value = oldValue;
			expectedPropertyInfoToRefreshBinding.ValueChanged += (sender, e) => refreshed = true;
			propertyInfoToSet.Value = newValue;

			Assert($"Changing {propertyInfoToSet.Name} should have refreshed binding for {expectedPropertyInfoToRefreshBinding.Name}", refreshed);
		}

		OrgSalesTestHelper Helper
		{
			get { return helper ?? (helper = new OrgSalesTestHelper(Factory)); }
		}
		OrgSalesTestHelper helper;
	}
}
