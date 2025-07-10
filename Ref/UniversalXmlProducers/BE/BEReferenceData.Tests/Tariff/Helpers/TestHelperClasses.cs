namespace CargoWise.RefDbRepo.BEReferenceData.Services.Testing
{
	internal class TestHelperClasses
	{
		public class DownloadManagerTester : DownloadManager
		{
			public DownloadManagerTester(string workingFolder) : base(workingFolder)
			{
			}

			public override IWebDriverHelperWrapper GetWebDriverHelper() => TestWebDriverHelperWrapper;

			public IWebDriverHelperWrapper TestWebDriverHelperWrapper { get; set; }
		}
	}
}
