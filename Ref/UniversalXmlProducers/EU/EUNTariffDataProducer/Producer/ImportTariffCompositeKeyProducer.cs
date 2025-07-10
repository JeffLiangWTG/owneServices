using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ImportTariffCompositeKeyProducer : ITariffProducer
	{
		public void Run(IEnumerable<RefCusTariff> tariffs)
		{
			Argument.NotNull(tariffs, nameof(tariffs));
			var xmlProducer = new ImportTariffCompositeKeyXmlProducer();
			xmlProducer.InitializeWriter(ApplicationConfig.PublishDate, null, UpdateType.Partial);
			xmlProducer.ExportToXml(tariffs);
		}

		public void Run(IEnumerable<string> tariffHeaders, ICollection<RefCusTariff> tariffCollection)
		{
			throw new NotImplementedException();
		}
	}
}
