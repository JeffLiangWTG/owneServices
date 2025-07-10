//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxConfigurationValidation
//
//    This class should be used for overriding validation in AutoAccTaxConfigurationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxConfigurationValidation : AutoAccTaxConfigurationValidation
	{
		public AccTaxConfigurationValidation(AutoAccTaxConfiguration parent)
			: base(parent)
		{
		}

		new AccTaxConfiguration Parent => (AccTaxConfiguration)base.Parent;

		protected override void CheckETC_RecoveryMethod()
		{
			base.CheckETC_RecoveryMethod();

			MandatoryValidation.CheckEntered(Parent.ETC_RecoveryMethodInfo);
			if (!Parent.ETC_RecoveryMethod.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ETC_RecoveryMethodInfo);
			}

			var recoveryMethodToCheck = TaxRecoveryMethods.RecoverTaxExpense.Code;
			if (Parent.ETC_RecoveryMethod == recoveryMethodToCheck)
			{
				var taxSystem = Parent.TaxSystem;
				var validLedger = TaxConfigurationLedgers.AccountsReceivable.Code;
				var validSuperType = TaxSuperTypeList.TurnoverTax.Code;
				if (Parent.ETC_Ledger != validLedger || taxSystem == null || taxSystem.TaxSuperType != validSuperType || taxSystem.IncludeInInvoceTotal)
				{
					Parent.ETC_RecoveryMethodInfo.AddError(Res.GetString("C80F5BCF-5492-46FD-A461-CAAF3F56081D", "{0} is only permitted when Ledger = {1} and Super type = {2} and Include in Invoice = NO", recoveryMethodToCheck, validLedger, validSuperType));
				}
			}
		}

		protected override void CheckETC_Description()
		{
			base.CheckETC_Description();

			MandatoryValidation.CheckEntered(Parent.ETC_DescriptionInfo);
		}

		protected override void CheckETC_TaxAuthorityCode()
		{
			base.CheckETC_TaxAuthorityCode();

			MandatoryValidation.CheckEntered(Parent.ETC_TaxAuthorityCodeInfo);
			if (!Parent.ETC_TaxAuthorityCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ETC_TaxAuthorityCodeInfo);
			}
		}

		protected override void CheckETC_TaxSystemCode()
		{
			base.CheckETC_TaxSystemCode();

			MandatoryValidation.CheckEntered(Parent.ETC_TaxSystemCodeInfo);
			if (!Parent.ETC_TaxSystemCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ETC_TaxSystemCodeInfo);
			}
		}

		protected override void CheckETC_Ledger()
		{
			base.CheckETC_Ledger();

			MandatoryValidation.CheckEntered(Parent.ETC_LedgerInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETC_LedgerInfo);

			if (GlbStaff.CurrentUser != null && !GlbStaff.CurrentUser.IsSupportUser && Parent.IsSPR_AR_Configuration())
			{
				Parent.ETC_LedgerInfo.AddError(Res.GetString("2295acf5-9122-4a11-ad35-77acab11f11c", "CargoWise does not support Tax Configuration with Ledger AR for a SPR Tax Type. You can only create a Tax Configuration with Ledger AP for a SPR type tax."));
			}
		}

		protected override void CheckETC_TaxRealisationMethod()
		{
			base.CheckETC_TaxRealisationMethod();

			MandatoryValidation.CheckEntered(Parent.ETC_TaxRealisationMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETC_TaxRealisationMethodInfo);

			var taxSystem = Parent.TaxSystem;
			var superType = taxSystem?.TaxSuperType.ToString();
			if (Parent.ETC_TaxRealisationMethod == TaxRealisationMethods.MatchDate.Code
				&& ((superType == TaxSuperTypeList.SalesTax.Code && Parent.ETC_Ledger == TaxConfigurationLedgers.AccountsPayable.Code)
				|| (superType == TaxSuperTypeList.TurnoverTax.Code && Parent.ETC_Ledger == TaxConfigurationLedgers.AccountsReceivable.Code)))
			{
				Parent.ETC_TaxRealisationMethodInfo.AddError(Res.GetString("DBA07B11-E46F-4E4E-927A-A7ADCA1D6A39", "'{0}' is not valid value for entered tax system and ledger combination.", TaxRealisationMethods.MatchDate.Code));
			}

			if (Parent.ETC_TaxRealisationMethod == TaxRealisationMethods.PostDateOfMatchTransaction.Code && !Parent.IsSPR_AP_Configuration())
			{
				Parent.ETC_TaxRealisationMethodInfo.AddError(Res.GetString("D515A960-7B8E-41BF-B0B9-1A324FCB91E0", "'{0}' realization method is allowed only for '{1}' super type tax system and {2} ledger.", TaxRealisationMethods.PostDateOfMatchTransaction.Code, TaxSuperTypeList.StandardPaymentRetention.Code, TaxConfigurationLedgers.AccountsPayable.Code));
			}
			else if (Parent.ETC_TaxRealisationMethod != TaxRealisationMethods.PostDateOfMatchTransaction.Code && Parent.IsSPR_AP_Configuration())
			{
				Parent.ETC_TaxRealisationMethodInfo.AddError(Res.GetString("19F2FDC6-2674-4320-A90D-1107DE3F2A8D", "Only '{0}' realization method is allowed for '{1}' super type tax system and {2} ledger.", TaxRealisationMethods.PostDateOfMatchTransaction.Code, TaxSuperTypeList.StandardPaymentRetention.Code, TaxConfigurationLedgers.AccountsPayable.Code));
			}
		}

		protected override void CheckETC_TaxAmountRounding()
		{
			base.CheckETC_TaxAmountRounding();

			MandatoryValidation.CheckEntered(Parent.ETC_TaxAmountRoundingInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETC_TaxAmountRoundingInfo);
		}

		protected override void CheckETC_TaxRecordCreationTrigger()
		{
			base.CheckETC_TaxRecordCreationTrigger();

			MandatoryValidation.CheckEntered(Parent.ETC_TaxRecordCreationTriggerInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETC_TaxRecordCreationTriggerInfo);

			if (Parent.ETC_TaxRecordCreationTrigger != TaxRecordCreationTrigger.PostDate.Code && Parent.IsSPR_AP_Configuration())
			{
				Parent.ETC_TaxRecordCreationTriggerInfo.AddError(Res.GetString("B336CF6E-5499-43D2-93FC-A58B8A480C3B", "Only '{0}' creation trigger is allowed for '{1}' super type tax system and {2} ledger.", TaxRecordCreationTrigger.PostDate.Code, TaxSuperTypeList.StandardPaymentRetention.Code, TaxConfigurationLedgers.AccountsPayable.Code));
			}
		}

		protected override void CheckETC_CancellationPolicy()
		{
			base.CheckETC_CancellationPolicy();

			MandatoryValidation.CheckEntered(Parent.ETC_CancellationPolicyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETC_CancellationPolicyInfo);
		}

		protected override void CheckETC_RN_NKCountry()
		{
			base.CheckETC_RN_NKCountry();

			ZString country = AccTaxConfiguration.GetCountryFromCompany(Parent.ParentCompany);
			if (country.IsEmpty)
			{
				country = AccTaxConfiguration.GetCountryFromBranch(Parent.ParentBranch);
			}
			if (!country.IsEmpty && country != Parent.ETC_RN_NKCountry)
			{
				Parent.ETC_RN_NKCountryInfo.AddError(Res.GetString("7117AE83-DC41-4045-9ED3-98805B23FF6F", "Country/Region must be the same as Company country/region."));
			}
		}

		protected override void CheckETC_AG_LedgerControlAccount()
		{
			base.CheckETC_AG_LedgerControlAccount();
			CheckGLAccount(Parent.ETC_AG_LedgerControlAccountInfo, Parent.GLAccRegTypes.LedgerControlAccount);
		}

		protected override void CheckETC_AG_TaxControlAccount()
		{
			base.CheckETC_AG_TaxControlAccount();
			CheckGLAccount(Parent.ETC_AG_TaxControlAccountInfo, Parent.GLAccRegTypes.TaxControlAccount);
		}

		protected override void CheckETC_AG_TaxExpenseAccount()
		{
			base.CheckETC_AG_TaxExpenseAccount();
			CheckGLAccount(Parent.ETC_AG_TaxExpenseAccountInfo, Parent.GLAccRegTypes.TaxExpenseAccount);
		}

		protected override void CheckETC_AG_TaxPendingControlAccount()
		{
			base.CheckETC_AG_TaxPendingControlAccount();
			CheckGLAccount(Parent.ETC_AG_TaxPendingControlAccountInfo, Parent.GLAccRegTypes.TaxPendingControlAccount);
		}

		void CheckGLAccount(ZPropertyInfo propertyInfo, GLAccountRegistryType gLAccountRegistryType)
		{
			if (gLAccountRegistryType == GLAccountRegistryType.Error)
			{
				propertyInfo.AddError(Res.GetString("A70ED1B4-DDFD-47D7-A6FB-B3A3189A75FF", "GL Account cannot be defined for this configuration."));
			}
			else if (gLAccountRegistryType != GLAccountRegistryType.NotApplicable)
			{
				MandatoryValidation.CheckEntered(propertyInfo);
				ListValidation.ErrorIfInvalidPK(propertyInfo);
			}
		}

		#region ValidateThresholdMethod

		protected override void CheckETC_ThresholdMethod()
		{
			base.CheckETC_ThresholdMethod();
			MandatoryValidation.CheckEntered(Parent.ETC_ThresholdMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETC_ThresholdMethodInfo);
		}

		protected override void CheckETC_ThresholdAmount()
		{
			base.CheckETC_ThresholdAmount();
			if (!Parent.ETC_ThresholdMethodInfo.HasErrors() && Parent.ETC_ThresholdMethod != ETC_ThresholdMethods.NoThreshold.Code && Parent.ETC_ThresholdAmount <= 0)
			{
				var thresholdMethods = new ETC_ThresholdMethods();
				Parent.ETC_ThresholdAmountInfo.AddError(Res.GetString("34924C04-9605-4980-B15B-6FF6948520E8", @"The Threshold Amount of a Tax Configuration must be a number greater than Zero when the Threshold Method is '{0}'.
Please review and update the Threshold Method and Threshold Amount values before saving.", ((CodeDescriptionPair)thresholdMethods[Parent.ETC_ThresholdMethod]).CodeAndDescription));
			}
			if (!Parent.ETC_ThresholdAmountInfo.HasErrors() && Parent.ETC_ThresholdMethod == ETC_ThresholdMethods.TransactionLevelGroup.Code)
			{
				var ledger = Parent.ETC_Ledger;
				var company = Parent.Company;
				var branch = Parent.ParentBranch;
				var taxConfigurations = Parent.ETC_ParentTableCode == GlbBranchSchema.Constants.Prefix ? branch?.AccTaxConfigurations : company?.AccTaxConfigurations;
				if (taxConfigurations != null && !taxConfigurations.Where(item => item.ETC_Ledger == ledger && item.ETC_ThresholdMethod == Parent.ETC_ThresholdMethod).All(item2 => item2.ETC_ThresholdAmount == Parent.ETC_ThresholdAmount))
				{
					Parent.ETC_ThresholdAmountInfo.AddError(Res.GetString("7F9B11C0-8747-4A3B-8349-1260B3DDD4A3", @"All Tax Configurations with GRP must have the same Threshold Amount configured.Please review and update the Threshold Method and Threshold Amount values before saving."));
				}
			}
		}

		#endregion
	}
}
