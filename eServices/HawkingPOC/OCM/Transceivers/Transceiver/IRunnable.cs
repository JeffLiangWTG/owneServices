using System.Threading;
using System.Threading.Tasks;

namespace OcmPoc.Transceivers
{
	public interface IRunnable
	{
		Task RunAsync(CancellationToken token);
	}
}
