using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface IDefaultedFromDateProvider : IStmALogProvider
	{
		ZDateTimeOffset PredecessorDefaultDate { get; }

		IWorkflowProvider Parent { get; }
	}
}
