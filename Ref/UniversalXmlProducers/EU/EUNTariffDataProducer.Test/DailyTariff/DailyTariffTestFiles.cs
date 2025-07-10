using System;
using System.Collections.Generic;
using System.IO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test.DailyTariff
{
	public static class DailyTariffTestFiles
	{
		public static string BaseDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\DailyTariffProducer\");

		public static string SampleMeasuresPath => Path.Combine(BaseDirectory, @"SampleMeasures.xlsx");
		public static string DuplicatedMeasuresPath => Path.Combine(BaseDirectory, @"DuplicatedMeasures.xlsx");
		public static string MeasuresWithDeletionsPath => Path.Combine(BaseDirectory, @"MeasuresWithDeletion.xlsx");
		public static string MeasuresWithFutureStartDatePath => Path.Combine(BaseDirectory, @"MeasuresWithFutureStartDate.xlsx");
		public static string MeasuresTwoUpdates => Path.Combine(BaseDirectory, @"MeasuresTwoUpdates.xlsx");
		public static string TariffDataWithMissingCodePath => Path.Combine(BaseDirectory, @"TariffDataWithMissingCode.xlsx");

		public static string MonthlyDutiesImportPath => Path.Combine(BaseDirectory, @"MonthlyDutiesImport.xlsx");
		public static string MonthlyMeasuresConditionPath => Path.Combine(BaseDirectory, @"MonthlyMeasureConditions.xlsx");
		public static string MonthlyMeasuresExclusionPath => Path.Combine(BaseDirectory, @"MonthlyMeasureExclusions.xlsx");
		public static string DailyTaricWebPageHtmlFile => Path.Combine(BaseDirectory, @"DailyTaricWebPage_20210810.html");
		public static string NewDailyTaricWebPageHtmlFile => Path.Combine(BaseDirectory, @"TARICDailyPublications_20220728.html");
		public static string DailyTaricWebPageHtmlWithTimeFile => Path.Combine(BaseDirectory, @"TARICDailyPublications7_30_2024_4_52_00 PM.html");
		public static string SampleDailyMeasuresDailyPath => Path.Combine(BaseDirectory, @"SampleDailyMeasures.xlsx");
		public static string MeasuresTwoUpdatesBeforeInsert => Path.Combine(BaseDirectory, @"MeasuresTwoUpdatesBeforeInsert.xlsx");
		public static string MeasuresTwoDuplicatedInsert => Path.Combine(BaseDirectory, @"MeasuresTwoDuplicatedInsert.xlsx");
		public static string SampleDailyGoodsNomenclature => Path.Combine(BaseDirectory, @"SampleDailyGoodsNomenclature.xlsx");

		public static WebFileInfo SampleMeasuresWebFileInfo => new WebFileInfo(@"SampleMeasures.csv", BaseDirectory, DateTime.MinValue);
		public static WebFileInfo MonthlyDutiesImportWebFileInfo => new WebFileInfo(@"MonthlyDutiesImport.xlsx", BaseDirectory, DateTime.MinValue);
		public static WebFileInfo MonthlyMeasuresConditionWebFileInfo => new WebFileInfo(@"MonthlyMeasureConditions.xlsx", BaseDirectory, DateTime.MinValue);
		public static WebFileInfo MonthlyMeasuresExclusionWebFileInfo => new WebFileInfo(@"MonthlyMeasureExclusions.xlsx", BaseDirectory, DateTime.MinValue);

		public static void SetLastWriteTime(DateTime date, IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				var filePath = Path.Combine(BaseDirectory, file);
				File.SetLastWriteTime(filePath, date);
			}
		}

		public static void SetCreationTime(DateTime date, IEnumerable<string> files)
		{
			foreach (var file in files)
			{
				var filePath = Path.Combine(BaseDirectory, file);
				File.SetCreationTime(filePath, date);
			}
		}
	}
}
