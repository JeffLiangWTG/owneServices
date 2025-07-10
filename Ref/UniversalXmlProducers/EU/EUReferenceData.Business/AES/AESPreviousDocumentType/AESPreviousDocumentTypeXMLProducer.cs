using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESPreviousDocumentType.Business
{
	public sealed class AESPreviousDocumentTypeXMLProducer : CommonXMLProducer
	{
		public AESPreviousDocumentTypeXMLProducer()
			: base(Constants.AESPreviousDocumentType.OutputFileName
				, Constants.AESPreviousDocumentType.OutputFileDataSource
				, XmlWriterHelper.GetRefCusCodeListConfigurationForAES(Constants.ZZRefCusCodeList.ExportPreviousDocumentCode))

		{
		}

		protected override CommonDataParser DataParser => new AESPreviousDocumentTypeDataParser();
	}
}
