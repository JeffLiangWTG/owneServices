using System.Collections.Generic;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config
{
	public class ConfigLoader : Common.ConfigLoader<CDSPortSources>
	{
		public static IEnumerable<ICDSPortSource> LoadConfigFile(IConfigProvider configProvider) => LoadConfigFileSources(configProvider).Sources;
	}
}
