using System.Net.Http;
using Enterprise.Integration;

namespace Enterprise.Services.ServiceHost
{
	public interface IeAdaptorController
	{
		IeAdaptorConfig Config { get; }

		HttpResponseMessage Get();

		HttpResponseMessage Post();
	}
}
