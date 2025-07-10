using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public class AmendCancelReasonProcessor : ExcelProcessor<AmendCancelReason, RefCusCodeList>
{
	public AmendCancelReasonProcessor(string outputPath, string fileName) : base(outputPath, fileName)
	{
	}

	protected override XmlWriterConfiguration GetXMLWriterConfiguration()
	{
		return XmlWriterHelper.GetRefCusCodeListWriterConfiguration(defaultDataGrouping: Constants.DataGrouping.Dubai);
	}

	protected override IDataConverter<AmendCancelReason, RefCusCodeList> Converter => new AmendCancelReasonDataConverter();

	protected override string DataSource => "Dubai Customs Reason";

	protected override string ProcessName => "Amend Cancel Reason";

}
