//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccAPAccountDetailsValidation
//
//    This class should be used for overriding validation in AutoAccAPAccountDetailsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAPAccountDetailsValidation : AutoAccAPAccountDetailsValidation
	{
		public AccAPAccountDetailsValidation(AutoAccAPAccountDetails parent)
			: base(parent)
		{
			this.Parent = (AccAPAccountDetails)parent;
		}

		readonly new AccAPAccountDetails Parent;

		protected override void CheckA1_EPaymentReferenceType()
		{
			base.CheckA1_EPaymentReferenceType();

			if (!AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				CheckEPaymentRelatedPropertiesAreEmptyWhenEPaymentFunctionalityIsDisabled();
			}
			else if (Parent.A1_PaymentMethod == EPaymentMethods.EPaymentViaOFX)
			{
				ListValidation.ErrorIfInvalidCode(Parent.A1_EPaymentReferenceTypeInfo);

				if (Parent.A1_EPaymentReferenceType.IsEmpty)
				{
					Parent.A1_EPaymentReferenceTypeInfo.AddError(AccountingMasterFilesConstants.ValidationErrorMessages.EPaymentReferenceTypeCannotBeEmptyErrorMessage);
				}
			}
		}

		protected override void CheckA1_EPaymentReference()
		{
			base.CheckA1_EPaymentReference();

			if (!AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				CheckEPaymentRelatedPropertiesAreEmptyWhenEPaymentFunctionalityIsDisabled();
			}
			else if (Parent.A1_PaymentMethod == EPaymentMethods.EPaymentViaOFX && Parent.A1_EPaymentReferenceType == EPaymentReferenceTypes.FreeText)
			{
				MandatoryValidation.CheckEntered(Parent.A1_EPaymentReferenceInfo);
			}
		}

		protected override void CheckA1_EPaymentReasonCode()
		{
			base.CheckA1_EPaymentReasonCode();
			if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				ListValidation.ErrorIfInvalidCode(Parent.A1_EPaymentReasonCodeInfo);
			}
			else
			{
				CheckEPaymentRelatedPropertiesAreEmptyWhenEPaymentFunctionalityIsDisabled();
			}
		}

		protected override void CheckA1_EPaymentBeneficiaryId()
		{
			base.CheckA1_EPaymentBeneficiaryId();

			if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				Parent.RemoveRowError(ErrorForBeneficiaryIdWhenEPaymentFunctionalityIsEnabled);
				Parent.RemoveRowWarning(WarningForBeneficiaryId);
				if (Parent.A1_EPaymentBeneficiaryId.IsEmpty && Parent.A1_PaymentMethod == EPaymentMethods.EPaymentViaOFX)
				{
					Parent.AddRowWarning(WarningForBeneficiaryId);
				}
				else if (!Parent.A1_EPaymentBeneficiaryId.IsEmpty && Parent.A1_PaymentMethod != EPaymentMethods.EPaymentViaOFX)
				{
					Parent.AddRowError(ErrorForBeneficiaryIdWhenEPaymentFunctionalityIsEnabled);
				}
				Parent.RemoveRowError(ErrorForBeneficiaryIdHasBeenUsedMoreThanOnce);
				if (!Parent.A1_EPaymentBeneficiaryIdInfo.HasErrors() && !Parent.A1_EPaymentBeneficiaryId.IsEmpty)
				{
					var query = new ZQuery(AccAPAccountDetailsSchema.A1_EPaymentBeneficiaryId, Parent.A1_EPaymentBeneficiaryId);
					query.AddToFilter(AccAPAccountDetailsSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					if (Parent.Factory.Exists(typeof(AccAPAccountDetails), query))
					{
						Parent.AddRowError(ErrorForBeneficiaryIdHasBeenUsedMoreThanOnce);
					}
				}
			}
			else
			{
				CheckEPaymentRelatedPropertiesAreEmptyWhenEPaymentFunctionalityIsDisabled();
			}
		}

		void CheckEPaymentRelatedPropertiesAreEmptyWhenEPaymentFunctionalityIsDisabled()
		{
			Parent.RemoveRowError(ErrorForEPaymentAccountsWhenEPaymentFunctionalityIsDisabled);
			if (!Parent.A1_EPaymentBeneficiaryId.IsEmpty ||
				!Parent.A1_EPaymentReasonCode.IsEmpty ||
				!Parent.A1_EPaymentReference.IsEmpty ||
				!Parent.A1_EPaymentReferenceType.IsEmpty)
			{
				Parent.AddRowError(ErrorForEPaymentAccountsWhenEPaymentFunctionalityIsDisabled);
			}
		}

		string ErrorForEPaymentAccountsWhenEPaymentFunctionalityIsDisabled => Res.GetString("c174919d-f2dd-48e2-a739-1888790c7eeb", "E-Payments Functionality has been disabled. Please remove this account.");

		string WarningForBeneficiaryId => Res.GetString("0bdb37ba-2aac-410e-9e34-a2e4517846c7", "To link this account with the OFX Beneficiary, right-click and select Config for E-Payment");

		string ErrorForBeneficiaryIdWhenEPaymentFunctionalityIsEnabled => Res.GetString("1ca9f6e4-a5e8-4eb6-9d1e-2d5edb69d680", "Provider Reference can be recorded only against bank accounts with EPO payment method");

		string ErrorForBeneficiaryIdHasBeenUsedMoreThanOnce => Res.GetString("62c6e319-2539-4fd3-a243-230c8bd7bc41", "The beneficiary has already been linked to another account");

		protected override void CheckA1_BankSwift()
		{
			base.CheckA1_BankSwift();
			if (!Parent.A1_BankSwiftInfo.HasErrors() && !Parent.A1_BankSwift.IsEmpty)
			{
				if (!AccValidationHelper.CheckBankSWIFT(Parent.A1_BankSwift))
				{
					Parent.A1_BankSwiftInfo.AddError(Res.GetString("32adb668-cc1e-4091-95fa-b80f4f74a6ef", "{0} is not a valid SWIFT Code.", Parent.A1_BankSwift));
				}
			}
		}

		protected override void CheckA1_IBANNumber()
		{
			base.CheckA1_IBANNumber();
			if (!Parent.A1_IBANNumberInfo.HasErrors() && !Parent.A1_IBANNumber.IsEmpty)
			{
				AccValidationHelper.CheckBankIBAN(Parent.A1_IBANNumberInfo, Parent.A1_RN_NKCountryCode);
			}

			if (!Parent.A1_IBANNumberInfo.HasErrors())
			{
				AccValidationHelper.CheckIbanForEUCountry(Parent.Factory, Parent.A1_IBANNumberInfo,
					Parent.A1_RN_NKCountryCode);
			}
		}

		protected override void CheckA1_RN_NKCountryCode()
		{
			base.CheckA1_RN_NKCountryCode();
			ListValidation.ErrorIfInvalidCode(Parent.A1_RN_NKCountryCodeInfo);
		}

		protected override void CheckA1_RX_NKAccountCurrency()
		{
			base.CheckA1_RX_NKAccountCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.A1_RX_NKAccountCurrencyInfo);

			if (Parent.CompanyData.OB_IsCreditor)
			{
				MandatoryValidation.CheckEntered(Parent.A1_RX_NKAccountCurrencyInfo);
			}
			ValidateA1_IsDefaultAccount();
		}

		protected override void CheckA1_PaymentMethod()
		{
			base.CheckA1_PaymentMethod();
			if (Parent.CompanyData.OB_IsCreditor)
			{
				MandatoryValidation.CheckEntered(Parent.A1_PaymentMethodInfo);
				ListValidation.ErrorIfInvalidCode(Parent.A1_PaymentMethodInfo);
			}
			ValidateA1_IsDefaultAccount();
			ValidateA1_EPaymentBeneficiaryId();
		}

		protected override void CheckA1_IsDefaultAccount()
		{
			if (Parent.CompanyData.OB_IsCreditor && !Parent.A1_PaymentMethod.IsEmpty && !Parent.A1_RX_NKAccountCurrency.IsEmpty)
			{
				base.CheckA1_IsDefaultAccount();
				ZString errorMessage = ZString.Empty;
				if (Parent.A1_IsDefaultAccount)
				{
					errorMessage = Res.GetString("7b8acf55-5478-4ca1-a162-c6ba09860ab5", "You have more than one account listed as the default account for the currency \"{0}\" and the payment type \"{1}\".\r\n\r\nPlease check which account really should be the default, and untick the \"Is Default Account\" flag of any others.", Parent.A1_RX_NKAccountCurrency, Parent.Lookups.A1_PaymentMethod_List.GetDescriptionFromCode(Parent.A1_PaymentMethod));
				}
				else
				{
					errorMessage = Res.GetString("70d92829-92d5-4692-9f3b-36deff0721ad", "This account must be the default because there is no other account listed with currency \"{0}\" and payment type \"{1}\" and marked as \"Is Default Account\".", Parent.A1_RX_NKAccountCurrency, Parent.Lookups.A1_PaymentMethod_List.GetDescriptionFromCode(Parent.A1_PaymentMethod));
				}
				SetErrorMessage(errorMessage);
			}
		}

		void SetErrorMessage(ZString errorMessage)
		{
			if (MoreThanOneAccountDetailsFound == Parent.A1_IsDefaultAccount)
			{
				Parent.A1_IsDefaultAccountInfo.AddError(errorMessage);
			}
		}

		bool MoreThanOneAccountDetailsFound
		{
			get
			{
				bool result = false;
				foreach (AccAPAccountDetails accDetails in Parent.CompanyData.AccountDetailsCollection)
				{
					if (accDetails.PaymentMethod == Parent.PaymentMethod && accDetails.A1_RX_NKAccountCurrency == Parent.A1_RX_NKAccountCurrency && accDetails.A1_IsDefaultAccount && accDetails.PK != Parent.PK)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public void ValidateAllIsDefaultAccount()
		{
			foreach (AccAPAccountDetails accDetails in Parent.CompanyData.AccountDetailsCollection)
			{
				accDetails.Validation.ValidateA1_IsDefaultAccount();
			}
		}

		protected override void CheckA1_BankAccount()
		{
			base.CheckA1_BankAccount();
			if (Parent.PaymentMethod == ZArchitecture.Core.ReceiptTypes.DirectDebit)
			{
				CheckEntered(Parent.A1_BankAccountInfo);
			}
		}

		protected override void CheckA1_BankBsb()
		{
			base.CheckA1_BankBsb();
			if (Parent.PaymentMethod == ZArchitecture.Core.ReceiptTypes.DirectDebit)
			{
				CheckEntered(Parent.A1_BankBsbInfo);
			}
		}

		protected override void CheckA1_BankName()
		{
			base.CheckA1_BankName();
			if (Parent.PaymentMethod == ZArchitecture.Core.ReceiptTypes.DirectDebit)
			{
				CheckEntered(Parent.A1_BankNameInfo);
			}
		}

		protected override void CheckA1_AccountName()
		{
			base.CheckA1_AccountName();
			if (Parent.PaymentMethod == ZArchitecture.Core.ReceiptTypes.DirectDebit)
			{
				CheckEntered(Parent.A1_AccountNameInfo);
			}
		}

		void CheckEntered(ZPropertyInfo propertyInfo)
		{
			if (Parent.CompanyData.OB_IsCreditor)
			{
				MandatoryValidation.CheckEntered(propertyInfo);
			}
		}
	}
}
