using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IStmMenuMenuPivotBase : IBusiness
	{
		IStmMenuItem Outward { get; }
	}
}
