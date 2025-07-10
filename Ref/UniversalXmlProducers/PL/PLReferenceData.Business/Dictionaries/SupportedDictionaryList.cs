using System.Collections.Generic;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryHelper;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;

public static class SupportedDictionaryList
{
	static readonly IReadOnlyList<DictionaryData> PuescDictionaries = new List<DictionaryData>
	{
		// use custom dictionary schema
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.CarsMarkAndModelCodes, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CarsMarkAndModelCodes, RefDataType.RefCusCodeList ), // Model & Mark schema

		// use PuescDictionary schema - polish version
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.TransactionCodes, DictionariesConstants.SupportedPuescDictionariesCW1Codes.TransactionCodes, RefDataType.RefCusCodeList ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ExportRequiredDocuments, DictionariesConstants.SupportedPuescDictionariesCW1Codes.ExportRequiredDocuments, RefDataType.RefCusCodeList, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportRequiredDocuments, DictionariesConstants.SupportedPuescDictionariesCW1Codes.ImportRequiredDocuments, RefDataType.RefCusCodeList ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.AdditionalInformationCodes, DictionariesConstants.SupportedPuescDictionariesCW1Codes.AdditionalInformationCodes, RefDataType.RefCusCodeList ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportPreviousDocuments, DictionariesConstants.SupportedPuescDictionariesCW1Codes.ImportPreviousDocuments, RefDataType.RefCusCodeList ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.NationalAdditionalCodesForSADField33, DictionariesConstants.SupportedPuescDictionariesCW1Codes.NationalAdditionalCodesForSADField33, RefDataType.RefCusCodeList ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportProcedureCodes, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CustomsProcedures, RefDataType.RefCusCodeListMerged,
			null,
			new List<DictionaryData>
			{
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ExportProcedureCodes, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CustomsProcedures, RefDataType.RefCusCodeListMerged, publishedByTestPuesc : true),
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportPreviousProcedureCodes, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CustomsProcedures, RefDataType.RefCusCodeListMerged, publishedByTestPuesc: true),
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ExportPreviousProcedureCodes, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CustomsProcedures, RefDataType.RefCusCodeListMerged, publishedByTestPuesc : true)
			},
			publishedByTestPuesc: true),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ImportConcessions, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CustomsConcessions, RefDataType.RefCusCodeListMerged,
			null,
			new List<DictionaryData>
			{
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ExportConcessions, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CustomsConcessions, RefDataType.RefCusCodeListMerged, publishedByTestPuesc: true)
			},
			publishedByTestPuesc: true),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.NatureOfTransactionCode, DictionariesConstants.SupportedPuescDictionariesCW1Codes.NatureOfTransactionCode, RefDataType.RefCusCodeList, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.AuthorisationType, DictionariesConstants.SupportedPuescDictionariesCW1Codes.AuthorisationType, RefDataType.RefCusCodeList, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ExportAdditionalInformation, DictionariesConstants.SupportedPuescDictionariesCW1Codes.ExportAdditionalInformation, RefDataType.RefCusCodeList, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ExportAdditionalReferencesType, DictionariesConstants.SupportedPuescDictionariesCW1Codes.ExportAdditionalReferencesType, RefDataType.RefCusCodeList, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ExportTransportDocumentCodes, DictionariesConstants.SupportedPuescDictionariesCW1Codes.ExportTransportDocumentCodes, RefDataType.RefCusCodeList, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.ExportPreviousDocuments, DictionariesConstants.SupportedPuescDictionariesCW1Codes.ExportPreviousDocuments, RefDataType.RefCusCodeList, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EXPCustomsDeclarationUnitsOfQuantity, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EXPCustomsDeclarationUnitsOfQuantity, RefDataType.RefCusCodeList, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EnquiryInformationCode, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EnquiryInformationCode, RefDataType.RefCusCodeListMerged, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.AlternativeEvidenceType, DictionariesConstants.SupportedPuescDictionariesCW1Codes.AlternativeEvidenceType, RefDataType.RefCusCodeListMerged, publishedByTestPuesc: true ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.NctsSupportingDocuments, DictionariesConstants.SupportedPuescDictionariesCW1Codes.NctsSupportingDocuments, RefDataType.RefCusCodeListMerged, publishedByTestPuesc: true),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.NctsAuthorisationType, DictionariesConstants.SupportedPuescDictionariesCW1Codes.NctsAuthorizationType, RefDataType.RefCusCodeListMerged, publishedByTestPuesc: true),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.NctsAdditionalInformation, DictionariesConstants.SupportedPuescDictionariesCW1Codes.NctsAdditionalInformation, RefDataType.RefCusCodeListMerged, publishedByTestPuesc: true),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.NctsGuarenteeType, DictionariesConstants.SupportedPuescDictionariesCW1Codes.NctsGuarenteeType, RefDataType.RefCusCodeListMerged, publishedByTestPuesc: true),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.PreviousDocumentsSpecialProcedures, DictionariesConstants.SupportedPuescDictionariesCW1Codes.PreviousDocumentsSpecialProcedures, RefDataType.RefCusCodeList, publishedByTestPuesc: false ),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.IdentificationOfGoodsInSpecialProcedures, DictionariesConstants.SupportedPuescDictionariesCW1Codes.IdentificationOfGoodsInSpecialProcedures, RefDataType.RefCusCodeList),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EconomicConditionsInSpecialProcedures, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EconomicConditionsInSpecialProcedures, RefDataType.RefCusCodeList),

		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CO15, RefDataType.RefCusCodeListWithAttributeImport,
			new List<ExcludedDictionaryCodes>
			{
				new ExcludedDictionaryCodes { ExcludedCodes = new List<string> { "CH", "NO", "IS", "LI", "TR", "MK", "RS", "XS" } }
			},
			new List<DictionaryData>
			{
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CO15, RefDataType.RefCusCodeListWithAttributeExport )
			}
		),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CO17, RefDataType.RefCusCodeListWithAttributeExport, null,
			new List<DictionaryData>
			{
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CO17, RefDataType.RefCusCodeListWithAttributeExport, null, null,
					new List<string> { "QQ", "QR", "QV" }
				),
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.CO17, RefDataType.RefCusCodeListWithAttributeImport )
			}),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EU15, RefDataType.RefCusCodeListWithAttributeImport,
			new List<ExcludedDictionaryCodes>
			{
				new ExcludedDictionaryCodes { DictionaryCode = DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES }
			},
			new List<DictionaryData>
			{
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EU15, RefDataType.RefCusCodeListWithAttributeExport )
			}
		),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.NOT_IN_EU_WPT_COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EU17, RefDataType.RefCusCodeListWithAttributeExport,
			new List<ExcludedDictionaryCodes>
			{
				new ExcludedDictionaryCodes { ExcludedCodes = new List<string> { "AD", "SM" } }
			},
			new List<DictionaryData>
			{
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EU17, RefDataType.RefCusCodeListWithAttributeImport )
			}
		),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EX15, RefDataType.RefCusCodeList),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EX17, RefDataType.RefCusCodeList,
			new List<ExcludedDictionaryCodes>
			{
				new ExcludedDictionaryCodes { DictionaryCode = DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES },
				new ExcludedDictionaryCodes { DictionaryCode = DictionariesConstants.SupportedPuescDictionaries.NOT_IN_EU_WPT_COUNTRY_CODES },
				new ExcludedDictionaryCodes { ExcludedCodes = new List<string> { "QQ", "QR", "QV" } }
			},
			new List<DictionaryData>
			{
				new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EX17, RefDataType.RefCusCodeList, null, null,
					new List<string> { "AD", "SM", "DE", "IT" }
				)
			}),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.IM15, RefDataType.RefCusCodeList,
			new List<ExcludedDictionaryCodes>
			{
				new ExcludedDictionaryCodes { DictionaryCode = DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES },
				new ExcludedDictionaryCodes { ExcludedCodes = new List<string> { "CH", "NO", "IS", "LI", "TR", "MK", "RS", "XS" } }
			}),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.EU_COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.IM17, RefDataType.RefCusCodeList),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.COUNTRY_CODES, DictionariesConstants.SupportedPuescDictionariesCW1Codes.IMP34, RefDataType.RefCusCodeListWithLanguage),
		new DictionaryData(DictionariesConstants.SupportedPuescDictionaries.CountryCodesFullList, DictionariesConstants.SupportedPuescDictionariesCW1Codes.EXP34, RefDataType.RefCusCodeListWithLanguage, publishedByTestPuesc: true),
	};

	public static IReadOnlyList<DictionaryData> SupportedDictionaries => PuescDictionaries;
}
