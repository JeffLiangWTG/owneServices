using System.Net.Http;

namespace CargoWise.RefDbRepo.NewService
{
	public class FileContentDownload : IFileContentDownload
	{
		public PushStreamContent StreamContent { get; set; }
		public string FileName { get; set; }
	}

	public interface IFileContentDownload
	{
		PushStreamContent StreamContent { get; set; }
		string FileName { get; set; }
	}
}
