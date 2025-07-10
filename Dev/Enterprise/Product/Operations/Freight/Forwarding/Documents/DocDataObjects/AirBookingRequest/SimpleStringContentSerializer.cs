using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Freight.Integration.ApiClient;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	class SimpleStringContentSerializer : IHttpContentSerializer
	{
		public async Task<T> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default)
		{
			if (typeof(T) == typeof(string))
			{
				var data = await content.ReadAsStringAsync().ConfigureAwait(false);
				return (T)(object)data;
			}

			throw new NotSupportedException($"Deserialization of type '{typeof(T).Name}' is not supported.");
		}

		public Task<Exception> SerializerErrorHandler(Exception e, HttpResponseMessage message)
		{
			throw e;
		}
	}
}
