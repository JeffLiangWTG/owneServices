using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class TariffForTest : Tariff
	{
		public TariffForTest(string url, IWebSourceProvider sourceProvider, string tariff) : base(sourceProvider, url)
		{
			var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), AppConfig.NACCS.CodeLists.JPNACCSTariffPath);
			Directory.CreateDirectory(folder);
			FullPath = Path.Combine(folder, $"JapanNaccsTariff{tariff}.csv");
			ChapterCompositeKeys = new Dictionary<string, string>();
		}

		public void Download()
		{
			Translate();
			Export();
		}
	}
}
