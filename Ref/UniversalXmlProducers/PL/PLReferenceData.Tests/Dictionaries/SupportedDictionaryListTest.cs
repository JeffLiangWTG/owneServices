using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;
using NUnit.Framework;
using CW1 = CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionariesConstants.SupportedPuescDictionariesCW1Codes;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionariesConstants.SupportedPuescDictionaries;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper.RefDataType;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionariesConstants;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Dictionaries;

[TestFixture]
sealed class SupportedDictionaryListTest
{
	[Test]
	public void TestSupportedPuescDictionariesPLCodes() => Assert.Multiple(() =>
	{
		var excludedCodes = new[] {
			ExportProcedureCodes,
			ImportPreviousProcedureCodes,
			ExportPreviousProcedureCodes,
			ExportConcessions
		};
		AssertSupportedCodes(29, typeof(SupportedPuescDictionaries), x => x.Code, excludedCodes);
	});

	[Test]
	public void TestSupportedPuescDictionariesCW1Codes() => Assert.Multiple(() =>
	{
		AssertSupportedCodes(35, typeof(CW1), x => x.CW1Code, Array.Empty<string>());
	});

	[TestCase(TransactionCodes, CW1.TransactionCodes, RefCusCodeList, false)]
	[TestCase(ExportRequiredDocuments, CW1.ExportRequiredDocuments, RefCusCodeList, true)]
	[TestCase(ImportRequiredDocuments, CW1.ImportRequiredDocuments, RefCusCodeList, false)]
	[TestCase(AdditionalInformationCodes, CW1.AdditionalInformationCodes, RefCusCodeList, false)]
	[TestCase(ExportPreviousDocuments, CW1.ExportPreviousDocuments, RefCusCodeList, true)]
	[TestCase(ImportPreviousDocuments, CW1.ImportPreviousDocuments, RefCusCodeList, false)]
	[TestCase(NationalAdditionalCodesForSADField33, CW1.NationalAdditionalCodesForSADField33, RefCusCodeList, false)]
	[TestCase(CarsMarkAndModelCodes, CW1.CarsMarkAndModelCodes, RefCusCodeList, false)]
	[TestCase(AuthorisationType, CW1.AuthorisationType, RefCusCodeList, true)]
	[TestCase(NatureOfTransactionCode, CW1.NatureOfTransactionCode, RefCusCodeList, true)]
	[TestCase(ExportAdditionalInformation, CW1.ExportAdditionalInformation, RefCusCodeList, true)]
	[TestCase(ExportAdditionalReferencesType, CW1.ExportAdditionalReferencesType, RefCusCodeList, true)]
	[TestCase(ExportTransportDocumentCodes, CW1.ExportTransportDocumentCodes, RefCusCodeList, true)]
	[TestCase(EXPCustomsDeclarationUnitsOfQuantity, CW1.EXPCustomsDeclarationUnitsOfQuantity, RefCusCodeList, true)]
	[TestCase(NctsSupportingDocuments, CW1.NctsSupportingDocuments, RefCusCodeListMerged, true)]
	[TestCase(NctsAuthorisationType, CW1.NctsAuthorizationType, RefCusCodeListMerged, true)]
	[TestCase(NctsAdditionalInformation, CW1.NctsAdditionalInformation, RefCusCodeListMerged, true)]
	[TestCase(NctsGuarenteeType, CW1.NctsGuarenteeType, RefCusCodeListMerged, true)]
	[TestCase(EnquiryInformationCode, CW1.EnquiryInformationCode, RefCusCodeListMerged, true)]
	[TestCase(AlternativeEvidenceType, CW1.AlternativeEvidenceType, RefCusCodeListMerged, true)]
	[TestCase(EU_COUNTRY_CODES, CW1.EX15, RefCusCodeList, false)]
	[TestCase(EU_COUNTRY_CODES, CW1.IM17, RefCusCodeList, false)]
	[TestCase(COUNTRY_CODES, CW1.IMP34, RefCusCodeListWithLanguage, false)]
	[TestCase(CountryCodesFullList, CW1.EXP34, RefCusCodeListWithLanguage, true)]
	[TestCase(PreviousDocumentsSpecialProcedures, CW1.PreviousDocumentsSpecialProcedures, RefCusCodeList, false)]
	[TestCase(EconomicConditionsInSpecialProcedures, CW1.EconomicConditionsInSpecialProcedures, RefCusCodeList, false)]
	[TestCase(IdentificationOfGoodsInSpecialProcedures, CW1.IdentificationOfGoodsInSpecialProcedures, RefCusCodeList, false)]
	public void TestSupportedPuescDictionaries_SimplePLToCW1(string plCode, string cw1Code, RefDataType refDataType, bool publishedByTestPuesc)
		=> AssertDictionaryData(plCode, cw1Code, refDataType, publishedByTestPuesc);

	[TestCase(
		ImportProcedureCodes,
		CW1.CustomsProcedures,
		new[] {
			ImportPreviousProcedureCodes,
			ExportProcedureCodes,
			ExportPreviousProcedureCodes })]
	[TestCase(
		ImportConcessions,
		CW1.CustomsConcessions,
		new[] { ExportConcessions })]
	public void TestSupportedPuescDictionariesCW1Codes_ProcedureCodes(string plCode, string cw1Code, string[] additionalDictionaries)
		=> AssertDictionaryData(plCode, cw1Code, RefCusCodeListMerged, true, additionalDictionaries);

	void AssertDictionaryData(string code, string cw1Code, RefDataType refDataType, bool publishedByTestPuesc = false, string[] additionalDictionaries = null) => Assert.Multiple(() =>
	{
		var dictionaryData = SupportedDictionaryList.SupportedDictionaries.Single(x => x.CW1Code == cw1Code && x.Code == code);
		Assert.AreEqual(code, dictionaryData.Code, "PUESC code");
		Assert.AreEqual(cw1Code, dictionaryData.CW1Code, "CW1 code");
		Assert.AreEqual(additionalDictionaries?.Length ?? 0, dictionaryData.AdditionalDictionaries.Count, "additionalDictionaries amount");
		Assert.AreEqual(publishedByTestPuesc, dictionaryData.PublishedByTestPuesc, "publishedByTestPuesc");
		Assert.AreEqual(refDataType, dictionaryData.RefDataType, "refDataType");

		foreach (var additionalDictionary in dictionaryData.AdditionalDictionaries)
		{
			var additionalCode = additionalDictionary.Code;
			Assert.Contains(additionalCode, additionalDictionaries, $"{cw1Code} - code {additionalCode}");
		}
	});

	IEnumerable<FieldInfo> GetConstantFieldsFromClass(Type type) => type
		.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
		.Where(fi => fi.IsLiteral && !fi.IsInitOnly);

	void AssertSupportedCodes(int expectedAmount,
		Type testedDictionaryCodesType,
		Func<DictionaryData, string> selector,
		IEnumerable<string> excludedCodes)
	{
		var data = SupportedDictionaryList.SupportedDictionaries.Select(selector).Distinct().ToArray();
		Assert.AreEqual(expectedAmount, data.Length, $"Expected {expectedAmount} codes");

		var fieldInfos = GetConstantFieldsFromClass(testedDictionaryCodesType);
		foreach (var fieldInfo in fieldInfos)
		{
			var value = fieldInfo.GetRawConstantValue();
			if (!excludedCodes.Contains(value))
			{
				Assert.Contains(value, data, $"{fieldInfo.Name}");
			}
		}
	}
}
