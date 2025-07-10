//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZDocsMAFCoverSheetValidation
//
//    This class should be used for overriding validation in AutoNZDocsMAFCoverSheetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet
{
	public class NZDocsMAFCoverSheetValidation : AutoNZDocsMAFCoverSheetValidation
	{
		public NZDocsMAFCoverSheetValidation(AutoNZDocsMAFCoverSheet parent)
			: base(parent)
		{
		}
	}
}
