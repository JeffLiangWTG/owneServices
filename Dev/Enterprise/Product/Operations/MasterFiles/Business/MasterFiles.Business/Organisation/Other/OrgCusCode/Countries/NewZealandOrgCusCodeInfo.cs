using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class NewZealandOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.GSTCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.GSTCode)); // Accounting consumption code

			list.AddPair(OrgCusCode.CodeTypes.CompanyNumber, Res.GetString("OrgCusCode.CodeTypes.CompanyNumber", "Company Number"));
			list.AddPair(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, Res.GetString("OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility", "Approved Transitional Facility Code"));
			list.AddPair(OrgCusCode.NZCodeTypes.SecureExportPartner, Res.GetString("OrgCusCode.NZCodeTypes.SecureExportPartner", "Secure Export Partner Code"));
			list.AddPair(OrgCusCode.NZCodeTypes.RegistrationNumber, Res.GetString("OrgCusCode.NZCodeTypes.RegistrationNumber", "Registration Number"));
			list.AddPair(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, Res.GetString("OrgCusCode.NZCodeTypes.MAFCoverSheetQE", "MPI Application Coversheet Account QE"));
			list.AddPair(OrgCusCode.NZCodeTypes.AEO, Res.GetString("OrgCusCode.NZCodeTypes.AEO", "Authorized Economic Operator"));
			list.RemoveCode(OrgCusCode.CodeTypes.GovBusinessCode);
			list.RemoveCode(OrgCusCode.CodeTypes.CorporationCode);
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokerageSiteID);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokeragePrinter);
			list.RemoveCode(OrgCusCode.CodeTypes.CarrierCode);
			list.RemoveCode(OrgCusCode.CodeTypes.ManifestProviderID);
			list.RemoveCode(OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.NewZealand);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			result.Add(OrgCusCode.NZCodeTypes.RegistrationNumber);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.NewZealand);
			result.Add(OrgCusCode.CodeTypes.CompanyNumber);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}
	}
}
