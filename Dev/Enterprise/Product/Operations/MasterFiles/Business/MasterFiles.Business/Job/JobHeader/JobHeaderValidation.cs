//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobHeaderValidation
//
//    This class should be used for overriding validation in AutoJobHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobHeaderValidation : AutoJobHeaderValidation
	{
		public JobHeaderValidation(AutoJobHeader parent) : base(parent)
		{
		}

		protected override void CheckJH_GE()
		{
			base.CheckJH_GE();

			GlbBranchCombinationValidation.CheckBranchDepartmentCombination(Parent.JH_GEInfo, Parent.Branch, Parent.Department);
		}

		public static string ClosingJobSecurityErrorMessage
		{
			get { return Res.GetString("4849ff5b-b272-4664-a6b2-dec678d292e3", "You do not have the security right to close a Job. Please check with your system administrator if you require access to this function."); }
		}

		public static string DisallowOverrideofFieldSecurityErrorMessage
		{
			get { return Res.GetString("8db7b3b9-edc0-41ba-84ba-1479145bfe60", "You must revert the value of this field to its original value of '{0}'"); }
		}

		protected override void CheckJH_GB_TaxBranch()
		{
			base.CheckJH_GB_TaxBranch();

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				MandatoryValidation.CheckEntered(Parent.JH_GB_TaxBranchInfo);
				ListValidation.ErrorIfInvalidPK(Parent.JH_GB_TaxBranchInfo);
			}
		}
	}
}
