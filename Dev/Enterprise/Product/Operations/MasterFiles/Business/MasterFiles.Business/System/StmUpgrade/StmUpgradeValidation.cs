//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmUpgradeValidation
//
//    This class should be used for overriding validation in AutoStmUpgradeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmUpgradeValidation : AutoStmUpgradeValidation
	{
		public StmUpgradeValidation(AutoStmUpgrade parent) : base(parent)
		{
		}

		protected override void CheckSZ_Status()
		{
			base.CheckSZ_Status();
			ListValidation.ErrorIfInvalidCode(Parent.SZ_StatusInfo);
		}
	}
}
