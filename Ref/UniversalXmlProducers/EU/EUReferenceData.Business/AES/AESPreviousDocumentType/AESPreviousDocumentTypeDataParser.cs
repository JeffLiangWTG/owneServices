using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AESPreviousDocumentType.Business
{
	public sealed class AESPreviousDocumentTypeDataParser : CommonDataParser
	{
		public AESPreviousDocumentTypeDataParser()
			: base(Constants.AESPreviousDocumentType.RDEntityAttributeValue, Constants.AESPreviousDocumentType.CodeAttributeValue)
		{
		}
	}
}
