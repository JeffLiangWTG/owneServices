using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(CustomsExchangeRate))]
	sealed class CustomsExchangeRateTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var rateForTest = (CustomsExchangeRate)GetNewBusinessObject();

			AssertNotNull("BizO", rateForTest);
			AssertNotNull("Rate DynamicBusinessObject", rateForTest.Rate);

			AssertEquals(rateForTest.Rate[CustomsExchangeRate.Schema.RX_Code].ToString(), rateForTest.RX_Code.ToString());
			AssertEquals(rateForTest.Rate[CustomsExchangeRate.Schema.RX_Desc].ToString(), rateForTest.RX_Desc.ToString());
			AssertEquals(rateForTest.Rate[CustomsExchangeRate.Schema.RE_ExpiryDate].ToString(), rateForTest.RE_ExpiryDate.ToString());
			AssertEquals(rateForTest.Rate[CustomsExchangeRate.Schema.RE_SellRate].ToString(), rateForTest.RE_SellRate.ToString());
			AssertEquals(6, rateForTest.Decimals);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var rates = new List<CustomsExchangeRate>();
			testRatesHeader.ExchangeRates.CopyToList(rates);
			return rates[0];
		}

		CustomsExchangeRatesHeader testRatesHeader;

		protected override void SetUp()
		{
			base.SetUp();

			testRatesHeader = new CustomsExchangeRatesHeader(Factory, GlbCompany.CurrentCompany.GC_Code);
		}
	}
}
