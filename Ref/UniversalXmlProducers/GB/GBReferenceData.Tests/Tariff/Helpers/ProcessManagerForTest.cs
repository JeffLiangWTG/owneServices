using CargoWise.RefDbRepo.GBReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.Helpers
{
	internal class ProcessManagerForTest : ProcessManager
	{
		public ProcessManagerForTest(IProcessor[] processors, ILoader[] loaders) : base(processors, loaders)
		{
		}

		public string[] TestChapters { get; set; }
		protected override string[] Chapters => TestChapters == null ? base.Chapters : TestChapters;

		public IFileManager TestFileManager { get; set; }
		protected override IFileManager GetFileManager() => TestFileManager == null ? base.GetFileManager() : TestFileManager;

		public IFileManager GetFileManagerExposed() => base.GetFileManager();
		public ILoader[] GetLoadersExposed() => base.GetLoaders();
		public IProcessor[] GetProcessorsExposed() => base.GetProcessors();
	}
}
