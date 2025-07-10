using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;
using NUnit.Framework;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries
{
	[TestFixture]
	sealed class DictionariesTest
	{
		[Test]
		public void TestOnModelAndMarkBasedDictionary()
		{
			var expected = new List<RefCusCodeList>
			{
				new RefCusCodeList
				{
					ZZD_Code = "9543",
					ZZD_Description = "Access Motor,BR400",
					ZZD_StartDate = new DateTime(2013, 01, 01),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes
				}
				,new RefCusCodeList
				{
					ZZD_Code = "15987",
					ZZD_Description = "Access Motor,BR400-SWDI",
					ZZD_StartDate = new DateTime(2013, 01, 01),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes
				}
			};

			var dictionaries = DictionariesTestHelper.DictionariesForTest(DictionariesTestHelper.TextXmlPath3041);

			var dictionaryData = new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes,
				DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes,
				RefDataType.RefCusCodeList);
			var result = dictionaries.GetDictionariesAsRefCusCodeList(dictionaryData);
			Assert.AreEqual(expected.Count, result.Count);

			for (var i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i].ZZD_Code, result[i].ZZD_Code);
				Assert.AreEqual(expected[i].ZZD_Description, result[i].ZZD_Description);
				Assert.AreEqual(expected[i].ZZD_StartDate, result[i].ZZD_StartDate);
				Assert.AreEqual(expected[i].ZZD_EndDate, result[i].ZZD_EndDate);
				Assert.AreEqual(expected[i].ZZD_ZZK_NKCodeType, result[i].ZZD_ZZK_NKCodeType);
			}
		}

		[Test]
		public void TestOnPuescBasedDictionary()
		{
			var expected = new List<RefCusCodeList>
			{
				new RefCusCodeList
				{
					ZZD_Code = "0001",
					ZZD_Description = "Przeznaczone do przetwórstwa newer Description",
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments
				},
				new RefCusCodeList
				{
					ZZD_Code = "0002",
					ZZD_Description = "Something",
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments
				},
				new RefCusCodeList
				{
					ZZD_Code = "0003",
					ZZD_Description = RefDataConstants.DefaultDescription,
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments
				},
				new RefCusCodeList
				{
					ZZD_Code = "1DK1",
					ZZD_Description = "Wniosek o wydanie pozwolenia w sytuacji, gdy pozwolenie zostanie wydane z mocą wsteczną (art. 294 ust.1 oraz art. 508 ust. 1 RWKC)",
					ZZD_StartDate = Convert.ToDateTime(DictionariesConstants.DefaultStartDate, CultureInfo.InvariantCulture),
					ZZD_EndDate = new DateTime(2016, 06, 06),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments
				}
			};

			var dictionaries = DictionariesTestHelper.DictionariesForTest(DictionariesTestHelper.TextXmlPath034);

			var dictionaryData = new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				RefDataType.RefCusCodeList);
			var result = dictionaries.GetDictionariesAsRefCusCodeList(dictionaryData);
			Assert.AreEqual(expected.Count, result.Count);

			for (var i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i].ZZD_Code, result[i].ZZD_Code);
				Assert.AreEqual(expected[i].ZZD_Description, result[i].ZZD_Description);
				Assert.AreEqual(expected[i].ZZD_StartDate, result[i].ZZD_StartDate);
				Assert.AreEqual(expected[i].ZZD_EndDate, result[i].ZZD_EndDate);
				Assert.AreEqual(expected[i].ZZD_ZZK_NKCodeType, result[i].ZZD_ZZK_NKCodeType);
			}
		}

		[Test]
		public void TestOnPuescBasedDictionaryExcludeCodes()
		{
			var expected = new List<RefCusCodeList>
			{
				new RefCusCodeList
				{
					ZZD_Code = "0001",
					ZZD_Description = "Przeznaczone do przetwórstwa newer Description",
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments
				},
				new RefCusCodeList
				{
					ZZD_Code = "0002",
					ZZD_Description = "Something",
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments
				},
				new RefCusCodeList
				{
					ZZD_Code = "1DK1",
					ZZD_Description = "Wniosek o wydanie pozwolenia w sytuacji, gdy pozwolenie zostanie wydane z mocą wsteczną (art. 294 ust.1 oraz art. 508 ust. 1 RWKC)",
					ZZD_StartDate = Convert.ToDateTime(DictionariesConstants.DefaultStartDate, CultureInfo.InvariantCulture),
					ZZD_EndDate = new DateTime(2016, 06, 06),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments
				}
			};

			var dictionaries = DictionariesTestHelper.DictionariesForTest(DictionariesTestHelper.TextXmlPath034);

			var dictionaryData = new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				RefDataType.RefCusCodeList, new List<ExcludedDictionaryCodes>
			{
				new ExcludedDictionaryCodes
				{
					DictionaryCode = string.Empty,
					ExcludedCodes = new List<string> { "0003" }
				}
			});
			var result = dictionaries.GetDictionariesAsRefCusCodeList(dictionaryData);
			Assert.AreEqual(expected.Count, result.Count);

			for (var i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i].ZZD_Code, result[i].ZZD_Code);
				Assert.AreEqual(expected[i].ZZD_Description, result[i].ZZD_Description);
				Assert.AreEqual(expected[i].ZZD_StartDate, result[i].ZZD_StartDate);
				Assert.AreEqual(expected[i].ZZD_EndDate, result[i].ZZD_EndDate);
				Assert.AreEqual(expected[i].ZZD_ZZK_NKCodeType, result[i].ZZD_ZZK_NKCodeType);
			}
		}

		[Test]
		public void TestOnPuescBasedDictionaryWithAttributeExport()
		{
			var expected = new List<RefCusCodeList>
			{
				new RefCusCodeList
				{
					ZZD_Code = "0001",
					ZZD_Description = "Przeznaczone do przetwórstwa newer Description",
					ZZD_StartDate = new DateTime(2010, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[] {
						new RefCusCodeListAttribute
						{
							ZZE_Value = RefDataConstants.Export,
							ZZE_ZXE_NKName = RefDataConstants.Direction
						}
					}
				},
				new RefCusCodeList
				{
					ZZD_Code = "0002",
					ZZD_Description = "Something",
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[] {
						new RefCusCodeListAttribute
						{
							ZZE_Value = RefDataConstants.Export,
							ZZE_ZXE_NKName = RefDataConstants.Direction
						}
					}
				},
				new RefCusCodeList
				{
					ZZD_Code = "0003",
					ZZD_Description = RefDataConstants.DefaultDescription,
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[] {
						new RefCusCodeListAttribute
						{
							ZZE_Value = RefDataConstants.Export,
							ZZE_ZXE_NKName = RefDataConstants.Direction
						}
					}
				},
				new RefCusCodeList
				{
					ZZD_Code = "1DK1",
					ZZD_Description = "Wniosek o wydanie pozwolenia w sytuacji, gdy pozwolenie zostanie wydane z mocą wsteczną (art. 294 ust.1 oraz art. 508 ust. 1 RWKC)",
					ZZD_StartDate = Convert.ToDateTime(DictionariesConstants.DefaultStartDate, CultureInfo.InvariantCulture),
					ZZD_EndDate = new DateTime(2016, 06, 06),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[] {
						new RefCusCodeListAttribute
						{
							ZZE_Value = RefDataConstants.Export,
							ZZE_ZXE_NKName = RefDataConstants.Direction
						}
					}
				}
			};

			var dictionaries = DictionariesTestHelper.DictionariesForTest(DictionariesTestHelper.TextXmlPath034);

			var dictionaryData = new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				RefDataType.RefCusCodeListWithAttributeExport);
			var result = dictionaries.GetDictionariesAsRefCusCodeList(dictionaryData);
			Assert.AreEqual(expected.Count, result.Count);

			for (var i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i].ZZD_Code, result[i].ZZD_Code);
				Assert.AreEqual(expected[i].ZZD_Description, result[i].ZZD_Description);
				Assert.AreEqual(expected[i].ZZD_StartDate, result[i].ZZD_StartDate);
				Assert.AreEqual(expected[i].ZZD_EndDate, result[i].ZZD_EndDate);
				Assert.AreEqual(expected[i].ZZD_ZZK_NKCodeType, result[i].ZZD_ZZK_NKCodeType);
				Assert.AreEqual(expected[i].RefCusCodeListAttributes.Length, result[i].RefCusCodeListAttributes.Length);
				for (var j = 0; j < expected[i].RefCusCodeListAttributes.Length; j++)
				{
					Assert.AreEqual(expected[i].RefCusCodeListAttributes[j].ZZE_Value, result[i].RefCusCodeListAttributes[j].ZZE_Value);
					Assert.AreEqual(expected[i].RefCusCodeListAttributes[j].ZZE_ZXE_NKName, result[i].RefCusCodeListAttributes[j].ZZE_ZXE_NKName);
				}
			}
		}

		[Test]
		public void TestOnPuescBasedDictionaryWithAttributeImport()
		{
			var expected = new List<RefCusCodeList>
			{
				new RefCusCodeList
				{
					ZZD_Code = "0001",
					ZZD_Description = "Przeznaczone do przetwórstwa newer Description",
					ZZD_StartDate = new DateTime(2010, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[] {
						new RefCusCodeListAttribute
						{
							ZZE_Value = RefDataConstants.Import,
							ZZE_ZXE_NKName = RefDataConstants.Direction
						}
					}
				},
				new RefCusCodeList
				{
					ZZD_Code = "0002",
					ZZD_Description = "Something",
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[] {
						new RefCusCodeListAttribute
						{
							ZZE_Value = RefDataConstants.Import,
							ZZE_ZXE_NKName = RefDataConstants.Direction
						}
					}
				},
				new RefCusCodeList
				{
					ZZD_Code = "0003",
					ZZD_Description = RefDataConstants.DefaultDescription,
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[] {
						new RefCusCodeListAttribute
						{
							ZZE_Value = RefDataConstants.Import,
							ZZE_ZXE_NKName = RefDataConstants.Direction
						}
					}
				},
				new RefCusCodeList
				{
					ZZD_Code = "1DK1",
					ZZD_Description = "Wniosek o wydanie pozwolenia w sytuacji, gdy pozwolenie zostanie wydane z mocą wsteczną (art. 294 ust.1 oraz art. 508 ust. 1 RWKC)",
					ZZD_StartDate = Convert.ToDateTime(DictionariesConstants.DefaultStartDate, CultureInfo.InvariantCulture),
					ZZD_EndDate = new DateTime(2016, 06, 06),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[] {
						new RefCusCodeListAttribute
						{
							ZZE_Value = RefDataConstants.Import,
							ZZE_ZXE_NKName = RefDataConstants.Direction
						}
					}
				}
			};

			var dictionaries = DictionariesTestHelper.DictionariesForTest(DictionariesTestHelper.TextXmlPath034);

			var dictionaryData = new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				RefDataType.RefCusCodeListWithAttributeImport);
			var result = dictionaries.GetDictionariesAsRefCusCodeList(dictionaryData);
			Assert.AreEqual(expected.Count, result.Count);

			for (var i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i].ZZD_Code, result[i].ZZD_Code);
				Assert.AreEqual(expected[i].ZZD_Description, result[i].ZZD_Description);
				Assert.AreEqual(expected[i].ZZD_StartDate, result[i].ZZD_StartDate);
				Assert.AreEqual(expected[i].ZZD_EndDate, result[i].ZZD_EndDate);
				Assert.AreEqual(expected[i].ZZD_ZZK_NKCodeType, result[i].ZZD_ZZK_NKCodeType);
				Assert.AreEqual(expected[i].RefCusCodeListAttributes.Length, result[i].RefCusCodeListAttributes.Length);
				for (var j = 0; j < expected[i].RefCusCodeListAttributes.Length; j++)
				{
					Assert.AreEqual(expected[i].RefCusCodeListAttributes[j].ZZE_Value, result[i].RefCusCodeListAttributes[j].ZZE_Value);
					Assert.AreEqual(expected[i].RefCusCodeListAttributes[j].ZZE_ZXE_NKName, result[i].RefCusCodeListAttributes[j].ZZE_ZXE_NKName);
				}
			}
		}

		[Test]
		public void TestInvalidStartAndEndDates()
		{
			var expected = new List<RefCusCodeList>
			{
				new RefCusCodeList
				{
					ZZD_Code = "122",
					ZZD_Description = "A,1",
					ZZD_StartDate = new DateTime(2013, 01, 01),
					ZZD_EndDate = DictionariesConstants.DefaultEndDateDateTime,
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes
				}
				,new RefCusCodeList
				{
					ZZD_Code = "123",
					ZZD_Description = "B,2",
					ZZD_StartDate = DictionariesConstants.DefaultEndDateDateTime,
					ZZD_EndDate = DictionariesConstants.DefaultEndDateDateTime,
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes
				}
				,new RefCusCodeList
				{
					ZZD_Code = "124",
					ZZD_Description = "C,3",
					ZZD_StartDate =  DictionariesConstants.DefaultStartDateDateTime,
					ZZD_EndDate = DictionariesConstants.DefaultEndDateDateTime,
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes
				}
				,new RefCusCodeList
				{
					ZZD_Code = "125",
					ZZD_Description = "D,4",
					ZZD_StartDate = DictionariesConstants.DefaultStartDateDateTime,
					ZZD_EndDate = DictionariesConstants.DefaultEndDateDateTime,
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes
				}
				,new RefCusCodeList
				{
					ZZD_Code = "126",
					ZZD_Description = "E,5",
					ZZD_StartDate = DictionariesConstants.DefaultEndDateDateTime,
					ZZD_EndDate = DictionariesConstants.DefaultEndDateDateTime,
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes
				}
				,new RefCusCodeList
				{
					ZZD_Code = "127",
					ZZD_Description = "F,6",
					ZZD_StartDate = DictionariesConstants.DefaultStartDateDateTime,
					ZZD_EndDate = DictionariesConstants.DefaultStartDateDateTime,
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes
				}
			};

			var dictionaries = DictionariesTestHelper.DictionariesForTest(DictionariesTestHelper.TextXmlPath3041InvalidDates);

			var dictionaryData = new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes,
				DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes,
				RefDataType.RefCusCodeList);
			var result = dictionaries.GetDictionariesAsRefCusCodeList(dictionaryData);
			Assert.AreEqual(expected.Count, result.Count);

			for (var i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i].ZZD_Code, result[i].ZZD_Code);
				Assert.AreEqual(expected[i].ZZD_Description, result[i].ZZD_Description);
				Assert.AreEqual(expected[i].ZZD_StartDate, result[i].ZZD_StartDate);
				Assert.AreEqual(expected[i].ZZD_EndDate, result[i].ZZD_EndDate);
				Assert.AreEqual(expected[i].ZZD_ZZK_NKCodeType, result[i].ZZD_ZZK_NKCodeType);
			}
		}

		[Test]
		public void TestOnPuescBasedDictionaryWithLanguage()
		{
			var expected = new List<RefCusCodeList>
			{
				new RefCusCodeList
				{
					ZZD_Code = "0001",
					ZZD_Description = RefDataConstants.DefaultDescription,
					ZZD_StartDate = new DateTime(2010, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListLanguages = new RefCusCodeListLanguage[] {
						new RefCusCodeListLanguage
						{
							ZXA_ZX6_NKLanguage = Constants.CountryCode,
							ZXA_Description = "Przeznaczone do przetwórstwa newer Description"
						}
					}
				},
				new RefCusCodeList
				{
					ZZD_Code = "0002",
					ZZD_Description = "Something",
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListLanguages = new RefCusCodeListLanguage[] {
						new RefCusCodeListLanguage
						{
							ZXA_ZX6_NKLanguage = Constants.CountryCode,
							ZXA_Description = RefDataConstants.DefaultDescription
						}
					}
				},
				new RefCusCodeList
				{
					ZZD_Code = "0003",
					ZZD_Description = RefDataConstants.DefaultDescription,
					ZZD_StartDate = new DateTime(2011, 06, 22),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListLanguages = new RefCusCodeListLanguage[] {
						new RefCusCodeListLanguage
						{
							ZXA_ZX6_NKLanguage = Constants.CountryCode,
							ZXA_Description = RefDataConstants.DefaultDescription
						}
					}
				},
				new RefCusCodeList
				{
					ZZD_Code = "1DK1",
					ZZD_Description = "Application for retroactive authorisation (Art. 294 par. 1 and Art. 508 par. 1 of Commission Regulation 2454/93.)",
					ZZD_StartDate = Convert.ToDateTime(DictionariesConstants.DefaultStartDate, CultureInfo.InvariantCulture),
					ZZD_EndDate = new DateTime(2016, 06, 06),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
					RefCusCodeListLanguages = new RefCusCodeListLanguage[] {
						new RefCusCodeListLanguage
						{
							ZXA_ZX6_NKLanguage = Constants.CountryCode,
							ZXA_Description = "Wniosek o wydanie pozwolenia w sytuacji, gdy pozwolenie zostanie wydane z mocą wsteczną (art. 294 ust.1 oraz art. 508 ust. 1 RWKC)"
						}
					}
				}
			};

			var dictionaries = DictionariesTestHelper.DictionariesForTest(DictionariesTestHelper.TextXmlPath034);

			var dictionaryData = new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments,
				RefDataType.RefCusCodeListWithLanguage);
			var result = dictionaries.GetDictionariesAsRefCusCodeList(dictionaryData);
			Assert.AreEqual(expected.Count, result.Count);

			for (var i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i].ZZD_Code, result[i].ZZD_Code);
				Assert.AreEqual(expected[i].ZZD_Description, result[i].ZZD_Description);
				Assert.AreEqual(expected[i].ZZD_StartDate, result[i].ZZD_StartDate);
				Assert.AreEqual(expected[i].ZZD_EndDate, result[i].ZZD_EndDate);
				Assert.AreEqual(expected[i].ZZD_ZZK_NKCodeType, result[i].ZZD_ZZK_NKCodeType);
				Assert.AreEqual(expected[i].RefCusCodeListLanguages.Length, result[i].RefCusCodeListLanguages.Length);
				for (var j = 0; j < expected[i].RefCusCodeListLanguages.Length; j++)
				{
					Assert.AreEqual(expected[i].RefCusCodeListLanguages[j].ZXA_Description, result[i].RefCusCodeListLanguages[j].ZXA_Description);
				}
			}
		}

		[Test]
		public void TestOnPuescBasedDictionaryCL008AES()
		{
			var expected = new List<RefCusCodeList>
			{
				new RefCusCodeList
				{
					ZZD_Code = "AD",
					ZZD_Description = "Andora",
					ZZD_StartDate = new DateTime(2021, 05, 11),
					ZZD_EndDate = Convert.ToDateTime(DictionariesConstants.DefaultEndDate, CultureInfo.InvariantCulture),
					ZZD_ZZK_NKCodeType = DictionariesConstants.SupportedPuescDictionaries.CountryCodesFullList
				}
			};

			var dictionaries = DictionariesTestHelper.DictionariesForTest(DictionariesTestHelper.TextXmlPathCL008AES);

			var dictionaryData = new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.CountryCodesFullList,
				DictionariesConstants.SupportedPuescDictionaries.CountryCodesFullList,
				RefDataType.RefCusCodeList);
			var result = dictionaries.GetDictionariesAsRefCusCodeList(dictionaryData);
			Assert.AreEqual(expected.Count, result.Count);

			for (var i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i].ZZD_Code, result[i].ZZD_Code);
				Assert.AreEqual(expected[i].ZZD_Description, result[i].ZZD_Description);
				Assert.AreEqual(expected[i].ZZD_StartDate, result[i].ZZD_StartDate);
				Assert.AreEqual(expected[i].ZZD_EndDate, result[i].ZZD_EndDate);
				Assert.AreEqual(expected[i].ZZD_ZZK_NKCodeType, result[i].ZZD_ZZK_NKCodeType);
			}
		}
	}
}
