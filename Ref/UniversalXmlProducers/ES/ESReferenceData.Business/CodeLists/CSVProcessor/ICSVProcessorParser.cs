namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public interface ICSVProcessorParser
	{
		string ConvertRecordsToXMLFile(string inputCsvFilePath, string outputXmlFilePath, string dataSource);
	}
}
