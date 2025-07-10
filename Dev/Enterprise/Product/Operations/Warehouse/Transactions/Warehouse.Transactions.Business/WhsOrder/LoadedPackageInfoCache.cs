using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	sealed class LoadedPackageInfoCache
	{
		public LoadedPackageInfoCache(BusinessObjectFactory factory, IEnumerable<PkgPackageJob> packageJobs)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(packageJobs, nameof(packageJobs));

			Cache = new Lazy<ILookup<ZGuid, LoadedPackageInfo>>(() =>
			{
				var packages = new DynamicBusinessObjectCollection(factory);

				var packageJobsInDB = packageJobs.Where(p => p.IsInDatabase).ToArray();
				if (packageJobsInDB.Length > 0)
				{
					var sql = @"
SELECT
	PkgPackage.KP_PK,
	KP_KJ_ParentPackageJob,
	WLO_JobID,
	WLP_LoadedTime
FROM
	dbo.PkgPackage
	JOIN dbo.WhsLoadPackage ON WhsLoadPackage.KP_PK = PkgPackage.KP_PK
	JOIN dbo.WhsLoad ON WLO_PK = WhsLoadPK
WHERE
	KP_KJ_ParentPackageJob IN (SELECT value FROM @PackageJobPKs)
	AND KP_KP_ParentPackage IS NULL";

					var sqlParam = ZSqlParameter.New("@PackageJobPKs", packageJobsInDB.Select(p => p.PK).ToArray(), PkgPackageSchema.KP_KJ_ParentPackageJob, isTableValued: true);
					packages.Load(sql, new[] { sqlParam });
				}

				return packages.ToLookup(p => (ZGuid)p[PkgPackageSchema.Constants.KP_KJ_ParentPackageJob],
					p => new LoadedPackageInfo((ZGuid)p[PkgPackageSchema.Constants.PK], (ZDateTimeOffset)p[WhsLoadPkgPackagePivotSchema.Constants.WLP_LoadedTime], (ZString)p[WhsLoadSchema.Constants.WLO_JobID]));
			});
		}

		public IReadOnlyDictionary<ZGuid, LoadedPackageInfo> GetLoadedPackageInfo(PkgPackageJob packageJob)
		{
			Argument.NotNull(packageJob, nameof(packageJob));
			return Cache.Value[packageJob.PK]?.ToImmutableDictionary(p => p.PackagePK) ?? ImmutableDictionary<ZGuid, LoadedPackageInfo>.Empty;
		}

		Lazy<ILookup<ZGuid, LoadedPackageInfo>> Cache { get; }
	}
}
