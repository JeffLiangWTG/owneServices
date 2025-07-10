//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondPayInfoValidation
//
//    This class should be used for overriding validation in AutoCusInBondPayInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusInBondPayInfoValidation : AutoCusInBondPayInfoValidation
	{
		public CusInBondPayInfoValidation(AutoCusInBondPayInfo parent)
			: base(parent)
		{
		}
	}
}
