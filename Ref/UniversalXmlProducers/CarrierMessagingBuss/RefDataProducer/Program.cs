using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer
{
	internal class Program
	{
		static async Task<int> Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: RefDataProducer <type>");
				Console.WriteLine("Type can be RefAccessorial.");
				return (int)ProducerStatus.Failure;
			}

			var config = AppConfigurationProvider.AppConfiguration;
			using var tokenProvider = new AccessTokenProvider(config.TenantId, config.ClientId, config.ServiceId, config.PrivateKeyFileName, config.CertificateFileName);
			var producer = XmlProducerFactory.CreateProducer(args[0], tokenProvider);// new RefAccessorialXmlProducer(httpWebHelper, tokenProvider);
			await producer.ProduceXmlAsync(config.OutputPath, DateTime.UtcNow).ConfigureAwait(false);

			return (int)ProducerStatus.Success;
		}
	}
}
