using System.Collections.Generic;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public interface IUpdateScriptProvider
	{
		IEnumerable<IUpdaterScriptInfo> GetScriptInfos(int dbVersion);
	}
}
