using CargoWise.RefDbRepo.GBReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff
{
	public class ProcessManager : SharedReferenceData.Business.Tariff.ProcessManager
	{
		public ProcessManager(IProcessor[] processors, ILoader[] loaders)
		{
			this.processors = processors;
			this.loaders = loaders;
			fileManager = new WebClientFileManager();
		}
		readonly IProcessor[] processors;
		readonly ILoader[] loaders;
		readonly SharedReferenceData.Services.Tariff.IFileManager fileManager;

		protected override SharedReferenceData.Services.Tariff.IFileManager GetFileManager() => fileManager;
		protected override IProcessor[] GetProcessors() => processors;
		protected override ILoader[] GetLoaders() => loaders;

		public override string SourceDataProviderCore => "HMRC";
		public override string TeamCore => "CUK";
	}
}
