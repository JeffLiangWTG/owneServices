using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public class CustomsResponseProcessor : ExcelProcessor<CustomsResponseStatus, RefCusCodeList>
{
	public CustomsResponseProcessor(string outputPath, string fileName) : base(outputPath, fileName)
	{
	}

	protected override XmlWriterConfiguration GetXMLWriterConfiguration()
	{
		return XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.RefCusCodeTypes.CustomsResponse, Constants.DataGrouping.Dubai);
	}

	protected override IDataConverter<CustomsResponseStatus, RefCusCodeList> Converter => new CustomsResponseStatusDataConverter();

	protected override string DataSource => "Dubai Customs Responses";

	protected override string ProcessName => "Customs Response Status";
}
