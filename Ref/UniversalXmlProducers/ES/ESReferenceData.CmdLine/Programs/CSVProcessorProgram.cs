using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine;

public static class CSVProcessorProgram
{
	public static void Run(string outputPath)
	{
		var dateTimeProvider = new DateTimeProvider();
		foreach (var (listType, dataSource) in
			new []
			{
				(Business.Constants.RefCusCodeListTypes.DC40A, Business.Constants.DataSources.DC40A),
				(Business.Constants.RefCusCodeListTypes.DC40E, Business.Constants.DataSources.DC40E),
				(Business.Constants.RefCusCodeListTypes.DC40N, Business.Constants.DataSources.DC40N),
				(Business.Constants.RefCusCodeListTypes.DC40T, Business.Constants.DataSources.DC40T),
				(Business.Constants.RefCusCodeListTypes.DC40W, Business.Constants.DataSources.DC40W),
				(Business.Constants.RefCusCodeListTypes.DC40X, Business.Constants.DataSources.DC40X),
				(Business.Constants.RefCusCodeListTypes.DC44H, Business.Constants.DataSources.DC44H),
				(Business.Constants.RefCusCodeListTypes.AI44E, Business.Constants.DataSources.AI44E),
				(Business.Constants.RefCusCodeListTypes.EXSEC, Business.Constants.DataSources.EXSEC),
				(Business.Constants.RefCusCodeListTypes.TD44E, Business.Constants.DataSources.TD44E),
				(Business.Constants.RefCusCodeListTypes.TD44G, Business.Constants.DataSources.TD44G)
			})
		{
			ExecuteCSVProcessorParser(dateTimeProvider, outputPath, listType, dataSource);
		}
	}

	static void ExecuteCSVProcessorParser(DateTimeProvider dateTimeProvider, string outputPath, string codeType, string dataSource)
	{
		var csvCodes = CSVProcessorProvider.RequestCodes(codeType);
		var xmlFileName = $"RefCodeListZZ_{codeType}_ES.xml";

		var parser = CSVProcessorFactory.GetCSVParser(codeType, dateTimeProvider, DateFormat);

		Program.PrintErrorMessage(parser.ConvertRecordsToXMLFile(csvCodes, Path.Combine(outputPath, xmlFileName), dataSource));
	}

	const string DateFormat = "dd-MM-yyyy";
}
