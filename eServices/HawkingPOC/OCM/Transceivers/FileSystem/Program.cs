using System.Threading.Tasks;
using OcmPoc.Utils;

namespace OcmPoc.Transceivers.FileSystem
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var transceiver = 
				new TransceiverBuilder(new ConnectionBuilder()).Build();

			using (var cancelHandler = new ConsoleCancelHandler())
			{
				await transceiver.RunAsync(cancelHandler.Token);
			}
		}
	}
}
