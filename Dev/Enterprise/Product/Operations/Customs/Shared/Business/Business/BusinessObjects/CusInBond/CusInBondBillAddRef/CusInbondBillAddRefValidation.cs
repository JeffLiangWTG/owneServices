//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInbondBillAddRefValidation
//
//    This class should be used for overriding validation in AutoCusInbondBillAddRefValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusInbondBillAddRefValidation : AutoCusInbondBillAddRefValidation
	{
		public CusInbondBillAddRefValidation(AutoCusInbondBillAddRef parent)
			: base(parent)
		{
		}
	}
}
