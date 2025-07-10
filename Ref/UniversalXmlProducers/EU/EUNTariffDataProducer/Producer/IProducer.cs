using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IProducer
	{
		IEnumerable<string> FilesToLocate { get; }
		DateTime PublishTime { get; }

		IEnumerable<IWebFileInfo> LocateWebFiles();

		bool DownloadFiles(IEnumerable<IWebFileInfo> webFileInfos);
	}
}
