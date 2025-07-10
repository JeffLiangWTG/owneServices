
namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public static class Constants
	{
		#region Log Names

		public const string BRNcmXlsLastUpdateDate = "BR_NCM_XLS_LAST_UPDATE_DATE";
		public const string CustomsExchangeRateLogName = "_RefDataRepo_BR_ExchangeRates";
		public const string OMCTecWTOLogName = "_RefDataRepo_BR_OMC_TEC_WTO";
		public const string BitAndBKLogName = "_RefDataRepo_BR_Bit_And_Bk";
		public const string GMCLogName = "_RefDataRepo_BR_GMC";
		public const string TIPILogName = "_RefDataRepo_BR_TIPI";
		public const string TecLogName = "_RefDataRepo_BR_TEC";
		public const string LebitLogName = "_RefDataRepo_BR_LEBIT";
		public const string AutoPartsLogName = "_RefDataRepo_BR_AUTO_PARTS_LIST";
		public const string COVIDLogName = "_RefDataRepo_BR_COVID";
		public const string LETECLogName = "_RefDataRepo_BR_LETEC";
		public const string IPITableLogName = "_RefDataRepo_BR_IPI_TABLE";
		public const string ConsentingBodyLogName = "_RefDataRepo_BR_ConsentingBody";
		public const string GSTPLogName = "_RefDataRepo_BR_GSTP";
		public const string LRTIILogName = "_RefDataRepo_BR_LRTII";
		public const string ReasonTemporaryAdmissionLogName = "_RefDataRepo_BR_ReasonTemporaryAdmission";
		public const string PaymentModalityLogName = "_RefDataRepo_BR_PaymentModality";
		public const string MethodOfPaymentLogName = "_RefDataRepo_BR_MethodOfPayment";
		public const string AllTariffRatesLogName = "_RefDataRepo_BR_AllTariffRates";
		public const string TaxationRegimeDuty = "_RefDataRepo_BR_TaxationRegimeDuty";
		public const string NaladiNccaLogName = "_RefDataRepo_BR_NaladiNcca";
		public const string NaladiShLogName = "_RefDataRepo_BR_NaladiSh";
		public const string PisCofinsLogName = "_RefDataRepo_BR_PisCofins";
		public const string PisCofinsLegalBasisLogName = "_RefDataRepo_BR_PisCofinsLegalBasis";
		public const string LaiaAgreementsLogName = "_RefDataRepo_BR_LAIA_AGREEMENTS_LIST";
		public const string TariffBRCharacteristicNveLogName = "_RefDataRepo_BR_TariffBRCharacteristicNve";
		public const string LaiaAgreementsTableLogName = "_RefDataRepo_BR_LAIA_AGREEMENTS_TABLE";
		public const string WarehousingSectorsLogName = "_RefDataRepo_BR_WarehousingSectors";
		public const string SACUTradeGroupLogName = "_RefDataRepo_BR_SACUTradeGroup";
		public const string PisCofinsBaseCalculationReductionLogName = "_RefDataRepo_BR_PisCofinsBaseCalculationReduction";
		public const string TariffAgreementsLogName = "_RefDataRepo_BR_AGREEMENTS";
		public const string ExTariffIPILogName = "_RefDataRepo_BR_ExTariffIPI";
		public const string TariffBRCharacteristicNcmLogName = "_RefDataRepo_BR_TariffBRCharacteristicNcm";
		public const string TariffBRCharacteristicNcmTeLogName = "_RefDataRepo_BR_TariffBRCharacteristicNcmTe";
		public const string SpecialClearanceAttributesProcedureLogName = "_RefDataRepo_BR_SpecialClearanceAttributesProcedure";
		public const string TariffTributaryLogName = "_RefDataRepo_BR_TariffTributary";
		public const string AttributesLogName = "_RefDataRepo_BR_Attributes";
		public const string AttributesTestLogName = "_RefDataRepo_BR_AttributesTest";
		public const string DispatchInstructionDocumentLogName = "_RefDataRepo_BR_DispatchInstructionDocument";
		public const string DispatchInstructionDocumentTestLogName = "_RefDataRepo_BR_DispatchInstructionDocumentTest";
		public const string TariffRatesLogName = "_RefDataRepo_BR_TariffRates";

		#endregion

		public static class ProgramFunctions
		{
			public const string CustomsTariffUnitOfMeasureCode = "TARIFFS_UOM";
			public const string NomenclatureGroupCode = "NOMENCLATURE_GROUP";
			public const string ExchangeRateCode = "EXCHANGERATES";
			public const string CustomsOfficeCode = "CUSTOMSOFFICES";
			public const string CustomsTariffSubitemCode = "TARIFFS_SUBITEM";
			public const string TariffBRCharacteristicNcmCode = "TARIFFS_CHARACTERISTIC_NCM";
			public const string TariffBRCharacteristicNcmTestCode = "TARIFFS_CHARACTERISTIC_NCM_TEST";
			public const string TariffBRCharacteristicNveCode = "TARIFFS_CHARACTERISTIC_NVE";
			public const string ExportProcedure = "PROCEDURE_CPC";
			public const string ImportSiscomexProcedure = "PROCEDURE_ISW";
			public const string ImportSiscomexProcedureManual = "MANUAL_PROCEDURE_ISW";
			public const string CustomsEnclosureCode = "CUSTOMS_ENCLOSURE";
			public const string CustomsTariffWTOCode = "CUSTOMS_TARIFF_WTO";
			public const string CustomsLPCOCode = "CUSTOMS_LPCO";
			public const string CustomsTariffGMCCode = "CUSTOMS_GMC";
			public const string CustomsTariffRateTIPI = "CUSTOMS_TIPI";
			public const string CustomsTariffTec = "CUSTOMS_TARIFF_TEC";
			public const string CustomsTariffAutoPartsList = "CUSTOMS_TARIFF_AUTO_PARTS_LIST";
			public const string CustomsTariffRateCovidCode = "CUSTOMS_TARIFF_COVID";
			public const string CustomsTariffRateLetecCode = "CUSTOMS_RATE_LETEC";
			public const string CustomsTariffIPITableCode = "CUSTOMS_IPI_TABLE";
			public const string CustomsTariffLebitCode = "CUSTOMS_TARIFF_LEBIT";
			public const string CustomsConsentingBodyCode = "CUSTOMS_CONSENTING_BODY";
			public const string CustomsTariffBitAndBkCode = "CUSTOMS_TARIFF_BIT_AND_BK";
			public const string CustomsDutyGSTPCode = "CUSTOMS_DUTY_GSTP";
			public const string WarehousingSectorsCode = "WAREHOUSING_SECTORS";
			public const string CustomsTariffRate = "CUSTOMS_TARIFF_RATE";
			public const string CustomsTariffDetaches = "CUSTOMS_TARIFF_DETACHES";
			public const string CustomsReasonTemporaryAdmission = "CUSTOMS_REASON_TEMPORARY_ADMISSION";
			public const string CustomsMethodOfPayment = "CUSTOMS_METHOD_OF_PAYMENT";
			public const string CustomsTariffVigentRate = "CUSTOMS_TARIFF_VIGENT_RATE";
			public const string CustomsTaxationRegimeDuty = "CUSTOMS_TAXATION_REGIME_DUTY";
			public const string CustomsNaladiNcca = "CUSTOMS_NALADI_NCCA";
			public const string CustomsNaladiSh = "CUSTOMS_NALADI_SH";
			public const string DutyLegalBasis = "DUTY_LEGAL_BASIS";
			public const string DutyLaiaAgreements = "DUTY_LAIA_AGREEMENTS";
			public const string PisCofinsLegalBasis = "PIS_COFINS_LEGAL_BASIS";
			public const string SACUTradeGroup = "SACU_TRADE_GROUP";
			public const string PisCofinsReductionTariff = "PIS_COFINS_REDUCTION_TARIFF";
			public const string TariffAgreements = "TARIFF_AGREEMENTS";
			public const string CustomsTariffExTariffIPI = "CUSTOMS_TARIFF_EXTARIFFIPI";
			public const string SpecialClearanceAttributesProcedure = "CUSTOMS_SPECIAL_CLEARANCE_ATTRIBUTES_PROCEDURE";
			public const string TariffTributary = "TARIFF_TRIBUTARY";
			public const string TariffAttributes = "TARIFF_ATTRIBUTES";
			public const string TariffAttributesTest = "TARIFF_ATTRIBUTES_TEST";
			public const string DispatchInstructionDocument = "DISPATCH_INSTRUCTION_DOCUMENT";
			public const string DispatchInstructionDocumentTest = "DISPATCH_INSTRUCTION_DOCUMENT_TEST";
			public const string TariffRates = "TARIFF_RATES";
		}

		public static class Groups
		{
			public const string ALL = "ALL";
		}

		public static class TariffTypes
		{
			public static class Codes
			{
				public const string HSN = "HSN";
				public const string GSTP = "GSTP";
				public const string WTOIII = "WTOL3";
				public const string LEBIT = "LEBIT";
				public const string AutoPartList = "AUTOR";
				public const string ExTariffIPI = "IPIEX";
				public const string EADOC = "EADOC";
				public const string EADTE = "EADTE";
			}

			public static class Descriptions
			{
				public const string EADOC = "Electronically Attached Documents";
				public const string EADTE = "Electronically Attached Documents (Test)";
			}

			public static string GetDescritionFromCode(string code)
			{
				switch (code)
				{
					case Codes.EADOC:
						return Descriptions.EADOC;
					case Codes.EADTE:
						return Descriptions.EADTE;
					default:
						return string.Empty;
				}
			}
		}

		public static class ShipmentTypes
		{
			public const string Export = "EXP";
			public const string ImportSiscomex = "ISW";
		}

		public static class DataGroupingCodes
		{
			public const string Brazil = "BR";
			public const string Mercosul = "NCM";
		}

		public static class TradeGroupCodes
		{
			public const string GSTP = "GSTP";
			public const string WTO = "WTO";
			public const string LEBIT = "LEBIT";
			public const string ALL = "ALL";
		}

		public static class TariffRateTypes
		{
			public const string DTY = "DTY";
			public const string IPI = "IPI";
		}

		public static class TariffUnitOfMeasureCodes
		{
			public const string CustomsUOM1 = "CU1";
		}

		public static class TariffTributary
		{
			public static class CountryBlock
			{
				public const string MERCOSUL = "MERCOSUL";
			}
		}

		public static class ExchangeRateTypes
		{
			public const string Customs = "CUS";
			public const string CustomsExport = "CUE";
		}

		public static class RefCusCodeTypes
		{
			public static class CustomsConsentingBody
			{
				public const string Code = "CUSCB";
				public const string Description = "Customs Consenting Body";
			}

			public static class CustomsEnclosure
			{
				public const string Code = "FAC";
				public const string Description = "Customs Enclosure";
			}

			public static class CustomsOffice
			{
				public const string Code = "CUSOF";
				public const string Description = "Customs Office";
			}

			public static class CustomsReasonTemporaryAdmission
			{
				public const string Code = "MATMP";
				public const string Description = "Customs Reason Temporary Admission";
			}

			public static class CustomsMethodOfPayment
			{
				public const string Code = "EXMOP";
				public const string Description = "Exchange Hedge Method of Payment";
			}

			public static class PisCofinsLegalBasis
			{
				public const string Code = "LRTPC";
				public const string Description = "Legal Basis Taxation Regime - PIS-COFINS";
			}

			public static class DutyLegalBasis
			{
				public const string Code = "LRTII";
				public const string Description = "Legal Basis Taxation Regime - Duty";
			}

			public static class CustomsTaxationRegimeDuty
			{
				public const string Code = "TR";
				public const string Description = "Taxation Regime";
			}

			public static class DutyLaiaAgreementCodes
			{
				public const string Code = "ALAIA";
				public const string Description = "Agreements LAIA";
			}

			public static class WarehousingSectorsCodes
			{
				public const string Code = "SCTR";
				public const string Description = "Warehousing Sectors";
			}

			public static class TariffAgreementsCodes
			{
				public const string Code = "TA";
				public const string Description = "Tariff Agreements";
			}
		}

		public static class RefCusCodeListAttributes
		{
			public static class TaxDutyRegimeLegalBase
			{
				public const string Code = "TAXDUTYREGIMELEGALBASE";
				public const string Description = "Tax Duty Regime Legal Base";
			}

			public static class PisCofinsLegalBase
			{
				public const string Code = "TAXPISCOFINSREGIMELEGALBASE";
				public const string Description = "Pis Cofins Legal Base";
			}
			public static class AgreementCountry
			{
				public const string Code = "AGREEMENTCOUNTRY";
				public const string Description = "Agreement Country";
			}

			public static class AgreementSubject
			{
				public const string Code = "AGREEMENTSUBJECT";
				public const string Description = "Agreement Subject";
			}

			public static class AgreementLegalAct
			{
				public const string Code = "AGREEMENTLEGALACT";
				public const string Description = "Agreement Legal Act";
			}

			public static class Country
			{
				public const string Code = "Country";
				public const string Description = "Country";
			}

			public static class TradeGroup
			{
				public const string Code = "TradeGroup";
				public const string Description = "Trade Group";
			}

			public static class Type
			{
				public const string Code = "Type";
				public const string Description = "Type";
			}

			public static class LegalActInImportEntry
			{
				public const string Code = "LegalActInImportEntry";
				public const string Description = "Legal Act In Import Entry";
			}

			public static class AgreementCodeInImportEntry
			{
				public const string Code = "AgreementCodeInImportEntry";
				public const string Description = "Agreement Code In Import Entry";
			}
		}

		public static class RefCusTradeGroup
		{
			public static class SACU
			{
				public const string Code = "SACU";
				public const string Description = "Southern African Customs Union";
			}
		}

		public static class Rates
		{
			public static class Codes
			{
				public const string IPI = "1038";
				public const string PIS = "5602";
				public const string COFINS = "5629";
				public const string Duty = "0086";
			}

			public static class Types
			{
				public const string IPI = "IPI";
				public const string PIS = "PIS";
				public const string COFINS = "COF";
				public const string Duty = "DTY";
			}
		}

		public static class HSNTariffDutyRateFileType
		{
			public const string XLS_TEC = "XLS_TEC";
			public const string XLS_RATES = "XLS_RATES";
		}

		public static class TariffAttributes
		{
			public const string LegalActType = "LegalActType";
			public const string LegalActIssuingBody = "LegalActIssuingBody";
			public const string LegalActNumber = "LegalActNumber";
			public const string LegalActNumberEx = "LegalActNumberEx";
			public const string LegalActYear = "LegalActYear";
			public const string EADocumentOperationType = "EADocumentOperationType";

			public static class OperationTypes
			{
				public const string CATP = "CATP";
				public const string DUIMP = "DUIMP";
				public const string LPCO = "LPCO";

				public static string[] ToArray => new[] { CATP, DUIMP, LPCO };
			}
		}

		public static class PreferenceCodes
		{
			public const string FullCollection = "1";
			public const string NORMAL = "NORMAL";
		}

		public static class RefCusTariffBRCharacteristicCodes
		{
			public static class CharacteristicTypes
			{
				public const string NCM = "NCM";
				public const string NVE = "NVE";
				public const string NCMTE = "NCMTE";
			}
		}

		public static class StyleConstants
		{
			public const string List = "LIST";
			public const string Boolean = "BOOLEAN";
			public const string Text = "STRING";
			public const string Number = "NUMBER";
			public const string Compound = "COMPOUND";
			public const string Date = "DATE";
		}

		public static class VariableStyleConstants
		{
			public const string Date = "DATA";
			public const string DateTime = "DATA_HORA";
			public const string StaticList = "LISTA_ESTATICA";
			public const string List = "LISTA";
			public const string Boolean = "BOOLEANO";
			public const string Text = "TEXTO";
			public const string NumberInteger = "NUMERO_INTEIRO";
			public const string NumberFloat = "NUMERO_REAL";
			public const string Compound = "COMPOSTO";
		}

		public static class TariffCharacteristicNCM
		{
			public const string EXP_ONLY = "EXP";
		}

		public static class SpecialSituation
		{
			public const string Name = "SpecialClearance";
			public static class Code
			{
				public const string _2001 = "2001";
				public const string _2002 = "2002";
				public const string _2003 = "2003";
			}
		}

		public static class ProfileTypes
		{
			public static class Codes
			{
				public const string DispatchInstructionDocument = "DOCKEY";
				public const string DispatchInstructionDocumentTest = "DOCKEYTE";
				public const string Tariff = "NCM";
				public const string TariffTest = "NCMTE";
			}

			public static class Descriptions
			{
				public const string DispatchInstructionDocument = "Document Keywords";
				public const string DispatchInstructionDocumentTest = "Document Keywords (Test)";
				public const string Tariff = "BR NCM Attributes";
				public const string TariffTest = "BR NCM Attributes Test";
			}

			public static string GetDescritionFromCode(string code)
			{
				switch (code)
				{
					case Codes.DispatchInstructionDocument:
						return Descriptions.DispatchInstructionDocument;
					case Codes.DispatchInstructionDocumentTest:
						return Descriptions.DispatchInstructionDocumentTest;
					case Codes.Tariff:
						return Descriptions.Tariff;
					case Codes.TariffTest:
						return Descriptions.TariffTest;
					default:
						return string.Empty;
				}
			}
		}

		public static class ProfileAttribute
		{
			public static class Names
			{
				public const string Modality = "Modality";
				public const string LEVEL = "LEVEL";
			}

			public static class Values
			{
				public const string Import = "IMP";
				public const string Export = "EXP";
			}

			public static string GetValueFromBRValue(string brValue)
			{
				switch (brValue)
				{
					case ModalityValues.Import:
						return Values.Import;
					case ModalityValues.Export:
						return Values.Export;
					default:
						return string.Empty;
				}
			}
		}

		public static class ModalityValues
		{
			public const string Import = "Importação";
			public const string Export = "Exportação";
		}

		public static class Booleans
		{
			public const string TRUE = "1";
			public const string FALSE = "0";
			public const string TRUE_TEXT = "TRUE";
			public const string FALSE_TEXT = "FALSE";
		}



		public static class ComparisonSymbols
		{
			public static class CW1Symbols
			{
				public const string EQUAL = "=";
				public const string OR = "|";
				public const string AND = "&";
				public const string NOT_EQUAL = "!=";
				public const string GREATER_THAN = ">";
				public const string GREATER_OR_EQUAL = ">=";
				public const string LESS_THAN = "<";
				public const string LESS_OR_EQUAL = "<=";
			}

			public static class BRSymbols
			{
				public const string EQUAL = "==";
				public const string OR = "||";
				public const string AND = "&&";
				public const string NOT_EQUAL = "!=";
				public const string GREATER_THAN = ">";
				public const string GREATER_OR_EQUAL = ">=";
				public const string LESS_THAN = "<";
				public const string LESS_OR_EQUAL = "<=";
			}
		}

		public static class Objectives
		{
			public const int LPCO = 6;
			public const int TAX_TREATMENT = 8;

			public static class Descriptions
			{
				public const string Objective = "Objetivo";
			}
		}
	}
}
