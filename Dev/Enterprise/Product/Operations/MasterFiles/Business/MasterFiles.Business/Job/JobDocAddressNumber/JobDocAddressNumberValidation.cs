//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobDocAddressNumberValidation
//
//    This class should be used for overriding validation in AutoJobDocAddressNumberValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressNumberValidation : AutoJobDocAddressNumberValidation
	{
		public JobDocAddressNumberValidation(AutoJobDocAddressNumber parent) : base(parent)
		{
		}
	}
}
