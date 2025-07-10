using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DataTransfer
{
	public class OrgPatternMatchCollectionForFinding : OrgPatternMatchCollection
	{
		public OrgPatternMatchCollectionForFinding(OrgHeaderForFinding orgHeader, ZQuery additionalFilter)
			: base(orgHeader.Factory, additionalFilter)
		{
			fOrgHeader = orgHeader;
		}

		readonly OrgHeaderForFinding fOrgHeader;

		public override ZBool AllowEmptyAddresses => fOrgHeader.AllowEmptyAddresses;
	}
}
