using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class IndiaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string SER = "SER";
			public const string PAN = "PAN";
			public const string TAN = "TAN";
			public const string IEC = "IEC";
			public const string UIN = "UIN";
			public const string GID = "GID";
			public const string UDY = "UDY";
			public const string ADH = "ADH";
			public const string BSN = "BSN";
			public const string ADC = "ADC";
			public const string CAN = "CAN";
			public const string AEO = "AEO";
		}

		ZString CountryCode => Core.Constants.CountryCodes.India;

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCode.CodeTypes.GSTCode:
					new IndiaGSTCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo, orgCusCode);
					break;
			}
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.GSTCode, Res.GetString("OrgCusCode.CodeTypes.GSTIdentificationNumber", "GSTIN - GST Identification Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.PAN, Res.GetString("OrgCusCode.CodeTypes.PermanentAccountNumber", "Permanent Account Number"));
			list.AddPair(OrgCusCodes.TAN, Res.GetString("OrgCusCode.CodeTypes.TaxDeductionAndCollectionAccountNumber", "Tax Deduction and Collection Account Number"));
			list.AddPair(OrgCusCodes.IEC, Res.GetString("OrgCusCode.CodeTypes.ImporterExportCode", "Importer Export Code"));
			list.AddPair(OrgCusCodes.SER, Res.GetString("Organisation|CustomsCodes|SERIndia", "Service Tax Code"));
			list.AddPair(OrgCusCodes.UIN, Res.GetString("OrgCusCode.CodeTypes.GSTUniqueIdentificationNumber", "GST Unique Identification Number"));
			list.AddPair(OrgCusCodes.GID, Res.GetString("OrgCusCode.CodeTypes.GovernmentDepartmentIdentificationNumber", "Government Department Identification Number"));
			list.AddPair(OrgCusCodes.UDY, Res.GetString("OrgCusCode.CodeTypes.Udyam", "UDYAM (MSME) Registration Number"));
			list.AddPair(OrgCusCodes.ADH, Res.GetString("OrgCusCode.CodeTypes.ADH", "Aadhar Card Number"));
			list.AddPair(OrgCusCodes.BSN, Res.GetString("OrgCusCode.CodeTypes.BranchSerialNumber", "Branch Serial Number"));
			list.AddPair(OrgCusCodes.ADC, Res.GetString("OrgCusCode.CodeTypes.AuthorizedDealerCode", "Authorized Dealer Code"));
			list.AddPair(OrgCusCodes.CAN, Res.GetString("OrgCusCode.CodeTypes.ConsolAgentRegistrationNumber", "Consol Agent Registration Number"));
			list.AddPair(OrgCusCodes.AEO, Res.GetString("OrgCusCode.CodeTypes.AuthorizedEconomicOperator", "Authorized Economic Operator"));
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Remove(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCodes.UDY);
			result.Add(OrgCusCodes.PAN);
			result.Add(OrgCusCodes.UIN);
			result.Add(OrgCusCodes.GID);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}
	}
}
