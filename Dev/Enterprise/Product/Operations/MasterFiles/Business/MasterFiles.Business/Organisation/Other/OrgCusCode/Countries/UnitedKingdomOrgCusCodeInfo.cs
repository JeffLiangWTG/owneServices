using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class UnitedKingdomOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCode.CodeTypes.CompanyNumber, Res.GetString("OrgCusCode.CodeTypes.CompanyNumber", "Company Number"));
			list.AddPair(OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode, Res.GetString("OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode", "Gems Customer Code"));
			list.AddPair(OrgCusCode.UnitedKingdomCodeTypes.AirCargoAgentsListedNumber, Res.GetString("OrgCusCode.UnitedKingdomCodeTypes.AirCargoAgentsListedNumber", "Air Cargo Agent's Listed Number"));
			list.AddPair(OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix, Res.GetString("OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix", "EORI branch suffix"));
			list.AddPair(OrgCusCode.UnitedKingdomCodeTypes.CTOShed, Res.GetString("OrgCusCode.UnitedKingdomCodeTypes.CTOShed", "Cargo Terminal Operator Shed"));
			list.AddPair(OrgCusCode.CodeTypes.VGMRegistrationNumber, Res.GetString("OrgCusCode.CodeTypes.VGMRegistrationNumber", "VGM Registration Number"));
			list.AddPair(OrgCusCode.UnitedKingdomCodeTypes.CustomsComprehensiveGuarantee, Res.GetString("OrgCusCode.UnitedKingdomCodeTypes.CustomsComprehensiveGuarantee", "Customs Comprehensive Guarantee"));
			list.AddPair(OrgCusCode.CodeTypes.CustomsOfficeForTransit, Res.GetString("OrgCusCode.CodeTypes.CustomsOfficeForTransit", "Customs Office For Transit"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.UnitedKingdom);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.UnitedKingdom);
			return result;
		}
	}
}
