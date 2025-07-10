using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GLAccountCommonPropertyReadOnlyGetter
	{
		public GLAccountCommonPropertyReadOnlyGetter(IGLAccount gLAccount)
		{
			this.GLAccount = gLAccount;
		}

		protected readonly IGLAccount GLAccount;

		public bool ShouldBeReadOnly(string accountType, string propertyInfo)
		{
			bool result = false;

			if (IGLAccountSchema.ControlAccountInfo == propertyInfo)
			{
				result = (accountType != "BSH");
			}
			else if (IGLAccountSchema.TotalLevelInfo == propertyInfo)
			{
				result = (accountType != "TTL");
			}
			else if (IGLAccountSchema.PercentNumInfo == propertyInfo)
			{
				result = (accountType == "HDR" || accountType == "NTE");
			}
			else if (IGLAccountSchema.ConsolidationNumInfo == propertyInfo)
			{
				result = (accountType == "HDR" || accountType == "CFW" || accountType == "NTE");
			}
			else if (IGLAccountSchema.AlternateNumInfo == propertyInfo)
			{
				result = (accountType != "BSH");
			}
			else if (IGLAccountSchema.HeaderDependsOnTotalInfo == propertyInfo)
			{
				result = (accountType != "HDR");
			}
			else if (IGLAccountSchema.CarriedForwardInfo == propertyInfo)
			{
				result = (accountType != "TTL");
			}
			else if (IGLAccountSchema.StatisticalUnitsInfo == propertyInfo)
			{
				result = (accountType != "NTE");
			}

			return result;
		}

		public void RefreshProperties()
		{
			RefreshOrClearValue(GLAccount.ControlAccountInfo);
			RefreshOrClearValue(GLAccount.TotalLevelInfo);
			RefreshOrClearValue(GLAccount.PercentNumInfo);
			RefreshOrClearValue(GLAccount.ConsolidationNumInfo);
			RefreshOrClearValue(GLAccount.AlternateNumInfo);
			RefreshOrClearValue(GLAccount.HeaderDependsOnTotalInfo);
			RefreshOrClearValue(GLAccount.CarriedForwardInfo);
			RefreshOrClearValue(GLAccount.StatisticalUnitsInfo);
		}

		void RefreshOrClearValue(ZPropertyInfo info)
		{
			if (info != null)
			{
				if (info.ReadOnly)
				{
					info.ClearValue();
				}
				else
				{
					info.RefreshBinding();
				}
			}
		}
	}
}
