using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public interface IUpgradeScriptProvider
	{
		int LatestVersion { get; }
		int RequiredVersion { get; }
		UpgradeWrapper GetUpgradeWrapperByVersion(int version);
		IEnumerable<int> GetAvailableVersionsAfterVersion(int version);
	}
}
