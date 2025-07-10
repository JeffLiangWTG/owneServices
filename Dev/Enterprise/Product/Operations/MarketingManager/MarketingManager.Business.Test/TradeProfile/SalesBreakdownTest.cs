using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesBreakdown))]
	sealed class SalesBreakdownTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShowRevenueOption()
		{
			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var transportProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader1 = salesHeaderCollection.AddNew(forwardingProduct);
			salesHeader1.EntitySalesCollectionProductView.AddNew();

			var salesHeader2 = salesHeaderCollection.AddNew(transportProduct);
			salesHeader2.EntitySalesCollectionProductView.AddNew();

			var salesBreakdown = new SalesBreakdown(org);
			AssertEquals(true, salesBreakdown.ShowFinancialYearRevenue);

			salesBreakdown.ShowPerAnnumRevenue = true;
			AssertEquals(SalesHeader.RevenueDisplay.PerAnnum, salesHeader1.RevenueDisplayOption);
			AssertEquals(SalesHeader.RevenueDisplay.PerAnnum, salesHeader2.RevenueDisplayOption);

			salesBreakdown.ShowFinancialYearRevenue = true;
			AssertEquals(SalesHeader.RevenueDisplay.FinancialYear, salesHeader1.RevenueDisplayOption);
			AssertEquals(SalesHeader.RevenueDisplay.FinancialYear, salesHeader2.RevenueDisplayOption);
		}

		public void TestCompanyFilter()
		{
			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var transportProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader1 = salesHeaderCollection.AddNew(forwardingProduct);
			salesHeader1.EntitySalesCollectionProductView.AddNew();

			var salesHeader2 = salesHeaderCollection.AddNew(transportProduct);
			salesHeader2.EntitySalesCollectionProductView.AddNew();

			var salesBreakdown = new SalesBreakdown(org);
			AssertEquals(Env.CurrentCompanyPK, salesBreakdown.CompanyFilter);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			salesBreakdown.CompanyFilter = company.PK;

			AssertEquals(company.PK, salesBreakdown.CompanyFilter);
			AssertEquals(company.PK, salesHeader1.CompanyFilter);
			AssertEquals(company.PK, salesHeader2.CompanyFilter);
		}

		#region Default Values

		public void TestDefaultValues()
		{
			var org = Factory.New<OrgHeader>();
			var salesBreakdown = new SalesBreakdown(org);
			AssertEquals(true, salesBreakdown.ShowAllRevenue);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			return new SalesBreakdown(org);
		}

		#endregion

		#region ReadOnly

		public void TestReadOnly()
		{
			var org = Factory.New<OrgHeader>();
			var salesBreakdown = new SalesBreakdown(org);
			AssertEquals(false, org.ReadOnly);
			AssertEquals(false, salesBreakdown.ReadOnly);

			org = Factory.New<OrgHeader>();
			org.SetReadOnlyIncludingChildren(true);
			salesBreakdown = new SalesBreakdown(org);
			AssertEquals(true, org.ReadOnly);
			AssertEquals(true, salesBreakdown.ReadOnly);
		}

		#endregion
	}
}
