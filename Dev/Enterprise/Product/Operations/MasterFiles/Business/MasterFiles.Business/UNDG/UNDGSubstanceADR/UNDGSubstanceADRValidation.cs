//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstanceADRValidation
//
//    This class should be used for overriding validation in AutoUNDGSubstanceADRValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceADRValidation : AutoUNDGSubstanceADRValidation
	{
		public UNDGSubstanceADRValidation(AutoUNDGSubstanceADR parent) : base(parent)
		{
		}
	}
}
