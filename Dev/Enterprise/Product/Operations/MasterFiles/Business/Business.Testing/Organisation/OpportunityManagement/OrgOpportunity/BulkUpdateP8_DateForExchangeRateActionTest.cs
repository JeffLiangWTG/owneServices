using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BulkUpdateP8_DateForExchangeRateAction))]
	sealed class BulkUpdateP8_DateForExchangeRateActionTest : NonPersistentBusinessObjectTestCase
	{
		#region Execute

		public void TestExecute()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			opp1.P8_RX_NKEstimatedValueCurrency = "AUD";
			var legacyValue1A = opp1.ValueItems.AddNew();
			legacyValue1A.PV_Value = 10;

			var opp2 = org.SalesOpportunities.AddNew();
			opp2.P8_RX_NKEstimatedValueCurrency = "USD";
			var legacyValue2A = opp2.ValueItems.AddNew();
			legacyValue2A.PV_Value = 10;

			var shipmentProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = shipmentProduct.Identifier;
			sales1.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			sales1.SalesAssociationPivotCollectionGlobal.AddNew(opp2);
			var detail1 = sales1.TradeDetails.AddNew();
			detail1.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail1.CurrentProspectPeriod.PAS_EstimatedProfit = 100;
			detail1.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			detail1.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			detail1.SalesAssociationPivotCollectionGlobal.AddNew(opp2);

			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = shipmentProduct.Identifier;
			sales2.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			sales2.SalesAssociationPivotCollectionGlobal.AddNew(opp2);
			var detail2 = sales2.TradeDetails.AddNew();
			detail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			detail2.CurrentProspectPeriod.PAS_EstimatedProfit = 1000;
			detail2.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";
			detail2.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			detail2.SalesAssociationPivotCollectionGlobal.AddNew(opp2);

			opp1.P8_GC = GlbCompany.CurrentCompany.PK;
			opp1.P8_DateForExchangeRate = new ZDate(2002, 1, 10);
			opp2.P8_GC = GlbCompany.CurrentCompany.PK;
			opp2.P8_DateForExchangeRate = new ZDate(2002, 1, 10);

			AddSellExchangeRateForTest("USD", 0.5, new ZDateTime(2002, 2, 2), new ZDateTime(2002, 3, 2));

			Factory.Save();

			var bulkUpdater = new BulkUpdateP8_DateForExchangeRateAction(new[] { opp1, opp2 });
			bulkUpdater.Date = new ZDateTime(2002, 2, 10);
			bulkUpdater.Execute(null);

			var newFactory = new BusinessObjectFactory();
			var opp1InNewFactory = newFactory.Load<OrgOpportunity>(opp1.PK);
			var opp2InNewFactory = newFactory.Load<OrgOpportunity>(opp2.PK);
			CombineAssertions("Should have updated P8_DateForExchangeRate, and P8_EstimatedValue", () =>
			{
				AssertEquals("opp1.P8_EstimatedValue", (10m * 12) + 1200m + (12000m * 2), opp1InNewFactory.P8_EstimatedValue);
				AssertEquals("opp1.P8_DateForExchangeRate", new ZDateTime(2002, 2, 10), opp1InNewFactory.P8_DateForExchangeRate);

				AssertEquals("opp2.P8_EstimatedValue", (10m * 12) + (1200m / 2) + 12000m, opp2InNewFactory.P8_EstimatedValue);
				AssertEquals("opp2.P8_DateForExchangeRate", new ZDateTime(2002, 2, 10), opp2InNewFactory.P8_DateForExchangeRate);
			});
		}

		#endregion

		#region Cancel

		public void TestCancel()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OH = org.PK;
			var legacyValue1A = opp1.ValueItems.AddNew();
			legacyValue1A.PV_Value = 10;

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OH = org.PK;
			var legacyValue2A = opp2.ValueItems.AddNew();
			legacyValue2A.PV_Value = 10;

			var oldDate = new ZDate(2002, 1, 10);
			opp1.P8_DateForExchangeRate = oldDate;
			opp2.P8_DateForExchangeRate = oldDate;

			Factory.Save();

			var bulkUpdater = new BulkUpdateP8_DateForExchangeRateAction(new[] { opp1, opp2 });
			bulkUpdater.Date = new ZDateTime(2002, 2, 10);

			var progressWithCancelAfterFirstProcess = new ZArchitecture.Core.Progress((status, percent) => bulkUpdater.Cancel());
			bulkUpdater.Execute(progressWithCancelAfterFirstProcess);

			AssertEquals(true, bulkUpdater.Cancelled);

			var newFactory = new BusinessObjectFactory();
			var opp1InNewFactory = newFactory.Load<OrgOpportunity>(opp1.PK);
			var opp2InNewFactory = newFactory.Load<OrgOpportunity>(opp2.PK);
			CombineAssertions("Should not have updated P8_DateForExchangeRate", () =>
			{
				AssertEquals("opp1.P8_DateForExchangeRate", oldDate, opp1InNewFactory.P8_DateForExchangeRate);
				AssertEquals("opp2.P8_DateForExchangeRate", oldDate, opp2InNewFactory.P8_DateForExchangeRate);
			});
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var opportunity1 = Factory.New<OrgOpportunity>();
			var opportunity2 = Factory.New<OrgOpportunity>();
			return new BulkUpdateP8_DateForExchangeRateAction(new[] { opportunity1, opportunity2 });
		}

		#endregion

		#region Implementation

		void AddSellExchangeRateForTest(ZString currency, ZDecimal rate, ZDateTime startDate, ZDateTime endDate)
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_RX_NKExCurrency = currency;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate.RE_StartDate = startDate;
			exchangeRate.RE_ExpiryDate = endDate;
			exchangeRate.RE_SellRate = rate;
		}

		#endregion
	}
}
