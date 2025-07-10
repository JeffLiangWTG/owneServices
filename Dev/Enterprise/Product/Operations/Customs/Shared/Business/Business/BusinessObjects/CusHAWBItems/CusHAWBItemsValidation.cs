//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusHAWBItemsValidation
//
//    This class should be used for overriding validation in AutoCusHAWBItemsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusHAWBItemsValidation : AutoCusHAWBItemsValidation
	{
		public CusHAWBItemsValidation(AutoCusHAWBItems parent) : base(parent)
		{
		}
	}
}
