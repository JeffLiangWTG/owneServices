using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Argentina
{
	public static class AFIPEquivalents
	{
		#region ElectronicInvoiceCurrencyEquivalentCodes

		public static class CurrencyEquivalentCodes
		{
			#region SuppressResourceStringsCheckRegion

			public static readonly ImmutableDictionary<string, string> CurrencyCodes = new Dictionary<string, string>()
			{
					{ "ANG" , "028" },
					{ "ARS" , "PES" },
					{ "AUD" , "026" },
					{ "BOB" , "031" },
					{ "BRL" , "012" },
					{ "CAD" , "018" },
					{ "CHF" , "009" },
					{ "CLP" , "033" },
					{ "CNY" , "064" },
					{ "COP" , "032" },
					{ "CZK" , "024" },
					{ "DKK" , "014" },
					{ "DOP" , "042" },
					{ "EGP" , "046" },
					{ "EUR" , "060" },
					{ "GBP" , "021" },
					{ "GTQ" , "055" },
					{ "HKD" , "051" },
					{ "HNL" , "063" },
					{ "HUF" , "056" },
					{ "ILS" , "030" },
					{ "INR" , "062" },
					{ "JMD" , "053" },
					{ "JPY" , "019" },
					{ "KWD" , "059" },
					{ "MAD" , "045" },
					{ "MKD" , "025" },
					{ "MXN" , "010" },
					{ "NIO" , "044" },
					{ "NOK" , "015" },
					{ "PAB" , "043" },
					{ "PEN" , "035" },
					{ "PLN" , "061" },
					{ "PYG" , "029" },
					{ "RON" , "040" },
					{ "SAR" , "047" },
					{ "SEK" , "016" },
					{ "SGD" , "052" },
					{ "THB" , "057" },
					{ "TWD" , "054" },
					{ "USD" , "DOL" },
					{ "UYU" , "011" },
					{ "VES" , "023" },
					{ "ZAR" , "034" },
				}.ToImmutableDictionary();

			#endregion
		}

		#endregion

		#region DocumentTypeEquivalentsCodes

		public static class DocumentTypeEquivalentCodes
		{
			#region SuppressResourceStringsCheckRegion

			public static readonly ImmutableDictionary<ZString, string> EquivalentCodes = new Dictionary<ZString, string>()
				{
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA,"1" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA,"2" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA,"3" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXB,"6" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDB,"7" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB,"8" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXC,"11" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDC,"12" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCC,"13" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE,"19" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE,"20" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCE,"21" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXM,"51" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDM,"52" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCM,"53" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA,"201" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA,"202" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA,"203" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXB,"206" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB,"207" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB,"208" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXC,"211" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC,"212" },
					{ ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC,"213" },
			}.ToImmutableDictionary();

			#endregion
		}

		#endregion

		#region OrgCusCodesEquivalent

		public static class OrgCusCodeEquivalentCodes
		{
			#region SuppressResourceStringsCheckRegion

			public static ZString GetRegTypeCodeEquivalent(ZString registrationCode)
			{
				switch (registrationCode)
				{
					case ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT:
					case ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF:
						return "80";
					case ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL:
						return "86";
					case ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI:
						return "96";
					case OrgCusCode.CodeTypes.PassportID:
						return "94";
					default:
						return default;
				}
			}

			#endregion
		}

		#endregion
	}
}
