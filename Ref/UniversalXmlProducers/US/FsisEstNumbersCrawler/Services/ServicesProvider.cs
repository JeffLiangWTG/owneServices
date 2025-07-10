namespace FsisEstNumbersCrawler.Services
{
	public static class ServicesProvider
	{
		public static ILogger Logger { get; } = new ConsoleLogger();

		public static IHtmlParser HtmlParser { get; } = new HtmlParser();

		public static IDownloader Downloader { get; } = new Downloader();

		public static ICsvParser CsvParser { get; } = new CsvParser();
	}
}
