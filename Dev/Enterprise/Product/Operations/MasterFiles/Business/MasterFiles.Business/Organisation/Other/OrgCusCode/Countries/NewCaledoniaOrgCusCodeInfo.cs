using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class NewCaledoniaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.NewCaledoniaCodeTypes.TGC, Res.GetString("9546bb50-3d74-48cf-abd8-3f1ccd6ced76", "VAT (TGC) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCode.NewCaledoniaCodeTypes.RDT, Res.GetString("A40894FA-77B3-4F78-A7A5-B0539756EC00", "RIDET Business Branch Identification Number"));
			list.AddPair(OrgCusCode.NewCaledoniaCodeTypes.RID, Res.GetString("39A659B1-F27A-4FD9-90CF-450D056E99EA", "RID Identification Number"));
			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.NewCaledonia);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.NewCaledonia);
			return result;
		}
	}
}
