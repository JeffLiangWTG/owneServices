using System;
using System.Collections.Generic;
using System.IO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	sealed class SampleTestDataProvider : INomenclaturePlusDailyDataTestProvider
	{
		public SampleTestDataProvider(DateTime monthlyFilePublishDate, DateTime dailyFilePublishDate, string dailyGoodsNomenclatureFileName = "SampleDailyGoodsNomenclature.xlsx")
		{
			this.monthlyFilePublishDate = monthlyFilePublishDate;
			this.dailyFilePublishDate = dailyFilePublishDate;
			this.dailyGoodsNomenclatureFileName = dailyGoodsNomenclatureFileName;
		}

		public IEnumerable<(string Path, DateTime PublicationDate)> DailyNomenclatureFileCollection
			=> new[] { (Path.Combine(BasePath, $"DailyTariffProducer\\{dailyGoodsNomenclatureFileName}"), dailyFilePublishDate) };

		public IEnumerable<(string Path, DateTime PublicationDate)> DailyRateFileCollection
			=> new[] { (Path.Combine(BasePath, "DailyTariffProducer\\SampleDailyMeasures.xlsx"), dailyFilePublishDate) };

		public (string Path, DateTime PublicationDate) DeclarableCodeFile
			=> (Path.Combine(BasePath, "SampleDeclarableCodes.xlsx"), monthlyFilePublishDate);

		public IEnumerable<(string Path, DateTime PublicationDate)> NomenclatureFileCollection
			=> new[]
			{
					(Path.Combine(BasePath, "SampleNomenclature.xlsx"), monthlyFilePublishDate),
					(Path.Combine(BasePath, "SampleNomenclature DE.xlsx"), monthlyFilePublishDate)
			};

		public IEnumerable<(int Number, string Description)> Sections
			=> new[] { (1, "LIVE ANIMALS; ANIMAL PRODUCTS") };

		public (string Path, DateTime PublicationDate) DutiesImportFile
			=> (Path.Combine(BasePath, "SampleDutiesImport.xlsx"), monthlyFilePublishDate);

		public (string Path, DateTime PublicationDate) DutiesExportFile
			=> (Path.Combine(BasePath, "SampleDutiesExport.xlsx"), monthlyFilePublishDate);

		public (string Path, DateTime PublicationDate) MeasureExclusions
			=> (Path.Combine(BasePath, "SampleMeasureExclusion.xlsx"), monthlyFilePublishDate);

		public (string Path, DateTime PublicationDate) MeasureConditions
			=> (Path.Combine(BasePath, "SampleMeasureConditions.xlsx"), monthlyFilePublishDate);

		static string BasePath => TestHelper.BasePath;

		readonly DateTime monthlyFilePublishDate;
		readonly DateTime dailyFilePublishDate;
		readonly string dailyGoodsNomenclatureFileName;
	}
}
