//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbGroupLocationValidation
//
//    This class should be used for overriding validation in AutoGlbGroupLocationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupLocationValidation : AutoGlbGroupLocationValidation
	{
		public GlbGroupLocationValidation(AutoGlbGroupLocation parent) : base(parent)
		{
		}
	}
}
