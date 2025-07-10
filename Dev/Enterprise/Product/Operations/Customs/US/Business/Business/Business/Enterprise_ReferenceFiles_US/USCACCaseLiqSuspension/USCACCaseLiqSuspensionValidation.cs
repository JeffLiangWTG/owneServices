//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCACCaseLiqSuspensionValidation
//
//    This class should be used for overriding validation in AutoUSCACCaseLiqSuspensionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseLiqSuspensionValidation : AutoUSCACCaseLiqSuspensionValidation
	{
		public USCACCaseLiqSuspensionValidation(AutoUSCACCaseLiqSuspension parent) : base(parent)
		{
		}
	}
}
