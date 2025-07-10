using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class HungaryOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string IDM = "IDM";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code
			list.AddPair(OrgCusCodes.IDM, Res.GetString("OrgCusCode.CodeTypes.InvoiceDeliveryMethod", "Invoice Delivery Method"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Hungary);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Hungary);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			if (orgCusCode.OK_CodeType == OrgCusCode.CodeTypes.VATCode)
			{
				new HUVATCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
			}

			if (orgCusCode.OK_CodeType == OrgCusCodes.IDM)
			{
				new HUIDMCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}
	}
}
