using System.Collections.Generic;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial interface IUpdaterScriptInfo
	{
		string DataSetName { get; }
		Dictionary<string, string> PrepareTemporaryTablesScripts { get; }
		string MergeScript { get; }
		string[] Prerequisites { get; }
		int UpdaterVersion { get; }
	}
}
