using System;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.VersionInfo;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Just a simple class to make importing StmUpgrades from file available without declaring it static 
	/// </summary>
	public class StmUpgradeImporter
	{
		public ZGuid ImportPackage(string packagePath, PackageVersionInfo versionInfo = null)
		{
			return ImportPackage(packagePath, null, versionInfo);
		}

		public ZGuid ImportPackage(string packagePath, Upgrades.Progress progress, PackageVersionInfo versionInfo = null)
		{
			UpgradeInfoExtended upgradeInfo;

			try
			{
				if (versionInfo == null)
				{
					UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
					upgradeInfo = upgradeManager.UploadUpgradePackage(
						packagePath,
						StmUpgrade.StmUpgradeStatus.Ready,
						Res.GetString("0af973d2-ae9e-4b0b-871d-8cfac965c3a5", "Imported from the file by {0}", Env.CurrentUser.LoginName),
						progress);
				}
				else
				{
					UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
					upgradeInfo = upgradeManager.UploadUpgradePackage(new Version(versionInfo.MajorVersion, versionInfo.MinorVersion, versionInfo.Release, versionInfo.Patch),
						versionInfo.ExeVersionDate.ToDateTime(),
						StmUpgrade.StmUpgradeStatus.Ready,
						Res.GetString("994c91f9-793a-d881-37a2-995c3764aa3d", "Package version information imported by {0}", Env.CurrentUser.LoginName));
				}
			}
			catch (Exception ex) when (ex is CorruptEdpException || ex is InvalidPackageException || ex is InvalidDataException)
			{
				throw new InvalidOperationException(Res.GetString("874e70ff-717e-4d13-b282-bbb00bcb8ee8", "Package {0} is corrupted.", versionInfo == null ? packagePath : versionInfo.PackageFileName));
			}

			try
			{
				new VersionReport.VersionReportBuilderFactory().SendDelivered(upgradeInfo.Version.ToString());
				if (EnvProxy.Instance.Registry.KeepOnlyLatestVersion)
				{
					RemoveOldDownloadedPackages(upgradeInfo.Version);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			return upgradeInfo.PK;
		}

#if DEBUG
		internal
#endif
		void RemoveOldDownloadedPackages(Version version)
		{
			string deleteStatement = string.Format(CultureInfo.InvariantCulture,
				@"delete from dbo.StmUpgrade
						where SZ_Status <> 'CUR'
						and SZ_Type = 'EDP'
						and SZ_PK not in (select top 1 SZ_PK from dbo.StmUpgrade 
							where SZ_Type = 'EDP'
							order by SZ_StatusTime desc)
						and (SZ_MajorVersion < {0} or
							SZ_MajorVersion = {0} and SZ_MinorVersion < {1} or
							SZ_MajorVersion = {0} and SZ_MinorVersion = {1} and SZ_Release < {2} or
							SZ_MajorVersion = {0} and SZ_MinorVersion = {1} and SZ_Release = {2} and SZ_Patch < {3})",
				version.Major, version.Minor, version.Build, version.Revision);

			using (DbCommand cmd = Db.Connection.Command(deleteStatement))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
