//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefComplianceListValidation
//
//    This class should be used for overriding validation in AutoRefComplianceListValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceListValidation : AutoRefComplianceListValidation
	{
		public RefComplianceListValidation(AutoRefComplianceList parent) : base(parent)
		{
		}
	}
}
