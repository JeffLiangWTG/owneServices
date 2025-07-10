using System;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IRegisterStatusChangeContext
	{
		IDisposable TemporarilySetStatusChangedByTriggerEvent(IStmALog triggeringEvent);
	}
}
