using System.Collections.Generic;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestClasses
{
	internal class CarrierLoaderForTest : CarrierLoader
	{
		readonly LoaderHelper loaderHelper;
		public CarrierLoaderForTest(string baseUrl, List<LoaderConfig> configs) : base(baseUrl)
		{
			loaderHelper = new LoaderHelper(configs);
		}

		protected override IFileDownloader GetDownloader(string baseUrl, string fileName) => loaderHelper.CreateDownloader(fileName);

		public Dictionary<string, CarrierMapping> GetFileMappingsExposed() => GetFileMappings();
	}
}
