using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESTransportDocumentType.Business
{
	public class AESTransportDocumentTypeDataParser : CommonDataParser
	{
		public AESTransportDocumentTypeDataParser()
			: base(Constants.AESTransportDocumentType.RDEntityAttributeValue, Constants.AESTransportDocumentType.CodeAttributeValue)
		{
		}
	}
}
