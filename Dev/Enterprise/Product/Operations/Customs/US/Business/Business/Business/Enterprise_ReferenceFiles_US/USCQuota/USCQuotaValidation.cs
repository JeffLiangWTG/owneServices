//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCQuotaValidation
//
//    This class should be used for overriding validation in AutoUSCQuotaValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCQuotaValidation : AutoUSCQuotaValidation
	{
		public USCQuotaValidation(AutoUSCQuota parent)
			: base(parent)
		{
		}
	}
}
