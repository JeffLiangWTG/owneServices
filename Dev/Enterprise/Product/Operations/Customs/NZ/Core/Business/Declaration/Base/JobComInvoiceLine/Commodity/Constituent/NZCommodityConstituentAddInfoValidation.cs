//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCommodityConstituentAddInfoValidation
//
//    This class should be used for overriding validation in AutoNZCommodityConstituentAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityConstituentAddInfoValidation : AutoNZCommodityConstituentAddInfoValidation
	{
		public NZCommodityConstituentAddInfoValidation(AutoNZCommodityConstituentAddInfo parent) : base(parent)
		{
		}
	}
}
