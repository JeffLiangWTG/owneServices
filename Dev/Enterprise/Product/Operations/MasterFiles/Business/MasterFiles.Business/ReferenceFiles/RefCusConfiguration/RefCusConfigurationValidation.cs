//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusConfigurationValidation
//
//    This class should be used for overriding validation in AutoRefCusConfigurationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefCusConfigurationValidation : AutoRefCusConfigurationValidation
	{
		public RefCusConfigurationValidation(AutoRefCusConfiguration parent) : base(parent)
		{
		}
	}
}

