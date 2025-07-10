//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgAirlineMAWBStockManagementValidation
//
//    This class should be used for overriding validation in AutoOrgAirlineMAWBStockManagementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("See WI00292226")]
	public class OrgAirlineMAWBStockManagementValidation : AutoOrgAirlineMAWBStockManagementValidation
	{
		public OrgAirlineMAWBStockManagementValidation(AutoOrgAirlineMAWBStockManagement parent) : base(parent)
		{
		}

		protected override void CheckOHM_MAWBStockThreshold()
		{
			base.CheckOHM_MAWBStockThreshold();

			if (Parent.OHM_MAWBStockThreshold < 1 || Parent.OHM_MAWBStockThreshold > 9999)
			{
				Parent.OHM_MAWBStockThresholdInfo.AddError(Res.GetString("3c73ca75-3272-4611-840a-548d4147bfc1", "The minimum MAWB Threshold value cannot be lower than 1 and the maximum cannot exceed 9999."));
			}
		}

		protected override void CheckOHM_GC_Company()
		{
			base.CheckOHM_GC_Company();

			// If branch is valid but company is empty then auto fill company
			if (Parent.OHM_GB_Branch.IsValid && Parent.OHM_GC_Company.IsEmpty && Parent.Branch?.Company != null)
			{
				using (Parent.GetValidationSuspender())
				{
					Parent.OHM_GC_Company = Parent.Branch.Company.PK;
				}
			}
		}

		protected override void CheckOHM_GB_Branch()
		{
			base.CheckOHM_GB_Branch();
			ValidateBranchBelongToCompany();
			ValidateNoDuplicateBranches();
		}

		void ValidateBranchBelongToCompany()
		{
			// Validation rule:
			// If branch and company are both valid then branch must belong to the company
			if (Parent.OHM_GC_Company.IsValid && Parent.OHM_GB_Branch.IsValid && Parent.Branch.Company.PK != Parent.OHM_GC_Company)
			{
				Parent.OHM_GB_BranchInfo.AddError($"The branch '{Parent.Branch.GB_BranchName}' does not belong to company '{Parent.Company.GC_Name}'.");
			}
		}

		void ValidateNoDuplicateBranches()
		{
			// Validation rule:
			// No - same company with same branch
			// Yes - same company with different branch
			// Yes - company with empty branch
			var matchingQuery = new ZQuery(OrgAirlineMAWBStockManagementSchema.OHM_OH_Carrier, Parent.OHM_OH_Carrier);
			matchingQuery.AddToFilter(OrgAirlineMAWBStockManagementSchema.OHM_GB_Branch, Parent.OHM_GB_Branch == ZGuid.Empty ? null : (ZGuid?)Parent.OHM_GB_Branch);
			matchingQuery.AddToFilter(OrgAirlineMAWBStockManagementSchema.OHM_GC_Company, Parent.OHM_GC_Company == ZGuid.Empty ? null : (ZGuid?)Parent.OHM_GC_Company);
			matchingQuery.AddToFilter(OrgAirlineMAWBStockManagementSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.Exists(typeof(OrgAirlineMAWBStockManagement), matchingQuery))
			{
				if (Parent.OHM_GC_Company.IsValid)
				{
					Parent.OHM_GB_BranchInfo.AddError(
						$"This organization already contains a MAWB configuration for company '{Parent.Company.GC_Name}' {(Parent.OHM_GB_Branch.IsValid ? $" and branch '{Parent.Branch.GB_BranchName}'" : string.Empty)}");
				}
				else
				{
					Parent.OHM_GB_BranchInfo.AddError(Res.GetString("6cad1a6d-87d7-42bb-808b-f953e3631ee1", "This organization already contains a MAWB configuration for global."));
				}
			}
		}
	}
}
