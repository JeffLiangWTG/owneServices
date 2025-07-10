using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class SwitzerlandOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCode.SwissCodeTypes.UID, Res.GetString("10941E80-130B-4CE2-9589-0AE0CF748898", "{0} / Enterprise ID", "Unternehmens ID"));
			list.AddPair(OrgCusCode.SwissCodeTypes.CAD, Res.GetString("499FB08D-5183-44ED-9990-414A52CD0744", "{0} / Account number duty", "ZAZ Kontonummer"));
			list.AddPair(OrgCusCode.SwissCodeTypes.CAV, Res.GetString("0DDCC87A-D3FC-4541-A1F3-0D4534E5C5BB", "{0} / Account number VAT", "MWST Kontonummer"));
			list.AddPair(OrgCusCode.SwissCodeTypes.CTP, Res.GetString("59C38FF2-A3BA-4B54-BB31-4973EEB83555", "{0} / Company number taxpayer", "Firmennummer Steuerpflichtiger"));
			list.AddPair(OrgCusCode.CodeTypes.CustomsOfficeForTransit, Res.GetString("OrgCusCode.CodeTypes.CustomsOfficeForTransitCH", "Customs Office For Transit"));
			list.AddPair(OrgCusCode.SwissCodeTypes.BID, Res.GetString("F7F53C5A-8696-42FB-97A7-5EB08345C7E4", "{0} / Business Partner ID", "Geschäftspartner ID"));
			list.AddPair(OrgCusCode.SwissCodeTypes.ASN, Res.GetString("69B1BDF1-9CB6-40E3-B0EC-71B94BB67A60", "{0} / Authorized Sender", "Expéditeur Agrée/Zugelassener Versender"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Switzerland);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Switzerland);
			return result;
		}
	}
}
