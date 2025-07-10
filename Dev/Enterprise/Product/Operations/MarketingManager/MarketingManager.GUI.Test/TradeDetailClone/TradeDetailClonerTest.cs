using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailCloner))]
	public class TradeDetailClonerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSelection()
		{
			var forwardingProduct = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sourceOpp = Factory.NewWithValidTestData<OrgOpportunity>();
			sourceOpp.P8_OH = org.PK;

			var tradeLane1 = org.SalesCollection.AddNew();
			tradeLane1.OW_MP_Product = forwardingProduct.PK;
			var detail1 = tradeLane1.TradeDetails.AddNew();
			detail1.PA_Status = Registry.Business.OpportunityTradeStatus.Codes.Unsuccessful;

			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = forwardingProduct.PK;
			var detail2 = tradeLane2.TradeDetails.AddNew();
			detail2.PA_Status = Registry.Business.OpportunityTradeStatus.Codes.Successful;

			sourceOpp.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			sourceOpp.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
			sourceOpp.AssociatedTradeLanesPivots.AddPivotFor(detail1);
			sourceOpp.AssociatedTradeLanesPivots.AddPivotFor(detail2);

			Factory.Save();

			var targetOpp = Factory.NewWithValidTestData<OrgOpportunity>();
			targetOpp.P8_OH = org.PK;

			var cloner = new TradeDetailCloner(sourceOpp, targetOpp);

			AssertEquals(false, cloner.SourceTradeDetails[0].Selected);
			AssertEquals(false, cloner.SourceTradeDetails[1].Selected);

			cloner.SelectAll();
			AssertEquals(true, cloner.SourceTradeDetails[0].Selected);
			AssertEquals(true, cloner.SourceTradeDetails[1].Selected);

			cloner.UnselectAll();
			AssertEquals(false, cloner.SourceTradeDetails[0].Selected);
			AssertEquals(false, cloner.SourceTradeDetails[1].Selected);

			cloner.SelectAll();
			cloner.SelectUnsuccessful();
			AssertEquals(true, cloner.SourceTradeDetails.Cast<TradeDetailCloneItem>().Single(x => x.TradeDetail.IsUnsuccessful).Selected);
			AssertEquals(false, cloner.SourceTradeDetails.Cast<TradeDetailCloneItem>().Single(x => !x.TradeDetail.IsUnsuccessful).Selected);
		}

		public void TestCopySelectionToTarget()
		{
			var forwardingProduct = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sourceOpp = Factory.NewWithValidTestData<OrgOpportunity>();
			sourceOpp.P8_OH = org.PK;

			var tradeLane1 = org.SalesCollection.AddNew();
			tradeLane1.OW_MP_Product = forwardingProduct.PK;
			var detail1 = tradeLane1.TradeDetails.AddNew();

			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = forwardingProduct.PK;
			var detail2 = tradeLane2.TradeDetails.AddNew();

			detail2.PA_TradeMode = "SEA";
			detail2.PA_TradeType = "FCL";
			detail2.PA_Status = Registry.Business.OpportunityTradeStatus.Codes.Successful;
			detail2.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			detail2.ProspectDetail.PAP_RC_NKContainer = "20GP";
			detail2.CurrentProspectPeriod.PAS_Units = 2;
			detail2.CurrentProspectPeriod.PAS_Weight = 10m;
			detail2.CurrentProspectPeriod.PAS_WeightUQ = "KG";
			detail2.CurrentProspectPeriod.PAS_Volume = 4m;
			detail2.CurrentProspectPeriod.PAS_VolumeUQ = "M3";
			detail2.CurrentProspectPeriod.PAS_RateOffered = 500m;
			detail2.CurrentProspectPeriod.PAS_RepeatsMnth = 2;
			detail2.CurrentProspectPeriod.PAS_EstimatedProfit = 1000m;
			detail2.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";

			sourceOpp.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			sourceOpp.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
			sourceOpp.AssociatedTradeLanesPivots.AddPivotFor(detail1);
			sourceOpp.AssociatedTradeLanesPivots.AddPivotFor(detail2);

			Factory.Save();

			var targetOpp = Factory.NewWithValidTestData<OrgOpportunity>();
			targetOpp.P8_OH = org.PK;

			var cloner = new TradeDetailCloner(sourceOpp, targetOpp);
			targetOpp.OnCopyOrgOpportunity(sourceOpp);

			var detailItem1 = cloner.SourceTradeDetails.Cast<TradeDetailSelectionItem>().Single(x => x.TradeDetail.PK == detail1.PK);
			var detailItem2 = cloner.SourceTradeDetails.Cast<TradeDetailSelectionItem>().Single(x => x.TradeDetail.PK == detail2.PK);

			detailItem1.Selected = false;
			detailItem2.Selected = true;

			cloner.CopySelectionToTarget();

			AssertEquals(1, targetOpp.ProspectiveSalesHeaderCollection.Count);
			AssertEquals(1, ((SalesHeader)targetOpp.ProspectiveSalesHeaderCollection[0]).EntitySalesCollectionProductView.Count);

			var targetSales = ((SalesHeader)targetOpp.ProspectiveSalesHeaderCollection[0]).EntitySalesCollectionProductView[0];
			AssertEquals("Reuse existing trade lane", true, targetSales.IsInDatabase);
			AssertEquals("Reuse existing trade lane", tradeLane2.PK, targetSales.PK);
			AssertEquals("Should only contain the copied trade detail", 1, targetSales.EntityTradeDetailsCollection.Count);

			var targetDetail = targetSales.EntityTradeDetailsCollection[0];
			AssertEquals("Copied trade detail is not save yet", false, targetDetail.IsInDatabase);
			AssertNotEquals("Copied trade detail has different pk", detail2.PK, targetDetail.PK);
			AssertEquals("Copied trade detail has default status", Registry.Business.OpportunityTradeStatus.Codes.Active, targetDetail.PA_Status);
			AssertEquals("Should set the flag", true, targetOpp.HasUnsavedCopiedOrgTradePeriods);
			var copiedOrgTradeDetails = typeof(OrgOpportunity).GetField("CopiedOrgTradeDetails", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(targetOpp);
			AssertEquals(true, ((List<OrgTradeDetail>)copiedOrgTradeDetails).Any());

			Factory.Save();
			AssertEquals("Copied trade detail has associations via trade lane to both source and target opportunity", 2, targetDetail.SalesAssociationPivotCollectionGlobal.Count);
			AssertEquals("Original base trade detail only has association to source opportunity", 1, detail2.SalesAssociationPivotCollectionGlobal.Count);
			var detailWrapper = EntityTradeDetailWrapper.Get(detail2, sourceOpp);
			AssertEquals("Original wrapped trade detail has association to both opportunities", 2, detailWrapper.SalesAssociationPivotCollectionGlobal.Count);
			AssertEquals("The flag shuold be cleared after save", false, targetOpp.HasUnsavedCopiedOrgTradePeriods);
			AssertEquals(false, ((List<OrgTradeDetail>)copiedOrgTradeDetails).Any());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var sourceOpp = Factory.New<OrgOpportunity>();
			var targetOpp = Factory.New<OrgOpportunity>();
			return new TradeDetailCloner(sourceOpp, targetOpp);
		}
	}
}
