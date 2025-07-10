//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbEmailAddressValidation
//
//    This class should be used for overriding validation in AutoGlbEmailAddressValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmailAddressValidation : AutoGlbEmailAddressValidation
	{
		public GlbEmailAddressValidation(AutoGlbEmailAddress parent) : base(parent)
		{
		}
	}
}
