namespace ZAReferenceData.Services
{
	public interface IDownloader
	{
		void Download(string url, string localPath);
	}
}
