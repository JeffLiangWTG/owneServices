using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PuertoRicoOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.PuertoRicoCodeTypes.NRC, string.Format((NoResString)"Número de Registro de Comerciantes / {0}", Res.GetString("6bd493dd-66ae-4a2b-a09f-0c73f149d595", "Merchant Registration"))); // Accounting consumption code

			list.OverridePair(OrgCusCode.CodeTypes.CarrierCode, Res.GetString("OrgCusCode.CodeTypes.CarrierCodeStandardUS", "Standard Carrier Alpha Code (Sea)"));
			list.AddPair(OrgCusCode.USACodeTypes.ABIRoutingCode, Res.GetString("OrgCusCode.USACodeTypes.ABIRoutingCode", "ABI Routing Code"));
			list.AddPair(OrgCusCode.USACodeTypes.ACEAssignedNumber, Res.GetString("OrgCusCode.USACodeTypes.ACEAssignedNumber", "ACE Assigned Number"));
			list.AddPair(OrgCusCode.USACodeTypes.FreeAndSecureTradeCode, Res.GetString("OrgCusCode.USACodeTypes.FreeAndSecureTradeCode", "FAST (Free and Secure Trade)"));
			list.AddPair(OrgCusCode.USACodeTypes.ForeignRegistrationNumber, Res.GetString("OrgCusCode.USACodeTypes.ForeignRegistrationNumber", "Foreign Registration Number"));
			list.AddPair(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, Res.GetString("OrgCusCode.CodeTypes.FDAEstablishmentIdentifier", "FDA Establishment Identifier"));
			list.AddPair(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Res.GetString("OrgCusCode.USACodeTypes.EmployerIdentificationNumber", "Employer Identification Number"));
			list.AddPair(OrgCusCode.USACodeTypes.SocialSecurityNumber, Res.GetString("OrgCusCode.USACodeTypes.SocialSecurityNumber", "Social Security Number"));
			list.AddPair(OrgCusCode.USACodeTypes.CBPAssignedNumber, Res.GetString("OrgCusCode.USACodeTypes.CBPAssignedNumber", "CBP Assigned Number"));
			list.AddPair(OrgCusCode.USACodeTypes.CBPAssignedSuretyCode, Res.GetString("OrgCusCode.USACodeTypes.CBPAssignedSuretyCode", "CBP Assigned Surety Code"));
			list.AddPair(OrgCusCode.USACodeTypes.ManufacturerID, Res.GetString("OrgCusCode.USACodeTypes.ManufacturerID", "Supplier/Manufacturer ID Number"));
			list.AddPair(OrgCusCode.USACodeTypes.FAAIndirectCarrierNumber, Res.GetString("OrgCusCode.USACodeTypes.FAAIndirectCarrierNumber", "FAA Indirect Air Carrier Number"));
			list.AddPair(OrgCusCode.USACodeTypes.CTPAT, Res.GetString("OrgCusCode.USACodeTypes.CTPAT", "Customs-Trade Partnership Against Terrorism Status Verification Interface"));
			list.AddPair(OrgCusCode.USACodeTypes.AlcoholImportLicence, Res.GetString("OrgCusCode.USACodeTypes.AlcoholImportLicence", "Alcohol Import License"));
			list.AddPair(OrgCusCode.USACodeTypes.ShipperRegistrationNumber, Res.GetString("OrgCusCode.USACodeTypes.ShipperRegistrationNumber", "Shipper Registration Number"));
			list.AddPair(OrgCusCode.PuertoRicoCodeTypes.ImpuestoSobreVentasyUso, string.Format((NoResString)"Impuesto sobre Ventas y Uso ({0})", Res.GetString("a9c60e3a-46dd-49e7-adf7-86cd3dfdcde9", "Sales and Use Tax")));
			list.AddPair(OrgCusCode.PuertoRicoCodeTypes.NumeroDeFianza, string.Format((NoResString)"Numero de Fianza ({0})", Res.GetString("6222ad3c-c204-498c-a02d-53ec79ea9184", "Tax Bond Number")));
			list.RemoveCode(OrgCusCode.CodeTypes.GovBusinessCode);
			list.RemoveCode(OrgCusCode.CodeTypes.CorporationCode);
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.RemoveCode(OrgCusCode.CodeTypes.CustomsClientCode);
			list.RemoveCode(OrgCusCode.CodeTypes.SupplierCode);
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
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.PuertoRico);
			result.Add(OrgCusCode.PuertoRicoCodeTypes.NumeroDeFianza);
			result.Add(OrgCusCode.PuertoRicoCodeTypes.ImpuestoSobreVentasyUso);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.PuertoRico);
			result.Add(OrgCusCode.USACodeTypes.ForeignRegistrationNumber);
			result.Add(OrgCusCode.USACodeTypes.CBPAssignedNumber);
			result.Remove(OrgCusCode.PuertoRicoCodeTypes.NRC);
			return result;
		}
	}
}
