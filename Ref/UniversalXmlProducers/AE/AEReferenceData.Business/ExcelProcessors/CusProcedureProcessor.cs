using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public class CusProcedureProcessor : ExcelProcessor<CusProcedure, RefCusProcedure>
{
	public CusProcedureProcessor(string outputPath, string fileName) : base(outputPath, fileName)
	{
	}

	protected override XmlWriterConfiguration GetXMLWriterConfiguration()
	{
		return XmlWriterHelper.GetRefCusProcedureWriterConfiguration();
	}

	protected override IDataConverter<CusProcedure, RefCusProcedure> Converter => new CusProcedureDataConverter();

	protected override string DataSource => "Dubai Customs Procedures";

	protected override string ProcessName => "Customs Procedures";
}
