using System.Threading;
using System.Threading.Tasks;

namespace OcmPoc.Transceivers
{
	class Transceiver : IRunnable
	{
		readonly IRunnable sender;
		readonly IRunnable receiver;

		public Transceiver(IRunnable sender, IRunnable receiver)
		{
			this.sender = sender;
			this.receiver = receiver;
		}

		public async Task RunAsync(CancellationToken token)
		{
			await Task.WhenAll(
				sender.RunAsync(token),
				receiver.RunAsync(token));
		}
	}
}
