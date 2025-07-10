using System;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public static class HttpClientUtils
	{
		public static HttpClient New() => new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
		public static HttpClient New(HttpMessageHandler handler) => new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) };
	}
}
