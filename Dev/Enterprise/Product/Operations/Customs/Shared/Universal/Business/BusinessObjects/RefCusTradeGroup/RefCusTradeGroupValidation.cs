//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTradeGroupValidation
//
//    This class should be used for overriding validation in AutoRefCusTradeGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTradeGroupValidation : AutoRefCusTradeGroupValidation
	{
		public RefCusTradeGroupValidation(AutoRefCusTradeGroup parent) : base(parent)
		{
		}
	}
}
