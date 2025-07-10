using System;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine.Wrappers
{
	[TestFixture]
	sealed class CurrencyRateSearchParamWrapperTest
	{
		[Test]
		public void TestRequestContentHeader()
		{
			Assert.NotNull(wrapper.RequestContentHeader);
		}

		[Test]
		public void TestCurrencyRate()
		{
			Assert.NotNull(wrapper.CurrencyRate);
		}

		[SetUp]
		public void SetUp()
		{
			wrapper = new CurrencyRateSearchParamWrapper(transmissionDateTime: new DateTime(2024, 06, 04), fromDate: new DateTime(2024, 06, 03), toDate: new DateTime(2024, 06, 04), "USD");
		}

		ICurrencyRateSearchParam wrapper;
	}
}
