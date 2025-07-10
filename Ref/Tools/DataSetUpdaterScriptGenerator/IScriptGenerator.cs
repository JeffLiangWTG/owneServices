using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public interface IScriptGenerator
	{
		IEnumerable<(string, string)> GeneratePrepareTemporaryTablesScripts(IDataSetUpdaterInfo info);
		string GenerateMergeScript(IDataSetUpdaterInfo info);
		IEnumerable<string> GetPrerequisites(IDataSetInfo info);
	}
}
