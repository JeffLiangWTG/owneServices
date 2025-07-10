using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class SouthAfricaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.VATCode)); // Accounting consumption code

			list.OverridePair(OrgCusCode.CodeTypes.CustomsClientCode, Res.GetString("OrgCusCode.CodeTypes.CustomsClientCodeSA", "Importer Code"));
			list.OverridePair(OrgCusCode.CodeTypes.SupplierCode, Res.GetString("OrgCusCode.CodeTypes.SupplierCodeSA", "Supplier Code"));
			list.OverridePair(OrgCusCode.CodeTypes.TaxFileCode, Res.GetString("ZAIncomeTaxNumber", "Income Tax Number"));
			list.OverridePair(OrgCusCode.CodeTypes.VATCode, Res.GetString("OrgCusCode.CodeTypes.VATCode", "Government VAT Number"));

			list.AddPair(OrgCusCode.CodeTypes.DepotControlledPremisesID, Res.GetString("OrgCusCode.CodeTypes.DepotControlledPremisesID", "Customs Controlled Premises Code - Depot"));
			list.AddPair(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Res.GetString("OrgCusCode.CodeTypes.WarehouseControlledPremisesID", "Customs Controlled Premises Code - Warehouse"));
			list.AddPair(OrgCusCode.CodeTypes.BondHolderCode, Res.GetString("OrgCusCode.CodeTypes.BondHolderCode", "Bond Holder Customs Carrier Code"));
			list.AddPair(OrgCusCode.CodeTypes.ReleaseAgentCode, Res.GetString("OrgCusCode.CodeTypes.ReleaseAgentCode", "Release Agent Code"));
			list.AddPair(OrgCusCode.CodeTypes.AgentCode, Res.GetString("OrgCusCode.CodeTypes.AgentCode", "Agent Code"));
			list.AddPair(OrgCusCode.CodeTypes.RebateUserCode, Res.GetString("OrgCusCode.CodeTypes.RebateUserCode", "Rebate User Code"));
			list.AddPair(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, Res.GetString("OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode", "Customs Dual Profile Code"));
			list.AddPair(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, Res.GetString("OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode", "Remover User Code"));
			list.AddPair(OrgCusCode.SouthAfricaCodeTypes.IDNumber, Res.GetString("OrgCusCode.SouthAfricaCodeTypes.IDNumber", "ID Number"));
			list.AddPair(OrgCusCode.SouthAfricaCodeTypes.CustomsApprovedExporter, Res.GetString("OrgCusCode.SouthAfricaCodeTypes.CustomsApprovedExporter", "Customs Approved Exporter"));
			list.AddPair(OrgCusCode.CodeTypes.VGMRegistrationNumber, Res.GetString("OrgCusCode.CodeTypes.VGMRegistrationNumber", "VGM Registration Number"));
			list.AddPair(OrgCusCode.SouthAfricaCodeTypes.BillIssuer, Res.GetString("OrgCusCode.SouthAfricaCodeTypes.BillIssuer", "House Bill Issuer Code"));
			list.AddPair(OrgCusCode.SouthAfricaCodeTypes.TPT, Res.GetString("OrgCusCode.SouthAfricaCodeTypes.TPT", "TPT Account Number"));
			list.AddPair(OrgCusCode.SouthAfricaCodeTypes.TNP, Res.GetString("OrgCusCode.SouthAfricaCodeTypes.TNP", "TNPA Registration Number"));
			list.AddPair(OrgCusCode.CodeTypes.TerminalControlledPremisesID, Res.GetString("OrgCusCode.CodeTypes.TerminalControlledPremisesID", "Customs Controlled Premises Code - Terminal"));
			list.AddPair(OrgCusCode.SouthAfricaCodeTypes.BGV, Res.GetString("OrgCusCode.SouthAfricaCodeTypes.BondGuaranteeValue", "Bond Guarantee Value"));
			list.RemoveCode(OrgCusCode.CodeTypes.ControlledPremisesID);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokerageRegistration);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokerageSiteID);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokeragePrinter);
			list.RemoveCode(OrgCusCode.CodeTypes.ManifestProviderID);
			list.RemoveCode(OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.SouthAfrica);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			result.Add(OrgCusCode.CodeTypes.GovBusinessCode);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCode.CodeTypes.VATCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.SouthAfrica);
			return result;
		}
	}
}
