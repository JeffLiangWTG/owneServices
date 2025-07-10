using System.Globalization;
using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;
using NUnit.Framework;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries
{
	[TestFixture]
	sealed class MergedSimpleRefCusCodeListMapperTest
	{
		[Test]
		public void TestGetMergedAndGroupedData()
		{
			var expectedCw1Code = "some CW1 Code";
			var expected = new RefCusCodeList[]
			{
				new RefCusCodeList
				{
					ZZD_Code = "0001",
					ZZD_Description = "Przeznaczone do przetwórstwa newer Description",
					ZZD_StartDate = new DateTime(2010, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = expectedCw1Code
				},
				new RefCusCodeList
				{
					ZZD_Code = "0002",
					ZZD_Description = "Something",
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = expectedCw1Code
				},
				new RefCusCodeList
				{
					ZZD_Code = "0003",
					ZZD_Description = RefDataConstants.DefaultDescription,
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = expectedCw1Code
				},
				new RefCusCodeList
				{
					ZZD_Code = "1DK1",
					ZZD_Description = "Wniosek o wydanie pozwolenia w sytuacji, gdy pozwolenie zostanie wydane z mocą wsteczną (art. 294 ust.1 oraz art. 508 ust. 1 RWKC)",
					ZZD_StartDate = Convert.ToDateTime(DictionariesConstants.DefaultStartDate, CultureInfo.InvariantCulture),
					ZZD_EndDate = new DateTime(2016, 06, 06),
					ZZD_ZZK_NKCodeType = expectedCw1Code
				}
			};

			var dictionaries = DictionariesTestHelper.DictionariesForTest(DictionariesTestHelper.TextXmlPath034);

			var dictionaryData = new DictionaryData("Code Puesc Code",
				expectedCw1Code,
				RefDataType.RefCusCodeListMerged);
			var result = dictionaries.GetDictionariesAsRefCusCodeList(dictionaryData);

			Assert.Multiple(() =>
			{
				Assert.AreEqual(expected.Length, result.Count, "amount");

				for (var i = 0; i < expected.Length; i++)
				{
					Assert.AreEqual(expected[i].ZZD_Code, result[i].ZZD_Code, $"{i} - ZZD_Code");
					Assert.AreEqual(expected[i].ZZD_Description, result[i].ZZD_Description, $"{i} - ZZD_Description");
					Assert.AreEqual(expected[i].ZZD_StartDate, result[i].ZZD_StartDate, $"{i} - ZZD_StartDate - min start date shuld be selected");
					Assert.AreEqual(expected[i].ZZD_EndDate, result[i].ZZD_EndDate, $"{i} - ZZD_EndDate - max end date should be selected");
					Assert.AreEqual(expected[i].ZZD_ZZK_NKCodeType, result[i].ZZD_ZZK_NKCodeType, $"{i} - ZZD_ZZK_NKCodeType");
					Assert.Null(expected[i].RefCusCodeListLanguages, $"{i} - RefCusCodeListLanguages - For now not needed");
					Assert.Null(expected[i].RefCusCodeListAttributes, $"{i} - RefCusCodeListAttributes - For now not needed");
					Assert.Null(expected[i].RefCusCodeOrAttributeTransportModes, $"{i} - RefCusCodeOrAttributeTransportModes - For now not needed");
				}
			});
		}
	}
}
