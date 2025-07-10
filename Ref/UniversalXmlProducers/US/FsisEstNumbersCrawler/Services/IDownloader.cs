namespace FsisEstNumbersCrawler.Services
{
	public interface IDownloader
	{
		void Download(string url, string localPath);
	}
}
