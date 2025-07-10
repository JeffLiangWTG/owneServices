using System.Globalization;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.RemoteDbManager;
using CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform;
using CloneExtensions;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor
{
	public class DataAdaptor : IDataAdaptor
	{
		//Please do not change this AnchorSRDbVersion, it should always be 564
		public const int AnchorSRDbVersion = 564;
		protected static int LatestSRDbVersion => new UpgradeScriptProvider().LatestVersion;

		//Please do not bump the CurrentVersion(ContractVersion) when doing Schema Change for SRDb
		protected static Tuple<int, int, int> CurrentVersion => Tuple.Create(0, 174, 9);

		public bool RequireTransform(string version, Type dataSetType)
		{
			Argument.NotNullOrEmpty(version, nameof(version));
			return GetTransformStrategies(version, dataSetType).Any();
		}

		public object ToVersion<T>(T currentVersionData, string version)
		{
			var strategies = GetTransformStrategies(version, typeof(T));
			if (strategies != null && strategies.Any())
			{
				var clonedData = CloneFactory.GetClone(currentVersionData, CloningFlags.Properties);
				TransformHelper.Transform(clonedData, strategies);
				return clonedData;
			}
			return currentVersionData;
		}

		IEnumerable<ITransformStrategy> GetTransformStrategies(string version, Type dataSetType)
		{
			if (string.IsNullOrEmpty(version))
			{
				return Enumerable.Empty<ITransformStrategy>();
			}

			if (IsSRDbVersion(version))
			{
				var sRDbVersion = ParseToSRDbVersion(version);
				if (sRDbVersion > LatestSRDbVersion)
				{
					throw new NotSupportedException($"SRDbVersion {sRDbVersion} is not supported. Latest SRDbVersion is {LatestSRDbVersion}");
				}
				return new TransformStrategyPicker().GetStrategiesBySRDbVersion(sRDbVersion, dataSetType);
			}

			var versionTuple = ParseVersion(version);
			var currentVersion = CurrentVersion;
			var requestVersion = versionTuple.Item2;
			if (versionTuple.Item1 != currentVersion.Item1 || requestVersion > currentVersion.Item2)
			{
				throw new NotSupportedException($"Version {versionTuple} is not supported. Current version is {currentVersion}");
			}
			return new TransformStrategyPicker().GetStrategies(requestVersion, dataSetType);
		}

		public Tuple<int, int, int> ParseVersion(string version)
		{
			Argument.NotNullOrEmpty(version, nameof(version));
			var components = version.Split('_');
			if (components.Length < 3)
			{
				throw new NotSupportedException($"Invalid ContractVersion {version} provided. Version should be of format #_##_##.");
			}
			var majorVersion = int.Parse(components[0], CultureInfo.InvariantCulture);
			var minorVersion = int.Parse(components[1], CultureInfo.InvariantCulture);
			var dataVersion = int.Parse(components[2], CultureInfo.InvariantCulture);
			return Tuple.Create(majorVersion, minorVersion, dataVersion);
		}

		public int ParseToSRDbVersion(string version)
		{
			Argument.NotNullOrEmpty(version, nameof(version));
			var components = version.Split('_');
			if (components.Length < 2 || !string.Equals(components[0], "SRDb", StringComparison.OrdinalIgnoreCase))
			{
				throw new NotSupportedException($"Invalid SRDbVersion {version} provided. Version should be of format SRDb_###.");
			}
			var sRDbVersion = int.Parse(components[1], CultureInfo.InvariantCulture);
			return sRDbVersion;
		}

		public bool IsSRDbVersion(string version) => version.StartsWith("SRDb_", StringComparison.OrdinalIgnoreCase);
	}
}
