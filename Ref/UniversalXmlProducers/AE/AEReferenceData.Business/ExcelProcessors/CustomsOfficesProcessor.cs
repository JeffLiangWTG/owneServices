using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public class CustomsOfficesProcessor : ExcelProcessor<CustomsOffice, RefCusCodeList>
{
	public CustomsOfficesProcessor(string outputPath, string fileName) : base(outputPath, fileName)
	{
	}

	protected override XmlWriterConfiguration GetXMLWriterConfiguration()
	{
		return XmlWriterHelper.GetRefCusCodeListForCustomsOfficesWriterConfiguration(Constants.RefCusCodeTypes.CustomsOffices, Constants.DataGrouping.Dubai);
	}

	protected override IDataConverter<CustomsOffice, RefCusCodeList> Converter => new CustomsOfficeDataConverter();

	protected override string DataSource => "Dubai Customs Office";

	protected override string ProcessName => "Customs Offices";
}
