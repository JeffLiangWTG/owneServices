using System;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public interface IWebClient : IDisposable
	{
		byte[] DownloadData(string address);
	}
}
