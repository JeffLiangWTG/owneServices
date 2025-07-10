//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondMoveHeaderValidation
//
//    This class should be used for overriding validation in AutoCusInBondMoveHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusInBondMoveHeaderValidation : AutoCusInBondMoveHeaderValidation
	{
		public CusInBondMoveHeaderValidation(AutoCusInBondMoveHeader parent)
			: base(parent)
		{
		}
	}
}
