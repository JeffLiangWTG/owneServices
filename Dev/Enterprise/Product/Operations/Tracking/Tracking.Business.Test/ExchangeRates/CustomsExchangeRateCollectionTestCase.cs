using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(CustomsExchangeRateCollection))]
	class CustomsExchangeRateCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<CustomsExchangeRateCollection>
	{
		protected override CustomsExchangeRateCollection GetCollectionToTest()
		{
			return new CustomsExchangeRatesHeader(Factory, "DEM").ExchangeRates;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			List<CustomsExchangeRate> rates = new List<CustomsExchangeRate>();
			TestRatesHeader.ExchangeRates.CopyToList(rates);
			RateIndex = (RateIndex + 1) % rates.Count;
			return rates[RateIndex];
		}

		public override void TestAddNew()
		{
			Assert("Not Supported", true);
		}

		#region Setup

		protected CustomsExchangeRatesHeader TestRatesHeader;
		int RateIndex = -1;

		protected override void SetUp()
		{
			base.SetUp();

			TestRatesHeader = new CustomsExchangeRatesHeader(Factory, GlbCompany.CurrentCompany.GC_Code);
		}

		#endregion
	}
}
