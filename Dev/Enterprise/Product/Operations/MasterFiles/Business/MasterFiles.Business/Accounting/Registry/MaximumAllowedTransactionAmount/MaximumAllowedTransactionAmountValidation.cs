using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class MaximumAllowedTransactionAmountValidation
	{
		public MaximumAllowedTransactionAmountValidation(MaximumAllowedTransactionAmount parent)
		{
			Parent = parent;
		}
		readonly MaximumAllowedTransactionAmount Parent;

		public void ValidateAll()
		{
			ValidateMaximumAllowedHeaderAmount();
			ValidateMaximumAllowedLineAmount();
		}

		public void ValidateMaximumAllowedHeaderAmount()
		{
			Parent.MaximumAllowedHeaderAmountInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.MaximumAllowedHeaderAmountInfo);

			if (Parent.ShouldCheckMaximumSettingExceedSystemDefined && Parent.CurrentFallbackLevel != null)
			{
				var systemDefined = AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.GetFallBackValueAtAllLevels(Parent.CurrentFallbackLevel.CompanyPK(false), Parent.CurrentFallbackLevel.BranchPK, Parent.CurrentFallbackLevel.DepartmentPK).MaximumAllowedHeaderAmount;

				if (Parent.MaximumAllowedHeaderAmount > systemDefined)
				{
					Parent.MaximumAllowedHeaderAmountInfo.AddError(ResString.GetMultilingualString("f2624dae-3835-4bf7-8693-801d8287c96d", "The maximum allowed header amount must not be more than {0}.", systemDefined));
				}
			}
		}

		public void ValidateMaximumAllowedLineAmount()
		{
			Parent.MaximumAllowedLineAmountInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.MaximumAllowedLineAmountInfo);

			if (Parent.ShouldCheckMaximumSettingExceedSystemDefined && Parent.CurrentFallbackLevel != null)
			{
				var systemDefined = AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.GetFallBackValueAtAllLevels(Parent.CurrentFallbackLevel.CompanyPK(false), Parent.CurrentFallbackLevel.BranchPK, Parent.CurrentFallbackLevel.DepartmentPK).MaximumAllowedLineAmount;

				if (Parent.MaximumAllowedLineAmount > systemDefined)
				{
					Parent.MaximumAllowedLineAmountInfo.AddError(ResString.GetMultilingualString("f3c01a32-819c-4d23-8672-01374bae1962", "The maximum allowed line amount must not be more than {0}.", systemDefined));
				}
			}
		}
	}
}
