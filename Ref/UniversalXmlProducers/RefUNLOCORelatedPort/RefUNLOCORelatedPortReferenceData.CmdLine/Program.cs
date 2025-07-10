using System.Net.Http;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.CmdLine
{
	class Program
	{
		static int Main()
		{
			using (var client = new HttpClient())
			{
				RefUNLOCORelatedPortReferenceDataProducer.ProduceXml(client);
			}

			return (int)ProducerStatus.Success;
		}
	}
}
