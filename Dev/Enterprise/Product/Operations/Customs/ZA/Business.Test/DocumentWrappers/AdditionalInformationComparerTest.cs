using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.Testing;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class AdditionalInformationComparerTest : TestCaseWithFactory
	{
		public void TestSorting()
		{
			var startDate = ZDateTime.Today.AddDays(-5);
			var endDate = ZDateTime.Today.AddDays(5);
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TST", "TestList");
			var vdn = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "TST", "VDN", startDate, endDate);
			helper.CreateCusCodeListAttribute(vdn.PK, "Pair", "ZZZZZZ");
			var ts1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "TST", "TS1", startDate, endDate);
			helper.CreateCusCodeListAttribute(ts1.PK, "Pair", "TS2");
			var ts2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "TST", "TS2", startDate, endDate);
			helper.CreateCusCodeListAttribute(ts2.PK, "Pair", "TS1");
			var ts0 = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "TST", "TS0", startDate, endDate);
			var ts3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "TST", "TS3", startDate, endDate);
			helper.CreateCusCodeListAttribute(ts3.PK, "Pair", "TS4");
			var ts4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "TST", "TS4", startDate, endDate);
			helper.CreateCusCodeListAttribute(ts4.PK, "Pair", "TS3");
			Factory.Save();
			CombineAssertions(() =>
			{
				var refCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, "TST", ZDateTime.Today);
				refCollection.Load();
				var refList = refCollection.OfType<ZZRefCusCodeListCombined>();
				var testList = new List<AdditionalInformationDocWrapper>();
				testList.Add(new AdditionalInformationDocWrapper("XXX1"));
				testList.Add(new AdditionalInformationDocWrapper("TS44"));
				testList.Add(new AdditionalInformationDocWrapper("TS05"));
				testList.Add(new AdditionalInformationDocWrapper("TS37"));
				testList.Add(new AdditionalInformationDocWrapper("TS12"));
				testList.Add(new AdditionalInformationDocWrapper("TS23"));
				testList.Add(new AdditionalInformationDocWrapper("VDN9"));
				testList.Sort(new AdditionalInformationComparer(refList));
				AssertEquals("VDN9", testList[0].Code + testList[0].Value);
				AssertEquals("TS12", testList[1].Code + testList[1].Value);
				AssertEquals("TS23", testList[2].Code + testList[2].Value);
				AssertEquals("TS37", testList[3].Code + testList[3].Value);
				AssertEquals("TS44", testList[4].Code + testList[4].Value);
				AssertEquals("TS05", testList[5].Code + testList[5].Value);
				AssertEquals("XXX1", testList[6].Code + testList[6].Value);
			});
		}
	}
}
