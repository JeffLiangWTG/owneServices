using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class IrelandOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCode.IrelandCodeTypes.PYE, Res.GetString("OrgCusCode.IrelandCodeTypes.PYE", "Pay as you earn"));
			list.AddPair(OrgCusCode.IrelandCodeTypes.ITX, Res.GetString("OrgCusCode.IrelandCodeTypes.ITX", "Income tax"));
			list.AddPair(OrgCusCode.IrelandCodeTypes.CGT, Res.GetString("OrgCusCode.IrelandCodeTypes.CGT", "Capital gains tax"));
			list.AddPair(OrgCusCode.IrelandCodeTypes.VatFreeAuthorisation, Res.GetString("B5DF5CB5-936C-43E2-BE27-609C3FFD7A79", "VAT Free Authorization"));
			list.AddPair(OrgCusCode.IrelandCodeTypes.TraderAccountNumber, Res.GetString("6BBD54EF-173E-4D71-8FDE-2E51DACE4A28", "Trader Account Number"));
			list.AddPair(OrgCusCode.IrelandCodeTypes.VatZeroRatedAct2010, Res.GetString("35E571AC-F76D-424D-A3B1-D4AFD75DF4B3", "Zero-Rated VAT Authorization"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Ireland);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Ireland);
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			if (orgCusCode.OK_CodeType == OrgCusCode.CodeTypes.VATCode)
			{
				new IEVATCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}
	}
}
