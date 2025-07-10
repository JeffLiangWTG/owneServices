using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTradeDetail))]
	sealed class OrgTradeDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSupportNotes()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			AssertEquals(false, tradeDetail.SupportsNotes);
		}

		#region Prospect

		public void TestProspectDetail()
		{
			var prospectSales = Factory.New<OrgSales>();
			prospectSales.OW_IsTraded = false;

			var prospectDetail = prospectSales.TradeDetails.AddNew();
			AssertNotNull(prospectDetail.ProspectDetail);

			var actualSales = Factory.New<OrgSales>();
			actualSales.OW_IsTraded = true;

			var actualDetail = actualSales.TradeDetails.AddNew();
			AssertNull(actualDetail.ProspectDetail);
		}

		public void TestCurrentProspectPeriod()
		{
			var prospectSales = Factory.New<OrgSales>();
			prospectSales.OW_IsTraded = false;

			var prospectDetail = prospectSales.TradeDetails.AddNew();
			AssertNotNull(prospectDetail.CurrentProspectPeriod);

			var actualSales = Factory.New<OrgSales>();
			actualSales.OW_IsTraded = true;

			var actualDetail = actualSales.TradeDetails.AddNew();
			AssertNull(actualDetail.CurrentProspectPeriod);
		}

		public void TestUpdateProspectPeriods()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_IsTraded = false;

			var detail = sales.TradeDetails.AddNew();
			AssertEquals(0, detail.ProspectPeriods.Count);

			detail.ProspectPeriodStart = new ZDate(2018, 1, 1);
			detail.ProspectPeriodEnd = new ZDate(2018, 3, 1);
			AssertEquals(12, detail.ProspectPeriods.Count);
			AssertEquals(3, detail.ProspectPeriods.Count(x => !x.PAS_IsForecast));
			AssertEquals(9, detail.ProspectPeriods.Count(x => x.PAS_IsForecast));
			var periods = detail.ProspectPeriods.Where(x => !x.PAS_IsForecast).OrderBy(x => x.PAS_Period).ToList();
			AssertEquals(new ZDate(2018, 1, 1), periods[0].PAS_Period);
			AssertEquals(new ZDate(2018, 2, 1), periods[1].PAS_Period);
			AssertEquals(new ZDate(2018, 3, 1), periods[2].PAS_Period);
			AssertEquals("STA", detail.ProspectDetail.PAP_ForecastType);
			AssertEquals(new ZDate(2019, 1, 1), detail.ProspectDetail.PAP_ExpiryDate);

			detail.ProspectPeriodStart = new ZDate(2018, 2, 1);
			AssertEquals(12, detail.ProspectPeriods.Count);
			AssertEquals(2, detail.ProspectPeriods.Count(x => !x.PAS_IsForecast));
			AssertEquals(10, detail.ProspectPeriods.Count(x => x.PAS_IsForecast));
			periods = detail.ProspectPeriods.Where(x => !x.PAS_IsForecast).OrderBy(x => x.PAS_Period).ToList();
			AssertEquals(new ZDate(2018, 2, 1), periods[0].PAS_Period);
			AssertEquals(new ZDate(2018, 3, 1), periods[1].PAS_Period);
			AssertEquals("STA", detail.ProspectDetail.PAP_ForecastType);
			AssertEquals(new ZDate(2019, 2, 1), detail.ProspectDetail.PAP_ExpiryDate);

			detail.ProspectPeriodStart = new ZDate(2017, 12, 1);
			AssertEquals(12, detail.ProspectPeriods.Count);
			AssertEquals(4, detail.ProspectPeriods.Count(x => !x.PAS_IsForecast));
			AssertEquals(8, detail.ProspectPeriods.Count(x => x.PAS_IsForecast));
			periods = detail.ProspectPeriods.Where(x => !x.PAS_IsForecast).OrderBy(x => x.PAS_Period).ToList();
			AssertEquals(new ZDate(2017, 12, 1), periods[0].PAS_Period);
			AssertEquals(new ZDate(2018, 1, 1), periods[1].PAS_Period);
			AssertEquals(new ZDate(2018, 2, 1), periods[2].PAS_Period);
			AssertEquals(new ZDate(2018, 3, 1), periods[3].PAS_Period);
			AssertEquals("STA", detail.ProspectDetail.PAP_ForecastType);
			AssertEquals(new ZDate(2018, 12, 1), detail.ProspectDetail.PAP_ExpiryDate);

			detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			detail.ProspectPeriodStart = new ZDate(2018, 10, 1);
			detail.ProspectPeriodEnd = new ZDate(2018, 10, 1);
			AssertEquals(1, detail.ProspectPeriods.Count);
			AssertEquals(ZString.Empty, detail.ProspectDetail.PAP_ForecastType);
			AssertEquals(ZDate.Empty, detail.ProspectDetail.PAP_ExpiryDate);
		}

		public void TestSetDefaultProspectPeriodEndType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var sales = Factory.New<OrgSales>();
			sales.OW_IsTraded = false;
			sales.OW_OH_Primary = org.PK;

			var detail = sales.TradeDetails.AddNew();
			detail.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter;
			detail.ProspectPeriodStart = new ZDate(2018, 1, 1);
			detail.ProspectPeriodEnd = new ZDate(2018, 2, 1);

			Factory.Save();

			var loadedDetail = new BusinessObjectFactory().Load<OrgTradeDetail>(detail.PK);
			AssertEquals(new ZDate(2018, 1, 1), loadedDetail.ProspectPeriodStart);
			AssertEquals(new ZDate(2018, 2, 1), loadedDetail.ProspectPeriodEnd);
			AssertEquals(OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter, loadedDetail.ProspectPeriodEndType);

			detail.ProspectPeriodStart = new ZDate(2018, 1, 2);
			detail.ProspectPeriodEnd = new ZDate(2018, 3, 15);

			Factory.Save();

			loadedDetail = new BusinessObjectFactory().Load<OrgTradeDetail>(detail.PK);
			AssertEquals(new ZDate(2018, 1, 1), loadedDetail.ProspectPeriodStart);
			AssertEquals(new ZDate(2018, 3, 1), loadedDetail.ProspectPeriodEnd);
			AssertEquals(OrgTradeProspectPeriodEndTypeList.Codes._3Months, loadedDetail.ProspectPeriodEndType);
		}

		#endregion

		#region Prospect Expiry

		public void TestSetProspectExpiry()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			sales.OW_IsTraded = false;

			var detail = sales.TradeDetails.AddNew();
			detail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			detail.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._12Months;
			detail.ProspectPeriodStart = new ZDate(2018, 3, 1);
			detail.ProspectPeriodEnd = new ZDate(2019, 2, 1);

			Factory.Save();

			AssertEquals("Pre-condition", ZDate.Empty, detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("Pre-condition", ZString.Empty, detail.ProspectDetail.PAP_ExpiryReason);

			detail.SetProspectExpiry(new ZDate(2018, 6, 22), "MAN");
			AssertEquals("Expiry info has been updated", new ZDate(2018, 6, 22), detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("Expiry info has been updated", "MAN", detail.ProspectDetail.PAP_ExpiryReason);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 3, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 4, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 5, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 6, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 7, 1), true);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 8, 1), true);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 9, 1), true);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 10, 1), true);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 11, 1), true);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 12, 1), true);
			AssertProspectPeriodExpiry(detail, new ZDate(2019, 1, 1), true);
			AssertProspectPeriodExpiry(detail, new ZDate(2019, 2, 1), true);

			detail.SetProspectExpiry(new ZDate(2018, 5, 1), OrgTradeProspectExpiryReasonList.Codes.Superseded);
			AssertEquals("Expiry info has been updated", new ZDate(2018, 5, 1), detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("Expiry info has been updated", OrgTradeProspectExpiryReasonList.Codes.Superseded, detail.ProspectDetail.PAP_ExpiryReason);

			detail.SetProspectExpiry(new ZDate(2018, 8, 15), "AAA");
			AssertEquals("System expiry info cannot be overridden", new ZDate(2018, 5, 1), detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("System expiry info cannot be overridden", OrgTradeProspectExpiryReasonList.Codes.Superseded, detail.ProspectDetail.PAP_ExpiryReason);
		}

		public void TestUndoProspectExpiry()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			sales.OW_IsTraded = false;
			var detail = sales.TradeDetails.AddNew();

			detail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			detail.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._6Months;
			detail.ProspectPeriodStart = new ZDate(2018, 3, 1);
			detail.ProspectPeriodEnd = new ZDate(2018, 9, 1);
			detail.SetProspectExpiry(new ZDate(2018, 6, 22), OrgTradeProspectExpiryReasonList.Codes.Lost);

			Factory.Save();

			AssertEquals("Expiry undo is not allowed", false, detail.ProspectDetail.AllowManualExpiry);
			AssertEquals("Pre-condition", new ZDate(2018, 6, 22), detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("Pre-condition", OrgTradeProspectExpiryReasonList.Codes.Lost, detail.ProspectDetail.PAP_ExpiryReason);

			detail.UndoProspectExpiry();

			AssertEquals("No change as system expiry does not allow undo", new ZDate(2018, 6, 22), detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("No change as system expiry does not allow undo", OrgTradeProspectExpiryReasonList.Codes.Lost, detail.ProspectDetail.PAP_ExpiryReason);

			detail.ProspectDetail.PAP_ExpiryReason = "MAN";
			AssertEquals("Expiry undo is allowed", true, detail.ProspectDetail.AllowManualExpiry);
			detail.UndoProspectExpiry();
			AssertEquals("Expiry info has been updated", ZDate.Empty, detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("Expiry info has been updated", ZString.Empty, detail.ProspectDetail.PAP_ExpiryReason);

			AssertProspectPeriodExpiry(detail, new ZDate(2018, 3, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 4, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 5, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 6, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 7, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 8, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 9, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 10, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 11, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2018, 12, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2019, 1, 1), false);
			AssertProspectPeriodExpiry(detail, new ZDate(2019, 2, 1), false);
		}

		void AssertProspectPeriodExpiry(OrgTradeDetail detail, ZDate period, bool expectedExpiry)
		{
			AssertEquals($"Period {period}", expectedExpiry, detail.ProspectPeriods.Single(x => x.PAS_Period == period).PAS_IsExpired);
		}

		#endregion

		#region PA_TradeType

		public void TestShouldNotDefaultTradeTypeForCustomsBrokerage()
		{
			var brokerageSales = Factory.New<OrgSales>();
			brokerageSales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage).Identifier;
			var detail = brokerageSales.TradeDetails.AddNew();
			detail.PA_TradeType = OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Export;

			detail.PA_TradeMode = Constants.TransportModes.Sea;
			AssertEquals("Trade type should remain unchanged", OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Export, detail.PA_TradeType);

			detail.ProspectDetail.PAP_RC_NKContainer = "XXX";
			AssertEquals("Trade type should remain unchanged", OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Export, detail.PA_TradeType);
		}

		#endregion

		#region PA_TradeMode

		public void TestPA_TradeMode_DefaultValues()
		{
			var transportSales = Factory.New<OrgSales>();
			transportSales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport).Identifier;
			var forwardingSales = Factory.New<OrgSales>();
			forwardingSales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var otherSales = Factory.New<OrgSales>();
			otherSales.OW_MP_Product = Factory.New<IOrgSalesProduct>().Identifier;

			AssertEquals(OrgTradeDetailLookups.TransportTradeModes.TransportBooking, transportSales.TradeDetails.AddNew().PA_TradeMode);
			AssertEquals("", forwardingSales.TradeDetails.AddNew().PA_TradeMode);
			AssertEquals("", otherSales.TradeDetails.AddNew().PA_TradeMode);
		}

		public void TestPA_TradeMode_ReadOnly()
		{
			var sales = Factory.New<OrgSales>();
			var details = sales.TradeDetails.AddNew();
			AssertEquals(false, details.PA_TradeModeInfo.ReadOnly);

			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport).Identifier;
			AssertEquals(true, details.PA_TradeModeInfo.ReadOnly);

			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage).Identifier;
			AssertEquals(false, details.PA_TradeModeInfo.ReadOnly);
		}

		#endregion

		#region PA_Status

		public void TestPA_Status()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			AssertEquals(OpportunityTradeStatus.Codes.Active, tradeDetail.PA_Status);
		}

		#endregion

		#region PA_Calc_AnnualCount

		public void TestPA_Calc_AnnualCount()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			period.PAS_RepeatsMnth = 2;
			AssertEquals(2m * 12, tradeDetail.PA_Calc_AnnualCount);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			period.PAS_RepeatsMnth = 5;
			AssertEquals(5m, tradeDetail.PA_Calc_AnnualCount);

			period.PAS_RepeatsMnth = 0;
			AssertEquals(0m, tradeDetail.PA_Calc_AnnualCount);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			period.PAS_RepeatsMnth = 3;
			AssertEquals(3 * 52m, tradeDetail.PA_Calc_AnnualCount);
		}

		#endregion

		#region PA_Calc_AnnualChargeable

		public void TestPA_Calc_AnnualChargeable()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			period.PAS_Chargeable = 2;
			AssertEquals(2m * 12, tradeDetail.PA_Calc_AnnualChargeable);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			period.PAS_Chargeable = 5;
			AssertEquals(5m, tradeDetail.PA_Calc_AnnualChargeable);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			AssertEquals(5m * 52m, tradeDetail.PA_Calc_AnnualChargeable);
		}

		#endregion

		#region PA_Calc_AnnualTEU

		public void TestPA_Calc_AnnualTEU()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			period.PAS_TEUQuantity = 2;
			AssertEquals(2m * 12, tradeDetail.PA_Calc_AnnualTEU);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			period.PAS_TEUQuantity = 5;
			AssertEquals(5m, tradeDetail.PA_Calc_AnnualTEU);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			period.PAS_TEUQuantity = 5;
			AssertEquals(5m * 52m, tradeDetail.PA_Calc_AnnualTEU);
		}

		public void TestPA_CalcWhenCurrentProspectPeriodIsNull()
		{
			var orgSales = Factory.NewWithValidTestData<OrgSales>();
			orgSales.OW_IsTraded = true;

			var tradeDetail = Factory.New<OrgTradeDetail>();
			tradeDetail.PA_OW = orgSales.PK;
			Factory.Save();

			AssertEquals(0m, tradeDetail.PA_Calc_AnnualTEU);
			AssertEquals(0m, tradeDetail.PA_Calc_AnnualPalletCount);
			AssertEquals(0m, tradeDetail.PA_Calc_AnnualChargeable);
			AssertEquals(0m, tradeDetail.PA_Calc_AnnualVolume);
			AssertEquals(0m, tradeDetail.PA_Calc_AnnualWeight);
			AssertEquals(0m, tradeDetail.PA_Calc_EstimatedPipelineAnnualTEUQuantity);
			AssertEquals(0m, tradeDetail.PA_Calc_EstimatedPipelineAnnualValue);
			AssertEquals(0m, tradeDetail.PA_Calc_EstimatedPipelineMonthlyValue);
			Assert(!tradeDetail.ChargeableUQIsMetric);
		}

		#endregion

		#region PA_Calc_AnnualPalletCount

		public void TestPA_Calc_AnnualPalletCount()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			period.PAS_PalletCount = 2;
			AssertEquals(2m * 12, tradeDetail.PA_Calc_AnnualPalletCount);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			period.PAS_PalletCount = 5;
			AssertEquals(5m, tradeDetail.PA_Calc_AnnualPalletCount);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			period.PAS_PalletCount = 5;
			AssertEquals(5m * 52m, tradeDetail.PA_Calc_AnnualPalletCount);
		}

		#endregion

		#region PA_Calc_EstimatedAnnualValue

		public void TestPA_Calc_EstimatedAnnualValue()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			period.PAS_RepeatsMnth = 2;
			period.PAS_EstimatedProfit = 20m;

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			AssertEquals(40m, tradeDetail.PA_Calc_EstimatedAnnualValue);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			AssertEquals(480m, tradeDetail.PA_Calc_EstimatedAnnualValue);

			prospect.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			AssertEquals(2 * 20m * 52, tradeDetail.PA_Calc_EstimatedAnnualValue);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			var actualSales = Factory.New<OrgSales>();
			actualSales.OW_IsTraded = false;

			var tradeDetail = actualSales.TradeDetails.AddNew();
			var prospect = tradeDetail.ProspectDetail;
			var period = tradeDetail.CurrentProspectPeriod;

			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail);
			var opportunity2 = Factory.New<OrgOpportunity>();
			opportunity2.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail);

			AssertEquals("Precondition", 1, opportunity.AssociatedTradeLanesPivots.Count);
			AssertEquals("Precondition", 1, opportunity2.AssociatedTradeLanesPivots.Count);

			tradeDetail.Delete();

			AssertEquals(0, opportunity.AssociatedTradeLanesPivots.Count);
			AssertEquals(0, opportunity2.AssociatedTradeLanesPivots.Count);
			Assert(prospect.IsDeleted);
			Assert(period.IsDeleted);
		}

		#endregion

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldTradeProfileValue = Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed;

			try
			{
				OrgSales testTradeLane = OrgInDB.SalesCollection.AddNew();
				OrgTradeDetail testDetail = testTradeLane.TradeDetails.AddNew();

				Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = true;
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_Calc_DestinationInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_Calc_OH_BuyerInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_Calc_OH_SupplierInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_Calc_OriginInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testDetail.PA_OWInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testDetail.PA_StatusInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testDetail.PA_TradeTypeInfo.ReadOnly);

				Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_Calc_DestinationInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_Calc_OH_BuyerInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_Calc_OH_SupplierInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_Calc_OriginInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_OWInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_StatusInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetail.PA_TradeTypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = oldTradeProfileValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region Prospect Period Start / End

		public void TestShouldResetProspectPeriodStartAndPeriodEndAfterSave()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			sales.OW_IsTraded = false;
			sales.OW_OH_Primary = org.PK;

			var detail = sales.TradeDetails.AddNew();
			detail.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter;
			AssertEquals(true, detail.ProspectPeriodStart.IsEmpty);
			AssertEquals(true, detail.ProspectPeriodEnd.IsEmpty);

			var p1 = detail.ProspectPeriods.AddNew();
			var p2 = detail.ProspectPeriods.AddNew();

			p1.PAS_Period = new ZDate(2018, 1, 1);
			p2.PAS_Period = new ZDate(2018, 2, 1);
			p1.PAS_OH_Client = org.PK;
			p2.PAS_OH_Client = org.PK;

			Factory.Save();

			AssertEquals(new ZDate(2018, 1, 1), detail.ProspectPeriodStart);
			AssertEquals(new ZDate(2018, 2, 1), detail.ProspectPeriodEnd);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgSales sales = Factory.NewWithValidTestData<OrgHeader>().SalesCollection.AddNew();
			return new OrgTradeDetailCollection(sales).AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			var sales = factory.NewWithValidTestData<OrgHeader>().SalesCollection.AddNew();
			sales.OW_IsTraded = true;
			var detail = new OrgTradeDetailCollection(sales).AddNew();
			var period = detail.TradedPeriods.AddNew();
			period.PAS_IsTraded = true;
			period.PAS_OH_Client = org.PK;

			return detail;
		}

		#endregion
	}
}
