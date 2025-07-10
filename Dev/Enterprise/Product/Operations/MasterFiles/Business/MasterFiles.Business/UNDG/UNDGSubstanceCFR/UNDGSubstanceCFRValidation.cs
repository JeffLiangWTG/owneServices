//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstanceCFRValidation
//
//    This class should be used for overriding validation in AutoUNDGSubstanceCFRValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceCFRValidation : AutoUNDGSubstanceCFRValidation
	{
		public UNDGSubstanceCFRValidation(AutoUNDGSubstanceCFR parent) : base(parent)
		{
		}
	}
}
