using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	class AccTaxConfigGLAccountsProvider : IAccTaxConfigGLAccountsProvider
	{
		(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount) IAccTaxConfigGLAccountsProvider.GetApplicableGLAccounts(AccTaxConfiguration config)
		{
			TaxSystemsConfiguration taxSystem = null;
			var taxSystemCode = config.ETC_TaxSystemCode;
			var ledger = config.ETC_Ledger;
			var realisationMethod = config.ETC_TaxRealisationMethod;

			if (CheckForNotApplicable())
			{
				return (GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
			}

			var superType = taxSystem.TaxSuperType;
			var includeInInvoiceTotal = taxSystem.IncludeInInvoceTotal;

			return CheckIf_PER_VAT_AP() ?? CheckIf_RII_SPR_AR() ?? CheckIf_SLX_AP_PDT() ?? CheckIf_TRX_AR_PDT() ?? CheckIf_SLX_PER_VAT_AR() ?? CheckIf_RII_TRX_AP() ?? CheckIf_SPR_AP() ?? (GLAccountRegistryType.Error, GLAccountRegistryType.Error, GLAccountRegistryType.Error, GLAccountRegistryType.Error);

			bool CheckForNotApplicable()
			{
				return taxSystemCode.IsEmpty || config.ETC_TaxSystemCodeInfo.HasErrors()
					|| ledger.IsEmpty || config.ETC_LedgerInfo.HasErrors()
					|| realisationMethod.IsEmpty || config.ETC_TaxRealisationMethodInfo.HasErrors()
					|| (taxSystem = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxSystem(taxSystemCode, config.Factory)) == null;
			}

			(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? CheckIf_PER_VAT_AP()
			{
				if ((superType == TaxSuperTypeList.Perceptions.Code || superType == TaxSuperTypeList.ValueAddedTax.Code)
								&& ledger == LedgerTypes.AccountsPayable)
				{
					if (realisationMethod == TaxRealisationMethods.PostDate.Code)
					{
						return (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
					}
					else if (realisationMethod == TaxRealisationMethods.MatchDate.Code)
					{
						return (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount);
					}
				}
				return null;
			}

			(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? CheckIf_RII_SPR_AR()
			{
				if (new[] { TaxSuperTypeList.RetentionInInvoice.Code, TaxSuperTypeList.StandardPaymentRetention.Code }
								.Contains(superType.ToString())
								&& ledger == LedgerTypes.AccountsReceivable)
				{
					if (realisationMethod == TaxRealisationMethods.PostDate.Code)
					{
						return (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
					}
					else if (realisationMethod == TaxRealisationMethods.MatchDate.Code)
					{
						return (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount);
					}
				}
				return null;
			}

			(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? CheckIf_SLX_AP_PDT()
			{
				if (superType == TaxSuperTypeList.SalesTax.Code
								&& ledger == LedgerTypes.AccountsPayable)
				{
					if (realisationMethod == TaxRealisationMethods.PostDate.Code)
					{
						return (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.TaxTransactionExpenseAccount, GLAccountRegistryType.NotApplicable);
					}
				}
				return null;
			}

			(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? CheckIf_TRX_AR_PDT()
			{
				if (superType == TaxSuperTypeList.TurnoverTax.Code
								&& ledger == LedgerTypes.AccountsReceivable)
				{
					if (realisationMethod == TaxRealisationMethods.PostDate.Code)
					{
						if (includeInInvoiceTotal)
						{
							return (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
						}
						else
						{
							return (GLAccountRegistryType.NotApplicable, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.TaxTransactionNegativeRevenueAccount, GLAccountRegistryType.NotApplicable);
						}
					}
				}
				return null;
			}

			(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? CheckIf_SLX_PER_VAT_AR()
			{
				if (new[] { TaxSuperTypeList.SalesTax.Code, TaxSuperTypeList.Perceptions.Code, TaxSuperTypeList.ValueAddedTax.Code }
								.Contains(superType.ToString())
								&& ledger == LedgerTypes.AccountsReceivable)
				{
					if (realisationMethod == TaxRealisationMethods.PostDate.Code)
					{
						return (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
					}
					else if (realisationMethod == TaxRealisationMethods.MatchDate.Code)
					{
						return (GLAccountRegistryType.ARControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount);
					}
				}
				return null;
			}

			(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? CheckIf_RII_TRX_AP()
			{
				if (new[] { TaxSuperTypeList.RetentionInInvoice.Code, TaxSuperTypeList.TurnoverTax.Code }.Contains(superType.ToString())
								&& ledger == LedgerTypes.AccountsPayable)
				{
					if (realisationMethod == TaxRealisationMethods.PostDate.Code)
					{
						return (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
					}
					else if (realisationMethod == TaxRealisationMethods.MatchDate.Code)
					{
						return (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount);
					}
				}
				return null;
			}

			(GLAccountRegistryType LedgerControlAccount, GLAccountRegistryType TaxControlAccount, GLAccountRegistryType TaxExpenseAccount, GLAccountRegistryType TaxPendingControlAccount)? CheckIf_SPR_AP()
			{
				if (superType == TaxSuperTypeList.StandardPaymentRetention.Code && ledger == LedgerTypes.AccountsPayable && realisationMethod == TaxRealisationMethods.PostDateOfMatchTransaction.Code)
				{
					return (GLAccountRegistryType.APControlAccount, GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount, GLAccountRegistryType.NotApplicable, GLAccountRegistryType.NotApplicable);
				}

				return null;
			}
		}

		Guid IAccTaxConfigGLAccountsProvider.GetRegistryValue(GLAccountRegistryType type)
		{
			Guid value;
			switch (type)
			{
				case GLAccountRegistryType.APControlAccount:
					value = ObjectFactory.Get<IAccounting>().APControlAccount;
					break;
				case GLAccountRegistryType.ARControlAccount:
					value = ObjectFactory.Get<IAccounting>().ARControlAccount;
					break;
				case GLAccountRegistryType.TaxTransactionPrepaidAssetControlAccount:
					value = AccountingMasterFilesRegistry.Instance.TaxTransactionPrepaidAssetControlAccount.Value;
					break;
				case GLAccountRegistryType.TaxTransactionRemittanceLiabilityControlAccount:
					value = AccountingMasterFilesRegistry.Instance.TaxTransactionRemittanceLiabilityControlAccount.Value;
					break;
				case GLAccountRegistryType.TaxTransactionExpenseAccount:
					value = AccountingMasterFilesRegistry.Instance.TaxTransactionExpenseAccount.Value;
					break;
				case GLAccountRegistryType.TaxTransactionNegativeRevenueAccount:
					value = AccountingMasterFilesRegistry.Instance.TaxTransactionNegativeRevenueAccount.Value;
					break;
				case GLAccountRegistryType.PendingTaxTransactionPrepaidAssetControlAccount:
					value = AccountingMasterFilesRegistry.Instance.PendingTaxTransactionPrepaidAssetControlAccount.Value;
					break;
				case GLAccountRegistryType.PendingTaxTransactionRemittanceLiabilityControlAccount:
					value = AccountingMasterFilesRegistry.Instance.PendingTaxTransactionRemittanceLiabilityControlAccount.Value;
					break;
				case GLAccountRegistryType.NotApplicable:
				case GLAccountRegistryType.Error:
					value = Guid.Empty;
					break;
				default:
					throw new InvalidOperationException("Invalid GLAccountRegistryType");
			}
			return value;
		}
	}
}
