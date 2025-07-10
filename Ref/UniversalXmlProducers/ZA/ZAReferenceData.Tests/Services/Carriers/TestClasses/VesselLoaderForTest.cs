using System.Collections.Generic;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestClasses
{
	internal class VesselLoaderForTest : VesselLoader
	{
		readonly LoaderHelper loaderHelper;
		public VesselLoaderForTest(string baseUrl, List<LoaderConfig> configs) : base(baseUrl)
		{
			loaderHelper = new LoaderHelper(configs);
		}

		protected override IFileDownloader GetDownloader(string baseUrl, string fileName) => loaderHelper.CreateDownloader(fileName);

		public string RemoveSpecialCharactersExposed(string str) => RemoveSpecialCharacters(str);
	}
}
