using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobDeclarationValueSetStrategy : EU.Business.Declaration.JobDeclarationValueSetStrategy
{
	public JobDeclarationValueSetStrategy(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
	{
	}

	protected override ZString GetDeclarantType(OrgHeader orgHeader)
	{
		var result = ZString.Empty;
		if (orgHeader != null)
		{
			var addInfo = EUOrgImpAddInfo.Get(orgHeader, Declaration.CountryCode);
			if (addInfo != null)
			{
				addInfo.Deserialise();
				result = addInfo.ZO_Box14UseIndirectRepresentation ? PLRepresentationTypeList.Codes._5Indirect : PLRepresentationTypeList.Codes._4Direct;
			}
		}
		return result;
	}
}
