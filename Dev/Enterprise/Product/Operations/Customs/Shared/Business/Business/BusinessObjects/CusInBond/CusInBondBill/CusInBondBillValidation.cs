//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondBillValidation
//
//    This class should be used for overriding validation in AutoCusInBondBillValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusInBondBillValidation : AutoCusInBondBillValidation
	{
		public CusInBondBillValidation(AutoCusInBondBill parent)
			: base(parent)
		{
		}
	}
}
