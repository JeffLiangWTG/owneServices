using System.Collections.Generic;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;
using NUnit.Framework;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries
{
	[TestFixture]
	sealed class DictionaryDataTest
	{
		[Test]
		public void TestDictionaryData()
		{
			var expectedCode = "034";
			var expectedCw1Code = "ABC034";

			var data = new DictionaryData(expectedCode, expectedCw1Code, RefDataType.RefCusCodeListWithAttributeExport,
				new List<ExcludedDictionaryCodes>
				{
					new ExcludedDictionaryCodes
					{
						DictionaryCode = "049",
						ExcludedCodes = new List<string> { "11", "AA", "T11" }
					},
					new ExcludedDictionaryCodes
					{
						ExcludedCodes = new List<string> { "22" }
					}
				},
				new List<DictionaryData>
				{
					new DictionaryData("007", "abba", RefDataType.RefCusCodeList)
				},
				new List<string> { "123", "AB", "A1" }
			);

			Assert.AreEqual(expectedCode, data.Code);
			Assert.AreEqual(expectedCw1Code, data.CW1Code);
			Assert.AreEqual(RefDataType.RefCusCodeListWithAttributeExport, data.RefDataType);

			Assert.IsNotNull(data.AdditionalDictionaries);
			Assert.AreEqual(1, data.AdditionalDictionaries.Count);
			Assert.AreEqual("007", data.AdditionalDictionaries[0].Code);
			Assert.AreEqual("abba", data.AdditionalDictionaries[0].CW1Code);
			Assert.IsNotNull(data.AdditionalDictionaries[0].ExcludedCodesList);
			Assert.AreEqual(0, data.AdditionalDictionaries[0].ExcludedCodesList.Count);
			Assert.IsEmpty(data.AdditionalDictionaries[0].AdditionalDictionaries);
			Assert.IsEmpty(data.AdditionalDictionaries[0].SearchedCodes);

			Assert.IsNotNull(data.ExcludedCodesList);
			Assert.AreEqual(2, data.ExcludedCodesList.Count);
			Assert.AreEqual("049", data.ExcludedCodesList[0].DictionaryCode);
			Assert.AreEqual(3, data.ExcludedCodesList[0].ExcludedCodes.Count);
			Assert.AreEqual(string.Empty, data.ExcludedCodesList[1].DictionaryCode);
			Assert.AreEqual(1, data.ExcludedCodesList[1].ExcludedCodes.Count);

			Assert.IsNotNull(data.SearchedCodes);
			Assert.AreEqual(3, data.SearchedCodes.Count);
		}
	}
}
