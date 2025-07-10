using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public class ExitPointProcessor : ExcelProcessor<ExitPoint, RefCusCodeList>
{
	public ExitPointProcessor(string outputPath, string fileName) : base(outputPath, fileName)
	{
	}

	protected override XmlWriterConfiguration GetXMLWriterConfiguration()
	{
		return XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.RefCusCodeTypes.ExitPoint, Constants.DataGrouping.Dubai);
	}

	protected override IDataConverter<ExitPoint, RefCusCodeList> Converter => new ExitPointDataConverter();

	protected override string DataSource => "Dubai Customs EXIT";

	protected override string ProcessName => "Exit Point";
}
