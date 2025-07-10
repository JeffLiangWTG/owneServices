//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstanceRIDValidation
//
//    This class should be used for overriding validation in AutoUNDGSubstanceRIDValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceRIDValidation : AutoUNDGSubstanceRIDValidation
	{
		public UNDGSubstanceRIDValidation(AutoUNDGSubstanceRID parent) : base(parent)
		{
		}
	}
}
