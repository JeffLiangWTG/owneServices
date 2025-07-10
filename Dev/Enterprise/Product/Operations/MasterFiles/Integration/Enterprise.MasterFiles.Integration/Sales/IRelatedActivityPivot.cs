using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRelatedActivityPivot : IBusiness
	{
		ZGuid PK { get; }
		IRelatableActivity ChildActivity { get; set; }
		IRelatableActivity ParentActivity { get; set; }

		ZGuid RAP_ChildActivityID { get; set; }
		ZString RAP_ChildActivityTableCode { get; set; }
		ZGuid RAP_ParentActivityID { get; set; }
		ZString RAP_ParentActivityTableCode { get; set; }
		ZGuid RAP_SalesRelationTreeID { get; }
		bool IsEditable { get; }
	}
}
