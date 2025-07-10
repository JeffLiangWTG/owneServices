namespace Enterprise.Customs.Universal
{
	public static class Constants
	{
		#region SuppressResourceStringsCheckRegion

		public static class DataGrouping
		{
			public const string EuropeanUnion = "EUN";
		}

		public static class RateTypes
		{
			public const string Deminimus = "DEM";
			public const string Duty = "DTY";
			public const string ExportTaxes = "EXP";
			public const string Excise = "EXC";
			public const string ExportDuty = "EXP";
			public const string Levy = "LVY";
			public const string AdValoremExcise = "EX1";
			public const string AntiDumping = "ADD";
			public const string Countervailing = "CVD";
			public const string Rebate = "REB";
			public const string Refund = "REF";
			public const string ProvisionalPayment = "PRP";
			public const string Penalty = "PEN";
			public const string ExciseTax = "EXS";
		}

		public static class ConditionClassDescription
		{
			public const string Control = "Customs Control";
			public const string Rate = "Rate";
			public const string Class = "Classification";
			public const string VAT = "VAT";
		}

		public static class ConditionValueType
		{
			public const string SupportingDocument = "SUP";
			public const string SupportingDocumentNoReferenceNumber = "SNR";
			public const string Information = "INF";
		}

		public static class ConditionSeverity
		{
			public const string MSG = "MSG";
		}

		public static class RefCusTariffFilters
		{
			public const string TariffCode = "Tariff Code";
			public const string TariffType = "Tariff Type";
			public const string CountryOrGrouping = "Country/Region or Grouping";
			public const string EffectiveDate = "Effective Date";
			public const string PublishedDate = "Published Date";
			public const string DefaultLanguageDescription = "Description (Default Language)";
			public const string AlternateLanguageDescription = "Description (Alternate Language)";
			public const string TariffRestriction = "Tariff Restriction";
			public const string ShowExpandedResults = "Show Expanded Results";
			public const string UnitOfQuantity1 = "Unit Of Quantity 1";
			public const string UnitOfQuantity2 = "Unit Of Quantity 2";
			public const string UnitOfQuantity3 = "Unit Of Quantity 3";
			public const string NotAvailable = "Not Available";
		}

		public static class ZZRefCarrierFilters
		{
			public const string Code = "Code";
			public const string Country = "Country/Region";
			public const string Description = "Description";
			public const string CarrierType = "Carrier Type";
			public const string TransportMode = "Transport Mode";
			public const string CountryOrGrouping = "Country/Region or Grouping";
		}

		public static class ZZRefCusCodeListFilters
		{
			public const string ListType = "List Type";
			public const string ListTypeDescription = "List Type Description";
			public const string CountryOrGrouping = "Country/Region or Grouping";
			public const string Code = "Code";
			public const string Description = "Description";
			public const string EffectiveDate = "Effective Date";
			public const string System = "Is System Defined";
			public const string TransportMode = "Transport Mode";
			public const string AttributeName = "Attribute Name";
			public const string AttributeValue = "Attribute Value";
			public const string AttributeTransportMode = "Attribute Transport Mode";
		}

		public static class ZZRefCusMapFilters
		{
			public const string CountryOrGrouping = "Country/Region or Grouping";
			public const string CustomsValue = "Customs Value";
			public const string CW1OrCommercialValue = "CW1 or Commercial Value";
			public const string MapType = "Mapping Type";
			public const string System = "Is System Defined";
		}

		public static class ZZRefCusProcedureFilters
		{
			public const string ProcedureCode = "Procedure Code";
			public const string PreviousProcedureCode = "Previous Procedure Code";
			public const string System = "System";
			public const string Category = "Category";
			public const string Concession = "Concession";
			public const string Description = "Description";
			public const string DataGrouping = "Data Grouping";
			public const string Group = "Group";
			public const string ShipmentType = "Shipment Type";
			public const string StartDate = "Start Date";
			public const string EndDate = "End Date";
			public const string EffectiveDate = "Effective Date";
			public const string CPC = "CPC";
		}

		public static class ZZRefCusRulingFilters
		{
			public const string RulingNumber = "Ruling Number";
			public const string RulingType = "Ruling Type";
			public const string EffectiveDate = "Effective Date";
			public const string AppliesTo = "Applies To";
			public const string AppliesToOrg = "Applies To Organisation";
		}

		public static class RefHarbourRateFilters
		{
			public const string Type = "Type";
			public const string Port = "Port";
			public const string Mode = "Mode";
			public const string Commodity = "Commodity";
			public const string PortTaxType = "Port Tax Type";
			public const string EffectiveDate = "Effective Date";
			public const string CountryOrGrouping = "Country/Region or Grouping";
			public const string RateFormula = "Rate Formula";
		}

		public static class UnitOfMeasureTypes
		{
			public const string StatisticalUOMType = "CU1";
			public const string AdditionalUOMType = "CU2";
			public const string ClassificationUOMType = "RU1";
			public const string CustomsUOM3Type = "CU3";
			public const string CustomsUOM4Type = "CU4";
			public const string CustomsUOM5Type = "CU5";
		}

		public static class TariffTypes
		{
			public const string HarmonizedSystem = "HSN";
			public const string HarmonizedSystemDescription = "Harmonized System Nomenclature";
			public const string ScheduleB = "SHB";
			public const string Import = "IMP";
			public const string Export = "EXP";
			public const string Commodity = "COM";
			public const string ExportUnionAdditional = "TREUA";
			public const string IndianTradeClassification = "CTH";
		}

		public static class FunctionalityTypes
		{
			public const string AESPLUS = "AESPLUS";
			public const string AESJurisdictionNumber = "AESJDXNO";
			public const string AESTransitionPeriod = "AESTP";
			public const string ALUSMELT2024 = "ALUSMELT2024";
			public const string CustomsUccTransitionalArrangmentsApply = "TRANS";
			public const string ExportInvoiceNumber = "EXPINV";
			public const string TemporaryStorage = "TSTORE";
			public const string DeclarationApplicationCode = "DECAPP";
			public const string DeclarationApplicationCodeExports = "DECAPPEXPORTS";
			public const string CNDecMessageV2020GoLiveDate = "DECMSG2020";
			public const string CNBuildMessageInCW1 = "CNBUILDMESSAGEINCW1";
			public const string SuspendDuplicateDeactivation = "DUPLP";
			public const string EMCS = "EMCS";
			public const string EMCS_Phase4_1 = "EMCS3.13";
			public const string ElectronicBond = "EBOND";
			public const string TDTSegmentSplit = "TDTSEGSPLIT";
			public const string CustomsManifest = "CUSMAN";
			public const string TCPMessage = "TCPM";
			public const string Transhipment = "TWINB";
			public const string CHIEF_CDS_EXPORT_DUAL_RUN = "CHIEF_CDS_EXPORT_DUAL_RUN";
			public const string CHIEF_SUNSET_IMP = "CHIEF_SUNSET_IMP";
			public const string CHIEF_SUNSET_EXP = "CHIEF_SUNSET_EXP";
			public const string ChiefDecommission = "ChiefDecommission";
			public const string ZATwoStepClearing = "ZATWOSTEPCLEARING";
			public const string EUImportControlSystem = "EUICS";
			public const string TRFOZBY = "FOZBY";
			public const string SGCMD = "SGCMD";
			public const string ZACALINFMessaging = "ZACALINF";
			public const string ZABLNSVatUplift = "ZABLNSVATUPLIFT";
			public const string ZAManifestCaseNumbers = "ZAMANIFESTCASENUMBERS";
			public const string MaxEntryLines = "MAXENTRYLINES";
			public const string UYMAN = "UYMAN";
			public const string ZAUSEPASSPORT = "ZAUSEPASSPORT";
			public const string PGAFSIS = "PGAFSIS";
			public const string AMSEGG = "AMSEGG";
			public const string AMSPNT = "AMSPNT";
			public const string AMSNOP = "AMSNOP";
			public const string PostalCodeIsRequiredForChinaMF = "USCNZIP";
			public const string COLS = "COLS";
			public const string PCOLS = "PCOLS";
			public const string NEXDOC_GRN = "NEXDOC_GRN";
			public const string NEXDOC_HOR = "NEXDOC_HOR";
			public const string NEXDOC_MEA = "NEXDOC_MEA";
			public const string NEXDOC_OTH = "NEXDOC_OTH";
			public const string QENT = "QENT";
			public const string PQENT = "PQENT";
			public const string PGAFWS = "PGAFWS";
			public const string USCPSC = "USCPSC";
			public const string USOMC = "USOMC";
			public const string PGADataCorrection2ndPhase = "PGADataCorrection2ndPhase";
			public const string NewFTZe214 = "NewFTZe214";
			public const string FTZZoneID9 = "FTZZoneID9";
			public const string EXDOCS_Errata48 = "EXDOCS_Errata48";
			public const string TMPIMP = "TMPIMP";
			public const string TMPEXP = "TMPEXP";
			public const string IWDPROC = "IWDPROC";
			public const string OWDPROC = "OWDPROC";
			public const string Risk = "RISK";
			public const string EXDOCS_Errata51 = "EXDOCS_Errata51";
			public const string EXDOCS_Errata53_1 = "EXDOCS_Errata53_1";
			public const string EXDOCS_Errata54 = "EXDOCS_Errata54";
			public const string GVMS = "GVMS";
			public const string ZAVALA100PERCENT = "ZAVALA100PERCENT";
			public const string CargRlsCES = "CargRlsCES";
			public const string TypeAMSHBRE = "AMSHBR";
			public const string GBIACTIVE = "GBIACTIVE";
			public const string USFTARECONIND = "USFTAReconInd";
			public const string USSmelt = "SMELT";
			public const string NCTSTransitionPeriod = "NC5TP";
			public const string NCTSPhase4 = "NCTS_PHASE4";
			public const string NCTSPhase5Override = "NCTS_PHASE5";
			public const string Ncts5UseApi21 = "Ncts5UseApi21";
			public const string SendForeignEoriToCds = "SendForeignEoriToCds";
			public const string BlueErrorMessageTypeAfterMessaging = "BlueErrorMessageTypeAfterMessaging";
			public const string H4ForbidWritingOff = "H4ForbidWritingOff";
			public const string CDS_ILE_PHASE1 = "CDS_ILE_PHASE1";
			public const string CDS_ILE_PHASE2 = "CDS_ILE_PHASE2";
			public const string ACEHTS = "ACEHTS";
			public const string CAMQWAR = "CAMQWAR";
			public const string NMFSCOAACTIVE = "NMFSCOAACTIVE";
			public const string NewEntrySummaryQuery = "NEWENTSUMQRY";
			public const string ImportMessageVersionUCC6 = "IMUC6";
			public const string GBAllowSendILEToMCP = "GBMCPILE";
			public const string CarmR2 = "CARMR2";
			public const string CBSABO = "CBSABO";
			public const string RPPGrace = "RPPGrace";
			public const string SeaCargoHouse = "SCRSH";
			public const string PilotSeaCargoHouse = "PSCRH";
			public const string GACMLT = "GACMLT";
			public const string EDA86 = "EDA86";
			public const string APHIS2024 = "APHIS2024";
			public const string EMCSGB_ValidationAttributeAllowed = "EMCSGB_ValidationAttributeAllowed";
			public const string ZAAddInvoiceDetailsToCUSDECMessage = "ZAINVDET";
			public const string CHNE015V3 = "CHNE015V3";
			public const string CHNT015V4 = "CHNT015V4";
			public const string CHNT044V4 = "CHNT044V4";
			public const string CHNT515V4 = "CHNT515V4";
			public const string DMSTransitionPeriod = "DMSTP";
			public const string ICS2TransportModeROA = "ICS2ROA";
			public const string ICS2TransportModeRAI = "ICS2RAI";
			public const string UCMPServiceTask = "UCMP";
			public const string UCMPServiceTaskAMS = "UCMPAMS";
			public const string HmrcDigitalPrompts = "HmrcDigitalPrompts";
			public const string USCLeCERT = "CLeCERT";
			public const string HSAssistant = "HSAssistant";
			public const string EnableFWS = "EnableFWS";
			public const string Sanctions = "SANCTIONS";
		}

		public static class UniversalReferenceBusinessProvider
		{
			public const string Default = "Default";
		}

		public static class ZZStoreProcedureReference
		{
			public static class GetRateSelectionCriteriaInfo
			{
				public const string QualifiedName = "GetRateSelectionCriteriaInfo";

				public static class Parameters
				{
					public const string SelectionCriteria = "@SelectionCriteria";
					public const string LanguageCode = "@LanguageCode";
				}

				public static class Columns
				{
					public const string EffectiveDate = "EffectiveDate";
					public const string TradeGroupCountry = "TradeGroupCountry";
					public const string ZY1_RateType = "ZY1_RateType";
					public const string ZY1_RateCode = "ZY1_RateCode";
					public const string ZZT_OrderNumber = "ZZT_OrderNumber";
					public const string ZZT_AdditionalCode = "ZZT_AdditionalCode";
					public const string ZZA_TradeGroup = "ZZA_TradeGroup";
					public const string ZZA_Description = "ZZA_Description";
					public const string ZZS_Preference = "ZZS_Preference";
					public const string ZZS_Description = "ZZS_Description";
					public const string SecondTradeGroup = "SecondTradeGroup";
					public const string TranslatedPreferenceDescription = "TranslatedPreferenceDescription";
					public const string Direction = "Direction";
				}
			}

			public static class GetConditionApplicabilitiesByCriteria
			{
				public const string QualifiedName = "GetConditionApplicabilitiesByCriteria";

				public static class Parameters
				{
					public const string ConditionCriteriaTvp = "@ConditionCriteriaTvp";
					public const string SecondTradeGroupTvp = "@SecondTradeGroupTvp";
				}

				public static class Columns
				{
					public const string EffectiveDate = "EffectiveDate";
					public const string TradeGroupCountry = "TradeGroupCountry";
					public const string ZX2_ConditionClass = "ZX2_ConditionClass";
					public const string ZX2_ConditionType = "ZX2_ConditionType";
					public const string ZZT_OrderNumber = "ZZT_OrderNumber";
					public const string ZZT_AdditionalCode = "ZZT_AdditionalCode";
					public const string ZZA_TradeGroup = "ZZA_TradeGroup";
					public const string ZZA_Description = "ZZA_Description";
					public const string ZZS_Preference = "ZZS_Preference";
					public const string ZZS_Description = "ZZS_Description";
					public const string SecondTradeGroup = "SecondTradeGroup";
				}
			}

			public static class GetMultiCriteriaSetRates
			{
				public const string QualifiedName = "dbo.GetMultiCriteriaSetRates";

				public static class Parameters
				{
					public const string RateCriteriaTvp = "@RateCriteriaTvp";
					public const string SecondTradeGroupTvp = "@SecondTradeGroupTvp";
				}

				public static class Columns
				{
					public const string CriteriaId = "CriteriaId";
					public const string RatePk = "RatePk";
				}
			}

			public static class TvpRateSelectionCriteria_V2
			{
				public const string QualifiedName = "dbo.TVP_RateSelectionCriteria_V2";

				public static class Columns
				{
					public const string CriteriaId = "CriteriaId";
					public const string TariffPK = "TariffPK";
					public const string EffectiveDate = "EffectiveDate";
					public const string TradeGroupCountry = "TradeGroupCountry";
					public const string DataGrouping = "DataGrouping";
					public const string Preference = "Preference";
					public const string AdditionalCodesXml = "AdditionalCodesXml";
					public const string OrderNumber = "OrderNumber";
					public const string RateType = "RateType";
					public const string RateCode = "RateCode";
					public const string Direction = "Direction";
				}
			}

			public static class TvpSecondTradeGroup
			{
				public const string QualifiedName = "dbo.TVP_SecondTradeGroup";

				public static class Columns
				{
					public const string Id = "Id";
					public const string CriteriaId = "CriteriaId";
					public const string SecondTradeGroup = "SecondTradeGroup";
				}
			}
		}

		public static class RefCusCodeListAttributeName
		{
			public static class ValueDataTypes
			{
				public const string String = "STRING";
				public const string Boolean = "BOOLEAN";
				public const string Integer = "INTEGER";
				public const string Decimal = "DECIMAL";
			}
		}

		public static class RefCusTaxOrFeeTypes
		{
			public const string Deminimus = "DEM";
			public const string ExportDeminimus = "EXD";
			public const string CACLVSRemissionThresholdAll = "RT1";
			public const string CACLVSRemissionThresholdDutyAndTax = "RT2";
			public const string CACLVSRemissionThresholdDutyOnly = "RT3";
			public const string VAT = "VAT";
		}

		public static class RefSysConfig
		{
			public static class ConfigCodes
			{
				public const string NoOfDaysIETransactionIDShouldBeKept = "IETIDDAY";
			}
		}

		public static class ProfileQuestion
		{
			public static class AnswerDataTypes
			{
				public const string List = "LIST";
				public const string String = "STRING";
				public const string Number = "NUMBER";
				public const string Boolean = "BOOLEAN";
				public const string Compound = "COMPOUND";
				public const string Date = "DATE";
			}

			public static class AttributeNames
			{
				public const string Caption = "Caption";
				public const string Example = "Exemplo";
			}
		}

		public static class RefCusTariffAttribute
		{
			public static class AttributeName
			{
				public const string ConveyanceRequired = "CONVEYANCEREQUIRED";
			}

			public static class AttributeValue
			{
				public const string Y = "Y";
			}
		}
	}

	#endregion
}
