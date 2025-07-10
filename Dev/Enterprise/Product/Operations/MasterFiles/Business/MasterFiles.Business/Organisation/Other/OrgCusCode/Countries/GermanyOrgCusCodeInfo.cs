using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GermanyOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		ZString CountryCode => Core.Constants.CountryCodes.Germany;

		public static class OrgCusCodes
		{
			public const string CWC = "CWC";
			public const string ZAP = "ZAP";
			public const string BHT = "BHT";
			public const string UST = "UST";
			public const string GermanCivilAviationAuthority = "GCA";
			public const string Handelsregister = "HRB";
			public const string DakosyParticipantCode = "DPC";
			public const string DakosyBerthCode = "DBI";
			public const string EoriBranchSuffix = "EBS";
			public const string AuthorizationTemporaryStorage = "ATS";
			public const string ATLASParticipantIdentificationNumber = "API";
			public const string EMCSParticipantIdentificationNumber = "EPI";
			public const string EMCSWarehouseParticipantIdentificationNumber = "WPI";
			public const string LocalClearanceOutwardProcessing = "LCO";
			public const string AccreditedExporter = "AEX";
			public const string TaxOffice = "TAO";
			public const string MST = "MST";
			public const string LID = "LID";
			public const string ZMI = "ZMI";
			public const string SteuerlicheIdentifikationsnummer = "STE";
			public const string IMA = "IMA";
		}

		public static class OrgCusCodeDescription
		{
			public static string CWC => Res.GetString("EB89D5F5-2DEA-43B0-8D05-54B26909FB65", "Customs Warehouse Code");
			public static string BHT => Res.GetString("F9636A9D-1149-4FF7-8B30-D3F6B40DB3B0", "Carrier code for BHT Port Community System");
			public static string ZAP => Res.GetString("269A5C38-3C26-4445-87E9-31982F0CACAC", "Carrier code for ZAPP Customs Export Declaration System");
			public static string GermanCivilAviationAuthority => Res.GetString("793D1FBC-7E85-4018-89DF-D1A20D039825", "German Civil Aviation Authority Code");
			public static string Handelsregister => Res.GetString("BCA2CD57-606E-4D87-8422-4B6EB7A5D3D9", "Handelsregister / Business Registration Number");
			public static string DakosyParticipantCode => Res.GetString("BB15AFAD-5CE5-48AD-910C-6532F39D79A3", "Dakosy Participant Code");
			public static string DakosyBerthCode => Res.GetString("7F0C2A46-0AE5-441A-BC39-9ECFEB59039F", "Dakosy Berth Id");
			public static string EoriBranchSuffix => Res.GetString("002A1964-9290-488E-9345-5728D230B723", "EORI branch suffix");
			public static string AuthorizationTemporaryStorage => Res.GetString("9FB31382-258C-4990-B2F1-0F462701A67A", "Authorization Temporary Storage");
			public static string ATLASParticipantIdentificationNumber => Res.GetString("1A9EA697-6976-4F28-9343-15FDBB4BE2DF", "ATLAS Participant Identification Number");
			public static string EMCSParticipantIdentificationNumber => Res.GetString("2885F590-DF74-4B23-94B3-80C7A1F532C5", "EMCS Participant Identification Number");
			public static string EMCSWarehouseParticipantIdentificationNumber => Res.GetString("55F2A39A-1C34-4F9A-A138-18D1ABE65C93", "EMCS Warehouse Participant Identification Number");
			public static string AccreditedExporter => Res.GetString("A4EC33AB-440C-4CA8-9483-9214E6DD5A7F", "Accredited Exporter");
			public static string LocalClearanceOutwardProcessing => Res.GetString("75CE7CA7-0403-47B2-A055-79DC86385A93", "Local Clearance Outward Processing (A7)");
			public static string TaxOffice => Res.GetString("42D35B56-8CA9-4469-9535-88CC9A0CE6A0", "Tax Office");
			public static string LID => "Leitweg-ID";
			public static string UST => Res.GetString("A3F9358E-29B9-423E-882B-0475A5C835C7", "VAT ({0}) Business Registration Number", "Ust ID");
			public static string MST => (NoResString)"Mehrwertsteuer";
			public static string ZMI => (NoResString)"ZM Registrierungs-ID";
			public static string SteuerlicheIdentifikationsnummer => (NoResString)"Steuerliche Identifikationsnummer";
			public static string IMA => Res.GetString("68442372-F5C9-4682-957B-AE16A886A25C", "IDEV Material number");
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.UST, OrgCusCodeDescription.UST); // Accounting consumption code

			list.AddPair(OrgCusCodes.LID, OrgCusCodeDescription.LID);
			list.AddPair(OrgCusCodes.CWC, OrgCusCodeDescription.CWC);
			list.AddPair(OrgCusCodes.BHT, OrgCusCodeDescription.BHT);
			list.AddPair(OrgCusCodes.ZAP, OrgCusCodeDescription.ZAP);
			list.AddPair(OrgCusCodes.GermanCivilAviationAuthority, OrgCusCodeDescription.GermanCivilAviationAuthority);
			list.AddPair(OrgCusCodes.Handelsregister, OrgCusCodeDescription.Handelsregister);
			list.AddPair(OrgCusCodes.DakosyParticipantCode, OrgCusCodeDescription.DakosyParticipantCode);
			list.AddPair(OrgCusCodes.DakosyBerthCode, OrgCusCodeDescription.DakosyBerthCode);
			list.AddPair(OrgCusCodes.EoriBranchSuffix, OrgCusCodeDescription.EoriBranchSuffix);
			list.AddPair(OrgCusCodes.AuthorizationTemporaryStorage, OrgCusCodeDescription.AuthorizationTemporaryStorage);
			list.AddPair(OrgCusCodes.ATLASParticipantIdentificationNumber, OrgCusCodeDescription.ATLASParticipantIdentificationNumber);
			list.AddPair(OrgCusCodes.EMCSParticipantIdentificationNumber, OrgCusCodeDescription.EMCSParticipantIdentificationNumber);
			list.AddPair(OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber, OrgCusCodeDescription.EMCSWarehouseParticipantIdentificationNumber);
			list.AddPair(OrgCusCodes.AccreditedExporter, OrgCusCodeDescription.AccreditedExporter);
			list.AddPair(OrgCusCodes.LocalClearanceOutwardProcessing, OrgCusCodeDescription.LocalClearanceOutwardProcessing);
			list.AddPair(OrgCusCodes.TaxOffice, OrgCusCodeDescription.TaxOffice);
			list.AddPair(OrgCusCodes.MST, OrgCusCodeDescription.MST);
			list.AddPair(OrgCusCodes.ZMI, OrgCusCodeDescription.ZMI);
			list.AddPair(OrgCusCodes.SteuerlicheIdentifikationsnummer, OrgCusCodeDescription.SteuerlicheIdentifikationsnummer);
			list.AddPair(OrgCusCodes.IMA, OrgCusCodeDescription.IMA);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCodes.Handelsregister);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);

			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			OrgCusCodeValidation.ValidateCustomsCodeEORI(customsRegNo);
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCodes.Handelsregister:
					GermanValidationHelper.ValidateHandelsRegister(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.EoriBranchSuffix:
					GermanValidationHelper.ValidateEBS(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.UST:
					GermanValidationHelper.ValidateUST(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.AccreditedExporter:
					GermanValidationHelper.ValidateAEX(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCode.EuropeanUnionSharedCodeTypes.OutwardProcessingReliefNumber:
					GermanValidationHelper.ValidateOPR(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.LocalClearanceOutwardProcessing:
					GermanValidationHelper.ValidateLCO(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.TaxOffice:
					GermanValidationHelper.ValidateTAO(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.DakosyBerthCode:
					GermanValidationHelper.ValidateDBI(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.IMA:
					GermanValidationHelper.ValidateIMA(customsRegNo.OK_CustomsRegNoInfo);
					break;
			}
		}
	}
}
