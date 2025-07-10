using System;
using System.Collections.Generic;
using System.IO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test;

sealed class WI00912940TestDataProvider : INomenclaturePlusDailyDataTestProvider
{
	public IEnumerable<(int Number, string Description)> Sections
		=> [(97, "WORKS OF ART, COLLECTORS' PIECES AND ANTIQUES")];


	public (string Path, DateTime PublicationDate) DeclarableCodeFile
		=> (Path.Combine(BasePath, TestCaseDirectory, "DeclarableCodes.xlsx"), MonthlyFilePublishDate);

	public IEnumerable<(string Path, DateTime PublicationDate)> NomenclatureFileCollection
		=> [(Path.Combine(BasePath, TestCaseDirectory, "Nomenclature.xlsx"), MonthlyFilePublishDate)];

	public (string Path, DateTime PublicationDate) DutiesImportFile
		=> (Path.Combine(BasePath, TestCaseDirectory, "DutiesImport.xlsx"), MonthlyFilePublishDate);

	public (string Path, DateTime PublicationDate) DutiesExportFile
		=> (Path.Combine(BasePath, TestCaseDirectory, "DutiesExport.xlsx"), MonthlyFilePublishDate);

	public (string Path, DateTime PublicationDate) MeasureExclusions
		=> (Path.Combine(BasePath, TestCaseDirectory, "MeasureExclusions.xlsx"), MonthlyFilePublishDate);

	public (string Path, DateTime PublicationDate) MeasureConditions
		=> (Path.Combine(BasePath, TestCaseDirectory, "MeasureConditions.xlsx"), MonthlyFilePublishDate);

	public IEnumerable<(string Path, DateTime PublicationDate)> DailyNomenclatureFileCollection
		=> [(Path.Combine(BasePath, TestCaseDirectory, "DailyNomenclature.xlsx"), DailyFilePublishDate)];

	public IEnumerable<(string Path, DateTime PublicationDate)> DailyRateFileCollection
		=> [(Path.Combine(BasePath, TestCaseDirectory, "DailyMeasures.xlsx"), DailyFilePublishDate)];

	static string BasePath => TestHelper.BasePath;

	internal static readonly DateTime MonthlyFilePublishDate = new(2025, 05, 1, 10, 11, 12);
	internal static readonly DateTime DailyFilePublishDate = new(2025, 05, 13, 10, 11, 12);
	const string TestCaseDirectory = "WI00912940";
}
