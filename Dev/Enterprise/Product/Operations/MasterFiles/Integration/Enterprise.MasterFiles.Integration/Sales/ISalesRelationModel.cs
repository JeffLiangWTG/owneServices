using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ISalesRelationModel : IBusiness
	{
		ZDateTime RecentActivityDate { get; }
		ZBool HasSalesRelation { get; }
	}
}
