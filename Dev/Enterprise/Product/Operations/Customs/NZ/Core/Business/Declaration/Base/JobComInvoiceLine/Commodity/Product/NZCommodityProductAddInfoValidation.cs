//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCommodityProductAddInfoValidation
//
//    This class should be used for overriding validation in AutoNZCommodityProductAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityProductAddInfoValidation : AutoNZCommodityProductAddInfoValidation
	{
		public NZCommodityProductAddInfoValidation(AutoNZCommodityProductAddInfo parent) : base(parent)
		{
		}
	}
}
