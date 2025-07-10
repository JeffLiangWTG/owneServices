//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefAirlineCommodityCodeValidation
//
//    This class should be used for overriding validation in AutoRefAirlineCommodityCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineCommodityCodeValidation : AutoRefAirlineCommodityCodeValidation
	{
		public RefAirlineCommodityCodeValidation(AutoRefAirlineCommodityCode parent) : base(parent)
		{
		}
	}
}
