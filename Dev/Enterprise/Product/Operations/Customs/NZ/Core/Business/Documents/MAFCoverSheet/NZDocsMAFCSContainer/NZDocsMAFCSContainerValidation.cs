//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZDocsMAFCSContainerValidation
//
//    This class should be used for overriding validation in AutoNZDocsMAFCSContainerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet
{
	public class NZDocsMAFCSContainerValidation : AutoNZDocsMAFCSContainerValidation
	{
		public NZDocsMAFCSContainerValidation(AutoNZDocsMAFCSContainer parent)
			: base(parent)
		{
		}
	}
}
