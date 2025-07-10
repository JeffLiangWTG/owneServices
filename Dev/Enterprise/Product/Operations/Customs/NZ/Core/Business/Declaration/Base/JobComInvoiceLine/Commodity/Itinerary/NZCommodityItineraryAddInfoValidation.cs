//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCommodityItineraryAddInfoValidation
//
//    This class should be used for overriding validation in AutoNZCommodityItineraryAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityItineraryAddInfoValidation : AutoNZCommodityItineraryAddInfoValidation
	{
		public NZCommodityItineraryAddInfoValidation(AutoNZCommodityItineraryAddInfo parent) : base(parent)
		{
		}
	}
}
