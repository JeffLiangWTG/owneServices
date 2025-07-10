using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface ISendDocumentsTriggerActionRunnerFactory
	{
		IProcessor GetNewRunner(IProcessTaskNotification notification, IBusiness business, Lazy<IStmALog> logProvider);
	}
}
