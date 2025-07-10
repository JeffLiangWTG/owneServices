using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefExchangeRateInOrgHeaderDependentCollection))]
	sealed class RefExchangeRateInOrgHeaderDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<RefExchangeRateInOrgHeaderDependentCollection>
	{
		public void TestDefaultValuesForNewChild()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ZUB";
			var item = currency.ExchangeRates.AddNew();
			AssertEquals(1m, item.RE_SellRate);
			AssertEquals("SEL", item.RE_ExRateType);
			AssertEquals(ZGuid.Empty, item.RE_OH_Client);
		}

		public void TestExchangeRateCollectionForRelatedOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var rate1 = RefExchangeRate.New(newFactory);
			rate1.RE_ExRateType = "SEL";
			rate1.RE_StartDate = ZDateTime.Today;
			rate1.RE_ExpiryDate = ZDateTime.Today;
			rate1.RE_SellRate = 10m;
			rate1.RE_GC = GlbCompany.CurrentCompany.PK;
			rate1.RE_RX_NKExCurrency = "AUD";
			rate1.RE_OH_Client = org.PK;

			var rate2 = RefExchangeRate.New(newFactory);
			rate2.RE_ExRateType = "SEL";
			rate2.RE_StartDate = ZDateTime.Today;
			rate2.RE_ExpiryDate = ZDateTime.Today;
			rate2.RE_SellRate = 10m;
			rate2.RE_GC = GlbCompany.CurrentCompany.PK;
			rate2.RE_RX_NKExCurrency = "USD";
			rate2.RE_OH_Client = org.PK;

			var rate3 = RefExchangeRate.New(newFactory);
			rate3.RE_ExRateType = "SEL";
			rate3.RE_StartDate = ZDateTime.Today;
			rate3.RE_ExpiryDate = ZDateTime.Today;
			rate3.RE_SellRate = 10m;
			rate3.RE_GC = GlbCompany.CurrentCompany.PK;
			rate3.RE_RX_NKExCurrency = "CNY";
			rate3.RE_OH_Client = ZGuid.Empty;

			newFactory.Save();

			AssertEquals("Should have 2 records.", 2, org.ExchangeRateCollection.Count);
			AssertNotNull("Should contain the related rate.", org.ExchangeRateCollection.FindByPK(rate1.PK));
			AssertNotNull("Should contain the related rate.", org.ExchangeRateCollection.FindByPK(rate2.PK));
			AssertNull("Shouldn't contain the unrelated rate.", org.ExchangeRateCollection.FindByPK(rate3.PK));
		}

		protected override RefExchangeRateInOrgHeaderDependentCollection GetCollectionToTest()
		{
			return new RefExchangeRateInOrgHeaderDependentCollection(Factory.New<OrgHeader>());
		}
	}
}
