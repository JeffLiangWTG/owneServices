//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbDeviceAssignmentDivotValidation
//
//    This class should be used for overriding validation in AutoGlbDeviceAssignmentDivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceAssignmentDivotValidation : AutoGlbDeviceAssignmentDivotValidation
	{
		public GlbDeviceAssignmentDivotValidation(AutoGlbDeviceAssignmentDivot parent)
			: base(parent)
		{
		}
	}
}
