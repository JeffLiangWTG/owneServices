using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	interface INomenclaturePlusDailyDataTestProvider
	{
		IEnumerable<(string Path, DateTime PublicationDate)> DailyNomenclatureFileCollection { get; }
		IEnumerable<(string Path, DateTime PublicationDate)> DailyRateFileCollection { get; }

		(string Path, DateTime PublicationDate) DeclarableCodeFile { get; }
		IEnumerable<(string Path, DateTime PublicationDate)> NomenclatureFileCollection { get; }

		IEnumerable<(int Number, string Description)> Sections { get; }

		(string Path, DateTime PublicationDate) DutiesImportFile { get; }
		(string Path, DateTime PublicationDate) DutiesExportFile { get; }
		(string Path, DateTime PublicationDate) MeasureExclusions { get; }
		(string Path, DateTime PublicationDate) MeasureConditions { get; }
	}
}
