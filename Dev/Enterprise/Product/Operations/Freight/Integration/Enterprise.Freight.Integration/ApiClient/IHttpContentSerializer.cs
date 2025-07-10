using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Freight.Integration.ApiClient
{
	public interface IHttpContentSerializer
	{
		Task<T> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default);

		Task<Exception> SerializerErrorHandler(Exception e, HttpResponseMessage message);
	}
}
