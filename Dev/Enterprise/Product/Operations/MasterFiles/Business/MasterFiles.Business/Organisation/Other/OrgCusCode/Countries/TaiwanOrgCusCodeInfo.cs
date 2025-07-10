using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class TaiwanOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.VATCode)); // Accounting consumption code

			list.OverridePair(OrgCusCode.CodeTypes.ControlledPremisesID, Res.GetString("98E51D1F-DB73-41C9-BC5F-87692C56FD05", "Customs Logistic Center"));

			list.AddPair(OrgCusCode.TaiwanCodeTypes.AEO, Res.GetString("OrgCusCode.TaiwanCodeTypes.AEO", "Authorized Economic Operator"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.TPC, Res.GetString("OrgCusCode.TaiwanCodeTypes.TPC", "Tax payment on account business identifier, coded"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.PID, Res.GetString("OrgCusCode.TaiwanCodeTypes.PID", "Republic of China (Taiwan) National ID Card Number"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.PBR, Res.GetString("OrgCusCode.TaiwanCodeTypes.PBR", "Customs Guarantee Number"));
			list.AddPair(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Res.GetString("C2A30EF5-47D0-45AC-92F3-94056D20539C", "Customs Bonded Warehouse"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.MCI, Res.GetString("OrgCusCode.TaiwanCodeTypes.MCI", "Mobile Carrier ID"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.PIG, Res.GetString("OrgCusCode.TaiwanCodeTypes.PIG", "Public Interest Group"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.EPZ, Res.GetString("OrgCusCode.TaiwanCodeTypes.EPZ", "Export Processing Zone"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.CBF, Res.GetString("OrgCusCode.TaiwanCodeTypes.CBF", "Bonded Factory"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.FTZ, Res.GetString("OrgCusCode.TaiwanCodeTypes.FTZ", "Free Trade Zone"));
			list.AddPair(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, Res.GetString("OrgCusCode.CodeTypes.FDAEstablishmentIdentifier", "FDA Establishment Identifier"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, Res.GetString("OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber", "Factory Registration Number"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, Res.GetString("OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark", "Agricultural Technology Park Bonded ID"));
			list.AddPair(OrgCusCode.TaiwanCodeTypes.SciencePark, Res.GetString("OrgCusCode.TaiwanCodeTypes.SciencePark", "Science Park Bonded ID"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Taiwan);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Taiwan);
			return result;
		}
	}
}
