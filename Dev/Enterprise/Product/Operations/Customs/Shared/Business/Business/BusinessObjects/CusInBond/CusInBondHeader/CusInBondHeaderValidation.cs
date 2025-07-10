//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondHeaderValidation
//
//    This class should be used for overriding validation in AutoCusInBondHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusInBondHeaderValidation : AutoCusInBondHeaderValidation
	{
		public CusInBondHeaderValidation(AutoCusInBondHeader parent)
			: base(parent)
		{
		}
	}
}
