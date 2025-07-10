using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IWebFileInfo
	{
		string FileName { get; }
		string DownloadPath { get; }
		DateTime LastModificationTime { get; }
		Exception Exception { get; }
	}
}
