using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public class DeclarationPurposeProcessor : ExcelProcessor<DeclarationPurpose, RefCusCodeList>
{
	public DeclarationPurposeProcessor(string outputPath, string fileName) : base(outputPath, fileName)
	{
	}

	protected override XmlWriterConfiguration GetXMLWriterConfiguration()
	{
		return XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.RefCusCodeTypes.DeclarationPurpose, Constants.DataGrouping.Dubai);
	}

	protected override IDataConverter<DeclarationPurpose, RefCusCodeList> Converter => new DeclarationPurposeDataConverter();

	protected override string DataSource => "Dubai Declaration Purpose";

	protected override string ProcessName => "Declaration Purpose";
}

