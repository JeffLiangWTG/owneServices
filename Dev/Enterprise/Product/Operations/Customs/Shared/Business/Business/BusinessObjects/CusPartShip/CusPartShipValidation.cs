//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPartShipValidation
//
//    This class should be used for overriding validation in AutoCusPartShipValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusPartShipValidation : AutoCusPartShipValidation
	{
		public CusPartShipValidation(AutoCusPartShip parent)
			: base(parent)
		{
		}
	}
}
