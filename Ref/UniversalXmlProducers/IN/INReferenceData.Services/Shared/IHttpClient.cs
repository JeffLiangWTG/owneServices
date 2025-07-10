using System;
using System.Net.Http;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public interface IHttpClient : IDisposable
	{
		HttpResponseMessage Get(Uri requestUri);
	}
}
