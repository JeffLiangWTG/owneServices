//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusQuotaValidation
//
//    This class should be used for overriding validation in AutoRefCusQuotaValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusQuotaValidation : AutoRefCusQuotaValidation
	{
		public RefCusQuotaValidation(AutoRefCusQuota parent) : base(parent)
		{
		}
	}
}
