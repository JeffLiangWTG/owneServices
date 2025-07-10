using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting.EPayment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AccBankAccount.Schema.AB_Desc)]
	public class AccBankAccount : AutoAccBankAccount, IAccBankAccount, IDocManagerSupport, IDataVersionLoggingSupported, IEDocsParsingSupport, IAuditParent
	{
		public AccBankAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AB_LastReconcileDate), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AB_LastStatementDate), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AB_StatementBalance), ConcurrencyPolicy.Strict);
		}

		public class CalculatedFieldsSchema : Schema
		{
			public const string CreditCardExpiryYear = "CreditCardExpiryYear";
			public const string CreditCardExpiryMonth = "CreditCardExpiryMonth";
			public const string IBAN = "IBAN";
		}

		#region Get Bank Account

		public static AccBankAccount GetDefaultReceiptBankAccountForDebtor(ZGuid organisation, ZString currency, GlbBranch branch, BusinessObjectFactory factory)
		{
			AccBankAccount result = null;
			OrgHeader header = (OrgHeader)factory.Load(typeof(OrgHeader), organisation);
			OrgCompanyData companyData = (header != null) ? header.GetCompanyDataForGlbCompany(branch.Company) : null;

			if (header != null && companyData != null && companyData.ARPayToAccount != null && companyData.ARPayToAccount.AB_IsActive)
			{
				result = companyData.ARPayToAccount;
			}

			if (result == null && header != null && companyData != null && companyData.ARDebtorGroup != null)
			{
				AccBankAccount tempAccount = companyData.ARDebtorGroup.GetBankAccountByCurrency(currency);
				if (tempAccount != null && tempAccount.AB_IsActive)
				{
					result = tempAccount;
				}
			}

			if (result == null && OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.Value.Count > 0)
			{
				AccBankAccount tempAccount = GetBankAccountByCurrencyFromRegistry(currency, factory);
				if (tempAccount != null && tempAccount.AB_IsActive)
				{
					result = tempAccount;
				}
			}

			if (result == null && header != null && companyData != null && companyData.ARDebtorGroup != null && companyData.ARDebtorGroup.DefaultBankAccount != null && companyData.ARDebtorGroup.DefaultBankAccount.AB_IsActive)
			{
				result = companyData.ARDebtorGroup.DefaultBankAccount;
			}

			if (result == null && currency.IsValid && branch != null)
			{
				AccBankAccount tempAccount = AccBankAccount.GetDefaultReceiptBankAccount(currency, branch, factory);
				if (tempAccount != null && tempAccount.AB_IsActive)
				{
					result = tempAccount;
				}
			}

			return result;
		}

		protected static AccBankAccount GetBankAccountByCurrencyFromRegistry(ZString currencyNK, BusinessObjectFactory factory)
		{
			AccBankAccount result = null;
			BankAccountBasedOnCurrencyCollection collection = OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.Value;

			foreach (BankAccountBasedOnCurrency bankAccountBasedOnCurrency in collection)
			{
				if (bankAccountBasedOnCurrency.Currency == currencyNK)
				{
					result = (AccBankAccount)factory.Load(typeof(AccBankAccount), bankAccountBasedOnCurrency.BankAccount);
				}
			}
			return result;
		}

		/// <summary>
		/// Returns the default receipt bank account for either a branch or company.
		/// </summary>
		/// <param name="Currency">The currency of the account</param>
		/// <param name="branch">The branch PK. If you have no branch pass in ZGuid.Empty and it will return the default account for the currency.</param>
		/// <returns></returns>
		public static AccBankAccount GetDefaultReceiptBankAccount(ZString currencyNK, GlbBranch branch, BusinessObjectFactory factory)
		{
			AccBankAccount account = null;

			if (branch != null)
			{
				account = GetBankAccount(currencyNK, branch, factory);
			}

			if (account != null)
			{
				return account;
			}

			account = GetBankAccount(currencyNK, null, factory);
			if (account != null)
			{
				return account;
			}

			if (branch != null)
			{
				account = GetBankAccount(branch.Company.GC_RX_NKLocalCurrency, branch, factory);
			}

			if (account != null)
			{
				return account;
			}

			account = GetBankAccount((branch != null) ? branch.Company.GC_RX_NKLocalCurrency : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, null, factory);
			if (account != null)
			{
				return account;
			}

			return null;
		}

		protected static AccBankAccount GetBankAccount(ZString currencyNK, GlbBranch branch, BusinessObjectFactory factory)
		{
			if (!currencyNK.IsEmpty)
			{
				RefCurrency defaultCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyNK);
				ZQuery query = new ZQuery();
				if (defaultCurrency != null)
				{
					query = new ZQuery(AccBankAccountSchema.AB_RX_NKAccountCurrency, defaultCurrency.RX_Code);
				}

				query.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, SQLComparisonOperator.Equal, ZBool.True);
				query.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_IsActive, SQLComparisonOperator.Equal, ZBool.True);

				if (branch != null)
				{
					query.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GC, SQLComparisonOperator.Equal, branch.Company.PK);
					query.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, branch.PK);
				}
				else if (GlbCompany.CurrentCompany != null)
				{
					query.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
					query.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);
				}
				else
				{
					return null;
				}

				return factory.LoadTop1<AccBankAccount>(query);
			}

			return null;
		}

		#endregion

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		public override void Delete()
		{
			EPaymentStaffTokenCollection.RemoveAndDeleteAll();
			base.Delete();
		}

		public override bool CanDelete => !AB_IsDefaultReceiptBankAccount;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (!CanDelete)
				{
					return ResString.GetMultilingualString("61ec4a53-3271-484b-9690-a4ba54afe8c7", "Can't delete bank account because: {0}", AccBankAccountValidation.DefaultReceiptBankAccountCannotBeInactive);
				}
				else
				{
					ZStringBuilder warningBuilder = new ZStringBuilder();
					warningBuilder.AppendIfNotEmpty(AccBankAccountValidation.BankToThisAccountWarningMessage(this));
					warningBuilder.AppendIfNotEmpty(AccBankAccountValidation.DefaultBankAccountForDebtorGroupWarningMessage(this));
					warningBuilder.AppendIfNotEmpty(AccBankAccountValidation.DefaultBankAccountInRegistryWarningMessage(this));
					return !warningBuilder.IsEmpty ? ResString.GetMultilingualString("b97097bc-704a-4db3-90d8-0398ce569d22", "Warnings before deleting the bank account:\r\n{0}", warningBuilder.ToStringWithNewLineBetweenAppends()) : (NoResString)string.Empty;
				}
			}
		}

		#region Default Values / Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AB_GC = GlbCompany.CurrentCompany.PK;
			AB_RX_NKAccountCurrency = CompanyCurrency;
			AB_DetailedDepositSlip = true;
			AB_BankAccountName = GlbCompany.CurrentCompany.GC_Name.Left(AB_BankAccountNameInfo.MaxLength);
			AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			AB_RN_NKBankAccountCountry = GlbCompany.CurrentCompany.Country.Code;
		}

		void SetDefaultValuesWhenAccountTypeIsChanged()
		{
			AB_AccountNum = ZString.Empty;
			AB_PaymentProvider = ZString.Empty;

			if (IsCashAccount)
			{
				AB_BankName = ZString.Empty;
				AB_BankAddress = ZString.Empty;
			}
			if (IsEPaymentAccount || IsCashAccount)
			{
				AB_BankAbbreviation = ZString.Empty;
				AB_BSB = ZString.Empty;
				AB_SWIFT = ZString.Empty;
				AB_AccountNumber = ZString.Empty;
				AB_FullAccountNumber = ZString.Empty;
				AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				AB_RN_NKBankAccountCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				AB_SO_ChequeTemplate = ZGuid.Empty;
				AB_ChequeNumDigits = 1;

				AB_DetailedDepositSlip = false;
				AB_IsDefaultReceiptBankAccount = false;

				AB_AutoDDRFormat = ZString.Empty;
				AB_AllowAutoDDR = false;
				AB_ShowDetailsOnDirectDebits = false;
				AB_AccountEFTUserID = ZString.Empty;
			}
			if (!IsCreditCardOrLinkedAccount)
			{
				AB_DebitCreditCardName = ZString.Empty;
				AB_DebitCreditCardExpiry = ZString.Empty;
				CreditCardExpiryMonth = ZString.Empty;
				CreditCardExpiryYear = ZString.Empty;
			}
		}

		#endregion

		#region Properties

		#region AB_AccountType

		[List("Lookups.AccountTypes")]
		public override ZString AB_AccountType
		{
			get
			{
				return base.AB_AccountType;
			}
			set
			{
				var hasChanges = base.AB_AccountType != value;
				base.AB_AccountType = value;

				if (!IsInDatabase || hasChanges)
				{
					SetDefaultValuesWhenAccountTypeIsChanged();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAB_RX_NKAccountCurrency();
				}
			}
		}

		#endregion

		#region AB_AllowAutoDDR

		public override ZBool AB_AllowAutoDDR
		{
			get { return base.AB_AllowAutoDDR; }
			set
			{
				base.AB_AllowAutoDDR = value;
				if (!value)
				{
					AB_AutoDDRFormat = "";
				}
			}
		}

		#endregion

		#region AB_SO_ChequeTemplate
		[List("ChequeTemplates")]
		public override ZGuid AB_SO_ChequeTemplate
		{
			get
			{
				return base.AB_SO_ChequeTemplate;
			}
			set
			{
				bool valueHasChanged = value != AB_SO_ChequeTemplate;

				base.AB_SO_ChequeTemplate = value;

				if (valueHasChanged)
				{
					CreateDocumentMenuAndPivotForChequeTemplateIfNotExists(value);
				}
			}
		}

		void CreateDocumentMenuAndPivotForChequeTemplateIfNotExists(ZGuid chequeTemplatePK)
		{
			if (chequeTemplatePK.IsValid && !chequeTemplatePK.IsEmpty)
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();

				ZDBOnlyQuery clientTemplateQuery = new ZDBOnlyQuery(typeof(StmTemplate));
				clientTemplateQuery.AddToFilter(StmTemplateSchema.PK, chequeTemplatePK);
				StmTemplate clientTemplate = (StmTemplate)newFactory.LoadTop1(typeof(StmTemplate), clientTemplateQuery);

				if (clientTemplate != null)
				{
					bool shouldSaveFactory = false;

					if (MenuPivotHelper.CreateMenuAndPivotWithEmptyMenuPathIfNotInDatabase(newFactory, clientTemplate, BusinessContext.APTransaction, (NoResString)"Standard", ZString.Empty))
					{
						shouldSaveFactory = true;
					}
					if (MenuPivotHelper.CreateMenuAndPivotWithEmptyMenuPathIfNotInDatabase(newFactory, clientTemplate, BusinessContext.HotCheque, (NoResString)"Standard", ZString.Empty))
					{
						shouldSaveFactory = true;
					}
					if (MenuPivotHelper.CreateMenuAndPivotWithEmptyMenuPathIfNotInDatabase(newFactory, clientTemplate, BusinessContext.CBDirectPayment, (NoResString)"Standard", ZString.Empty))
					{
						shouldSaveFactory = true;
					}

					if (shouldSaveFactory)
					{
						newFactory.Save();
					}
				}
			}
		}

		#endregion

		#region AB_AutoDDRFormat

		[List("AB_AutoDDRFormat_List")]
		public override ZString AB_AutoDDRFormat
		{
			get { return base.AB_AutoDDRFormat; }
			set { base.AB_AutoDDRFormat = value; }
		}
		#endregion

		#region AB_RX_NKAccountCurrency

		[List("RefCurrencies")]
		public override ZString AB_RX_NKAccountCurrency
		{
			get { return base.AB_RX_NKAccountCurrency; }
			set { base.AB_RX_NKAccountCurrency = value; }
		}

		#endregion

		#region AB_AG

		[List("GlobalHeaders")]
		public override ZGuid AB_AG
		{
			get
			{
				return base.AB_AG;
			}
			set
			{
				base.AB_AG = value;
			}
		}

		#endregion

		#region AB_GB
		[List("GlbBranches")]
		public override ZGuid AB_GB
		{
			get
			{
				return base.AB_GB;
			}
			set
			{
				base.AB_GB = value;
			}
		}
		#endregion

		#region AB_GC
		[List("GlbCompanies")]
		public override ZGuid AB_GC
		{
			get
			{
				return base.AB_GC;
			}
			set
			{
				base.AB_GC = value;
			}
		}
		#endregion

		#region AB_PaymentProvider

		[List("Lookups.PaymentProviderCodeList")]
		public override ZString AB_PaymentProvider { get => base.AB_PaymentProvider; set => base.AB_PaymentProvider = value; }

		#endregion

		#region AB_IsDefaultReceiptBankAccount

		public override ZBool AB_IsDefaultReceiptBankAccount
		{
			get
			{
				return base.AB_IsDefaultReceiptBankAccount;
			}
			set
			{
				base.AB_IsDefaultReceiptBankAccount = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAB_IsActive();
				}
			}
		}

		#endregion

		#region CompanyCurrency
		[MaxLength(3)]
		[List("RefCurrencies")]
		public ZString CompanyCurrency
		{
			get
			{
				if (((ZGuid)Env.CurrentCompany.PK).IsValid)
				{
					ZQuery filter = new ZQuery(GlbCompanySchema.PK, (ZGuid)Env.CurrentCompany.PK);
					GlbCompany company = Factory.LoadTop1(typeof(GlbCompany), filter) as GlbCompany;
					if (company != null)
					{
						return company.GC_RX_NKLocalCurrency;
					}
				}
				return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		public ZPropertyInfo CompanyCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyCurrency)); }
		}

		#endregion

		#region OSCurrency
		[MaxLength(3)]
		[List("RefCurrencies")]
		public ZGuid OSCurrency
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, AB_RX_NKAccountCurrency);
				if (currency != null)
				{
					return currency.PK;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
		}

		public ZPropertyInfo OSCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(OSCurrency)); }
		}

		#endregion

		#region ReadOnly Fields

		protected bool AB_BankName_ReadOnly						=> IsCashAccount;
		protected bool AB_BankAddress_ReadOnly					=> IsCashAccount;
		protected bool AB_AccountType_ReadOnly					=> IsCashAccount;

		protected bool AB_PaymentProvider_ReadOnly				=> !IsEPaymentAccount;

		protected bool AB_BankAbbreviation_ReadOnly				=> IsEPaymentAccount || IsCashAccount;
		protected bool AB_BSB_ReadOnly							=> IsEPaymentAccount || IsCashAccount;
		protected bool AB_SWIFT_ReadOnly						=> IsEPaymentAccount || IsCashAccount;
		protected bool AB_AccountNumber_ReadOnly				=> IsEPaymentAccount || IsCashAccount;
		protected bool AB_FullAccountNumber_ReadOnly			=> IsEPaymentAccount || IsCashAccount;
		protected bool AB_SO_ChequeTemplate_ReadOnly			=> IsEPaymentAccount || IsCashAccount;
		protected bool AB_ChequeNumDigits_ReadOnly				=> IsEPaymentAccount || IsCashAccount;
		protected bool AB_AllowAutoDDR_ReadOnly					=> IsEPaymentAccount || IsCashAccount;
		protected bool AB_ShowDetailsOnDirectDebits_ReadOnly	=> IsEPaymentAccount || IsCashAccount;
		protected bool AB_AccountEFTUserID_ReadOnly				=> IsEPaymentAccount || IsCashAccount;
		protected bool IBAN_ReadOnly							=> IsEPaymentAccount || IsCashAccount;

		protected bool AB_AccountNum_ReadOnly					=> IsEPaymentAccount || IsCashAccount || IsCreditCardAccount;
		protected bool AB_AutoDDRFormat_ReadOnly				=> !AB_AllowAutoDDR;

		protected bool AB_DebitCreditCardName_ReadOnly			=> !IsCreditCardOrLinkedAccount;
		protected bool AB_DebitCreditCardExpiry_ReadOnly		=> !IsCreditCardOrLinkedAccount;
		protected bool CreditCardExpiryMonth_ReadOnly			=> !IsCreditCardOrLinkedAccount;
		protected bool CreditCardExpiryYear_ReadOnly			=> !IsCreditCardOrLinkedAccount;

		protected bool AB_RN_NKBankAccountCountry_ReadOnly		=> IsEPaymentAccount;
		protected bool AB_RX_NKAccountCurrency_ReadOnly			=> IsEPaymentAccount;
		protected bool AB_DetailedDepositSlip_ReadOnly			=> IsEPaymentAccount;
		protected bool AB_IsDefaultReceiptBankAccount_ReadOnly	=> IsEPaymentAccount;

		#endregion

		#region Human Readble Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("db1cf24f-d58e-484a-9c21-d7c11fd61e56", "Bank Account");
			}
		}

		#endregion

		[ReadOnly(true)]
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal AB_ClosingBalance
		{
			get { return base.AB_ClosingBalance; }
			set { base.AB_ClosingBalance = value; }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AB_ClosingOSBalance
		{
			get { return base.AB_ClosingOSBalance; }
			set { base.AB_ClosingOSBalance = value; }
		}

		#region CreditCardExpiryMonth
		[MaxLength(3)]
		[List("Lookups.Months")]
		public virtual ZString CreditCardExpiryMonth
		{
			get
			{
				if ((AB_DebitCreditCardExpiry.Length == 4) && (fCreditCardExpiryMonth == ZString.Empty))
				{
					string month = AB_DebitCreditCardExpiry.Substring(0, 2);
					int monthInt = 0;
					if (int.TryParse(month, out monthInt))
					{
						if (monthInt >= 1 && monthInt <= 12)
						{
							fCreditCardExpiryMonth = Lookups.Months[monthInt - 1].Code;
						}
					}
				}
				return fCreditCardExpiryMonth;
			}
			set
			{
				if (fCreditCardExpiryMonth != value)
				{
					CheckMaximumLength(CreditCardExpiryMonthInfo, value);
					fCreditCardExpiryMonth = value;
					CreditCardExpiryMonthInfo.RefreshBinding();
					setDebitCreditCardExpiry();
					Validation.ValidateCreditCardExpiryMonth();
				}
			}
		}
		ZString fCreditCardExpiryMonth;

		public ZPropertyInfo CreditCardExpiryMonthInfo => GetZPropertyInfo(CalculatedFieldsSchema.CreditCardExpiryMonth);

		#endregion

		#region CreditCardExpiryYear

		[MaxLength(2)]
		public virtual ZString CreditCardExpiryYear
		{
			get
			{
				if ((AB_DebitCreditCardExpiry.Length == 4) && (fCreditCardExpiryYear == ZString.Empty))
				{
					fCreditCardExpiryYear = AB_DebitCreditCardExpiry.Substring(2, 2);
				}
				return fCreditCardExpiryYear;
			}
			set
			{
				if (fCreditCardExpiryYear != value)
				{
					CheckMaximumLength(CreditCardExpiryYearInfo, value);
					fCreditCardExpiryYear = value;
					CreditCardExpiryYearInfo.RefreshBinding();
					setDebitCreditCardExpiry();
					Validation.ValidateCreditCardExpiryYear();
				}
			}
		}
		ZString fCreditCardExpiryYear;

		public ZPropertyInfo CreditCardExpiryYearInfo => GetZPropertyInfo(CalculatedFieldsSchema.CreditCardExpiryYear);

		#endregion

		#region AB_DebitCreditCardExpiry

		void setDebitCreditCardExpiry()
		{
			string temp = string.Empty;
			if (fCreditCardExpiryMonth != "")
			{
				int month = Lookups.Months.IndexOfCode(CreditCardExpiryMonth);
				month++;
				temp = month.ToString().PadLeft(2, '0');
			}
			if (fCreditCardExpiryYear != "")
			{
				temp += CreditCardExpiryYear;
			}
			AB_DebitCreditCardExpiry = temp;
		}

		#endregion

		#region AB_DebitCreditCardNumber

		public override ZString AB_DebitCreditCardNumber
		{
			get
			{
				return base.AB_DebitCreditCardNumber;
			}
			set
			{
				base.AB_DebitCreditCardNumber = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAB_AccountNum();
				}
			}
		}
		#endregion

		#region IsCreditCardOrLinkedAccount

		public bool IsCreditCardOrLinkedAccount => IsCreditCardAccount || IsLinkedAccount;

		#endregion

		#region IsEPaymentAccount

		public bool IsEPaymentAccount => AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.EPA;

		#endregion

		#region IsCashAccount

		public bool IsCashAccount => AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.CSH;

		#endregion

		#region IsCreditCardAccount

		public bool IsCreditCardAccount => AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.CCD;

		#endregion

		#region IsLinkedAccount

		public bool IsLinkedAccount => AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.LNK;

		#endregion

		#region AB_OpenBalance

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal AB_OpenBalance
		{
			get { return base.AB_OpenBalance; }
			set { base.AB_OpenBalance = value; }
		}

		#endregion

		#region AB_OpenOSBalance

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AB_OpenOSBalance
		{
			get { return base.AB_OpenOSBalance; }
			set { base.AB_OpenOSBalance = value; }
		}

		#endregion

		#region AB_IBAN
		/// <summary>
		/// The IBAN is stored in the AB_AccountNumber field, but this property is better named.
		/// </summary>
		public ZString IBAN
		{
			get { return base.AB_AccountNumber; }
			set { base.AB_AccountNumber = value; }
		}

		public ZPropertyInfo IBANInfo => GetZPropertyInfo(Schema.AB_AccountNumber);

		#endregion

		#endregion

		#region Lookups

		#region Cheque Templates

		StmTemplateFilteredCollection fChequeTemplates;

		public StmTemplateFilteredCollection ChequeTemplates
		{
			get
			{
				if (fChequeTemplates == null)
				{
					ZQuery templateFilter = new ZQuery(StmTemplateSchema.SO_DataContext, nameof(Core.Constants.DataContext.Cheques));
					fChequeTemplates = new StmTemplateFilteredCollection(Factory, templateFilter);
				}
				return fChequeTemplates;
			}
		}

		#endregion

		#region Companies

		GlbCompanyCollection fGlbCompanies;

		public GlbCompanyCollection GlbCompanies
		{
			get
			{
				if (fGlbCompanies == null)
				{
					fGlbCompanies = new GlbCompanyCollection(Factory);
				}
				return fGlbCompanies;
			}
		}

		#endregion

		#region E-Payment Staff Token
		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public AccEPaymentStaffTokenDependentCollection EPaymentStaffTokenCollection
		{
			get
			{
				if (fEPaymentStaffTokenCollection == null)
				{
					fEPaymentStaffTokenCollection = new AccEPaymentStaffTokenDependentCollection(this);
					fEPaymentStaffTokenCollection.Load();
					RegisterEditableChildObject(fEPaymentStaffTokenCollection);
				}
				return fEPaymentStaffTokenCollection;
			}
		}
		AccEPaymentStaffTokenDependentCollection fEPaymentStaffTokenCollection;
		#endregion

		#region GL Headers

		AccGLHeaderCollection fGlobalHeaders;

		public AccGLHeaderCollection GlobalHeaders
		{
			get
			{
				if (fGlobalHeaders == null)
				{
					ZDBOnlyQuery companyFilterQuery = new ZDBOnlyQuery(typeof(AccGLHeader));
					ZDBOnlySubQuery companyFilterSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeaderCompanyFilter), AccGLHeaderCompanyFilterSchema.ACF_AG_Header);
					companyFilterSubQuery.AddToFilter(AccGLHeaderCompanyFilterSchema.ACF_GC_Company, AB_GC);
					companyFilterQuery.AddSubQuery(companyFilterSubQuery, JoinCondition.And);
					companyFilterQuery.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_IsGlobal, true);

					ZQuery headerFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, Constants.AccountType.BalanceSheetAccount);
					headerFilter.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, true);
					headerFilter.AddToFilter(companyFilterQuery);
					ZQuery query = new ZQuery(headerFilter);

					fGlobalHeaders = new AccGLHeaderCollection(Factory, query);
				}
				return fGlobalHeaders;
			}
		}

		#endregion

		#region Branches

		GlbBranchCollection fGlbBranches;

		public GlbBranchCollection GlbBranches
		{
			get
			{
				if (fGlbBranches == null)
				{
					ZQuery branchesFilter = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
					ZQuery query = new ZQuery(branchesFilter);
					fGlbBranches = new GlbBranchCollection(Factory, query);
				}
				return fGlbBranches;
			}
		}

		#endregion

		#region Currencies

		RefCurrencyCollection fRefCurrencies;

		public RefCurrencyCollection RefCurrencies
		{
			get
			{
				if (fRefCurrencies == null)
				{
					fRefCurrencies = new RefCurrencyCollection(Factory);
				}
				return fRefCurrencies;
			}
		}

		#endregion

		#region Auto DDR Format List

		CodeDescriptionPairList fAB_AutoDDRFormat_List;

		public CodeDescriptionPairList AB_AutoDDRFormat_List
		{
			get
			{
				if (fAB_AutoDDRFormat_List == null)
				{
					fAB_AutoDDRFormat_List = new BankDDRFormatList();
					if (AB_RN_NKBankAccountCountry != CountryCodes.Australia)
					{
						fAB_AutoDDRFormat_List.RemoveCode(BankDDRFormatList.Codes.AB1);
						fAB_AutoDDRFormat_List.RemoveCode(BankDDRFormatList.Codes.AB2);
					}
				}
				return fAB_AutoDDRFormat_List;
			}
		}

		#endregion

		#endregion

		#region Utility Methods

		public ZBool HasChequeNumberBeenUsed(ZString chequeNumber, ZGuid transactionHeaderPKToExclude)
		{
			return HasChequeNumberBeenUsed(chequeNumber, transactionHeaderPKToExclude, ZGuid.Empty);
		}

		public ZBool HasChequeNumberBeenUsed(ZString chequeNumber, ZGuid transactionHeaderPKToExclude, ZGuid jobChargePKToExlude)
		{
			return HasChequeNumberBeenUsedOnAPayment(chequeNumber, transactionHeaderPKToExclude) ||
				HasChequeNumberBeenUsedOnAJobCharge(chequeNumber, jobChargePKToExlude) ||
				HasChequeNumberBeenUsedOnAPaymentApproval(chequeNumber, ZGuid.Empty, transactionHeaderPKToExclude);
		}

		public ZString GetTheFirstUsedChequeNumber(params ZString[] chequeNumbers)
		{
			var result = GetTheFirstUsedChequeNumberOnAPayment(chequeNumbers);
			if (string.IsNullOrWhiteSpace(result))
			{
				result = GetTheFirstUsedChequeNumberOnAJobCharge(chequeNumbers);
			}
			if (string.IsNullOrWhiteSpace(result))
			{
				result = GetTheFirstUsedChequeNumberOnAPaymentApproval(chequeNumbers);
			}
			return result;
		}

		#region Payment

		public ZBool HasChequeNumberBeenUsedOnAPayment(ZString chequeNumber, ZGuid transactionHeaderPKToExclude)
		{
			return HasChequeNumberBeenUsedOnPostedTransactions(chequeNumber, new List<ZGuid>() { transactionHeaderPKToExclude });
		}

		public ZBool HasChequeNumberBeenUsedOnAPayment(ZString chequeNumber, IEnumerable<ZGuid> transactionHeadersPKToExclude)
		{
			return HasChequeNumberBeenUsedOnPostedTransactions(chequeNumber, transactionHeadersPKToExclude);
		}

		public ZBool HasChequeNumberBeenUsedOnPostedTransactions(ZString chequeNumber, IEnumerable<ZGuid> transactionHeadersPKToExclude)
		{
			var paymentFilter = GetFilterForExistingPayments(new ZString[] { chequeNumber }, transactionHeadersPKToExclude);

			return Factory.ExistsInDatabase(AccTransactionHeaderSchema.Constants.TableName, paymentFilter);
		}

		ZString GetTheFirstUsedChequeNumberOnAPayment(IEnumerable<ZString> chequeNumbers)
		{
			ZQuery paymentFilter = GetFilterForExistingPayments(chequeNumbers, new List<ZGuid>());
			var payment = Factory.LoadTop1<AccTransactionHeader>(paymentFilter);
			return payment != null ? payment.AH_ChequeOrReference : ZString.Empty;
		}

		ZQuery GetFilterForExistingPayments(IEnumerable<ZString> chequeNumber, IEnumerable<ZGuid> transactionHeaderPKsToExclude)
		{
			ZQuery paymentFilter = new ZQuery();

			ZQuery transactionTypeFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Payment);
			transactionTypeFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.DirectPayment);

			paymentFilter.AddToFilter(transactionTypeFilter, JoinCondition.And);
			paymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_ReceiptType, SQLComparisonOperator.Equal, ZArchitecture.Core.ReceiptTypes.Cheque);
			paymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_ChequeOrReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			paymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_ChequeOrReference, chequeNumber);
			paymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_IsCancelled, SQLComparisonOperator.Equal, false);
			paymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_AB, SQLComparisonOperator.Equal, this.PK);
			paymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, this.AB_GC);

			if (transactionHeaderPKsToExclude != null && transactionHeaderPKsToExclude.Any())
			{
				paymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionHeaderPKsToExclude);
			}

			paymentFilter.AddOptionRecompileConditionally = true;

			return paymentFilter;
		}

		#endregion

		#region APaymentApproval

		public ZBool HasChequeNumberBeenUsedOnAPaymentApproval(ZString chequeNumber, List<ZGuid> paymentApprovalsToExclude)
		{
			return HasChequeNumberBeenUsedOnAPaymentApproval(new ZString[] { chequeNumber }, ZGuid.Empty, paymentApprovalsToExclude);
		}

		public ZBool HasChequeNumberBeenUsedOnAPaymentApproval(ZString chequeNumber, ZGuid paymentApprovalPKToExclude, ZGuid transactionHeaderPKToExclude)
		{
			return HasChequeNumberBeenUsedOnAPaymentApproval(new ZString[] { chequeNumber }, transactionHeaderPKToExclude, new List<ZGuid>() { paymentApprovalPKToExclude });
		}

		ZBool HasChequeNumberBeenUsedOnAPaymentApproval(IEnumerable<ZString> chequeNumbers, ZGuid transactionHeaderPKToExclude, List<ZGuid> paymentApprovalsToExclude)
		{
			var chequeNumber = GetChequeNumberBeenUsedOnAPaymentApproval(chequeNumbers, transactionHeaderPKToExclude, paymentApprovalsToExclude);
			return !string.IsNullOrWhiteSpace(chequeNumber);
		}

		ZString GetTheFirstUsedChequeNumberOnAPaymentApproval(IEnumerable<ZString> chequeNumbers)
		{
			return GetChequeNumberBeenUsedOnAPaymentApproval(chequeNumbers, ZGuid.Empty, null);
		}

		ZString GetChequeNumberBeenUsedOnAPaymentApproval(IEnumerable<ZString> chequeNumbers, ZGuid transactionHeaderPKToExclude, List<ZGuid> paymentApprovalsToExclude)
		{
			ZQuery accPaymentApprovalFilter = new ZQuery(AccPaymentApprovalSchema.AV_ChequeOrReference, chequeNumbers);
			accPaymentApprovalFilter.AddToFilter(JoinCondition.And, AccPaymentApprovalSchema.AV_ChequeOrReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			accPaymentApprovalFilter.AddToFilter(JoinCondition.And, AccPaymentApprovalSchema.AV_PaymentType, SQLComparisonOperator.Equal, ZArchitecture.Core.ReceiptTypes.Cheque);
			accPaymentApprovalFilter.AddToFilter(JoinCondition.And, AccPaymentApprovalSchema.AV_AB, SQLComparisonOperator.Equal, this.PK);
			accPaymentApprovalFilter.AddToFilter(AccPaymentApprovalSchema.AV_Status, SQLComparisonOperator.NotEqual, ZArchitecture.Core.PaymentApprovalStatus.Posted);

			if (!transactionHeaderPKToExclude.IsEmpty)
			{
				accPaymentApprovalFilter.AddToFilter(JoinCondition.And, AccPaymentApprovalSchema.AV_AH, SQLComparisonOperator.NotEqual, transactionHeaderPKToExclude);
			}

			if (paymentApprovalsToExclude != null)
			{
				foreach (ZGuid paymentApprovalPK in paymentApprovalsToExclude)
				{
					accPaymentApprovalFilter.AddToFilter(JoinCondition.And, AccPaymentApprovalSchema.PK, SQLComparisonOperator.NotEqual, paymentApprovalPK);
				}
			}

			AccPaymentApproval paymentApproval = null;

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			SELECT TOP 1
				{0}
			FROM
				{1}
				{2}
			OPTION (RECOMPILE)"
			, AccPaymentApprovalSchema.PK.Name
			, AccPaymentApprovalSchema.Constants.TableName
			, accPaymentApprovalFilter.GetAsWhereClause(false));

			#region Check if PaymentApproval is in DB

			DynamicBusinessObjectCollection query = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			query.Load(sqlText, new ZSqlParameterCollection(accPaymentApprovalFilter.Params));

			if (query.Count > 0)
			{
				var approvalPk = (ZGuid)query[0][AccPaymentApprovalSchema.PK];
				paymentApproval = query.Factory.Load<AccPaymentApproval>(approvalPk); //Instead of loading by PK, we could do Factory.LoadTop1 using AccPaymentApprovalFilter, but this is done intentionally to improve index utilisation. Factory.LoadTop1<> does a 'select *', we wanted to prevent this.
			}

			#endregion

			#region Check if PaymentApproval found in DB is loaded and changed in local cache

			bool isPaymentApprovalFromDbChangedInLocalCache = false;
			if (paymentApproval != null)
			{
				var paymentApprovalFromDbFilterForLocalCacheOnly = new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK);
				paymentApprovalFromDbFilterForLocalCacheOnly.FetchOnlyFromLocalCache = true;
				var paymentApprovalFromDbInLocalCache = Factory.LoadTop1<AccPaymentApproval>(paymentApprovalFromDbFilterForLocalCacheOnly);
				isPaymentApprovalFromDbChangedInLocalCache = paymentApprovalFromDbInLocalCache != null && paymentApprovalFromDbInLocalCache.HasChanges;
			}

			#endregion

			#region Check if PaymentApproval is in local cache

			if (paymentApproval == null || isPaymentApprovalFromDbChangedInLocalCache)
			{
				var paymentApprovalFilterForLocalCache = new ZQuery();
				paymentApprovalFilterForLocalCache.FetchOnlyFromLocalCache = true;
				paymentApprovalFilterForLocalCache.AddToFilter(accPaymentApprovalFilter);

				var paymentApprovalInLocalCache = Factory.Load<AccPaymentApproval>(paymentApprovalFilterForLocalCache);
				paymentApproval = paymentApprovalInLocalCache.FirstOrDefault(x => !x.IsInDatabase || x.HasChanges);
			}

			#endregion

			return paymentApproval != null ? paymentApproval.AV_ChequeOrReference : ZString.Empty;
		}

		#endregion

		#region JobCharge

		public ZBool HasChequeNumberBeenUsedOnAJobCharge(ZString chequeNumber, ZGuid jobChargePKToExclude)
		{
			return GetJobChargesUsingChequeNumber(new ZString[] { chequeNumber }, jobChargePKToExclude).Length > 0;
		}

		public ZBool HasChequeNumberBeenUsedOnAJobCharge(ZString chequeNumber, List<ZGuid> jobConsolCostsPKToExclude)
		{
			ZQuery jobChargeFilter = new ZQuery(JobChargeSchema.JR_ChequeNo, SQLComparisonOperator.Equal, chequeNumber);
			jobChargeFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_PaymentType, SQLComparisonOperator.Equal, ZArchitecture.Core.ReceiptTypes.Cheque);
			jobChargeFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_AB, SQLComparisonOperator.Equal, this.PK);

			jobChargeFilter.AddToFilter(JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, jobConsolCostsPKToExclude);

			return Factory.LoadTop1<JobCharge>(jobChargeFilter) != null;
		}

		public JobCharge[] GetJobChargesUsingChequeNumber(ZString chequeNumbers, ZGuid jobChargePKToExclude)
		{
			return GetJobChargesUsingChequeNumber(new ZString[] { chequeNumbers }, jobChargePKToExclude);
		}

		public JobCharge[] GetJobChargesUsingChequeNumber(IEnumerable<ZString> chequeNumbers, ZGuid jobChargePKToExclude)
		{
			ZQuery jobChargeFilter = new ZQuery(JobChargeSchema.JR_ChequeNo, chequeNumbers);
			jobChargeFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_PaymentType, SQLComparisonOperator.Equal, ZArchitecture.Core.ReceiptTypes.Cheque);
			jobChargeFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_AB, SQLComparisonOperator.Equal, this.PK);

			if (!jobChargePKToExclude.IsEmpty)
			{
				jobChargeFilter.AddToFilter(JoinCondition.And, JobChargeSchema.PK, SQLComparisonOperator.NotEqual, jobChargePKToExclude);
			}

			return Factory.Load<JobCharge>(jobChargeFilter);
		}

		ZString GetTheFirstUsedChequeNumberOnAJobCharge(IEnumerable<ZString> chequeNumbers)
		{
			var charges = GetJobChargesUsingChequeNumber(chequeNumbers, ZGuid.Empty);
			return (charges != null && charges.Length > 0) ? charges[0].JR_ChequeNo : ZString.Empty;
		}

		#endregion

		public ZBool IsChequeNumberUsedByCancelledPayment(ZString chequeNumber)
		{
			ZQuery cancelledPaymentFilter = new ZQuery();

			ZQuery transactionTypeFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Payment);
			transactionTypeFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.DirectPayment);

			cancelledPaymentFilter.AddToFilter(transactionTypeFilter, JoinCondition.And);
			cancelledPaymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_ReceiptType, SQLComparisonOperator.Equal, ZArchitecture.Core.ReceiptTypes.Cheque);
			cancelledPaymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_IsCancelled, SQLComparisonOperator.Equal, true);
			cancelledPaymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_ChequeOrReference, SQLComparisonOperator.Equal, chequeNumber);
			cancelledPaymentFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_AB, SQLComparisonOperator.Equal, this.PK);

			return Factory.ExistsInDatabase(AccTransactionHeaderSchema.Constants.TableName, cancelledPaymentFilter);
		}

		public ZBool HasChequeNumberBeenUsedOnAnotherTransactionAndNotYetSaved(ZString chequeNumber, ZGuid transactionHeaderPKToExclude)
		{
			ZQuery paymentFilter = GetFilterForExistingPayments(new ZString[] { chequeNumber }, new List<ZGuid>() { transactionHeaderPKToExclude });
			paymentFilter.FetchOnlyFromLocalCache = true;
			AccTransactionHeader existingPayment = Factory.LoadTop1<AccTransactionHeader>(paymentFilter);
			return existingPayment != null;
		}

		public ZBool IsValidBSBNumber(ZString bSBNumber)
		{
			ZBool result = true;

			if ((AB_AutoDDRFormat == Constants.DDRFileFormat.ANZ &&
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia) ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.NAB ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.CBA ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.WBC ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.BBL)
			{
				if (!Regex.IsMatch(bSBNumber, @"^[0-9]{3}-[0-9]{3}$"))
				{
					result = false;
				}
			}
			else if ((AB_AutoDDRFormat == Constants.DDRFileFormat.ANZ && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand) ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.BNZ ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.ASB ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.WNZ ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.BCS ||
				(AB_AutoDDRFormat == Constants.DDRFileFormat.ANZ &&
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand))
			{
				if (!Regex.IsMatch(bSBNumber, @"^[0-9]{6}$"))
				{
					result = false;
				}
			}

			return result;
		}

		public ZBool IsValidAccountNumber(ZString accountNumber)
		{
			ZBool result = true;

			if (AB_AutoDDRFormat == Constants.DDRFileFormat.ASB || AB_AutoDDRFormat == Constants.DDRFileFormat.BNZ)
			{
				result = (accountNumber.Length == 9 || accountNumber.Length == 10);
			}
			else if (AB_AutoDDRFormat == Constants.DDRFileFormat.WNZ)
			{
				result = accountNumber.Length <= 12;
			}
			else if (AB_AutoDDRFormat == Constants.DDRFileFormat.BCS)
			{
				result = Regex.IsMatch(accountNumber, @"^[0-9]{8}$");
			}
			else if (accountNumber.Length > 9 && AB_AutoDDRFormat != Constants.DDRFileFormat.CUS && AB_AutoDDRFormat != Constants.DDRFileFormat.BTM)
			{
				result = false;
			}

			return result;
		}

		public ZString GetInvalidAccountNumberErrorMessage()
		{
			ZString result = "";

			if (AB_AutoDDRFormat == Constants.DDRFileFormat.ASB)
			{
				result = Res.GetString("fc957c0b-b45c-44bb-8888-f19d343baf74", "The account number must be 9 or 10 characters in length for ASB Bank DDR format");
			}
			else if (AB_AutoDDRFormat == Constants.DDRFileFormat.BNZ)
			{
				result = Res.GetString("b3f3b5a1-23ea-4aa2-a64d-4768c79964f6", "The account number must be 9 or 10 characters in length for BNZ Bank DDR format");
			}
			else if (AB_AutoDDRFormat == Constants.DDRFileFormat.WNZ)
			{
				result = Res.GetString("5bc77c14-3d23-4ea6-ae80-bdec9c1115e3", "The account number must be less than or equal to 12 characters in length for WNZ Bank DDR format");
			}
			else
			{
				result = Res.GetString("192f8151-1ef9-4e33-8722-676d4bdb1161", "The account number must be 9 or less characters in length");
			}

			return result;
		}

		public ZString GetInvalidBSBNumberErrorMessage()
		{
			ZString result = "";

			if (AB_AutoDDRFormat == Constants.DDRFileFormat.NAB ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.CBA ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.WBC ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.BBL ||
				(AB_AutoDDRFormat == Constants.DDRFileFormat.ANZ &&
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia))
			{
				result = Res.GetString("c94100db-d84c-4fdf-b438-e0f42dd5ce09", "The BSB when using the '{0}' Direct Debit System must have the pattern 'XXX-XXX'", AB_AutoDDRFormat);
			}
			else if (AB_AutoDDRFormat == Constants.DDRFileFormat.BCS ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.BNZ ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.ASB ||
				AB_AutoDDRFormat == Constants.DDRFileFormat.WNZ ||
				(AB_AutoDDRFormat == Constants.DDRFileFormat.ANZ &&
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand))
			{
				result = Res.GetString("501cb8a0-4b40-46e5-a7a2-e56c2e9fdb45", "The BSB when using the '{0}' Direct Debit System must have the pattern 'XXXXXX'", AB_AutoDDRFormat);
			}

			return result;
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.BankAccount);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(AccEPaymentStaffTokenSchema.TK_AB, null);
			}
		}

		#endregion
		public int LocalCurrencyDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public int OSCurrencyDecimals => AccountCurrency?.Decimals ?? LocalCurrencyDecimals;
	}
}
