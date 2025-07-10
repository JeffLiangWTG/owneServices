using System.Net;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class WebClientWrapper : WebClient, IWebClient
	{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
		public WebClientWrapper()
		{
			this.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
			this.UseDefaultCredentials = true;
		}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
	}
}
