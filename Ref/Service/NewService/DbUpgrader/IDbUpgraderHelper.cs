using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.NewService
{
	public interface IDbUpgraderHelper
	{
		IEnumerable<UpgradeWrapper> GetDeltaScriptsAfterVersion(int versionFromClient);
		int LatestVersion { get; }
		int RequiredVersion { get; }
	}
}
