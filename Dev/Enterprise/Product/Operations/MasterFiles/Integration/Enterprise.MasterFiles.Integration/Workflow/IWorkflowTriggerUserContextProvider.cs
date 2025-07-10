using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkflowTriggerUserContextProvider
	{
		ZString GetBranch(IBaseTrigger trigger, BusinessObject job, IWorkflowTriggerSource userContextSource);
		IDisposable SetTemporaryUserContext(IBaseTrigger trigger, BusinessObject job);
	}
}
