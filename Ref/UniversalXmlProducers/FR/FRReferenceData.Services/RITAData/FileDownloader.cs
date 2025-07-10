using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class FileDownloader : IFileDownloader
	{
		public virtual void DownloadFile(string address, string fileName)
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var webClient = new WebClient())
			{
				RetryHelper.RetryWithDelay(
					() => { webClient.DownloadFile(address, fileName); },
					TimeSpan.FromMilliseconds(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfDownloadError),
					ApplicationConfig.Instance.MaxAttemptsCountInCaseOfDownloadError);
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
		}
	}
}
