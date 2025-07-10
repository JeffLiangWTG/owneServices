using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff;

namespace CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.Helpers
{
	internal class FileManagerForTest : FileManager
	{
		public FileManagerForTest(string contentFolder)
		{
			this.contentFolder = contentFolder;
		}
		readonly string contentFolder;

		protected override void DownloadFiles(string contentFolder) { }
		protected override string GetContentFolder() => contentFolder;
	}
}
