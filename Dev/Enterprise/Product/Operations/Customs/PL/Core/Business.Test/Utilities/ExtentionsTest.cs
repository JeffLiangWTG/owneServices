using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ExtentionsTest : TestCaseWithFactory
{
	public void TestToInt()
	{
		CombineAssertions(() =>
		{
			AssertEquals("A Number", 5, '5'.ToInt());
			AssertEquals("Not a Number", -1, 'a'.ToInt());
		});
	}

	public void TestIsAESTransitionPeriod()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals(true, entryInstruction.IsAESTransitionPeriod());
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals(false, entryInstruction.IsAESTransitionPeriod());
			}
		});
	}

	public void TestDistinctSpecificDocuments() => CombineAssertions(() =>
	{
		IEnumerable<CusSupportingInfo> result = null;
		AssertNoExceptionThrown("Null source", () => result = ((IEnumerable<CusSupportingInfo>)null).DistinctSpecificDocuments(null));
		AssertNull("null source should return null data", null);

		var expected = new List<CusSupportingInfo>();
		var source = new List<CusSupportingInfo>();
		var dataToAdd = Factory.New<CusSupportingInfo>();
		dataToAdd.CSI_Code = "QWE";
		source.Add(dataToAdd);
		expected.Add(dataToAdd);
		AssertNoExceptionThrown("Null documentCodesAsUnique", () => result = source.DistinctSpecificDocuments(null));
		AssertEquals("Unchanged source should be returned for null documentCodesAsUnique", source, result);
		AssertNoExceptionThrown("Empty documentCodesAsUnique", () => result = source.DistinctSpecificDocuments(Array.Empty<ZString>()));
		AssertEquals("Unchanged source should be returned for empty documentCodesAsUnique", source, result);
		AssertNoExceptionThrown("Empty documentCodesAsUnique", () => result = source.DistinctSpecificDocuments(new ZString[] { "QWE" }));
		AssertNotEquals("New IEnumerable should be returned", source, result);
		AssertDistinctSpecificDocuments("New IEnumerable", expected, result);

		AssertNoExceptionThrown("Should support Previous Documents", () => new List<PreviousDocument>().DistinctSpecificDocuments(null));
		AssertNoExceptionThrown("Should support Additional Information", () => new List<AdditionalInfo>().DistinctSpecificDocuments(null));
		AssertNoExceptionThrown("Should support Supporting Documents", () => new List<SupportingDocument>().DistinctSpecificDocuments(null));

		dataToAdd = Factory.New<AdditionalInfo>();
		dataToAdd.CSI_Code = "QWE";
		source.Add(dataToAdd);
		expected.Add(dataToAdd);
		dataToAdd = Factory.New<AdditionalInfo>();
		dataToAdd.CSI_Code = "ASD";
		source.Add(dataToAdd);
		expected.Add(dataToAdd);
		AssertNoExceptionThrown("Empty documentCodesAsUnique", () => result = source.DistinctSpecificDocuments(Array.Empty<ZString>()));
		AssertDistinctSpecificDocuments("Unchanged source should be returned", source, result);

		dataToAdd = Factory.New<AdditionalInfo>();
		dataToAdd.CSI_Code = "ZXC";
		source.Add(dataToAdd);
		expected.Add(dataToAdd);
		dataToAdd = Factory.New<AdditionalInfo>();
		dataToAdd.CSI_Code = "ZXC";
		source.Add(dataToAdd);
		AssertNoExceptionThrown("Distinct ZXC documents", () => result = source.DistinctSpecificDocuments(new ZString[] { "ZXC" }));
		AssertDistinctSpecificDocuments("New IEnumerable should be generated with Distinct ZXC codes", expected, result);

		dataToAdd = Factory.New<AdditionalInfo>();
		dataToAdd.CSI_Code = "POI";
		source.Add(dataToAdd);
		expected.Add(dataToAdd);
		dataToAdd = Factory.New<AdditionalInfo>();
		dataToAdd.CSI_Code = "POI";
		source.Add(dataToAdd);
		AssertNoExceptionThrown("Distinct ZXC and POI documents", () => result = source.DistinctSpecificDocuments(new ZString[] { "ZXC", "POI" }));
		AssertDistinctSpecificDocuments("New IEnumerable should be generated with Distinct ZXC and POI codes", expected, result);

		void AssertDistinctSpecificDocuments(string message, List<CusSupportingInfo> expected, IEnumerable<CusSupportingInfo> result)
		{
			var resultAsArray = result.ToArray();
			AssertEquals($"{message} - amount", expected.Count, resultAsArray.Length);
			AssertArrayEqualsByElements($"{message}", expected.Select(x => x.CSI_Code).ToArray(), result.Select(x => x.CSI_Code).ToArray());
		}
	});
}
