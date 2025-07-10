//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefSysConfigValidation
//
//    This class should be used for overriding validation in AutoRefSysConfigValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefSysConfigValidation : AutoRefSysConfigValidation
	{
		public RefSysConfigValidation(AutoRefSysConfig parent) : base(parent)
		{
		}
	}
}
