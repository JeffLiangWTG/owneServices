//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefSysConfigTypeValidation
//
//    This class should be used for overriding validation in AutoRefSysConfigTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefSysConfigTypeValidation : AutoRefSysConfigTypeValidation
	{
		public RefSysConfigTypeValidation(AutoRefSysConfigType parent) : base(parent)
		{
		}
	}
}
