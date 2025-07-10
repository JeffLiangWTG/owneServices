using System;
using CargoWise.RefDbRepo.Staging.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public interface IZANomenclatureGroupParser
	{
		void Parse(IFileDownloader fileDownloader);
		DateTime CreatedTime { get; }
	}
}
