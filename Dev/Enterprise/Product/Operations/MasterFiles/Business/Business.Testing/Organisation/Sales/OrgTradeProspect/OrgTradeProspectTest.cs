using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTradeProspect))]
	sealed class OrgTradeProspectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPropertyMaxLength()
		{
			AssertEquals("PA_IndustryVerticalMaxLength", OrgMiscServ.Schema.OM_CMIndustryVerticalMaxLength, OrgTradeProspect.Schema.PAP_IndustryVerticalMaxLength);
			AssertEquals("PA_PeriodOfActivityMaxLength", OrgMiscServ.Schema.OM_CMPeriodOfActivityMaxLength, OrgTradeProspect.Schema.PAP_PeriodOfActivityMaxLength);
		}

		public void TestConversionCertainty_DefaultValue()
		{
			var prospectDetail = Factory.New<OrgTradeProspect>();
			AssertEquals(CertaintyLikertItemList.Descriptions._5ExtremelyLikely, prospectDetail.ConversionCertaintyLikertItemDescription);
		}

		#region PAP_RC_NKContainer

		public void TestPAP_RC_NKContainer_UpdatesTradeTypeBasedOnTradeMode_ForwardingShipment()
		{
			var forwardingSales = Factory.New<OrgSales>();
			forwardingSales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var detail = forwardingSales.TradeDetails.AddNew();
			var prospect = detail.ProspectDetail;

			detail.PA_TradeMode = Constants.TransportModes.Air;
			{
				prospect.PAP_RC_NKContainer = "XXX";
				AssertEquals(Constants.ContainerModes.ULD, detail.PA_TradeType);

				prospect.PAP_RC_NKContainer = "";
				AssertEquals(Constants.ContainerModes.Loose, detail.PA_TradeType);
			}

			detail.PA_TradeMode = Constants.TransportModes.Sea;
			{
				prospect.PAP_RC_NKContainer = "XXX";
				AssertEquals(Constants.ContainerModes.FCL, detail.PA_TradeType);

				prospect.PAP_RC_NKContainer = "";
				AssertEquals(Constants.ContainerModes.LCL, detail.PA_TradeType);
			}

			detail.PA_TradeMode = Constants.TransportModes.Road;
			{
				prospect.PAP_RC_NKContainer = "XXX";
				AssertEquals(Constants.ContainerModes.FTL, detail.PA_TradeType);

				prospect.PAP_RC_NKContainer = "";
				AssertEquals(Constants.ContainerModes.LTL, detail.PA_TradeType);
			}

			detail.PA_TradeMode = Constants.TransportModes.Rail;
			{
				prospect.PAP_RC_NKContainer = "XXX";
				AssertEquals(Constants.ContainerModes.FCL, detail.PA_TradeType);

				prospect.PAP_RC_NKContainer = "";
				AssertEquals(Constants.ContainerModes.LCL, detail.PA_TradeType);
			}
		}

		public void TestPA_RC_NKContainerType_UpdatesTradeTypeBasedOnTradeMode_Transport()
		{
			var transportSales = Factory.New<OrgSales>();
			transportSales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport).Identifier;
			var detail = transportSales.TradeDetails.AddNew();
			var prospect = detail.ProspectDetail;

			prospect.PAP_RC_NKContainer = "XXX";
			AssertEquals(Constants.ContainerModes.FTL, detail.PA_TradeType);

			prospect.PAP_RC_NKContainer = "";
			AssertEquals(Constants.ContainerModes.LTL, detail.PA_TradeType);
		}

		public void TestPA_TradeMode_UpdatesTradeTypeBasedOnContainerType_ForwardingShipment()
		{
			var forwardingSales = Factory.New<OrgSales>();
			forwardingSales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var detail = forwardingSales.TradeDetails.AddNew();
			var prospect = detail.ProspectDetail;

			prospect.PAP_RC_NKContainer = "XXX";
			{
				detail.PA_TradeMode = Constants.TransportModes.Air;
				AssertEquals(Constants.ContainerModes.ULD, detail.PA_TradeType);

				detail.PA_TradeMode = Constants.TransportModes.Sea;
				AssertEquals(Constants.ContainerModes.FCL, detail.PA_TradeType);

				detail.PA_TradeMode = Constants.TransportModes.Road;
				AssertEquals(Constants.ContainerModes.FTL, detail.PA_TradeType);

				detail.PA_TradeMode = Constants.TransportModes.Rail;
				AssertEquals(Constants.ContainerModes.FCL, detail.PA_TradeType);
			}

			prospect.PAP_RC_NKContainer = "";
			{
				detail.PA_TradeMode = Constants.TransportModes.Air;
				AssertEquals(Constants.ContainerModes.Loose, detail.PA_TradeType);

				detail.PA_TradeMode = Constants.TransportModes.Sea;
				AssertEquals(Constants.ContainerModes.LCL, detail.PA_TradeType);

				detail.PA_TradeMode = Constants.TransportModes.Road;
				AssertEquals(Constants.ContainerModes.LTL, detail.PA_TradeType);

				detail.PA_TradeMode = Constants.TransportModes.Rail;
				AssertEquals(Constants.ContainerModes.LCL, detail.PA_TradeType);
			}
		}

		#endregion

		#region PAP_RecurrenceType

		public void TestPAP_RecurrenceType_RefreshesBinding()
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;

			AssertSettingPropertyRefreshesBinding<ZString>(
				prospect.PAP_RecurrenceTypeInfo,
				OrgTradeProspectRecurrenceTypeList.Codes.Yearly,
				OrgTradeProspectRecurrenceTypeList.Codes.Monthly,
				sales.OW_Calc_TotalAnnualChargeableInfo);

			AssertSettingPropertyRefreshesBinding<ZString>(
				prospect.PAP_RecurrenceTypeInfo,
				OrgTradeProspectRecurrenceTypeList.Codes.Yearly,
				OrgTradeProspectRecurrenceTypeList.Codes.Monthly,
				sales.OW_Calc_TotalAnnualCountInfo);

			AssertSettingPropertyRefreshesBinding<ZString>(
				prospect.PAP_RecurrenceTypeInfo,
				OrgTradeProspectRecurrenceTypeList.Codes.Yearly,
				OrgTradeProspectRecurrenceTypeList.Codes.Monthly,
				sales.OW_Calc_TotalAnnualPalletCountInfo);

			AssertSettingPropertyRefreshesBinding<ZString>(
				prospect.PAP_RecurrenceTypeInfo,
				OrgTradeProspectRecurrenceTypeList.Codes.Yearly,
				OrgTradeProspectRecurrenceTypeList.Codes.Monthly,
				sales.OW_Calc_TotalAnnualTEUInfo);

			AssertSettingPropertyRefreshesBinding<ZString>(
				prospect.PAP_RecurrenceTypeInfo,
				OrgTradeProspectRecurrenceTypeList.Codes.Yearly,
				OrgTradeProspectRecurrenceTypeList.Codes.Monthly,
				sales.OW_Calc_TotalAnnualVolumeInfo);

			AssertSettingPropertyRefreshesBinding<ZString>(
				prospect.PAP_RecurrenceTypeInfo,
				OrgTradeProspectRecurrenceTypeList.Codes.Yearly,
				OrgTradeProspectRecurrenceTypeList.Codes.Monthly,
				sales.OW_Calc_TotalAnnualWeightInfo);
		}

		#endregion

		#region PAP_RH_NKCommodityCode

		public void TestPAP_RH_NKCommodityCode_ShouldUpdateRequiresTempControlAndIsDangerous()
		{
			var hazardCommodity = Factory.New<RefCommodityCode>();
			hazardCommodity.RH_Code = "HAZ1";
			hazardCommodity.RH_IsHazardous = true;
			hazardCommodity.RH_IsFlammable = false;
			hazardCommodity.RH_IsPerishable = false;

			var flammableCommodity = Factory.New<RefCommodityCode>();
			flammableCommodity.RH_Code = "FLA";
			flammableCommodity.RH_IsHazardous = false;
			flammableCommodity.RH_IsFlammable = true;
			flammableCommodity.RH_IsPerishable = false;

			var perishableCommodity = Factory.New<RefCommodityCode>();
			perishableCommodity.RH_Code = "PER";
			perishableCommodity.RH_IsHazardous = false;
			perishableCommodity.RH_IsFlammable = false;
			perishableCommodity.RH_IsPerishable = true;

			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;

			prospect.PAP_IsDangerous = false;
			prospect.PAP_RequiresTemperatureControl = false;

			prospect.PAP_RH_NKCommodityCode = "HAZ1";
			AssertEquals(true, prospect.PAP_IsDangerous);
			AssertEquals(false, prospect.PAP_RequiresTemperatureControl);

			prospect.PAP_RH_NKCommodityCode = "PER";
			AssertEquals(false, prospect.PAP_IsDangerous);
			AssertEquals(true, prospect.PAP_RequiresTemperatureControl);

			prospect.PAP_RH_NKCommodityCode = "FLA";
			AssertEquals(true, prospect.PAP_IsDangerous);
			AssertEquals(false, prospect.PAP_RequiresTemperatureControl);
		}

		#endregion

		#region	PAP_RequiresPacking

		public void TestPAP_RequiresPacking_ClearContainerTypeWhenFalseForWarehouse()
		{
			var warehouse = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = warehouse.Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;

			prospect.PAP_RequiresPacking = true;
			prospect.PAP_RC_NKContainer = "XXX";

			prospect.PAP_RequiresPacking = false;
			AssertEquals("", prospect.PAP_RC_NKContainer);
		}

		#endregion

		#region PAP_IndustryVertical

		public void TestOverallIndustryVertical()
		{
			var orgAAA = Factory.New<OrgHeader>();
			orgAAA.MiscServ.OM_CMIndustryVertical = "AAA";
			var orgBBB = Factory.New<OrgHeader>();
			orgBBB.MiscServ.OM_CMIndustryVertical = "BBB";
			var orgCCC = Factory.New<OrgHeader>();
			orgCCC.MiscServ.OM_CMIndustryVertical = "CCC";

			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;

			prospect.IsIndustryVerticalOverridden = false;
			AssertEquals("", prospect.OverallIndustryVertical);

			sales.OW_OH_Supplier = orgAAA.PK;
			AssertEquals("AAA", prospect.OverallIndustryVertical);

			sales.OW_OH_Buyer = orgBBB.PK;
			AssertEquals("BBB", prospect.OverallIndustryVertical);

			sales.OW_OH_Primary = orgCCC.PK;
			AssertEquals("CCC", prospect.OverallIndustryVertical);

			orgCCC.MiscServ.OM_CMIndustryVertical = "DDD";
			AssertEquals("DDD", prospect.OverallIndustryVertical);

			prospect.IsIndustryVerticalOverridden = true;
			AssertEquals("", prospect.OverallIndustryVertical);

			prospect.PAP_IndustryVertical = "111";
			AssertEquals("111", prospect.OverallIndustryVertical);

			prospect.PAP_IndustryVertical = "222";
			AssertEquals("222", prospect.OverallIndustryVertical);

			prospect.IsIndustryVerticalOverridden = false;
			AssertEquals("", prospect.PAP_IndustryVertical);
		}

		public void TestOverallIndustryVertical_ReadOnly()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			var prospect = tradeDetail.ProspectDetail;

			prospect.IsIndustryVerticalOverridden = false;
			AssertEquals(true, prospect.OverallIndustryVerticalInfo.ReadOnly);

			prospect.IsIndustryVerticalOverridden = true;
			AssertEquals(false, prospect.OverallIndustryVerticalInfo.ReadOnly);
		}

		public void TestIsIndustryVerticalOverridden_OnLoaded()
		{
			var overriddenTradeDetail = Factory.NewWithValidTestData<OrgTradeDetail>();
			var overriddenProspect = overriddenTradeDetail.ProspectDetail;
			overriddenProspect.PAP_IndustryVertical = "XXX";

			var nonoverriddenTradeDetail = Factory.NewWithValidTestData<OrgTradeDetail>();
			var nonoverriddenProspect = nonoverriddenTradeDetail.ProspectDetail;
			nonoverriddenProspect.PAP_IndustryVertical = "";

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			AssertEquals(true, otherFactory.Load<OrgTradeProspect>(overriddenProspect.PK).IsIndustryVerticalOverridden);
			AssertEquals(false, otherFactory.Load<OrgTradeProspect>(nonoverriddenProspect.PK).IsIndustryVerticalOverridden);
		}

		#endregion

		#region PAS_PeriodOfActivity

		public void TestOverallPeriodOfActivity()
		{
			var orgAAA = Factory.New<OrgHeader>();
			orgAAA.MiscServ.OM_CMPeriodOfActivity = "AAA";
			var orgBBB = Factory.New<OrgHeader>();
			orgBBB.MiscServ.OM_CMPeriodOfActivity = "BBB";
			var orgCCC = Factory.New<OrgHeader>();
			orgCCC.MiscServ.OM_CMPeriodOfActivity = "CCC";

			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;

			prospect.IsPeriodOfActivityOverridden = false;
			AssertEquals("", prospect.OverallPeriodOfActivity);

			sales.OW_OH_Supplier = orgAAA.PK;
			AssertEquals("AAA", prospect.OverallPeriodOfActivity);

			sales.OW_OH_Buyer = orgBBB.PK;
			AssertEquals("BBB", prospect.OverallPeriodOfActivity);

			sales.OW_OH_Primary = orgCCC.PK;
			AssertEquals("CCC", prospect.OverallPeriodOfActivity);

			orgCCC.MiscServ.OM_CMPeriodOfActivity = "DDD";
			AssertEquals("DDD", prospect.OverallPeriodOfActivity);

			prospect.IsPeriodOfActivityOverridden = true;
			AssertEquals("", prospect.OverallPeriodOfActivity);

			prospect.PAP_PeriodOfActivity = "111";
			AssertEquals("111", prospect.OverallPeriodOfActivity);

			prospect.PAP_PeriodOfActivity = "222";
			AssertEquals("222", prospect.OverallPeriodOfActivity);

			prospect.IsPeriodOfActivityOverridden = false;
			AssertEquals("", prospect.PAP_PeriodOfActivity);
		}

		public void TestOverallPeriodOfActivity_ReadOnly()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			var prospect = tradeDetail.ProspectDetail;

			prospect.IsPeriodOfActivityOverridden = false;
			AssertEquals(true, prospect.OverallPeriodOfActivityInfo.ReadOnly);

			prospect.IsPeriodOfActivityOverridden = true;
			AssertEquals(false, prospect.OverallPeriodOfActivityInfo.ReadOnly);
		}

		public void TestIsPeriodOfActivityOverridden_OnLoaded()
		{
			var overriddenTradeDetail = Factory.NewWithValidTestData<OrgTradeDetail>();
			var overriddenProspect = overriddenTradeDetail.ProspectDetail;
			overriddenProspect.PAP_PeriodOfActivity = "XXX";
			var nonoverriddenTradeDetail = Factory.NewWithValidTestData<OrgTradeDetail>();
			var nonoverriddenProspect = nonoverriddenTradeDetail.ProspectDetail;
			nonoverriddenProspect.PAP_PeriodOfActivity = "";

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			AssertEquals(true, otherFactory.Load<OrgTradeProspect>(overriddenProspect.PK).IsPeriodOfActivityOverridden);
			AssertEquals(false, otherFactory.Load<OrgTradeProspect>(nonoverriddenProspect.PK).IsPeriodOfActivityOverridden);
		}

		#endregion

		public void TestSetProspectPeriodRange()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			var prospectDetail = tradeDetail.ProspectDetail;

			AssertEquals(ZDate.Empty, tradeDetail.ProspectPeriodStart);
			AssertEquals(ZDate.Empty, tradeDetail.ProspectPeriodEnd);

			prospectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			AssertEquals(ZDate.Empty, tradeDetail.ProspectPeriodStart);
			AssertEquals(ZDate.Empty, tradeDetail.ProspectPeriodEnd);

			prospectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 2, 9);
			AssertEquals(new ZDate(2018, 2, 1), tradeDetail.ProspectPeriodStart);
			AssertEquals(new ZDate(2019, 1, 1), tradeDetail.ProspectPeriodEnd);

			tradeDetail.ProspectPeriodEndType = "";
			prospectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			AssertEquals(new ZDate(2018, 2, 1), tradeDetail.ProspectPeriodStart);
			AssertEquals(new ZDate(2018, 2, 1), tradeDetail.ProspectPeriodEnd);

			tradeDetail.ProspectPeriodEndType = "";
			prospectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			AssertEquals(new ZDate(2018, 2, 1), tradeDetail.ProspectPeriodStart);
			AssertEquals(new ZDate(2019, 1, 1), tradeDetail.ProspectPeriodEnd);

			prospectDetail.PAP_ExpectedTradeStartDate = ZDate.Empty;
			AssertEquals(ZDate.Empty, tradeDetail.ProspectPeriodStart);
			AssertEquals(ZDate.Empty, tradeDetail.ProspectPeriodEnd);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var bizObj = (OrgTradeProspect)base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();

			bizObj.IsIndustryVerticalOverridden = true; // required to set OverallIndustryVertical property
			bizObj.IsPeriodOfActivityOverridden = true; // required to set OverallPeriodOfActivity property

			return bizObj;
		}

		void AssertSettingPropertyRefreshesBinding<T>(ZPropertyInfo propertyInfoToSet, T oldValue, T newValue, ZPropertyInfo expectedPropertyInfoToRefreshBinding) where T : IZType
		{
			var refreshed = false;
			propertyInfoToSet.Value = oldValue;
			expectedPropertyInfoToRefreshBinding.ValueChanged += (sender, e) => refreshed = true;
			propertyInfoToSet.Value = newValue;

			Assert($"Changing {propertyInfoToSet.Name} should have refreshed binding for {expectedPropertyInfoToRefreshBinding.Name}", refreshed);
		}
	}
}
