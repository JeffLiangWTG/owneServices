using System;
using System.Threading;

namespace OcmPoc.Utils
{
	public interface IConsoleCancelHandler : IDisposable
	{
		CancellationToken Token { get; }
	}
}