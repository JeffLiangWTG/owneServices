using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public interface ISRDbUpdaterRegistration
	{
		ISRDbDataSetUpdater[] Get(IServerProxy proxy, IDBHelper dbHelper, IErrorReportingClientWrapper errorReportingWrapper, ILogger logger);
	}
}
