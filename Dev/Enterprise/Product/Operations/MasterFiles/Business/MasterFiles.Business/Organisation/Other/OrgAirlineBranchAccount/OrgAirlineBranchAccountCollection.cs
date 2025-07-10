using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAirlineBranchAccountCollection : ActiveBusinessObjectCollection<OrgAirlineBranchAccount>
	{
		public OrgAirlineBranchAccountCollection(OrgHeader orgHeader)
			: base(orgHeader.Factory, new DependentRelationship(orgHeader, typeof(OrgAirlineBranchAccount), new ZQuery(OrgAirlineBranchAccountSchema.OAA_OH_Carrier, orgHeader.PK), OrgAirlineBranchAccountSchema.OAA_OH_Carrier))
		{
		}
	}
}
