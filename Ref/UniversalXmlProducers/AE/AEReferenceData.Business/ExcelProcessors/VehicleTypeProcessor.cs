using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public sealed class VehicleTypeProcessor : ExcelProcessor<VehicleType, RefCusCodeList>
{
	public VehicleTypeProcessor(string outputPath, string fileName) : base(outputPath, fileName)
	{
	}

	protected override XmlWriterConfiguration GetXMLWriterConfiguration()
	{
		return XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.RefCusCodeTypes.VehicleType, Constants.DataGrouping.Dubai);
	}

	protected override IDataConverter<VehicleType, RefCusCodeList> Converter => new VehicleTypeDataConverter();

	protected override string DataSource => "Dubai Vehicle Types";

	protected override string ProcessName => "Vehicle Types";
}
