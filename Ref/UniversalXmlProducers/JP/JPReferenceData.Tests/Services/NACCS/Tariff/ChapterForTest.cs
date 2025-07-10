using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class ChapterForTest : Chapter
	{
		public ChapterForTest(string url, IWebSourceProvider sourceProvider, int section, int chapter) : base(sourceProvider, url, chapter, section)
		{
			var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), AppConfig.NACCS.CodeLists.JPNACCSTariffPath);
			Directory.CreateDirectory(folder);
			FullPath = Path.Combine(folder, $"JapanNaccsChapter{chapter}.csv");
		}

		public void Download()
		{
			Translate();
			Export();
		}
	}
}
