//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusCodeOrAttributeTransportModeValidation
//
//    This class should be used for overriding validation in AutoRefCusCodeOrAttributeTransportModeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeOrAttributeTransportModeValidation : AutoRefCusCodeOrAttributeTransportModeValidation
	{
		public RefCusCodeOrAttributeTransportModeValidation(AutoRefCusCodeOrAttributeTransportMode parent) : base(parent)
		{
		}
	}
}
