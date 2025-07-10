using System;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer
{
	public interface IXmlProducer
	{
		public Task ProduceXmlAsync(string outputPath, DateTime publicationDate);
	}
}
