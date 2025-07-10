//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbMergedPersonValidation
//
//    This class should be used for overriding validation in AutoGlbMergedPersonValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbMergedPersonValidation : AutoGlbMergedPersonValidation
	{
		public GlbMergedPersonValidation(AutoGlbMergedPerson parent) : base(parent)
		{
		}
	}
}
