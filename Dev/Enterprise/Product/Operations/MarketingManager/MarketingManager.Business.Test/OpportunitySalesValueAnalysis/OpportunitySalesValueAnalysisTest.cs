
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OpportunitySalesValueAnalysis))]
	sealed class OpportunitySalesValueAnalysisTest : NonPersistentBusinessObjectTestCase
	{
		public void TestChildEditableChildObject_SalesHeader()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, salesProduct);
			var analysis = new OpportunitySalesValueAnalysis(salesHeader);
			AssertEquals("Should be a child so that notifications appear", true, analysis.IsRegisteredEditableChildObject(salesHeader));
		}

		public void TestChildEditableChildObject_OrgOpportunityValue()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var orgOpportunityValue = opportunity.ValueItems.AddNew();
			var analysis = new OpportunitySalesValueAnalysis(orgOpportunityValue);
			AssertEquals("Should be a child so that notifications appear", true, analysis.IsRegisteredEditableChildObject(orgOpportunityValue));
		}

		public void TestTotalCurrencyCode_SalesHeader()
		{
			var org = Factory.New<OrgHeader>();
			var product = Factory.New<OrgSalesProduct>();
			var salesHeader = new SalesHeader(org, product);
			var analysis = new OpportunitySalesValueAnalysis(salesHeader);

			AssertEquals(salesHeader.TotalCurrencyCode, analysis.TotalCurrencyCode);
		}

		public void TestTotalCurrencyCode_OpprtunityValue()
		{
			var opp = Factory.New<OrgOpportunity>();
			opp.P8_RX_NKEstimatedValueCurrency = "USD";
			var opportunityValue = opp.ValueItems.AddNew();
			var analysis = new OpportunitySalesValueAnalysis(opportunityValue);
			AssertEquals("USD", analysis.TotalCurrencyCode);

			opportunityValue.PV_P8 = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, analysis.TotalCurrencyCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, salesProduct);
			return new OpportunitySalesValueAnalysis(salesHeader);
		}

		public void TestTotalAnnualMetricVolume_SalesHeader()
		{
			var org = Factory.New<OrgHeader>();
			var product = Factory.New<OrgSalesProduct>();
			var salesHeader = new SalesHeader(org, product);
			var analysis = new OpportunitySalesValueAnalysis(salesHeader);

			AssertEquals(salesHeader.TotalAnnualMetricVolume, analysis.TotalAnnualMetricVolume);
		}
	}
}
