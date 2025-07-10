using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLAccountDescriptorValidation : AutoAccGLAccountDescriptorValidation
	{
		public AccGLAccountDescriptorValidation(AutoAccGLAccountDescriptor parent)
			: base(parent)
		{
			this.ParentListInternals = parent;
			this.ZValidationInternals = this;
		}

		public new AccGLAccountDescriptor Parent
		{
			get { return (AccGLAccountDescriptor)base.Parent; }
		}

		public virtual AccGLAccountDescriptorValidationHelper ValidationHelper
		{
			get { return new AccGLAccountDescriptorValidationHelper(Parent, Parent.AJ_Language, Parent.AJ_RN_NKCountryOfCompliance, Parent.Factory); }
		}

		protected override void CheckAJ_Language()
		{
			base.CheckAJ_Language();
			MandatoryValidation.CheckEntered(Parent.AJ_LanguageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AJ_LanguageInfo);

			if (Parent.AJ_LocalAccountNumber != ZString.Empty)
			{
				ValidateAJ_LocalAccountNumber();
			}

			if (Parent.ParentGLHeaderPK != ZGuid.Empty)
			{
				ValidateParentGLHeaderPK();
			}

			if (!Parent.AJ_ReportCategory.IsEmpty)
			{
				ValidateAJ_ReportCategory();
			}
		}

		protected override void CheckAJ_AccountDescription()
		{
			base.CheckAJ_AccountDescription();
			MandatoryValidation.CheckEntered(Parent.AJ_AccountDescriptionInfo);
		}

		protected override void CheckAJ_LocalAccountNumber()
		{
			base.CheckAJ_LocalAccountNumber();
			MandatoryValidation.CheckEntered(Parent.AJ_LocalAccountNumberInfo);

			if (Parent.AJ_LocalAccountNumber != ZString.Empty)
			{
				ValidationHelper.ValidateAccountNumber();
				ValidateAJ_AJ_AlternativeNum();
				ValidateAJ_AJ_PercentNum();
			}
		}

		protected override void CheckAJ_ReportCategory()
		{
			base.CheckAJ_ReportCategory();

			MandatoryValidation.CheckEntered(Parent.AJ_ReportCategoryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AJ_ReportCategoryInfo);

			ValidateOnlyOneCFWType();
		}

		protected override void CheckAJ_DebitCredit()
		{
			AJ_DebitCreditIsValidating = true;
			base.CheckAJ_DebitCredit();

			MandatoryValidation.CheckEntered(Parent.AJ_DebitCreditInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AJ_DebitCreditInfo);

			if (!Parent.AJ_DebitCreditInfo.HasErrors() && !Parent.AJ_DebitCreditInfo.ReadOnly)
			{
				ValidateAlternateAccountDebitCredit(Parent.AJ_DebitCreditInfo);
			}

			if (!AJ_AJ_AlternativeNumIsValidating)
			{
				ValidateAJ_AJ_AlternativeNum();
			}
			AJ_DebitCreditIsValidating = false;
		}

		protected bool AJ_DebitCreditIsValidating;

		protected override void CheckAJ_TotalLevel()
		{
			base.CheckAJ_TotalLevel();

			if (!Parent.AJ_TotalLevelInfo.HasErrors() && !Parent.AJ_TotalLevelInfo.ReadOnly)
			{
				if (Parent.AJ_TotalLevel < 1 || Parent.AJ_TotalLevel > 999)
				{
					Parent.AJ_TotalLevelInfo.AddError(Res.GetString("0cbd0357-93d4-4513-a0bc-ec6a3ff6efb5", "Total Level should be between 1 and 999"));
				}
			}
		}

		protected override void CheckAJ_AJ_ConsolidationNum()
		{
			base.CheckAJ_AJ_ConsolidationNum();

			ValidateAccountType(Parent.AJ_AJ_ConsolidationNum, AccountTypeComboBoxConstants.Consolidation, Parent.AJ_AJ_ConsolidationNumInfo);
			ValidateAJ_LocalAccountNumber();
		}

		protected override void CheckAJ_AJ_AlternativeNum()
		{
			AJ_AJ_AlternativeNumIsValidating = true;
			base.CheckAJ_AJ_AlternativeNum();

			ValidateAccountType(Parent.AJ_AJ_AlternativeNum, AccountTypeComboBoxConstants.Alternate, Parent.AJ_AJ_AlternativeNumInfo);
			ValidateAlternateAccountDebitCredit(Parent.AJ_AJ_AlternativeNumInfo);
			ValidateALTReferredByOtherAccount(Parent.AJ_AJ_AlternativeNum);

			if (!AJ_DebitCreditIsValidating)
			{
				ValidateAJ_DebitCredit();
			}
			AJ_AJ_AlternativeNumIsValidating = false;
		}

		protected void ValidateALTReferredByOtherAccount(ZGuid accountPK)
		{
			if (ValidationHelper.ColumnReferredByMoreThanOneAccount(AccGLAccountDescriptorSchema.AJ_AJ_AlternativeNum, accountPK))
			{
				Parent.AJ_AJ_AlternativeNumInfo.AddError(Res.GetString("04fe87b0-4ad7-45cb-860e-e760dde95d02", "This Alternate Account is already in use. Please select different Account."));
			}
		}

		protected bool AJ_AJ_AlternativeNumIsValidating;

		protected override void CheckAJ_AJ_HeaderDependsOnTotal()
		{
			base.CheckAJ_AJ_HeaderDependsOnTotal();

			ValidateAccountType(Parent.AJ_AJ_HeaderDependsOnTotal, AccountTypeComboBoxConstants.Total, Parent.AJ_AJ_HeaderDependsOnTotalInfo);
			ValidateAJ_LocalAccountNumber();
			ValidateTTLReferredByOtherAccount(Parent.AJ_AJ_HeaderDependsOnTotal);
		}

		protected void ValidateTTLReferredByOtherAccount(ZGuid accountPK)
		{
			if (ValidationHelper.ColumnReferredByMoreThanOneAccount(AccGLAccountDescriptorSchema.AJ_AJ_HeaderDependsOnTotal, accountPK))
			{
				Parent.AJ_AJ_HeaderDependsOnTotalInfo.AddError(Res.GetString("ca3241dc-59c5-42c8-b744-cf1a532e9350", "This Total Reference Account is already in use. Please select different Account."));
			}
		}

		protected override void CheckAJ_AJ_CarriedForwardAccount()
		{
			base.CheckAJ_AJ_CarriedForwardAccount();

			ValidateAccountType(Parent.AJ_AJ_CarriedForwardAccount, AccountTypeComboBoxConstants.CarriedForwardAccount, Parent.AJ_AJ_CarriedForwardAccountInfo);
			ValidateCFWReferredByOtherAccount(Parent.AJ_AJ_CarriedForwardAccount);
		}

		protected void ValidateCFWReferredByOtherAccount(ZGuid accountPK)
		{
			if (ValidationHelper.ColumnReferredByMoreThanOneAccount(AccGLAccountDescriptorSchema.AJ_AJ_CarriedForwardAccount, accountPK))
			{
				Parent.AJ_AJ_CarriedForwardAccountInfo.AddError(Res.GetString("3da61f2a-9bb5-49c9-b9c0-f6a20d9cf0d7", "This Carried Forward Account is already in use. Please select different Account."));
			}
		}

		protected string GetAccountDescription(string type)
		{
			string description = "";
			switch (type)
			{
				case AccountTypeComboBoxConstants.Alternate:
					description = Res.GetString("f9ac0b23-dc20-4eba-b6ba-c4c301b2cc3f", "Alternate");
					break;

				case AccountTypeComboBoxConstants.BalanceSheetAccount:
					description = Res.GetString("f7c729b9-ee44-4e91-a01c-031b659a1f42", "Balance Sheet");
					break;

				case AccountTypeComboBoxConstants.CarriedForwardAccount:
					description = Res.GetString("f5d8e3a9-ebf1-4498-a4e1-f54050298b0f", "Carried Forward");
					break;

				case AccountTypeComboBoxConstants.Consolidation:
					description = Res.GetString("d44a018c-4013-4222-8d87-3543eea03828", "Consolidated");
					break;

				case AccountTypeComboBoxConstants.Header:
					description = Res.GetString("b38e9f22-46e9-4344-b573-939bb0f916d2", "Header");
					break;

				case AccountTypeComboBoxConstants.ProfitAndLossAccount:
					description = Res.GetString("6500d7b8-26e9-4ae1-ada4-ee984649970c", "Profit And Loss");
					break;

				case AccountTypeComboBoxConstants.Total:
					description = Res.GetString("8ccc8939-d0ba-42f0-a3ec-99c0f440c177", "Total Reference");
					break;
			}

			return description;
		}

		protected void ValidateOnlyOneCFWType()
		{
			if (Parent.AJ_ReportCategory == AccountTypeComboBoxConstants.CarriedForwardAccount && ValidationHelper.IsCFWAlreadyDefinedForThisLanguage(Parent.AJ_Language))
			{
				Parent.AJ_ReportCategoryInfo.AddError(Res.GetString("56d2f19b-0756-4eff-bf21-84745e75d2ad", "There can only be only one Carried Forward Reference per each language Code."));
			}
		}

		protected void ValidateAlternateAccountDebitCredit(ZPropertyInfo infoToSetError)
		{
			AccGLAccountDescriptor alternateAccount = Parent.Factory.Load(typeof(AccGLAccountDescriptor), Parent.AJ_AJ_AlternativeNum) as AccGLAccountDescriptor;

			if (alternateAccount != null && alternateAccount.AJ_DebitCredit == Parent.AJ_DebitCredit)
			{
				infoToSetError.AddError(Res.GetString("080687e6-eabb-43c0-b5fc-333d3ac5179d", "Alternate account debit & credit must be opposite to the current setting."));
			}
		}

		protected void ValidateAccountType(ZGuid accountPK, string reportCategory, ZPropertyInfo accountInfo)
		{
			AccGLAccountDescriptor accountToValidate = Parent.Factory.Load(typeof(AccGLAccountDescriptor), accountPK) as AccGLAccountDescriptor;
			if (accountToValidate != null && accountToValidate.AJ_ReportCategory != reportCategory)
			{
				accountInfo.AddError(Res.GetString("c15e4c43-ab34-4a3d-9139-2bdef81d2eb5", "{0} Account must be type of {0}.", GetAccountDescription(reportCategory)));
			}
		}

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				base.ValidateAll();
				ValidateParentGLHeaderPK();
			}
		}

		public void ValidateParentGLHeaderPK()
		{
			ZValidationInternals.Validate(Parent.ParentGLHeaderPKInfo, GetParentGLHeaderPKValidationInvoker());
		}

		RunValidationInvoker GetParentGLHeaderPKValidationInvoker()
		{
			return delegate
			{
				CheckParentGLHeaderPKIsValidZGuid();
				CheckParentGLHeaderPK();
			};
		}

		protected virtual void CheckParentGLHeaderPKIsValidZGuid()
		{
			TypeValidation.CheckValidGuid(Parent.ParentGLHeaderPKInfo);
		}

		public void CheckParentGLHeaderPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.ParentGLHeaderPKInfo);

			if (Parent.AJ_ReportCategory == AccountTypeComboBoxConstants.ProfitAndLossAccount ||
							Parent.AJ_ReportCategory == AccountTypeComboBoxConstants.BalanceSheetAccount)
			{
				if (Parent.AccGLDescriptorPivotCOA == null)
				{
					Parent.ParentGLHeaderPKInfo.AddError(Res.GetString("4c6458eb-2b01-4197-aa81-4e982dee08a3", "This descriptor must reference a global GL account"));
				}
			}
			if (Parent.AccGLDescriptorPivotCOA != null && !Parent.ParentGLHeaderPKInfo.Value.IsEmpty && Parent.ParentGLHeaderPKInfo.Value.IsValid && !Parent.AJ_ReportCategory.IsEmpty)
			{
				ValidateParentAccountNotTheSame();
			}
		}

		protected void ValidateParentAccountNotTheSame()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_Language, SQLComparisonOperator.Equal, Parent.AJ_Language);
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, SQLComparisonOperator.Equal, Parent.AJ_RN_NKCountryOfCompliance);
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_ReportType, SQLComparisonOperator.Equal, Parent.AJ_ReportType);
			query.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_ReportCategory, SQLComparisonOperator.Equal, Parent.AJ_ReportCategory);
			ZDBOnlySubQuery accGLDescriptorPivotSubQuery = new ZDBOnlySubQuery(typeof(AutoAccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AJ);

			accGLDescriptorPivotSubQuery.AddToFilter(AccGLDescriptorPivotSchema.YJ_AG, Parent.ParentGLHeaderPK);
			query.AddSubQuery(AccGLAccountDescriptorSchema.PK, accGLDescriptorPivotSubQuery, JoinCondition.And);

			AccGLAccountDescriptor nonUniqueDescriptor = (AccGLAccountDescriptor)Parent.Factory.LoadTop1(typeof(AccGLAccountDescriptor), query);

			if (nonUniqueDescriptor != null)
			{
				Parent.ParentGLHeaderPKInfo.AddError(Res.GetString("4c2f3fbb-5d83-4101-9056-46e7dc69af49", "Please choose another GL Account as this one is already referenced by another Local Account for the current language."));
			}
		}

		#region Implementation

		readonly ISingleElementListInternal ParentListInternals;
		readonly IValidationInternals ZValidationInternals;

		#endregion
	}
}
