using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial interface IUpdaterScriptInfo
	{
		DataTableMapping Mapping { get; }
	}
}
