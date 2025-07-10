using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;

namespace CargoWise.RefDbRepo.NewService
{
	public static class DataSetControllerBaseHelper
	{
		public static bool DoesCacheVersionMatter(string datasetName)
		{
			return DatasetsNeedingAdditionalCacheVersionsMapping.ContainsKey(datasetName);
		}

		public static string GetVersionForCacheString(string datasetName, string version)
		{
			IDataAdaptor adaptor = new DataAdaptor();
			if (!adaptor.IsSRDbVersion(version))
			{
				var requestVersion = adaptor.ParseVersion(version);
				var additionalCacheVersion = adaptor.ParseVersion(DatasetsNeedingAdditionalCacheVersionsMapping[datasetName]);
				if (requestVersion.Item1 == additionalCacheVersion.Item1 && requestVersion.Item2 <= additionalCacheVersion.Item2)
				{
					return DatasetsNeedingAdditionalCacheVersionsMapping[datasetName];
				}
			}
			return string.Empty;
		}

		static Dictionary<string, string> DatasetsNeedingAdditionalCacheVersionsMapping = new Dictionary<string, string>()
		{
			{ "RefCusCodeType","0_26_9" },
			{ "RefVesselZZ","0_33_9" },
			{ "RefCarrierCode","0_33_9" },
			{ "RefStlScript","0_40_9" },
			{ "RefCusQuota", "0_0_9" },
			{ "RefCusTaxOrFeeType", "0_0_9" },
			{ "RefExchangeRateZZ", "0_0_9"}
		};
	}
}
