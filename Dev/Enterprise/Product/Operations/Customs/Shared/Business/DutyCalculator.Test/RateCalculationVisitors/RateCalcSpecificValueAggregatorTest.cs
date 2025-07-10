using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(RateCalcSpecificValueAggregator))]
[CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Latvia)]
sealed class RateCalcSpecificValueAggregatorTest : TestCaseWithFactory
{
	public void TestGetSpecificValueDictionary()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var rateCalcData = new RateCalcSpecificValueAggregator().GetSpecificValueDictionary(entryLine);
		AssertEquals(1, rateCalcData.Count);
		AssertEquals(0m, rateCalcData["NIHIL"]);
	}

	public void TestGetSpecificValueDictionaryAgainstDifferentEntryLines()
	{
		var countrySpecificValueProviderObjectHandle = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CountrySpecificValueProviderObjectHandleForTest() } };
		using (ObjectFactory.Substitute("ICountrySpecificValueProvider", countrySpecificValueProviderObjectHandle))
		{
			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_StatisticalValue = 123m;
			var rateCalcData1 = new RateCalcSpecificValueAggregator().GetSpecificValueDictionary(entryLine1);
			AssertEquals(2, rateCalcData1.Count);
			AssertEquals(123m, rateCalcData1["STATVAL"]);

			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_StatisticalValue = 456m;
			var rateCalcData2 = new RateCalcSpecificValueAggregator().GetSpecificValueDictionary(entryLine2);
			AssertEquals(2, rateCalcData2.Count);
			AssertEquals("Different entry line should get correct specific value list, instead of using the cache from the previous line.", 456m, rateCalcData2["STATVAL"]);
		}
	}

	public void TestGetSpecificValueDictionaryForInvoiceLine()
	{
		var rateCalcData = new RateCalcSpecificValueAggregator().GetSpecificValueDictionary(Factory.New<BaseJobComInvoiceLine>());
		AssertEquals(1, rateCalcData.Count);
		AssertEquals(0m, rateCalcData["NIHIL"]);
	}
}

class CountrySpecificValueProviderObjectHandleForTest : ObjectHandle
{
	public override object GetObject() => new CountrySpecificValueProviderForTest();
}

class CountrySpecificValueProviderForTest : Integration.Customs.ICountrySpecificValueProvider
{
	public IDictionary<string, decimal> GetCountrySpecificValueList(Integration.Customs.ICusEntryLine entryLine)
	{
		return new Dictionary<string, decimal>
		{
			{ "STATVAL", ((CusEntryLine)entryLine).CL_StatisticalValue }
		};
	}
}
