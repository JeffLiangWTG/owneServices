using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccountFeeSettingsValidation : ZValidation
	{
		public AccountFeeSettingsValidation(AccountFeeSettings accFeeSettings)
			: base(accFeeSettings)
		{
			this.ParentBizO = accFeeSettings;
			this.ZValidationInternals = this;
		}

		public override Type AutoValidationType
		{
			get { throw new NotImplementedException(); }
		}

		public override void ValidateAll()
		{
			ValidateAAF_AG_GLAccount();
			ValidateAAF_FeeAmount();
			ValidateAAF_Rule();
			ValidateAAF_RX_NKFeeCurrency();
		}

		#region AAF_AG_GLAccount
		public void ValidateAAF_AG_GLAccount()
		{
			ZValidationInternals.Validate(ParentBizO.AAF_AG_GLAccountInfo, GetAAF_AG_GLAccountValidationInvoker());
		}
		RunValidationInvoker GetAAF_AG_GLAccountValidationInvoker()
		{
			return delegate
			{
				CheckAAF_AG_GLAccount();
			};
		}
		public void CheckAAF_AG_GLAccount()
		{
			ParentBizO.AAF_AG_GLAccountInfo.ClearAllNotifications();
			if (ParentBizO.OverrideSettings)
			{
				AccAccountFeeValidationHelper.ValidateAAF_AG_GLAccount(ParentBizO.AAF_AG_GLAccountInfo);
			}
		}
		#endregion

		#region AAF_FeeAmount
		public void ValidateAAF_FeeAmount()
		{
			ZValidationInternals.Validate(ParentBizO.AAF_FeeAmountInfo, GetAAF_FeeAmountValidationInvoker());
		}
		RunValidationInvoker GetAAF_FeeAmountValidationInvoker()
		{
			return delegate
			{
				CheckAAF_FeeAmount();
			};
		}
		public void CheckAAF_FeeAmount()
		{
			ParentBizO.AAF_FeeAmountInfo.ClearAllNotifications();
			if (ParentBizO.OverrideSettings)
			{
				AccAccountFeeValidationHelper.ValidateAAF_FeeAmount(ParentBizO.AAF_FeeAmountInfo, ParentBizO.AAF_FeeAmount, ParentBizO.AAF_Rule);
			}
		}
		#endregion

		#region AAF_Rule
		public void ValidateAAF_Rule()
		{
			ZValidationInternals.Validate(ParentBizO.AAF_RuleInfo, GetAAF_RuleValidationInvoker());
		}
		RunValidationInvoker GetAAF_RuleValidationInvoker()
		{
			return delegate
			{
				CheckAAF_Rule();
			};
		}
		public void CheckAAF_Rule()
		{
			ParentBizO.AAF_RuleInfo.ClearAllNotifications();
			if (ParentBizO.OverrideSettings)
			{
				AccAccountFeeValidationHelper.ValidateAAF_Rule(ParentBizO.AAF_RuleInfo, ParentBizO.Lookups.AccountFeeCalculationRuleList);
			}
		}
		#endregion

		#region AAF_RX_NKFeeCurrency
		public void ValidateAAF_RX_NKFeeCurrency()
		{
			ZValidationInternals.Validate(ParentBizO.AAF_RX_NKFeeCurrencyInfo, GetAAF_RX_NKFeeCurrencyValidationInvoker());
		}
		RunValidationInvoker GetAAF_RX_NKFeeCurrencyValidationInvoker()
		{
			return delegate
			{
				CheckAAF_RX_NKFeeCurrency();
			};
		}
		public void CheckAAF_RX_NKFeeCurrency()
		{
			ParentBizO.AAF_RX_NKFeeCurrencyInfo.ClearAllNotifications();
			if (ParentBizO.OverrideSettings)
			{
				AccAccountFeeValidationHelper.ValidateAAF_RX_NKFeeCurrency(ParentBizO.AAF_RX_NKFeeCurrencyInfo, ParentBizO.Lookups.FeeCurrencies);
			}
		}
		#endregion

		protected readonly AccountFeeSettings ParentBizO;
		readonly IValidationInternals ZValidationInternals;
	}
}
