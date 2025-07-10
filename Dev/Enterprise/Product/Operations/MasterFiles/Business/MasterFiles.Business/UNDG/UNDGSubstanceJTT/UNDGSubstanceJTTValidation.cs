//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstanceJTTValidation
//
//    This class should be used for overriding validation in AutoUNDGSubstanceJTTValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceJTTValidation : AutoUNDGSubstanceJTTValidation
	{
		public UNDGSubstanceJTTValidation(AutoUNDGSubstanceJTT parent) : base(parent)
		{
		}
	}
}
