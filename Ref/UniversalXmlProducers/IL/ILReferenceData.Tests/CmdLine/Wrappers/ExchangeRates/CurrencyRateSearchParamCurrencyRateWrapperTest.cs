using System;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine.Wrappers
{
	[TestFixture]
	sealed class CurrencyRateSearchParamCurrencyRateWrapperTest
	{
		[Test]
		public void TestCurrencyTypeID()
		{
			Assert.AreEqual("USD", wrapper.currencyTypeID);
		}

		[Test]
		public void TestFromDate()
		{
			Assert.AreEqual(new DateTime(2024, 06, 03), wrapper.fromDate);
		}

		[Test]
		public void TestToDate()
		{
			Assert.AreEqual(new DateTime(2024, 06, 04), wrapper.toDate);
		}

		[SetUp]
		public void SetUp()
		{
			wrapper = new CurrencyRateSearchParamCurrencyRateWrapper(fromDate: new DateTime(2024, 06, 03), toDate: new DateTime(2024, 06, 04), "USD");
		}

		ICurrencyRateSearchParamCurrencyRate wrapper;
	}
}
