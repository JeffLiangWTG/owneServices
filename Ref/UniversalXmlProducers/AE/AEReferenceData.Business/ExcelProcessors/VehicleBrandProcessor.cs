using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public sealed class VehicleBrandProcessor : ExcelProcessor<VehicleBrand, RefCusCodeList>
{
	public VehicleBrandProcessor(string outputPath, string fileName) : base(outputPath, fileName)
	{
	}

	protected override XmlWriterConfiguration GetXMLWriterConfiguration()
	{
		return XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.RefCusCodeTypes.VehicleBrand, Constants.DataGrouping.Dubai);
	}

	protected override IDataConverter<VehicleBrand, RefCusCodeList> Converter => new VehicleBrandDataConverter();

	protected override string DataSource => "Dubai Vehicle Brand";

	protected override string ProcessName => "Vehicle Brands";
}
