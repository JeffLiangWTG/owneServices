//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbReleaseNoteReadValidation
//
//    This class should be used for overriding validation in AutoGlbReleaseNoteReadValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbReleaseNoteReadValidation : AutoGlbReleaseNoteReadValidation
	{
		public GlbReleaseNoteReadValidation(AutoGlbReleaseNoteRead parent) : base(parent)
		{
		}
	}
}
