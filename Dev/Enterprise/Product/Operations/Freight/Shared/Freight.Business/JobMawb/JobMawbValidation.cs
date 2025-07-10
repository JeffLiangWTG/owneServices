using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class JobMawbValidation : AutoJobMawbValidation
	{
		MAWBStockManagementStrategy mAWBStockManagementStrategy;

		public JobMawbValidation(AutoJobMawb parent) : base(parent)
		{
		}

		#region JM_GB

		protected override void CheckJM_GB()
		{
			base.CheckJM_GB();

			// For carrier MAWB, branch cannot be empty
			if (Parent.JM_IsPaper)
			{
				MandatoryValidation.CheckEntered(Parent.JM_GBInfo);
			}

			// If branch is not empty then branch must belong to the company
			if (Parent.JM_GC_Company.IsValid && Parent.JM_GB.IsValid && Parent.Branch?.Company != null && Parent.Branch.Company.PK != Parent.JM_GC_Company)
			{
				Parent.JM_GBInfo.AddError($"The branch '{Parent.Branch.GB_BranchName}' does not belong to company '{Parent.Company.GC_Name}'.");
			}
		}

		protected override void CheckJM_GC_Company()
		{
			base.CheckJM_GC_Company();

			// For carrier MAWB, company cannot be empty
			// For neutral MAWB, company cannot be empty if branch is not empty
			if (Parent.JM_IsPaper || Parent.JM_GB.IsValid && Parent.JM_GC_Company.IsEmpty && Parent.Branch?.Company != null)
			{
				MandatoryValidation.CheckEntered(Parent.JM_GC_CompanyInfo);
			}
		}
		#endregion

		#region JM_MAWB

		protected override void CheckJM_MAWB()
		{
			base.CheckJM_MAWB();

			if (!Parent.JM_MAWBInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.JM_MAWBInfo);
				MAWBValidation.CheckMawbCheckDigit(Parent.JM_MAWBInfo);
			}

			if (!Parent.JM_MAWBInfo.HasErrors())
			{
				CheckMawbForAllocation();
			}
		}

		#endregion

		#region JM_Airline3DigitPrefix

		protected override void CheckJM_Airline3DigitPrefix()
		{
			base.CheckJM_Airline3DigitPrefix();

			MandatoryValidation.CheckEntered(Parent.JM_Airline3DigitPrefixInfo);
			RefAirline airline = RefAirline.LoadFromAirlinePrefix(Parent.Factory, Parent.JM_Airline3DigitPrefix);

			if (airline == null)
			{
				Parent.JM_Airline3DigitPrefixInfo.AddError(Res.GetString("d5a0e687-d582-4421-858e-ce371cf7d7ae", "Please enter a valid Airline Code."));
			}

			if (!Parent.JM_Airline3DigitPrefixInfo.HasErrors())
			{
				CheckJM_AirlinePrefixCompanyBranch();
			}
		}

		void CheckMawbForAllocation()
		{
			// For carrier MAWB, don't check
			if (Parent.JM_IsPaper)
			{
				return;
			}

			// In Create New MAWB form, the MAWB will be created then allocate to consol immediately so should check this
			// In Add New MAWB Range form, Edit MAWB form, this should not be checked
			var jobMawb = Parent as JobMawb;
			if (jobMawb?.ShouldValidateAllocation == true)
			{
				mAWBStockManagementStrategy ??= new MAWBStockManagementStrategy(Parent.Factory);
				if (!mAWBStockManagementStrategy.CanAllocateMawbToCurrentBranch(jobMawb, out var stockLevel))
				{
					Parent.JM_MAWBInfo.AddError($"Cannot allocate this MAWB to Branch '{GlbBranch.CurrentBranch.GB_BranchName}' because this branch is not allowed to use {stockLevel} stock.");
				}
			}
		}

		protected virtual void CheckJM_AirlinePrefixCompanyBranch()
		{
			// For carrier MAWB, don't check
			if (Parent.JM_IsPaper)
			{
				return;
			}

			// For neutral MAWB, check company/branch/airline prefix based on MAWB stock management
			mAWBStockManagementStrategy ??= new MAWBStockManagementStrategy(Parent.Factory);
			if (mAWBStockManagementStrategy.CanAddMawb(Parent.JM_Airline3DigitPrefix, Parent.Company, Parent.Branch))
			{
				return;
			}

			var companyName = Parent.Company?.GC_Name;
			var branchName = Parent.Branch?.GB_BranchName;
			var airlineMsg = $"Cannot add MAWB with Airline Code '{Parent.JM_Airline3DigitPrefix}'";

			if (string.IsNullOrEmpty(companyName) && string.IsNullOrEmpty(branchName))
			{
				Parent.JM_Airline3DigitPrefixInfo.AddError($"{airlineMsg} on global level.");
			}
			else if (!string.IsNullOrEmpty(companyName) && string.IsNullOrEmpty(branchName))
			{
				Parent.JM_Airline3DigitPrefixInfo.AddError($"{airlineMsg} for company '{companyName}' on company level.");
			}
			else if (!string.IsNullOrEmpty(companyName) && !string.IsNullOrEmpty(branchName))
			{
				Parent.JM_Airline3DigitPrefixInfo.AddError($"{airlineMsg} for company '{companyName}' and branch '{branchName}'.");
			}
		}
		#endregion

		#region JM_ServiceLevel

		protected override void CheckJM_ServiceLevel()
		{
			base.CheckJM_ServiceLevel();

			if (!Parent.JM_ServiceLevelInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.JM_ServiceLevelInfo);
				ListValidation.ErrorIfInvalidCode(Parent.JM_ServiceLevelInfo);
			}
		}

		#endregion
	}
}
