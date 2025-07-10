//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusReconCustomsChargeValidation
//
//    This class should be used for overriding validation in AutoCusReconCustomsChargeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusReconCustomsChargeValidation : AutoCusReconCustomsChargeValidation
	{
		public CusReconCustomsChargeValidation(AutoCusReconCustomsCharge parent) : base(parent)
		{
		}
	}
}
