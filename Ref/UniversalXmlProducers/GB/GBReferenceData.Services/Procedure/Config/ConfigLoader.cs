using System.Collections.Generic;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Config
{
	public class ConfigLoader : Common.ConfigLoader<CDSProcedureSources>
	{
		public static IEnumerable<ICDSProcedureSource> LoadConfigFile(IConfigProvider configProvider) => LoadConfigFileSources(configProvider).Sources;
	}
}
