//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGCountryReferencePivotValidation
//
//    This class should be used for overriding validation in AutoUNDGCountryReferencePivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGCountryReferencePivotValidation : AutoUNDGCountryReferencePivotValidation
	{
		public UNDGCountryReferencePivotValidation(AutoUNDGCountryReferencePivot parent) : base(parent)
		{
		}

		protected override void CheckDCP_StorageInstruction()
		{
			base.CheckDCP_StorageInstruction();
			ListValidation.ErrorIfInvalidCode(Parent.DCP_StorageInstructionInfo);
		}
	}
}
