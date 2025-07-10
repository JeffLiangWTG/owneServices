using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.RemoteDbManager;

namespace CargoWise.RefDbRepo.NewService
{
	public class DbUpgraderHelper : IDbUpgraderHelper
	{
		public DbUpgraderHelper(IUpgradeScriptProvider provider)
		{
			Argument.NotNull(provider, nameof(provider));
			this.provider = provider;
		}

		static ConcurrentDictionary<int, Lazy<UpgradeWrapper>> CachedDeltaScript
		{
			get
			{
				if (_cachedDeltaScript == null)
				{
					_cachedDeltaScript = new ConcurrentDictionary<int, Lazy<UpgradeWrapper>>();
				}
				return _cachedDeltaScript;
			}
		}
		static ConcurrentDictionary<int, Lazy<UpgradeWrapper>> _cachedDeltaScript;
		public IEnumerable<UpgradeWrapper> GetDeltaScriptsAfterVersion(int versionFromClient)
		{
			foreach (var version in provider.GetAvailableVersionsAfterVersion(versionFromClient))
			{
				yield return GetDeltaScriptsByVersion(version);
			}
		}

		UpgradeWrapper GetDeltaScriptsByVersion(int version)
		{
			return CachedDeltaScript.GetOrAdd(version, key => new Lazy<UpgradeWrapper>(() => provider.GetUpgradeWrapperByVersion(key))).Value;
		}

		public int LatestVersion => provider.LatestVersion;
		
		public int RequiredVersion => provider.RequiredVersion;

		readonly IUpgradeScriptProvider provider;
	}
}
