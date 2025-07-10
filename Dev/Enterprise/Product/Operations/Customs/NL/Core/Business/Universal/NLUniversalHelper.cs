using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public static class NLUniversalHelper
{
	public static ZString GetDeclarantType(OrgHeader orgHeader)
	{
		var result = ZString.Empty;
		if (orgHeader != null)
		{
			var addInfo = EUOrgImpAddInfo.Get(orgHeader, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (addInfo != null)
			{
				addInfo.Deserialise();
				if (orgHeader.PK == GlbCompany.CurrentCompany.OrgProxy?.PK)
				{
					result = RepresentationTypeList.Codes._1Self;
				}
				else
				{
					result = addInfo.ZO_Box14UseIndirectRepresentationForExporter ? RepresentationTypeList.Codes._3Indirect : RepresentationTypeList.Codes._2Direct;
				}
			}
		}
		return result;
	}
}
