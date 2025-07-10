using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Telematics.ServiceTasks
{
	public interface IHttpClientAdapter
	{
		Task<string> GetStringAsync(Uri requestUri, CancellationToken cancellationToken);
	}
}
