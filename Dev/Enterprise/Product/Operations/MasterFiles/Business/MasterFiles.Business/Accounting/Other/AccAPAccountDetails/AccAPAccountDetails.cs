using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class AccAPAccountDetails : AutoAccAPAccountDetails, IDataVersionLoggingSupported
	{
		public AccAPAccountDetails(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		public static ZString GetDefaultPaymentMethod(ZString inputPaymentMethod)
		{
			return inputPaymentMethod != AccAPAccountDetailsLookups.DefaultPayment ? inputPaymentMethod : (ZString)OrganisationsDataRegistry.Instance.APPaymentMethod.Value;
		}

		public ZString PaymentMethod
		{
			get { return GetDefaultPaymentMethod(A1_PaymentMethod); }
		}

		public bool AutoDirectDebit
		{
			get { return !A1_AccountName.IsEmpty && !A1_BankAccount.IsEmpty && !A1_BankBsb.IsEmpty && PaymentMethod == ZArchitecture.Core.ReceiptTypes.DirectDebit; }
		}

		protected virtual bool ModifyAccountDetailsSecurity
		{
			get { return CompanyData.Header.SecurityProvider.HasModifyPayablesAccountDetailsSecurity; }
		}

		public AccEPaymentBeneficiary EPaymentBeneficiary => Factory.Load<AccEPaymentBeneficiary>(A1_EPaymentBeneficiaryId);

		public bool ModifyPayableAccountDetailsEPaymentSecurity
		{
			get { return CompanyData.Header.SecurityProvider.HasModifyPayablesAccountDetailsEPaymentSecurity; }
		}

		#endregion

		bool IsElectronicPaymentMethod => A1_PaymentMethod == EPaymentMethods.EPaymentViaOFX;

		bool IsEPaymentFunctionalityEnabled => AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider;

		#region Overriden Properties

		[List("Lookups.A1_EPaymentReferenceType_List")]
		public override ZString A1_EPaymentReferenceType
		{
			get
			{
				return base.A1_EPaymentReferenceType;
			}
			set
			{
				base.A1_EPaymentReferenceType = value;
				UpdateEPaymentReference();
			}
		}

		public bool A1_EPaymentReferenceType_ReadOnly => !IsEPaymentFunctionalityEnabled;

		void UpdateEPaymentReference()
		{
			if (A1_EPaymentReferenceType != EPaymentReferenceTypes.FreeText)
			{
				A1_EPaymentReference = ZString.Empty;
			}
		}
		public bool A1_EPaymentReference_ReadOnly => !IsEPaymentFunctionalityEnabled || A1_EPaymentReferenceType != EPaymentReferenceTypes.FreeText;
		
		[List("Lookups.A1_EPaymentReasonCode_List")]
		public override ZString A1_EPaymentReasonCode
		{
			get
			{
				return base.A1_EPaymentReasonCode;
			}
			set
			{
				base.A1_EPaymentReasonCode = value;
			}
		}

		public bool A1_EPaymentReasonCode_ReadOnly => !IsEPaymentFunctionalityEnabled;

		[List("Lookups.A1_PaymentMethod_List")]
		public override ZString A1_PaymentMethod
		{
			get
			{
				return base.A1_PaymentMethod;
			}
			set
			{
				base.A1_PaymentMethod = value;
				UpdateEPaymentReason();
				UpdateEPaymentReferences();
			}
		}

		void UpdateEPaymentReferences()
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				return;
			}

			var defaultValue = EPaymentPropertyHelper.GetDefaultEPaymentReferenceForEPaymentMethod(A1_PaymentMethod);
			A1_EPaymentReferenceType = defaultValue?.ReferenceType ?? ZString.Empty;
			A1_EPaymentReference = defaultValue?.Reference ?? ZString.Empty;
		}

		void UpdateEPaymentReason()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				if (A1_PaymentMethod != EPaymentMethods.EPaymentViaOFX)
				{
					A1_EPaymentReasonCode = ZString.Empty;
				}
				else if (!IsInDatabase || (ZString)A1_PaymentMethodInfo.OriginalValue != EPaymentMethods.EPaymentViaOFX)
				{
					A1_EPaymentReasonCode = EPaymentPropertyHelper.GetDefaultPaymentReasonForEPaymentMethod(Env.CurrentCompanyPK, Env.CurrentBranchPK, A1_PaymentMethod);
				}
				else
				{
					A1_EPaymentReasonCode = (ZString)A1_EPaymentReasonCodeInfo.OriginalValue;
				}
			}
		}

		#region A1_IsDefaultAccount

		public override ZBool A1_IsDefaultAccount
		{
			get { return base.A1_IsDefaultAccount; }
			set
			{
				base.A1_IsDefaultAccount = value;
				ValidateIsDefaultAccountForAllAccounts();
			}
		}

		public void ValidateIsDefaultAccountForAllAccounts()
		{
			foreach (AccAPAccountDetails accDetails in CompanyData.AccountDetailsCollection)
			{
				accDetails.Validation.ValidateA1_IsDefaultAccount();
			}
		}

		#endregion

		[List("Lookups.AccountCurrencies")]
		public override ZString A1_RX_NKAccountCurrency
		{
			get
			{
				return base.A1_RX_NKAccountCurrency;
			}
			set
			{
				base.A1_RX_NKAccountCurrency = value;
			}
		}

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003", Justification = "Expect field length will be truncated")]
		public void SetAccountDetailsValuesFromBeneficiary(AccEPaymentBeneficiary beneficiary)
		{
			if (beneficiary != null)
			{
				A1_EPaymentBeneficiaryId = beneficiary.PK;
				A1_AccountName = NormaliseString(AccAPAccountDetailsSchema.A1_AccountName.MaxLength, beneficiary.ABF_BeneficiaryFullName);
				A1_RX_NKAccountCurrency = beneficiary.ABF_RX_NKAccountCurrency;
				A1_RN_NKCountryCode = beneficiary.ABF_RN_NKCountryCode;
				A1_BankName = NormaliseString(AccAPAccountDetailsSchema.A1_BankName.MaxLength, beneficiary.ABF_BankName);
				A1_BankBranchName = NormaliseString(AccAPAccountDetailsSchema.A1_BankBranchName.MaxLength, beneficiary.ABF_BankBranchName);
				A1_BankBsb = NormaliseString(AccAPAccountDetailsSchema.A1_BankBsb.MaxLength, beneficiary.ABF_BankBsb);
				A1_BankAccount = NormaliseString(AccAPAccountDetailsSchema.A1_BankAccount.MaxLength, beneficiary.ABF_BankAccount);
				A1_BankSwift = NormaliseString(AccAPAccountDetailsSchema.A1_BankSwift.MaxLength, beneficiary.ABF_BankSwift);
				A1_BankAddress1 = NormaliseString(AccAPAccountDetailsSchema.A1_BankAddress1.MaxLength, beneficiary.ABF_BankAddress1);
				A1_BankAddress2 = NormaliseString(AccAPAccountDetailsSchema.A1_BankAddress2.MaxLength, beneficiary.ABF_BankAddress2);
				A1_BankAddress3 = NormaliseString(AccAPAccountDetailsSchema.A1_BankAddress3.MaxLength, beneficiary.ABF_BankAddress3);
			}
		}

		internal static string NormaliseString(int maxLength, string val) => val.Length > maxLength ? val.Substring(0, maxLength - 1) : val;

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (CompanyData != null && CompanyData.Header != null && !ModifyAccountDetailsSecurity)
				|| ShouldBeReadOnlyForElectronicPayment()
				|| MetaData.GetReadOnlyExcludingMethodProvider(this, property);

			bool ShouldBeReadOnlyForElectronicPayment()
			{
				if (property.Name == nameof(A1_IsDefaultAccount))
				{
					return false;
				}

				var electronicPaymentProperties = new[] { nameof(A1_EPaymentReferenceType), nameof(A1_EPaymentReference), nameof(A1_EPaymentReasonCode) };
				if (IsElectronicPaymentMethod)
				{
					return !electronicPaymentProperties.Contains(property.Name);
				}
				else
				{
					return electronicPaymentProperties.Contains(property.Name);
				}
			}
		}

		#endregion
	}
}
