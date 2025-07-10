//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondContainerValidation
//
//    This class should be used for overriding validation in AutoCusInBondContainerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusInBondContainerValidation : AutoCusInBondContainerValidation
	{
		public CusInBondContainerValidation(AutoCusInBondContainer parent)
			: base(parent)
		{
		}
	}
}
