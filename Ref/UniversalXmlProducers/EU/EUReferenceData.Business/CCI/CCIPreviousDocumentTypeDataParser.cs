using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class CCIPreviousDocumentTypeDataParser : CommonDataParser
	{
		public CCIPreviousDocumentTypeDataParser()
			: base(Constants.CCIPreviousDocumentType.RDEntityAttributeValue,
				  Constants.CCIPreviousDocumentType.DataItemAttributeValue)
		{
		}
	}
}
