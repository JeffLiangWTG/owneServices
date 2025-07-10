using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UnitedStatesOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			OrgCusCodeListHelpers.InsertUsaOrTerritoryCodes(list);
			list.AddPair(OrgCusCode.CodeTypes.TruckCarrierCode, Res.GetString("OrgCusCode.CodeTypes.TruckCarrierCode", "Standard Carrier Alpha Code (Truck)"));
			list.AddPair(OrgCusCode.USACodeTypes.TireManufacturerCode, Res.GetString("OrgCusCode.USACodeTypes.TireManufacturerCode", "Tire Manufacturer Code"));
			list.AddPair(OrgCusCode.USACodeTypes.GlazingManufacturerCode, Res.GetString("OrgCusCode.USACodeTypes.GlazingManufacturerCode", "Glazing Manufacturer Code"));
			list.AddPair(OrgCusCode.USACodeTypes.AMSRegistrationNumber, Res.GetString("6B01F015-23A4-48E2-9D11-0245BED0B9AD", "AMS (USDA) Assigned ID Number"));
			list.AddPair(OrgCusCode.USACodeTypes.DOTDepartmentOfTransportation, Res.GetString("12345678-23A4-48E2-9D11-0245BED0B9AD", "Department of Transportation"));
			list.AddPair(OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode, Res.GetString("44CED098-87CA-45EB-8EFB-3E6B147B02D7", "Department of Defense Activity Address Code"));
			list.AddPair(OrgCusCode.USACodeTypes.ACASOriginatorCode, Res.GetString("8DACB1E5-F108-40CB-8AF8-BF51ACFB41EE", "ACAS (Air Cargo Advance Screening) Originator Code"));
			list.AddPair(OrgCusCode.USACodeTypes.StandardCarrierAlphaCodeAir, Res.GetString("C4FA9D81-C267-42E5-A408-38605EDA44BB", "Standard Carrier Alpha Code (Air)"));
			list.AddPair(OrgCusCode.USACodeTypes.CarrierPrefixCode, Res.GetString("0302843F-821D-41CA-BE7E-2C9963076568", "Carrier Prefix Code"));
			list.AddPair(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, Res.GetString("2935D28C-09B4-47DB-9A75-60C7A2322D27", "FWS eDecs Account Number"));
			list.AddPair(OrgCusCode.USACodeTypes.FDAForeignSellerRegistrationNumber, Res.GetString("6AE3B46A-B0A1-4798-9C45-191EDA60F096", "FDA Foreign Seller Registration Number"));
			list.AddPair(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, Res.GetString("ED4472C2-FC8C-44DC-9E37-7B6AE96D7680", "Air AMS Originator Code"));
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
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.UnitedStates);
			result.Add(OrgCusCode.USACodeTypes.ForeignRegistrationNumber);
			result.Add(OrgCusCode.USACodeTypes.SocialSecurityNumber);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.UnitedStates);
			result.Add(OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
			result.Add(OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4);
			result.Add(OrgCusCode.USACodeTypes.ForeignRegistrationNumber);
			result.Add(OrgCusCode.USACodeTypes.CBPAssignedNumber);
			return result;
		}
	}
}
