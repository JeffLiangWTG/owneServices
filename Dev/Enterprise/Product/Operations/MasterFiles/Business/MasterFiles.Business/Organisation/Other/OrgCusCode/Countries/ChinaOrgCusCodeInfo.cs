using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ChinaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Res.GetString("Organisation|CustomsCodes|VATCodeChina", "VAT Registration Code")); // Accounting consumption code

			list.AddPair(OrgCusCode.ChinaCodeTypes.BST, Res.GetString("7B1E96C7-3362-416E-8C96-612EAF4A0AED", "Business Tax Registration Code"));
			list.AddPair(OrgCusCode.ChinaCodeTypes.CIQ, Res.GetString("522AD961-753D-46E1-ACA5-FC557F7181B3", "China Import-Export Inspection and Quarantine Code"));
			list.AddPair(OrgCusCode.ChinaCodeTypes.VAG, Res.GetString("AC09570D-C60A-404B-ACB6-293BF7CB272A", "VAT General Tax Payer"));
			list.AddPair(OrgCusCode.ChinaCodeTypes.VAS, Res.GetString("F5134CB2-2B81-49B8-9DB2-B5738DD3085B", "VAT Small Scale Tax Payer"));
			list.AddPair(OrgCusCode.CodeTypes.DepotControlledPremisesID, Res.GetString("CEC463E0-7962-42F1-A46A-8D63FA7AFE3D", "Customs Controlled Premises Code - Depot"));
			list.AddPair(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Res.GetString("D895D6F5-E208-4C7D-8375-1109D6320F0C", "Customs Controlled Premises Code - Warehouse"));
			list.AddPair(OrgCusCode.ChinaCodeTypes.ENP, Res.GetString("DE711281-394E-4707-975C-BEB93FD87BB1", "Easipass Client ID"));
			list.AddPair(OrgCusCode.ChinaCodeTypes.USC, Res.GetString("7B8108B1-2CB3-4A81-8761-BABE75E4CA80", "Unified Social Credit Identifier"));
			list.AddPair(OrgCusCode.ChinaCodeTypes.NGB, Res.GetString("986C2CB9-EE36-45F2-8643-6B89161BCE0C", "Ningbo EDI Center Client Code"));
			list.AddPair(OrgCusCode.ChinaCodeTypes.MMR, Res.GetString("F8A1110C-DB6A-4793-BED1-1283B9F7ADB0", "Meat Manufacturer Registration Number"));
			list.AddPair(OrgCusCode.ChinaCodeTypes.SMR, Res.GetString("F595C6DD-3536-4011-8F5B-D5EF06724142", "Seafood Manufacturer Registration Number"));
			list.AddRange(new Customs.CN.EnterpriseQualificationList());
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.China);
			result.Add(OrgCusCode.ChinaCodeTypes.AEO);
			result.Add(OrgCusCode.ChinaCodeTypes.USC);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.China);
			result.Add(OrgCusCode.ChinaCodeTypes.AEO);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}
	}
}
