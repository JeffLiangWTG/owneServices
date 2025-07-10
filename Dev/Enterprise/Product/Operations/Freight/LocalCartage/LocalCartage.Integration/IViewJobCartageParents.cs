using CargoWise.Types;

namespace Enterprise.Freight.LocalCartage.Integration
{
	public interface IViewJobCartageParents
	{
		ZString VCP_JobNumber { get; }
		ZString VCP_JobType { get; }
	}
}
