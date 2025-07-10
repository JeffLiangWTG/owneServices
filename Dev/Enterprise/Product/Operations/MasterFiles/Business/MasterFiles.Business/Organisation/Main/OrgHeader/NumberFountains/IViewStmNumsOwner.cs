using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IViewStmNumsOwner : IBusiness
	{
		public ZGuid PK { get; }
		IViewStmNumsCollection<ViewStmNums> Fountains { get; }
		StmNumberRangeMatchingDetailsCollection NumberRangeMatchingDetails { get; }
	}
}
