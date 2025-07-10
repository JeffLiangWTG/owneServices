//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZDocsMAFCSCommodityValidation
//
//    This class should be used for overriding validation in AutoNZDocsMAFCSCommodityValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet
{
	public class NZDocsMAFCSCommodityValidation : AutoNZDocsMAFCSCommodityValidation
	{
		public NZDocsMAFCSCommodityValidation(AutoNZDocsMAFCSCommodity parent)
			: base(parent)
		{
		}
	}
}
