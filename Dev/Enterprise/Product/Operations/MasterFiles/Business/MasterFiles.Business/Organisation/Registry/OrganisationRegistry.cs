using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public sealed class OrganisationRegistry : RegistryItemSet
	{
		#region Construction

		public static OrganisationRegistry Instance
		{
			get { return instance ?? (instance = new OrganisationRegistry()); }
		}

		[ThreadStatic]
		static OrganisationRegistry instance;

		OrganisationRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : OrganisationsDataRegistry.Categories
		{
			public static MultilingualString Organizations_GUIState { get { return CombineCategories(Organizations, ResString.GetMultilingualString("2aede169-0718-4929-b9f1-d93f46ad3661", "GUI State")); } }
			public static MultilingualString Accounting_JobInvoicing_ProfitShare { get { return CombineCategories(Accounting_JobInvoicing, ResString.GetMultilingualString("7085ef44-6f9e-4c89-bbe6-d60c10efa9df", "Profit Share")); } }
			public static MultilingualString SalesMarketing_TradeLanesSynchronisation { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("21b09bcd-1af3-4efc-9438-e7f37bc38628", "Trade Lanes Synchronization")); } }
		}

		#endregion

		#region Strict enforcement of registration number formats

		public static class RegistrationNumberFormatFields
		{
			public const string AONIF = "NIF";
			public const string ARCUIL = "CUL";
			public const string ARCUIT = "CUI";
			public const string ARDNI = "DNI";
			public const string ARIBL = "IBL";
			public const string ARIBM = "IBM";
			public const string ARIBN = "IBN";
			public const string ARIBS = "IBS";
			public const string AUABN = "ABN";
			public const string AUARN = "ARN";
			public const string AUCCP = "ACP";
			public const string AUEAP = "EAP";
			public const string AUEEN = "EEN";
			public const string AUEEU = "EEU";
			public const string AUESN = "ESN";
			public const string AUNEN = "NEN";
			public const string BRCNPJ = "CJN";
			public const string CABNC = "BNC";
			public const string CABRB = "BRB";
			public const string CABRE = "BRE";
			public const string CABRL = "BRL";
			public const string CABRM = "BRM";
			public const string CACCC = "CAC";
			public const string CargoWiseOneCarrierCode = "C1C";
			public const string CarrierCode = "CCC";
			public const string CAWMI = "WMI";
			public const string CCP = "CCP";
			public const string CLACT = "ACT";
			public const string CLRUTSOL = "CLR";
			public const string CNBSTVAT = "CNB";
			public const string CNVAGVAS = "CNV";
			public const string CONIT = "NIT";
			public const string CRCID = "CCI";
			public const string CRCIJ = "CIJ";
			public const string CRDIM = "DIM";
			public const string CREAC = "EAC";
			public const string CRNIT = "CNI";
			public const string CRUBI = "UBI";
			public const string DEHRB = "HRB";
			public const string DKPNR = "PNR";
			public const string DOCED = "CED";
			public const string DORNC = "RNC";
			public const string EINCBNSSN4811PartyID = "481";
			public const string EUEOR = "EOR";
			public const string GS1 = "GS1";
			public const string HUIDM = "IDM";
			public const string ILVAT = "IVT";
			public const string ISCOC = "COC";
			public const string ITCAT = "CAT";
			public const string ITCOD = "COD";
			public const string ITIVA = "IVA";
			public const string KR01 = "K01";
			public const string KR08 = "K08";
			public const string KRKBC = "KBC";
			public const string KRKBT = "KBT";
			public const string KRVAT = "KVT";
			public const string MXCFD = "CFD";
			public const string MXIVA = "IVX";
			public const string MXREG = "REG";
			public const string MXRFC = "RFC";
			public const string MYOTH = "OTH";
			public const string MYPIC = "PIC";
			public const string MYSIC = "MSC";
			public const string MYTIN = "TIN";
			public const string NAICS = "NAI";
			public const string NLCCN = "CCN";
			public const string NOGBR = "GBR";
			public const string NOMVA = "MVA";
			public const string NZCCP = "NCP";
			public const string PABRC = "PBC";
			public const string PTIVA = "IVP";
			public const string SIC = "SIC";
			public const string TWGTX = "GTX";
			public const string TWMCI = "MCI";
			public const string TWPIG = "PIG";
			public const string TWVAT = "VAT";
			public const string USCCC = "USC";
			public const string USEIN = "EIN";
			public const string USMID = "MID";
			public const string USNMF = "NMF";
			public const string USPFR = "PFR";
			public const string UYBRC = "BRC";
			public const string UYCID = "CID";
			public const string UYRUT = "RUT";
			public const string VNVAT = "VVT";
		}

		public CodeDescriptionBoolRegistryItem StrictEnforcementOfRegistrationNumberFormats
		{
			get
			{
				return GetItem("StrictEnforcementOfRegistrationNumberFormats", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
											"StrictEnforcementOfRegistrationNumberFormats",
											Categories.Organizations,
											ResString.GetMultilingualString("8c515e2b-0b5c-4bd5-a46c-c380eba7090d", "Strict Enforcement Of Registration Number Formats"),
											ResString.GetMultilingualString("050b615e-54ca-4a5f-8922-b741b7b8f70d", "Certain kinds of registration numbers and codes have validation that will prevent saving. Untick a specific format to make them only warnings. CargoWise strongly recommends that validation is used to preserve data quality."),
											RegistryStorageFlags.System,
											new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("924984aa-62ec-4065-a03d-e18c53ca0dd5", "Is Validated?"), true, true),
											GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "Enterprise.ZArchitecture.Core.CodeDescriptionPairList.get_Item(System.String)")]
		CodeDescriptionBoolCollection GetStrictEnforcementOfRegistryNumberFormatsValue(bool defaultValue)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var lists = new OrgCodeLists();
			var aU = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")));
			var mY = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "MY")));
			var gB = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "GB")));
			var iS = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "IS")));
			var cA = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "CA")));
			var aO = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AO")));
			var nL = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "NL")));
			var dE = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "DE")));
			var dK = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "DK")));
			var uS = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")));
			var iT = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "IT")));
			var nZ = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "NZ")));
			var bR = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "BR")));
			var tW = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "TW")));
			var nO = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "NO")));
			var aR = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AR")));
			var mX = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Mexico)));
			var uY = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Uruguay)));
			var vN = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.VietNam)));
			var pT = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Portugal)));
			var cO = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Colombia)));
			var cL = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Chile)));
			var dO = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.DominicanRepublic)));
			var pA = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Panama)));
			var kR = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.KoreaSouth)));
			var iL = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Israel)));
			var hU = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Hungary)));
			var cR = lists.CustomsCodes_List(factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.CostaRica)));

			return new CodeDescriptionBoolCollection(null, defaultValue)
			{
				{ RegistrationNumberFormatFields.EINCBNSSN4811PartyID,
				ResString.GetMultilingualString("28080c2b-702a-4cde-8ad4-678e443363b8", "4811 Party ID (EIN/CBN/SSN)"), defaultValue },
				{ RegistrationNumberFormatFields.CCP,
				ResString.GetMultilingualString("183f60f9-7ac6-4d91-9196-a4cdaebfcd96", "Any country/region - {0} ({1})", OrgCusCode.CodeTypes.ControlledPremisesID, aU[OrgCusCode.CodeTypes.ControlledPremisesID].Description), defaultValue },
				{ RegistrationNumberFormatFields.CarrierCode,
				ResString.GetMultilingualString("3889a364-a831-4313-9232-713078e7a000", "Any country/region - {0}", uS[OrgCusCode.CodeTypes.CarrierCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.GS1,
				ResString.GetMultilingualString("16944acb-84d0-4054-bd0f-768218086fe6", "Any country/region - {0}", uS[OrgCusCode.CodeTypes.GS1].Description), defaultValue },
				{ RegistrationNumberFormatFields.CargoWiseOneCarrierCode,
				ResString.GetMultilingualString("7adceb38-22be-4599-bd5b-56482b98a08d", "Any country/region - {0}", uS[OrgCusCode.CodeTypes.CargoWiseOneCarrierCode].Description), defaultValue }, // Select "US" as an example and it could be any country.
				{ RegistrationNumberFormatFields.NAICS,
				ResString.GetMultilingualString("34365CC3-5554-4DAC-94D9-12EE7E9F86C1", "Any country/region - {0}", uS[OrgCusCode.CodeTypes.NorthAmericanIndustryClassificationSystem].Description), defaultValue },
				{ RegistrationNumberFormatFields.SIC,
				ResString.GetMultilingualString("873A349A-04C1-4107-BE53-83DADCD99D8C", "Any country/region - {0}", uS[OrgCusCode.CodeTypes.StandardIndustrialClassification].Description), defaultValue },
				{ RegistrationNumberFormatFields.AUABN,
				(NoResString)"AU - ABN (Australian Business Number ( Registration Code))", defaultValue },
				{ RegistrationNumberFormatFields.AUARN,
					(NoResString)"AU - ARN (ATO Reference Number (Australian Tax Office))", defaultValue },
				{ RegistrationNumberFormatFields.AUCCP,
				ResString.GetMultilingualString("67f67955-4557-48ef-815b-9dd7cc2d1741", "{0} - {1} ({2})", "AU", OrgCusCode.CodeTypes.ControlledPremisesID, aU[OrgCusCode.CodeTypes.ControlledPremisesID].Description), defaultValue },
				{ RegistrationNumberFormatFields.AUEEN,
				ResString.GetMultilingualString("c10feed4-9332-4c82-abe0-569beea7c2d7", "{0} - {1} ({2})", "AU", OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, aU[OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.AUNEN,
				ResString.GetMultilingualString("014A3114-2703-4762-9781-08AA5D7D6387", "{0} - {1} ({2})", "AU", OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, aU[OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.AUEAP,
				ResString.GetMultilingualString("954de08e-36c4-42f0-a2fa-776fd4b46d98", "{0} - {1} ({2})", "AU", OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber, aU[OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.AUESN,
				ResString.GetMultilingualString("38c02620-37bb-411c-bb85-263bb39a1374", "{0} - {1} ({2})", "AU", OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, aU[OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.AUEEU,
				ResString.GetMultilingualString("e77565de-20f6-41e1-b128-1b84d183b205", "{0} - {1} ({2})", "AU", OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, aU[OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser].Description), defaultValue },
				{ RegistrationNumberFormatFields.MYOTH,
				ResString.GetMultilingualString("ca742860-e227-4c58-a0a1-b117c9cc4be4", "{0} - {1} ({2})", "MY", MalaysiaOrgCusCodeInfo.OrgCusCodes.OtherBusinessCode, MalaysiaOrgCusCodeInfo.OrgCusCodesDescription.OtherBusinessCode), defaultValue },
				{ RegistrationNumberFormatFields.MYPIC,
				ResString.GetMultilingualString("AACFBA44-D486-4B7B-9898-29A38DD37F98", "{0} - {1} ({2})", "MY", MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber, MalaysiaOrgCusCodeInfo.OrgCusCodesDescription.PersonalIdentificationCardNumber), defaultValue },
				{ RegistrationNumberFormatFields.MYSIC,
				ResString.GetMultilingualString("B764B69C-DC70-44E0-B932-B9B954A7293B", "{0} - {1} ({2})", "MY", OrgCusCode.CodeTypes.StandardIndustrialClassification, mY[OrgCusCode.CodeTypes.StandardIndustrialClassification].Description), defaultValue },
				{ RegistrationNumberFormatFields.MYTIN,
				ResString.GetMultilingualString("75FA6EC1-4006-49DD-968E-71E3B65FE06F", "{0} - {1} ({2} / {3})", "MY", MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "Nombor Pengenalan Cukai", MalaysiaOrgCusCodeInfo.OrgCusCodesDescription.TaxIdentificationNumber), defaultValue },
				{ RegistrationNumberFormatFields.EUEOR,
				ResString.GetMultilingualString("2d85b64c-f3e0-4afb-9cd5-efba70ad5086", "{0} - {1} ({2})", "GB", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, gB[OrgCusCode.EuropeanUnionSharedCodeTypes.Eori].Description), defaultValue },
				{ RegistrationNumberFormatFields.ISCOC,
				ResString.GetMultilingualString("74b27aed-6184-4598-a1ae-0a3f8986b2ba", "{0} - {1} ({2})", "IS", OrgCusCode.IcelandCodeTypes.CustomsOfficeCode, iS[OrgCusCode.IcelandCodeTypes.CustomsOfficeCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.CABRM,
				ResString.GetMultilingualString("af0da694-4e84-4eab-914b-3befb7247593", "{0} - {1} ({2})", "CA", OrgCusCode.CACodeTypes.BusinessNumberForImportExport, cA[OrgCusCode.CACodeTypes.BusinessNumberForImportExport].Description), defaultValue },
				{ RegistrationNumberFormatFields.CABRB,
				ResString.GetMultilingualString("B3974315-9F06-458D-A279-728B0814710A", "{0} - {1} ({2})", "CA", OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, cA[OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker].Description), defaultValue },
				{ RegistrationNumberFormatFields.CABNC,
				ResString.GetMultilingualString("B28F7492-1781-4D5E-A141-4E3CF4E4B349", "{0} - {1} ({2})", "CA", OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, cA[OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial].Description), defaultValue },
				{ RegistrationNumberFormatFields.CABRE,
				ResString.GetMultilingualString("8da63d35-83f7-4b28-b037-bb89e7ea5e36", "{0} - {1} ({2})", "CA", OrgCusCode.CACodeTypes.BusinessNumberForExport, cA[OrgCusCode.CACodeTypes.BusinessNumberForExport].Description), defaultValue },
				{ RegistrationNumberFormatFields.CABRL,
				ResString.GetMultilingualString("f1724375-b0c7-499a-8b67-7b2384c0cf51", "{0} - {1} ({2})", "CA", OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, cA[OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments].Description), defaultValue },
				{ RegistrationNumberFormatFields.CAWMI,
				ResString.GetMultilingualString("cb2967aa-d4ac-4e3c-9caa-07cfc82bd54d", "{0} - {1} ({2})", "CA", OrgCusCode.CACodeTypes.WorldManufacturerIdentifier, cA[OrgCusCode.CACodeTypes.WorldManufacturerIdentifier].Description), defaultValue },
				{ RegistrationNumberFormatFields.AONIF,
				ResString.GetMultilingualString("de9da79c-cd8e-4072-a727-1d7cd01256f2", "{0} - {1} ({2})", "AO", OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal, aO[OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal].Description), defaultValue },
				{ RegistrationNumberFormatFields.NLCCN,
				ResString.GetMultilingualString("822b6ba8-5446-49d6-aa05-982d8c1cbbb3", "{0} - {1} ({2})", "NL", OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber, nL[OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.DEHRB,
				ResString.GetMultilingualString("7250892e-f11c-4e6c-8db5-0db71dd732d5", "{0} - {1} ({2})", "DE", GermanyOrgCusCodeInfo.OrgCusCodes.Handelsregister, dE[GermanyOrgCusCodeInfo.OrgCusCodes.Handelsregister].Description), defaultValue },
				{ RegistrationNumberFormatFields.DKPNR,
				ResString.GetMultilingualString("ad519952-dbfe-47a5-a2e3-d7e416ce2e5c", "{0} - {1} ({2})", "DK", OrgCusCode.DenmarkCodeTypes.ProductionNumber, dK[OrgCusCode.DenmarkCodeTypes.ProductionNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.CLRUTSOL,
				ResString.GetMultilingualString("ac6a01ab-3bb2-4c7e-9cbb-55f45a9f5d44", "{0} - {1} / {2}", Core.Constants.CountryCodes.Chile, ChileOrgCusCodeInfo.OrgCusCodes.RUT, ChileOrgCusCodeInfo.OrgCusCodes.SOL), defaultValue },
				{ RegistrationNumberFormatFields.CLACT,
				ResString.GetMultilingualString("DE6A2617-DBDB-4369-9FC6-05B85C43B8DF", "{0} - {1} ({2})", Core.Constants.CountryCodes.Chile, ChileOrgCusCodeInfo.OrgCusCodes.ACT, cL[ChileOrgCusCodeInfo.OrgCusCodes.ACT].Description ), defaultValue },
				{ RegistrationNumberFormatFields.CNBSTVAT,
				ResString.GetMultilingualString("bd8b1fbd-65bf-4810-a2e8-a6cfe1f336ec", "{0} - {1} / {2} mutual exclusivity", "CN", OrgCusCode.ChinaCodeTypes.BST, OrgCusCode.CodeTypes.VATCode), defaultValue },
				{ RegistrationNumberFormatFields.CNVAGVAS,
				ResString.GetMultilingualString("b2be01e7-c5cc-4d90-8716-856f9468b054", "{0} - {1} / {2} mutual exclusivity", "CN", OrgCusCode.ChinaCodeTypes.VAG, OrgCusCode.ChinaCodeTypes.VAS), defaultValue },
				{ RegistrationNumberFormatFields.USCCC,
				ResString.GetMultilingualString("9c1da357-4be4-46d7-8250-9d739c61bcee", "{0} - {1} ({2})", "US", OrgCusCode.CodeTypes.CarrierCode, uS[OrgCusCode.CodeTypes.CarrierCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.USEIN,
				ResString.GetMultilingualString("bad8e381-13ff-44da-858d-f493ebed99c0", "{0} - {1} ({2})", "US", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, uS[OrgCusCode.USACodeTypes.EmployerIdentificationNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.USNMF,
				ResString.GetMultilingualString("4f2f4a92-4979-4aa9-9815-028bab1f41b2", "{0} - {1} ({2})", "US", OrgCusCode.USACodeTypes.NMFCParticipant, uS[OrgCusCode.USACodeTypes.NMFCParticipant].Description), defaultValue },
				{ RegistrationNumberFormatFields.USMID,
				ResString.GetMultilingualString("8ab6d720-4a5b-4eec-8f33-eadc9a55df6e", "{0} - {1} ({2})", "US", OrgCusCode.USACodeTypes.ManufacturerID, uS[OrgCusCode.USACodeTypes.ManufacturerID].Description), defaultValue },
				{ RegistrationNumberFormatFields.USPFR,
				ResString.GetMultilingualString("1f25ed73-ab9a-4e11-be54-125748168347", "{0} - {1} ({2})", "US", OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber, uS[OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.ITCAT,
				ResString.GetMultilingualString("9470388b-08e3-4b03-af7d-a73f5b9bb428", "{0} - {1} ({2})", "IT", ItalyOrgCusCodeInfo.OrgCusCodes.CodiceAttive, iT[ItalyOrgCusCodeInfo.OrgCusCodes.CodiceAttive].Description), defaultValue },
				{ RegistrationNumberFormatFields.ITCOD,
				ResString.GetMultilingualString("4c26ca39-a2f2-4f8d-aab1-051a927fb0ed", "{0} - {1} ({2})", "IT", ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, iT[ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale].Description), !defaultValue },
				{ RegistrationNumberFormatFields.ITIVA,
				ResString.GetMultilingualString("30d620f6-4569-4b95-be7e-6f010a7df94a", "{0} - {1} ({2})", "IT", OrgCusCode.CodeTypes.IVA, iT[OrgCusCode.CodeTypes.IVA].Description), !defaultValue },
				{ RegistrationNumberFormatFields.NZCCP,
				ResString.GetMultilingualString("7E496D91-A08D-4021-8F52-708C9AF3FFD0", "{0} - {1} ({2})", "NZ", OrgCusCode.CodeTypes.ControlledPremisesID, nZ[OrgCusCode.CodeTypes.ControlledPremisesID].Description), defaultValue },
				{ RegistrationNumberFormatFields.BRCNPJ,
				ResString.GetMultilingualString("3a093d22-ca24-4309-8496-ed1ffba5deef", "{0} - {1} ({2})", "BR", BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, bR[BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ].Description), defaultValue },
				{ RegistrationNumberFormatFields.TWVAT,
				ResString.GetMultilingualString("36BD7B2E-FBFF-4053-826D-DD22ED15F369", "{0} - {1} ({2})", "TW", OrgCusCode.CodeTypes.VATCode, tW[OrgCusCode.CodeTypes.VATCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.TWGTX,
				ResString.GetMultilingualString("F6C76A0E-1057-47FB-AD52-4101F5B01C68", "{0} - {1} ({2})", "TW", OrgCusCode.CodeTypes.TaxFileCode, tW[OrgCusCode.CodeTypes.TaxFileCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.TWMCI,
				ResString.GetMultilingualString("ABC9A3AC-5A18-480D-8D76-04A1B7C80A1C", "{0} - {1} ({2})", "TW", OrgCusCode.TaiwanCodeTypes.MCI, tW[OrgCusCode.TaiwanCodeTypes.MCI].Description), defaultValue },
				{ RegistrationNumberFormatFields.TWPIG,
				ResString.GetMultilingualString("AB862429-D6F7-46E0-A611-302E890383DE", "{0} - {1} ({2})", "TW", OrgCusCode.TaiwanCodeTypes.PIG, tW[OrgCusCode.TaiwanCodeTypes.PIG].Description), defaultValue },
				{ RegistrationNumberFormatFields.ARCUIT,
				ResString.GetMultilingualString("E652601B-7DF7-4D11-A855-6CD9CF01A119", "{0} - {1} ({2})", "AR", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, aR[ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT].Description), !defaultValue },
				{ RegistrationNumberFormatFields.ARCUIL,
				ResString.GetMultilingualString("366E24D2-B889-46D0-8287-67E3932F6082", "{0} - {1} ({2})", "AR", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL, aR[ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL].Description), !defaultValue },
				{ RegistrationNumberFormatFields.ARDNI,
				ResString.GetMultilingualString("8217AA8C-EB09-4A5F-82CC-CD889B39F2FE", "{0} - {1} ({2})", "AR", ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI, aR[ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI].Description), !defaultValue },
				{ RegistrationNumberFormatFields.ARIBL,
				ResString.GetMultilingualString("6B9C70C7-3192-4983-BE10-4067415301AA", "{0} - {1} ({2})", "AR", ArgentinaOrgCusCodeInfo.OrgCusCodes.IBL, aR[ArgentinaOrgCusCodeInfo.OrgCusCodes.IBL].Description), !defaultValue },
				{ RegistrationNumberFormatFields.ARIBM,
				ResString.GetMultilingualString("483048A5-FC95-4D5D-AB7B-50DCA916BE71", "{0} - {1} ({2})", "AR", ArgentinaOrgCusCodeInfo.OrgCusCodes.IBM, aR[ArgentinaOrgCusCodeInfo.OrgCusCodes.IBM].Description), !defaultValue },
				{ RegistrationNumberFormatFields.ARIBS,
				ResString.GetMultilingualString("3A927F86-457E-4B77-89AB-5FED25A29E47", "{0} - {1} ({2})", "AR", ArgentinaOrgCusCodeInfo.OrgCusCodes.IBS, aR[ArgentinaOrgCusCodeInfo.OrgCusCodes.IBS].Description), !defaultValue },
				{ RegistrationNumberFormatFields.MXIVA,
				ResString.GetMultilingualString("E5907F17-9D05-4300-855F-C98F5858858B", "{0} - {1} ({2})", "MX", OrgCusCode.CodeTypes.IVA, mX[OrgCusCode.CodeTypes.IVA].Description), !defaultValue },
				{ RegistrationNumberFormatFields.MXRFC,
				ResString.GetMultilingualString("57E65D23-6F69-4E3E-8A1C-E17808712145", "{0} - {1} ({2})", "MX", MexicoOrgCusCodeInfo.OrgCusCodes.RFC, mX[MexicoOrgCusCodeInfo.OrgCusCodes.RFC].Description), !defaultValue },
				{ RegistrationNumberFormatFields.MXCFD,
				ResString.GetMultilingualString("528ED579-A6CA-4BFA-BD7B-F85239C67064", "{0} - {1} ({2})", "MX", MexicoOrgCusCodeInfo.OrgCusCodes.CFD, mX[MexicoOrgCusCodeInfo.OrgCusCodes.CFD].Description), defaultValue },
				{ RegistrationNumberFormatFields.MXREG,
				ResString.GetMultilingualString("2383408F-796B-48F2-9C74-D76FE4C4E32B", "{0} - {1} ({2})", "MX", MexicoOrgCusCodeInfo.OrgCusCodes.REG, mX[MexicoOrgCusCodeInfo.OrgCusCodes.REG].Description), defaultValue },
				{ RegistrationNumberFormatFields.UYRUT,
				ResString.GetMultilingualString("846DF6F4-D1C2-4C74-9F48-5060AE83C055", "{0} - {1} ({2})", Core.Constants.CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, uY[UruguayOrgCusCodeInfo.OrgCusCodes.RUT].Description), !defaultValue },
				{ RegistrationNumberFormatFields.UYCID,
				ResString.GetMultilingualString("1B508477-2769-44C8-B270-F31A40A1B2E6", "{0} - {1} ({2})", Core.Constants.CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.CID, uY[UruguayOrgCusCodeInfo.OrgCusCodes.CID].Description), !defaultValue },
				{ RegistrationNumberFormatFields.UYBRC,
				ResString.GetMultilingualString("6bc45499-04fe-4185-991c-ac5d0fffb5e0", "{0} - {1} ({2})", Core.Constants.CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.BRC, uY[UruguayOrgCusCodeInfo.OrgCusCodes.BRC].Description), defaultValue },
				{ RegistrationNumberFormatFields.VNVAT,
				ResString.GetMultilingualString("B8390D9D-36FA-4CB1-919E-FB0B7F43420C", "{0} - {1} ({2})", Core.Constants.CountryCodes.VietNam, OrgCusCode.CodeTypes.VATCode, vN[OrgCusCode.CodeTypes.VATCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.PTIVA,
				ResString.GetMultilingualString("d730d06e-3e0e-42df-a60b-e5a4fdbd4164", "{0} - {1} ({2})", Core.Constants.CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, pT[OrgCusCode.CodeTypes.IVA].Description), defaultValue },
				{ RegistrationNumberFormatFields.CONIT,
				ResString.GetMultilingualString("6B9B4FF8-0625-466B-872D-A015BE383780", "{0} - {1} ({2})", Core.Constants.CountryCodes.Colombia, ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, cO[ColombiaOrgCusCodeInfo.OrgCusCodes.NIT].Description), defaultValue },
				{ RegistrationNumberFormatFields.DORNC,
				ResString.GetMultilingualString("667046E1-A784-41E9-9AFA-22BC400FB2FC", "{0} - {1} ({2})", Core.Constants.CountryCodes.DominicanRepublic, DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RNC, dO[DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RNC].Description), defaultValue },
				{ RegistrationNumberFormatFields.DOCED,
				ResString.GetMultilingualString("DEC0E01B-9126-4FB5-9751-37891110B2E7", "{0} - {1} ({2})", Core.Constants.CountryCodes.DominicanRepublic, DominicanRepublicOrgCusCodeInfo.OrgCusCodes.CED, dO[DominicanRepublicOrgCusCodeInfo.OrgCusCodes.CED].Description), defaultValue },
				{ RegistrationNumberFormatFields.PABRC,
				ResString.GetMultilingualString("CD218A58-6409-4DAA-9B89-323A7DF905B8", "{0} - {1} ({2})", Core.Constants.CountryCodes.Panama, PanamaOrgCusCodeInfo.OrgCusCodes.BRC, pA[PanamaOrgCusCodeInfo.OrgCusCodes.BRC].Description), defaultValue },
				{ RegistrationNumberFormatFields.NOGBR,
				ResString.GetMultilingualString("97FADF88-BAFC-431C-818A-456C1F00D0A3", "{0} - {1} ({2})", Core.Constants.CountryCodes.Norway, OrgCusCode.CodeTypes.GovBusinessCode, nO[OrgCusCode.CodeTypes.GovBusinessCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.NOMVA,
				ResString.GetMultilingualString("F2573208-0F9B-44C9-B1BA-97F4DFD1C94A", "{0} - {1} ({2})", Core.Constants.CountryCodes.Norway, OrgCusCode.NorwayCodeTypes.MVA, nO[OrgCusCode.NorwayCodeTypes.MVA].Description), defaultValue },
				{ RegistrationNumberFormatFields.KRVAT,
				ResString.GetMultilingualString("497A0874-4B5C-4103-8452-481009890C22", "{0} - {1} ({2})", Core.Constants.CountryCodes.KoreaSouth, OrgCusCode.CodeTypes.VATCode, kR[OrgCusCode.CodeTypes.VATCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.KR08,
				ResString.GetMultilingualString("BCE47422-F365-4B59-AEDA-4D1B7310CAFB", "{0} - {1} ({2})", Core.Constants.CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.OfficeID, kR[KoreaSouthComplianceInfo.CodeTypes.OfficeID].Description), defaultValue },
				{ RegistrationNumberFormatFields.KR01,
				ResString.GetMultilingualString("C3E49551-A949-4D2D-8FF7-88FAD8B9C5D1", "{0} - {1} ({2})", Core.Constants.CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident, kR[KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident].Description), defaultValue },
				{ RegistrationNumberFormatFields.KRKBT,
				ResString.GetMultilingualString("A8B4263D-B33A-46B6-8ED8-3A0EFC8811E2", "{0} - {1} ({2})", Core.Constants.CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.KBT, kR[KoreaSouthComplianceInfo.CodeTypes.KBT].Description), defaultValue },
				{ RegistrationNumberFormatFields.KRKBC,
				ResString.GetMultilingualString("F2DB95C8-E485-485A-98D2-A913A76E6226", "{0} - {1} ({2})", Core.Constants.CountryCodes.KoreaSouth, KoreaSouthComplianceInfo.CodeTypes.KBC, kR[KoreaSouthComplianceInfo.CodeTypes.KBC].Description), defaultValue },
				{ RegistrationNumberFormatFields.ILVAT,
				ResString.GetMultilingualString("47D7D030-3CE0-419D-BAFF-AC96C7A44C02", "{0} - {1} ({2})", Core.Constants.CountryCodes.Israel, OrgCusCode.CodeTypes.VATCode, iL[OrgCusCode.CodeTypes.VATCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.HUIDM,
				ResString.GetMultilingualString("291D14FC-324B-4C10-BD5A-DDBFE38527B6", "{0} - {1} ({2})", Core.Constants.CountryCodes.Hungary, HungaryOrgCusCodeInfo.OrgCusCodes.IDM, hU[HungaryOrgCusCodeInfo.OrgCusCodes.IDM].Description), defaultValue },
				{ RegistrationNumberFormatFields.CRCID,
				ResString.GetMultilingualString("EC627E50-D3EC-4CCA-BB34-FEF084BF880B", "{0} - {1} ({2})", Core.Constants.CountryCodes.CostaRica, CostaRicaOrgCusCodeInfo.OrgCusCodes.IndividualIdentificationNumber, cR[CostaRicaOrgCusCodeInfo.OrgCusCodes.IndividualIdentificationNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.CRCIJ,
				ResString.GetMultilingualString("41A66396-1685-4EBC-9CC0-A85939C6CC14", "{0} - {1} ({2})", Core.Constants.CountryCodes.CostaRica, CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber, cR[CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.CRDIM,
				ResString.GetMultilingualString("2FB0A643-3845-419C-8072-A81D5EA9E0E5", "{0} - {1} ({2})", Core.Constants.CountryCodes.CostaRica, CostaRicaOrgCusCodeInfo.OrgCusCodes.DIMEXDocumentIdentificationNumber, cR[CostaRicaOrgCusCodeInfo.OrgCusCodes.DIMEXDocumentIdentificationNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.CREAC,
				ResString.GetMultilingualString("1A0CF289-8513-4246-B590-EFFD3E44DF52", "{0} - {1} ({2})", Core.Constants.CountryCodes.CostaRica, CostaRicaOrgCusCodeInfo.OrgCusCodes.EACEconomicActivityCode, cR[CostaRicaOrgCusCodeInfo.OrgCusCodes.EACEconomicActivityCode].Description), defaultValue },
				{ RegistrationNumberFormatFields.CRNIT,
				ResString.GetMultilingualString("8F09AC15-BCB1-424D-95C9-22D4C8837379", "{0} - {1} ({2})", Core.Constants.CountryCodes.CostaRica, CostaRicaOrgCusCodeInfo.OrgCusCodes.NITIdentificationNumber, cR[CostaRicaOrgCusCodeInfo.OrgCusCodes.NITIdentificationNumber].Description), defaultValue },
				{ RegistrationNumberFormatFields.CRUBI,
				ResString.GetMultilingualString("03D78A8D-51C2-4139-87C3-4C36F71047DB", "{0} - {1} ({2})", Core.Constants.CountryCodes.CostaRica, CostaRicaOrgCusCodeInfo.OrgCusCodes.UBILocacionCode, cR[CostaRicaOrgCusCodeInfo.OrgCusCodes.UBILocacionCode].Description), defaultValue }
			};
		}

#if DEBUG
		public
#endif
 CodeDescriptionBoolCollection GetStrictEnforcementOfRegistryNumberFormatsDefaultValue()
		{
			if (strictEnforcementOfRegistryNumberFormatsDefaultValue == null)
			{
				strictEnforcementOfRegistryNumberFormatsDefaultValue = GetStrictEnforcementOfRegistryNumberFormatsValue(true);
			}
			return strictEnforcementOfRegistryNumberFormatsDefaultValue;
		}

		CodeDescriptionBoolCollection strictEnforcementOfRegistryNumberFormatsDefaultValue;

#if DEBUG
		public
		CodeDescriptionBoolCollection GetStrictEnforcementOfRegistryNumberFormatsDisabledValue()
		{
			if (strictEnforcementOfRegistryNumberFormatsDisabledValue == null)
			{
				strictEnforcementOfRegistryNumberFormatsDisabledValue = GetStrictEnforcementOfRegistryNumberFormatsValue(false);
			}
			return strictEnforcementOfRegistryNumberFormatsDisabledValue;
		}

		CodeDescriptionBoolCollection strictEnforcementOfRegistryNumberFormatsDisabledValue;
#endif

		#endregion

		#region INACTIVE Address Warning/Error

		public BooleanRegistryItem InactiveAddressWarningOrError
		{
			get
			{
				return GetItem("InactiveAddressWarningOrError", delegate
				{
					return new BooleanRegistryItem(
						"InactiveAddressWarningOrError",
						Categories.Organizations,
						ResString.GetMultilingualString("9F54C10B-8382-4262-B6F9-72E45A22F027", "INACTIVE Address Warning/Error"),
						ResString.GetMultilingualString("4B0E5081-9423-4106-B940-52CAC84BC30A", "When set to Yes, an error will replace the warning on an INACTIVE Address."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Duplicate Main Address / AddressNotOnFile

		public BooleanRegistryItem EnableAddressNotOnFileLogging
		{
			get
			{
				return GetItem("EnableAddressNotOnFileLogging", delegate
				{
					return new BooleanRegistryItem(
						"EnableAddressNotOnFileLogging",
						Categories.Organizations,
						(NoResString)"Enable Address Not On File Logging",
						(NoResString)"This registry will enable additional logging focused on the creation of the ***Address Not On File*** address. This address is usually created when there is no main address against an organization",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion Duplicate Main Address / AddressNotOnFile

		#region Allow Merge Ignoring AR AP

		public BooleanRegistryItem AllowMergeIgnoringARAP
		{
			get
			{
				return GetItem("AllowMergeIgnoringARAP", delegate
				{
					return new BooleanRegistryItem(
						"AllowMergeIgnoringARAP",
						Categories.Organizations,
						ResString.GetMultilingualString("4aab1825-d28f-4de3-b9f7-e856667ee011", "Allow Merge Ignoring AR and AP"),
						ResString.GetMultilingualString("2372b88c-2fe9-47ed-ae07-3c9501ae1e95", "Allow Merge Ignoring AR and AP settings for other companies. Normally if you try to merge Organization A into Organization B, and Organization A is Payable/Receivable in any country/region, you will not be allowed to merge until Organization B is marked as Payable/Receivable for that country/region. With this option turned on, users will be able to merge if applicable."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Default Behavior Of Show For All Companies Checkbox

		public BooleanRegistryItem DefaultBehaviorOfShowForAllCompaniesCheckbox
		{
			get
			{
				return GetItem("DefaultBehaviorOfShowForAllCompaniesCheckbox", delegate
				{
					return new BooleanRegistryItem("DefaultBehaviorOfShowForAllCompaniesCheckbox",
						Categories.Organizations,
						ResString.GetMultilingualString("4BC192DB-35D9-4D11-965D-FC5DD84CBD4D", "Default Behavior of Show for all companies Checkbox"),
						ResString.GetMultilingualString("690E138E-E88D-4239-946A-49CB4582B8FD", "Show for all companies selected. This setting will only be applied if the user also has the security Maintain -> Master Data -> Organization -> View -> View Other Company's Staff Assignments enabled."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Profit Share Agreement

		public ChargeCodeListRegistryItem CustomProfitShareAgreementTypeChargeCodes
		{
			get
			{
				return GetItem("CustomProfitShareAgreementTypeChargeCodes", delegate
				{
					return new ChargeCodeListRegistryItem(
											"CustomProfitShareAgreementTypeChargeCodes",
											Categories.Accounting_JobInvoicing_ProfitShare,
											ResString.GetMultilingualString("8296A43C-6F75-43E2-85D5-2EFD553E67BE", "Customized Profit Share Agreement Type Charge Codes"),
											ResString.GetMultilingualString("80FF5CC1-40D1-4A0C-9715-5A4ECC5A43C5", "List of Charge Codes for Customizing Profit Share Agreement Type"),
											DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
											string.Empty,
											RegistryFindBoxFilter.None);
				});
			}
		}

		#endregion

		#region Sales & Marketing

		#region Trade Lanes

		public BooleanRegistryItem ShouldDoFullTradeLanesSyncIfRequired
		{
			get
			{
				return GetItem("ShouldDoFullTradeLanesSyncIfRequired", delegate
				{
					return new BooleanRegistryItem(
											"ShouldDoFullTradeLanesSyncIfRequired",
											Categories.SalesMarketing_TradeLanesSynchronisation,
											(NoResString)"Should do Full Trade Lane synchronization",
											(NoResString)"The Full Trade Lane Synchronization (TLS) service task will run if this registry item is true",
											RegistryStorageFlags.System,
											RegistryOptions.IsOnlyForSupport,
											true);
				});
			}
		}

		public DateTimeRegistryItem FullTradeLanesSyncDateTimeThreshold
		{
			get
			{
				return GetItem("FullTradeLanesSyncDateTimeThreshold", delegate
				{
					return new DateTimeRegistryItem(
											"FullTradeLanesSyncDateTimeThreshold",
											null,
											null,
											null,
											RegistryStorageFlags.System,
											RegistryOptions.IsHidden,
											SqlDateTime.MinValue.Value);
				});
			}
		}

		public DateTime FullTradeLanesSyncFromDate
		{
			get
			{
				var registryDate = FullTradeLanesSyncFromDateRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				return new DateTime(registryDate.Year, registryDate.Month, 1);
			}
#if DEBUG
			set { FullTradeLanesSyncFromDateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		internal DateTimeRegistryItem FullTradeLanesSyncFromDateRaw
		{
			get
			{
				return GetItem("FullTradeLanesSyncFromDate", delegate
				{
					return new DateTimeRegistryItem(
						"FullTradeLanesSyncFromDate",
						Categories.SalesMarketing_TradeLanesSynchronisation,
						ResString.GetMultilingualString("d468dc9a-6190-44c7-b8c7-e13f881b46e4", "Earliest Month included in Full Trade Lane Synchronization"),
						ResString.GetMultilingualString("5f22913c-b76b-40cb-9b9d-a02f27b6aca4", "The Full Trade Lane Synchronization (TLS) service task will process records dating between the first day of the month specified here and the current date."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new DateTime(ZDateTime.Now.Year - 2, ZDateTime.Now.Month, 1)
					);
				});
			}
		}

		public DateTimeRegistryItem FullTradeLanesSyncLatestSyncMonth
		{
			get
			{
				return GetItem("FullTradeLanesLatestSyncMonth", delegate
				{
					return new DateTimeRegistryItem(
						"FullTradeLanesLatestSyncMonth",
						null,
						null,
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						DateTime.MinValue);
				});
			}
		}

		public BooleanRegistryItem UpdateExistingClientCommenceDateFromTradeLaneSync
		{
			get
			{
				return GetItem("UpdateExistingClientCommenceDateFromTradeLaneSync", delegate
				{
					return new BooleanRegistryItem(
						"UpdateExistingClientCommenceDateFromTradeLaneSync",
						Categories.SalesMarketing_TradeLanesSynchronisation,
						ResString.GetMultilingualString("C5F32058-888A-40EA-8ECB-CEEC44532A75", "Update Existing Client Commence Date From Trade Lane Synchronization"),
						ResString.GetMultilingualString("297044AF-9E03-4F6F-8303-D6AB8E0F6172", @"When the TLS service task recognizes that an organization is involved as either a Consignor, Consignee or Local Client where no Client Commenced date exists, it updates this organization with a date reflecting the period in which it started trading.

When this registry setting set to Yes, the TLS service task will update the Client Commenced date if a trade period older than the current Client Commenced is identified.

Note: the number of trade periods considered by TLS is dependent on the TLS Service task schedule configuration."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Commission

		public CodeDescriptionBoolRegistryItem CommissionAgreementStreams
		{
			get
			{
				return GetItem("CommissionAgreementStreams", delegate
				{
					CodeDescriptionBoolRegistryItem result = new CodeDescriptionBoolRegistryItem(
							"CommissionAgreementStreams",
							Categories.SalesMarketing_Commission,
							ResString.GetMultilingualString("af3e5645-c0b7-4c73-a7bf-a16937b186b6", "Commission Agreement Streams"),
							ResString.GetMultilingualString("d5e76ec2-32b2-4afd-b04e-1e9bc51428e1", @"The list of streams available for commission agreements.

Commission agreements on different streams are allowed to have commissions from the same jobs. Additionally, the commissionable amount will be independent of the other streams.
e.g. if a job has a profit of $200, and there are 3 agreements on different streams for this same job; each agreement is applicable for $200 commission."),
							RegistryStorageFlags.System,
							ResString.GetMultilingualString("5ffc9f70-52d3-4390-b452-6bfc43049b98", "Is Enabled?"),
							true)
					{ Options = DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default };
					return result;
				});
			}
		}

		public GuidRegistryItem CommissionAgreementConflictNotificationGroup
		{
			get
			{
				return GetItem("CommissionAgreementConflictNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
											"CommissionAgreementConflictNotificationGroup",
											Categories.SalesMarketing_Commission,
											ResString.GetMultilingualString("e62e879b-b031-4f4d-adce-6ffe99d33fc9", "Commission Agreement Conflict Notification Group"),
											ResString.GetMultilingualString("98a1a54d-2e1d-45f5-baac-98a35ef280b4", "The staff group that will receive notifications about commission agreement conflicts."),
											new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
											RegistryStorageFlags.System | RegistryStorageFlags.Company,
											DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsValueOptional,
											Guid.Empty);
					return result;
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default Email Template Text")]
		public NotificationEmailTemplateRegistryItem CommissionAgreementConflictNotificationGroupEmailTemplate
		{
			get
			{
				return GetItem("CommissionAgreementConflictNotificationGroupEmailTemplate", delegate
				{
					const string defaultSubject = "Commission Agreement (*GenericAgreementID*) ((*GenericAgreementCustomer*)) Conflict";
					const string defaultBody = @"Dear (*RecipientName*),

Commission agreement (*SpecificAgreementIDHyperlink*) has recently been added by (*SpecificAgreementCreateUser*) which has a subset of commission in conflict with commission agreement (*GenericAgreementIDHyperlink*).

The commission for the following items will now be shared amongst the wolf pack members listed in the new agreement (*SpecificAgreementIDHyperlink*):
 Client: (*GenericAgreementCustomer*)
(*SpecificAgreementItems*)


You are receiving this email because you are a member of the commission agreement conflict notification group.";

					return new NotificationEmailTemplateRegistryItem(
							"CommissionAgreementConflictNotificationGroupEmailTemplate",
							Categories.SalesMarketing_Commission,
							ResString.GetMultilingualString("9dc220c6-5409-4c60-958d-64cba3d7ad09", "Commission Agreement Conflict Email Template for Notification Group"),
							ResString.GetMultilingualString("5ca51716-3b8c-4937-abc2-a58ffe42329e", "The email template that will be sent to the commission agreement conflict notification group when a more specific agreement is added."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							typeof(DocCommissionAgreementConflictEmailCreator),
							defaultSubject,
							defaultBody);
				});
			}
		}

		public BooleanRegistryItem SendCommissionAgreementConflictEmailToAllRecipients
		{
			get
			{
				return GetItem("SendCommissionAgreementConflictEmailToAllRecipients", delegate
				{
					return new BooleanRegistryItem(
											"SendCommissionAgreementConflictEmailToAllRecipients",
											Categories.SalesMarketing_Commission,
											ResString.GetMultilingualString("1b7d9857-8188-458a-9ed5-66d84d5a562e", "Send Commission Agreement Conflict Notification Email to Wolf Pack"),
											ResString.GetMultilingualString("ca02cf4a-0f67-409a-9765-27d50eda7fa6", "When this flag is set to true, a notification email will be sent to all wolf pack members of an existing agreement when a more specific agreement is added."),
											RegistryStorageFlags.System | RegistryStorageFlags.Company,
											DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
											true);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default Email Template Text")]
		public NotificationEmailTemplateRegistryItem CommissionAgreementConflictRecipientsEmailTemplate
		{
			get
			{
				return GetItem("CommissionAgreementConflictRecipientsEmailTemplate", delegate
				{
					const string defaultSubject = "Commission Agreement (*GenericAgreementID*) ((*GenericAgreementCustomer*)) Conflict";
					const string defaultBody = @"Dear (*RecipientName*),

Commission agreement (*SpecificAgreementIDHyperlink*) has recently been added by (*SpecificAgreementCreateUser*).
You are receiving this email because a subset of commission in the mentioned agreement is in conflict with commission agreement (*GenericAgreementIDHyperlink*) of which you are a member of its wolf pack.

The commission for the following items will now be shared amongst the wolf pack members listed in the new agreement (*SpecificAgreementIDHyperlink*):
 Client: (*GenericAgreementCustomer*)
(*SpecificAgreementItems*)";

					return new NotificationEmailTemplateRegistryItem(
							"CommissionAgreementConflictRecipientsEmailTemplate",
							Categories.SalesMarketing_Commission,
							ResString.GetMultilingualString("d065ca65-465b-4459-b0a5-7809bb26b118", "Commission Agreement Conflict Email Template for Wolf Pack"),
							ResString.GetMultilingualString("f59e6707-98cc-47fd-8380-784f8c8fc631", "The email template that will be sent to existing commission agreement wolf pack members when a more specific agreement is added."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							typeof(DocCommissionAgreementConflictEmailCreator),
							defaultSubject,
							defaultBody);
				});
			}
		}

		public DateTimeRegistryItem DefaultSpecifiedBackdate
		{
			get
			{
				return GetItem("DefaultSpecifiedBackdate", delegate
				{
					return new DateTimeRegistryItem(
							"DefaultSpecifiedBackdate",
							Categories.SalesMarketing_Commission,
							ResString.GetMultilingualString("20abc661-e431-4389-83ea-f0c1908c463c", "Commission Agreement Approval Default Specified Backdate"),
							ResString.GetMultilingualString("8b7a9297-bda4-4e91-b154-0e9d628a21d3", "The default value for Backdate Commission Options -> From -> Specified Date, on the Commission Agreement Approval form."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#endregion

		#region Default Values

		public ARTermsRegistryItem TermsAndTermDays
		{
			get
			{
				return GetItem("TermsAndTermDays", delegate
				{
					ARTermsCollection @default = new ARTermsCollection();
					ARTerms item = @default.AddNew();

					return new ARTermsRegistryItem(
						"TermsAndTermDays",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("79733cb1-5aa0-4e79-8624-2572531fbc1e", "Terms and Term Days"),
						ResString.GetMultilingualString("bd397d4a-159e-47ce-a3c4-f7f4fcef014e", "When an Organization is newly set as Receivables, Terms and Term Days will be populated with the following."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						@default
						);
				});
			}
		}

		public DefaultOrgTimetableRegistryItem DefaultOrgTimetable
		{
			get
			{
				return GetItem("DefaultOrgTimetable", delegate
				{
					DefaultOrgTimetableSettingsCollection @default = new DefaultOrgTimetableSettingsCollection();
					var settings = @default.AddNew();
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Pickup, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "MON", settings.Timetables);
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Deliver, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "MON", settings.Timetables);
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Pickup, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "TUE", settings.Timetables);
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Deliver, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "TUE", settings.Timetables);
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Pickup, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "WED", settings.Timetables);
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Deliver, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "WED", settings.Timetables);
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Pickup, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "THU", settings.Timetables);
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Deliver, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "THU", settings.Timetables);
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Pickup, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "FRI", settings.Timetables);
					settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Deliver, new ZDateTime(1900, 1, 1, 9, 0, 0), new ZDateTime(1900, 1, 1, 17, 0, 0), "FRI", settings.Timetables);

					return new DefaultOrgTimetableRegistryItem(
						"DefaultOrgTimetable",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("2605CF38-3BD7-4D1B-8717-D907EF76E552", "Pickup & Delivery Timetable"),
						ResString.GetMultilingualString("DA973255-8830-4370-AEBA-E7CA70FA7FD4", "Pickup & Delivery Timetable"),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						@default
						);
				});
			}
		}

		public CodePairRegistryItem PreApprovalTerms
		{
			get
			{
				return GetItem("PreApprovalTerms", delegate
				{
					return new CodePairRegistryItem(
						"PreApprovalTerms",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("b101144c-e5f4-41df-aa86-5f2cf816b322", "Pre-Approval Terms"),
						ResString.GetMultilingualString("3603e574-c251-4c81-bb1a-9c78c5b627a8", "When Credit is not approved, this pre-approval term is used as a default. (If credit is on hold, the registry item for on hold takes precedence over this one.)"),
						new CodeDescriptionPairListProvider(() => new ARInvoiceTermsList()),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ARInvoiceTermsList.CashOnDelivery.Code);
				});
			}
		}
		public CodePairRegistryItem InvoiceTermsEndOfWeek
		{
			get
			{
				return GetItem("InvoiceTermsEndOfWeek", delegate
				{
					var item = new CodePairRegistryItem(
						"InvoiceTermsEndOfWeek",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("E6D8F9C3-07B7-4A83-A06D-592CA55ED503", "Invoice Term - From End of Week"),
						ResString.GetMultilingualString("BDC3BAA8-1572-44E3-B521-F99EF3706C7C", @"This registry define the 'Last Day of the Week' for the purpose of  'EWK - From End of Week' Invoice Term's due date calculation. By default, this registry is set to 'SUN-Sunday'.Please override the value where applicable.
						Example 1
						If the last day of the week is set to Sunday and the invoice term days is set to 10 days.
						An invoice issued with an 'Invoice Date' of Tuesday, 02-Apr-19 will have a 'Due Date' of 17-Apr-19.
						The calculation will be as follows:
						Step 1: The End of Week in relation to the Invoice Date will be Sunday 07-Apr-19.
						Step 2: Add 10 days to 07-Apr-19 to compute the Due Date which is 17-Apr-19.

						Example 2
						If the last day of the week is set to Friday and the invoice term days is set to 10 days.
						An invoice issued with an 'Invoice Date' of Friday, 05-Apr-19 will have a 'Due Date' of 15-Apr-19.
						The calculation will be as follows:
						Step 1: The End of Week in relation to the Invoice Date will be Friday 05-Apr-19.
						Step 2: Add 10 days to 05-Apr-19 to compute the Due Date which is 15-Apr-19."),
						new CodeDescriptionPairListProvider(() => new DayOfWeekCodeList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						DayOfWeekCodeList.Codes.Sunday);
					item.OnBuildLogReference += (args) => Res.GetString("943607DB-F403-46F8-9573-941046707B20",
						"Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		public OnHoldTermsRegistryItem OnHoldTerms
		{
			get
			{
				return GetItem("OnHoldTerms", delegate
				{
					return new OnHoldTermsRegistryItem(
						"OnHoldTerms",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("b157d102-510e-4f67-8016-3a8189317b1f", "On Hold Terms"),
						ResString.GetMultilingualString("d014cc90-a11f-449e-86c8-c3a5e0487a3c", "When Credit is on hold, this term is used as a default. (If credit is also not approved, this registry item takes precedence over the registry item for pre-approval.)"),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public BooleanRegistryItem UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold
		{
			get
			{
				return GetItem("UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold", delegate
				{
					return new BooleanRegistryItem(
						"UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("685A4825-E267-44D4-A81D-BC793E400F65", "Use AR Invoice Terms And Term Days When Credit Is On Hold"),
						ResString.GetMultilingualString("7612B4BB-4227-498B-A5A3-DAB514E47D52", @"By default, this registry will be set to 'No' in which case the system will use the invoice terms and term days as configured under Master Data > Organizations > Default Values > On Hold Terms when issuing AR invoices if the debtor is put on credit hold.

When this registry is set to 'Yes', the system will use the invoice terms and term days as configured under Organization > A/R > Credit Control and Settlement > Local > Terms and Term Days."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public InvoiceRollupOrGroupRegistryItem InvoiceRollupOrGroup
		{
			get
			{
				return GetItem("InvoiceRollupOrGroup", delegate
				{
					InvoiceRollupOrGroupCollection @default = new InvoiceRollupOrGroupCollection();
					InvoiceRollupOrGroup item = @default.AddNew();
					item.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
					item.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
					item.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
					item.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
					item.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
					item.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
					item.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

					return new InvoiceRollupOrGroupRegistryItem(
						"InvoiceRollupOrGroup",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("AC0B1BD7-3557-4684-B84F-684F60B6C19D", "Charge Grouping & Roll Up"),
						ResString.GetMultilingualString("ad16bfc9-8c39-4194-acef-8de67c95ec7f", "Specify the grouping , roll up, posting and display settings Accounts Receivable Invoices that are sent to customers. These settings are the defaults for new organizations, and can be overridden on each organization."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						@default);
				});
			}
		}

		#region Charge Grouping & Roll Up

		public RatingDocRollupOrGroupRegistryItem RatingDocRollupOrGroup
			=> GetItem
			(
				"RatingDocRollupOrGroup",
				delegate
				{
					var shouldBeHidden = DataRegistry.Instance.ProductivityWiseModeEnabled
						|| !RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

					var @default = new RatingDocRollupOrGroupRegistryCollection();
					var ratingDocRollupOrSort = @default.AddNew();
					ratingDocRollupOrSort.Module = DocRollupOrSortModuleList.Codes.All;
					ratingDocRollupOrSort.JobType = DocRollupOrSortJobTypeList.Codes.All;
					ratingDocRollupOrSort.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
					ratingDocRollupOrSort.Display = DocRollupOrSortDisplayList.Codes.RollUpCharges;
					ratingDocRollupOrSort.Style = DocRollupOrSortStyleList.Codes.NoGrouping;

					return new RatingDocRollupOrGroupRegistryItem
					(
						"RatingDocRollupOrGroup",
						Categories.Organizations_Rating,
						ResString.GetMultilingualString("2647abd8-6c0f-43b4-8c26-7337e21b8bf5", "Rating Documents Charge Grouping and Roll Up"),
						ResString.GetMultilingualString("411818b7-7863-4352-bffd-551f80a799bd", "Specify the module, job type, transport mode, display, and style for Rating Documents Printing. These settings are the defaults for new organizations, and can be overridden on each organization."),
						RegistryStorageFlags.All,
						shouldBeHidden ? RegistryOptions.IsHidden : RegistryOptions.Default,
						@default
					);
				}
			);

		#endregion

		public DefaultOSMGRegistryItem OrgSecurityManagementGroupDefault
		{
			get
			{
				return GetItem("OrgSecurityManagementGroupDefault", delegate
				{
					return new DefaultOSMGRegistryItem(
						"OrgSecurityManagementGroupDefault", Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("06b5c056-1006-4570-b69c-d343b3cd504f", "Organization Security Management Group Default"),
						ResString.GetMultilingualString("06b655e6-86e9-4b7d-aa79-b5a1c75b3c54", "Any new Organization will be defaulted to the below Organization Security Management Group."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						new DefaultOSMG());
				});
			}
		}

		public OrgSecurityProfileRegistryItem WebSecurityDefaultValues
		{
			get
			{
				return GetItem("WebSecurityDefaultValues", delegate
				{
					return new OrgSecurityProfileRegistryItem(
						"WebSecurityDefaultValues",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("626f7645-e306-4b11-aaf1-10cfaf6b5685", "Web Security Default Values"),
						ResString.GetMultilingualString("d4077359-2029-42bb-8d50-01e4e247bc96", "Below are the Web Security Default Values for web access. Click on GLOW Portal User Admin to manage Glow Web Security Rights."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new OrgSecurityProfileCollection());
				});
			}
		}

		public BooleanRegistryItem WebSecurityRightsDeniedByDefault
		{
			get
			{
				return GetItem("WebSecurityRightsDeniedByDefault", () =>
					new BooleanRegistryItem("WebSecurityRightsDeniedByDefault",
						Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("231976d3-9554-4989-9929-ef61811f716d", "Web Security Rights Denied By Default"),
						ResString.GetMultilingualString("4e1154c9-bf6d-4885-bde5-a24387819a90", "When enabled, all Web Security Rights will be denied by default (unless explicitly overridden to be granted)."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false));
			}
		}

		#endregion

		#region GUI State

		public IntRegistryItem GuiStateIntegers
		{
			get
			{
				return GetItem("GuiStateInts", delegate
				{
					return new IntRegistryItem(
											"GuiStateInts",
											Categories.Organizations_GUIState,
											null,
											null,
											RegistryStorageFlags.Company,
											RegistryOptions.NotLogged | RegistryOptions.IsHidden,
											-1);
				});
			}
		}

		#endregion

		#region Maximum Credit Limit

		public MaximumCreditLimitCollectionRegistryItem MaximumCreditLimit
		{
			get
			{
				return GetItem("MaximumCreditLimit", delegate
				{
					return new MaximumCreditLimitCollectionRegistryItem(
						"MaximumCreditLimit",
						Categories.Organizations_CreditReports,
						(NoResString)"Maximum Credit Limit",
						(NoResString)"Users are required to purchase a Credit Report when a credit limit exceeds this value. Select the type of report to be purchased when a Credit Limit exceeds the maximum credit limit.",
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						new MaximumCreditLimitCollection());
				});
			}
		}

		public IntRegistryItem CreditReportLifetime
		{
			get
			{
				return GetItem("CreditReportLifetime", delegate
				{
					const int defaultValue = 12;
					const int minValue = 1;
					const int maxValue = 12;

					return new IntRegistryItem(
						"CreditReportLifetime",
						Categories.Organizations_CreditReports,
						(NoResString)"Credit Report Lifetime",
						(NoResString)"The length of time in months a Credit Report is considered valid for the purposes of the Maximum Credit Limit.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue,
						minValue,
						maxValue);
				});
			}
		}

		#endregion

		#region Import Credit Report
		public BooleanRegistryItem EnableImportFromCreditReports
		{
			get => GetItem("EnableImportFromCreditReports", () => new BooleanRegistryItem(
				"EnableImportFromCreditReports",
				Categories.Organizations_CreditReports,
				(NoResString)"Enable Import from Credit Reports",
				(NoResString)"Enables Importing from Credit Reports when this flag is True.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false
				));
		}

		#endregion

		#region Allow Job Override Address Additional Information 

		public BooleanRegistryItem AllowOverrideAddressAdditionalInformation
		{
			get
			{
				return GetItem("JobOverrideAddressAdditionalInformation", delegate
				{
					return new BooleanRegistryItem("JobOverrideAddressAdditionalInformation",
						Categories.Organizations,
						ResString.GetMultilingualString("993090BA-C043-41CC-9FD6-FC1C99A6EB0B", "Allow Job Override Address Additional Information"),
						ResString.GetMultilingualString("1C2AD459-8973-419B-906A-49489B9DC456", "Setting this registry to \"Yes\" enables users to override the Additional Address Info field on jobs linked to an organization without breaking the link to the organization."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Address Validation

		MDMSupportCertificateRegistryItem MDMSupportCertificate
		{
			get
			{
				return GetItem("MDMSupportCertificate", delegate
				{
					return new MDMSupportCertificateRegistryItem(
					"MDMSupportCertificate",
					Categories.Organizations_AddressValidationService,
					(NoResString)"MDM Support Certificate For AVS",
					(NoResString)"With the System To System Trust Authentication option turned on for Address Validation Web Service uri, if a MDM Support Certificate exists, it takes precedence.",
					MDMProductCodes.AVS);
				});
			}
		}

		public SystemToSystemTrustInfo MDMSupportCertificateInfo
		{
			get => MDMSupportCertificate.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#endregion Address Validation

		#region EnableBoleroEHBLIntegration

		public BoleroEBLForOrganisationConfigurationRegistryItem EnableBoleroEHBLIntegration
		{
			get => GetItem<BoleroEBLForOrganisationConfigurationRegistryItem>("EnableBoleroEHBLIntegrationForOrg", () =>
				new BoleroEBLForOrganisationConfigurationRegistryItem(
					"EnableBoleroEHBLIntegrationForOrg",
					Categories.Organizations,
					(NoResString)"Enable Bolero eHBL Integration",
					(NoResString)"If yes, enable Bolero eHBL Integration for testing.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					new BoleroEBLForOrganisationConfiguration())
				);
		}

		#endregion
	}
}
