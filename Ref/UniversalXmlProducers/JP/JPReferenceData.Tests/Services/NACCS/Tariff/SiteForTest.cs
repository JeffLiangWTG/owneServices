using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class SiteForTest : Site
	{
		public SiteForTest(string url, IWebSourceProvider sourceProvider) : base(sourceProvider, url)
		{
			var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), AppConfig.NACCS.CodeLists.JPNACCSTariffPath);
			Directory.CreateDirectory(folder);
			FullPath = Path.Combine(folder, "JapanNaccsSite.csv");
		}

		public void Download()
		{
			Translate();
			Export();
			Records.Clear();
		}
	}
}
