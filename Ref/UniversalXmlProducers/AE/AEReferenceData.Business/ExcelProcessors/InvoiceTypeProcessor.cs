using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public class InvoiceTypeProcessor : ExcelProcessor<InvoiceType, RefCusCodeList>
{
	public InvoiceTypeProcessor(string outputPath, string fileName) : base(outputPath, fileName)
	{
	}

	protected override XmlWriterConfiguration GetXMLWriterConfiguration()
	{
		return XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.RefCusCodeTypes.InvoiceType, Constants.DataGrouping.Dubai);
	}

	protected override IDataConverter<InvoiceType, RefCusCodeList> Converter => new InvoiceTypeDataConverter();

	protected override string DataSource => "Dubai Invoice Type";

	protected override string ProcessName => "Invoice Type";
}
