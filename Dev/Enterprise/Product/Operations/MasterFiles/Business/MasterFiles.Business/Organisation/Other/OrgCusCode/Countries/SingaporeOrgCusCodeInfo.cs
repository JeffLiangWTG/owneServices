using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class SingaporeOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.GSTCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.GSTCode)); // Accounting consumption code

			list.OverridePair(OrgCusCode.CodeTypes.GSTCode, Res.GetString("OrgCusCode.CodeTypes.SGGSTCodeTemplate", "Government GST Number"));
			list.OverridePair(OrgCusCode.CodeTypes.CorporationCode, Res.GetString("OrgCusCode.CodeTypes.CorporationCode", "Accounting and Corporate Regulatory Authority Number"));
			list.AddPair(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, Res.GetString("OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber", "Central Registration (CR) Number"));
			list.AddPair(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, Res.GetString("OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber", "Unique Entity Number"));
			list.AddPair(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber, Res.GetString("OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber", "Central Provident Fund Number"));
			list.AddPair(OrgCusCode.CodeTypes.IdentityCardNumber, Res.GetString("OrgCusCode.CodeTypes.IdentityCardNumber", "Identity Card Number"));
			list.AddPair(OrgCusCode.SingaporeCodeTypes.QualifiedCompanyIdentificationCode, Res.GetString("OrgCusCode.SingaporeCodeTypes.QualifiedCompanyIdentificationCode", "Qualified Company Identification Code"));
			list.AddPair(OrgCusCode.SingaporeCodeTypes.PartyStatusType, Res.GetString("OrgCusCode.SingaporeCodeTypes.PartyStatusType", "Party Status Type"));
			list.AddPair(OrgCusCode.SingaporeCodeTypes.InterbankGIRO, Res.GetString("OrgCusCode.SingaporeCodeTypes.InterbankGIRO", "Interbank GIRO (IBG)"));
			list.AddPair(OrgCusCode.SingaporeCodeTypes.DirectDelivery, Res.GetString("OrgCusCode.SingaporeCodeTypes.DirectDelivery", "Direct Delivery"));
			list.AddPair(OrgCusCode.SingaporeCodeTypes.AEO, Res.GetString("OrgCusCode.SingaporeCodeTypes.AEO", "Authorized Economic Operator"));
			list.RemoveCode(OrgCusCode.CodeTypes.GovBusinessCode);
			list.RemoveCode(OrgCusCode.CodeTypes.CustomsClientCode);
			list.RemoveCode(OrgCusCode.CodeTypes.SupplierCode);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokerageRegistration);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokerageSiteID);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokeragePrinter);
			list.RemoveCode(OrgCusCode.CodeTypes.CarrierCode);
			list.RemoveCode(OrgCusCode.CodeTypes.ManifestProviderID);
			list.RemoveCode(OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Singapore);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber);
			result.Add(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber);
			result.Add(OrgCusCode.SingaporeCodeTypes.PartyStatusType);
			result.Add(OrgCusCode.SingaporeCodeTypes.InterbankGIRO);
			result.Add(OrgCusCode.SingaporeCodeTypes.DirectDelivery);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Singapore);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			result.Add(OrgCusCode.SingaporeCodeTypes.CentralProvidentFundNumber);
			return result;
		}
	}
}
