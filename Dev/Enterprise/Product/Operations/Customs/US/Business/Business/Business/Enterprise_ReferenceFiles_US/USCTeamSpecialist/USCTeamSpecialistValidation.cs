//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCTeamSpecialistValidation
//
//    This class should be used for overriding validation in AutoUSCTeamSpecialistValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCTeamSpecialistValidation : AutoUSCTeamSpecialistValidation
	{
		public USCTeamSpecialistValidation(AutoUSCTeamSpecialist parent)
			: base(parent)
		{
		}
	}
}
