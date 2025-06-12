using System;

namespace CargoWise.eHub.Shared.ServiceTaskHost.Core
{
	public interface IServiceTaskRunner : IRunnerConfiguration, IDisposable
	{
		void Start();
		void Stop();
	}
}