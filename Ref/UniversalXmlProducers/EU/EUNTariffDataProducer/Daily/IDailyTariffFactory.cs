using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IDailyTariffFactory
	{
		ICollection<RefCusTariff> TariffCollectionFromNomenclature { get; }

		IRawRecord GetRawRecord(IDailyTariffRateParser dailyTariffRateParser, ISEMeasureParser seMeasureParser);
		IFileDownloaderWrapper GetFileDownloaderWrapper();
		ITariffDataProducer GetDailyTariffDataProducer(IRawRecord dailyRawRecord, ITariffGenerator tariffGenerator, ITariffMerger tariffMerger);
		IXmlProducer<RefCusTariff> GetSEXmlProducer();
		IXmlProducer<RefCusTariff> GetXmlProducer();
		IEnumerable<IWebFileInfo> GetPreDownloadedFiles();
		DateTime ExtractDailyZipPublishTimeFromTaricWebSite(IWebDriverHelper webDriverHelper);
		IEnumerable<IWebFileInfo> GetOrDownloadMonthlyFiles(IProducer monthlyProducer);
		IEnumerable<IWebFileInfo> LocateWebFiles(IWebDriverHelper webDriverHelper);
		IWebDriverHelper GetWebDriverHelper();
		ISEMeasureParser GetSEMeasureParser();
	}
}
