using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests
{
	internal class ProcessManagerForTest : ProcessManager
	{
		public IProcessor[] TestProcessors { get; set; } = { };
		public ILoader[] TestLoaders { get; set; } = { };

		public IFileManager TestFileManager { get; set; }

		protected override IFileManager GetFileManager() => TestFileManager;

		protected override ILoader[] GetLoaders() => TestLoaders;

		protected override IProcessor[] GetProcessors() => TestProcessors;

		public string[] ChaptersExposed => Chapters;
	}
}
