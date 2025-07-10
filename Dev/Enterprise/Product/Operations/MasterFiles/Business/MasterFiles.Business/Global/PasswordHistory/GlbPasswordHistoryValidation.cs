//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbPasswordHistoryValidation
//
//    This class should be used for overriding validation in AutoGlbPasswordHistoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbPasswordHistoryValidation : AutoGlbPasswordHistoryValidation
	{
		public GlbPasswordHistoryValidation(AutoGlbPasswordHistory parent) : base(parent)
		{
		}
	}
}
