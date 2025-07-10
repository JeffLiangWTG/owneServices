using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Registry.Business
{
	public class ComplianceSubTypeAttributionRuleConfigurationRegistryItem : StronglyTypedRegistryItem<ComplianceSubTypeAttributionRuleConfigurationCollection>
	{
		public ComplianceSubTypeAttributionRuleConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ComplianceSubTypeAttributionRuleConfigurationRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		class ComplianceSubTypeAttributionRuleConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public ComplianceSubTypeAttributionRuleConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new ComplianceSubTypeAttributionRuleConfigurationRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var defaultCollection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				GlbCompany company = null;

				if (companyPK != Guid.Empty)
				{
					company = new BusinessObjectFactory().Load<GlbCompany>(companyPK);
				}
				if (company == null)
				{
					return defaultCollection;
				}

				defaultCollection.SuspendValidation();

				var countryCode = company.Country.Code;
				if (Storage == RegistryStorageFlags.Branch)
				{
					GetComplianceSubTypeAttributionRuleConfigurationByTransactionHeaderBranchCollection(countryCode, companyPK, branchPK, departmentPK, defaultCollection);
				}
				else
				{
					GetComplianceSubTypeAttributionRuleConfigurationCollection(countryCode, companyPK, branchPK, departmentPK, defaultCollection);
				}

				defaultCollection.ResumeValidation();
				return defaultCollection;
			}

			void GetComplianceSubTypeAttributionRuleConfigurationCollection(ZString countryCode, Guid companyPK, Guid branchPK, Guid departmentPK, ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
			{
				CountryComplianceFactory.GetIComplianceSubTypeRuleProvider(countryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(defaultCollection);

				if (defaultCollection.Count == 0)
				{
					string ruleSetCode = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK).RuleSetCode;
					ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(countryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(defaultCollection, ruleSetCode);
				}

				#region This code is obsolete please do not add new countries in this section instead use CountryComplianceFactory
				if (defaultCollection.Count == 0)
				{
					switch (countryCode)
					{
						case Constants.CountryCodes.Peru:
							GetDefaultsForPeru(defaultCollection);
							break;
						case Constants.CountryCodes.VietNam:
							GetDefaultsForVietnam(defaultCollection);
							break;
						case Constants.CountryCodes.Ecuador:
							GetDefaultsForEcuador(defaultCollection);
							break;
						case Constants.CountryCodes.CostaRica:
							GetDefaultsForCostaRica(defaultCollection);
							break;
						case Constants.CountryCodes.Guatemala:
							GetDefaultsForGuatemala(defaultCollection);
							break;
						case Constants.CountryCodes.Honduras:
							GetDefaultsForHonduras(defaultCollection);
							break;
					}
				}
				#endregion
			}

			void GetComplianceSubTypeAttributionRuleConfigurationByTransactionHeaderBranchCollection(ZString countryCode, Guid companyPK, Guid branchPK, Guid departmentPK, ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
			{
				string ruleSetCode = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK).RuleSetCode;

				if (!string.IsNullOrEmpty(ruleSetCode))
				{
					ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(countryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(defaultCollection, ruleSetCode);
				}
			}

			static void GetDefaultsForPeru(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
			{
				ComplianceSubTypeAttributionRuleConfiguration configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Peru;
				configuration.SubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.TaxRegistrationType = TaxRegistrationTypeCodes.OrgRegisteredForTaxInPeru;
				configuration.OrganisationLocation = ZString.Empty;

				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Peru;
				configuration.SubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration.TaxRegistrationType = TaxRegistrationTypeCodes.OrgRegisteredForTaxInPeru;
				configuration.OrganisationLocation = ZString.Empty;

				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Peru;
				configuration.SubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.TaxRegistrationType = TaxRegistrationTypeCodes.OrgRegisteredForTaxInPeru;
				configuration.OrganisationLocation = ZString.Empty;

				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Peru;
				configuration.SubType = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration.OrganisationLocation = ZString.Empty;

				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Peru;
				configuration.SubType = PeruComplianceInfo.ComplianceSubTypeCodes.DSB;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration.OrganisationLocation = ZString.Empty;

				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Peru;
				configuration.SubType = PeruComplianceInfo.ComplianceSubTypeCodes.TBO;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.OrganisationLocation = ZString.Empty;

				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Peru;
				configuration.SubType = PeruComplianceInfo.ComplianceSubTypeCodes.TBC;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration.OrganisationLocation = ZString.Empty;
			}

			static void GetDefaultsForVietnam(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
			{
				ComplianceSubTypeAttributionRuleConfiguration configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.VietNam;
				configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.OrganisationLocation = Constants.CountryCodes.VietNam;

				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.VietNam;
				configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.OrganisationLocation = ZString.Empty;
			}

			static void GetDefaultsForEcuador(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
			{
				//TXI
				var configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Ecuador;
				configuration.SubType = EcuadorComplianceInfo.ComplianceSubTypeCodes.TXI;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TCR
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Ecuador;
				configuration.SubType = EcuadorComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TCR
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Ecuador;
				configuration.SubType = EcuadorComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TCD
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Ecuador;
				configuration.SubType = EcuadorComplianceInfo.ComplianceSubTypeCodes.TCD;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TXV
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Ecuador;
				configuration.SubType = EcuadorComplianceInfo.ComplianceSubTypeCodes.TXV;
				configuration.LedgerType = LedgerTypes.AccountsPayable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				configuration.OrganisationLocation = ZString.Empty;
				configuration.SelfBillingRule = SelfBillingRuleCodes.SelfBillingTransactions;

				//XCL
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Ecuador;
				configuration.SubType = EcuadorComplianceInfo.ComplianceSubTypeCodes.XCL;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//XCL
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Ecuador;
				configuration.SubType = EcuadorComplianceInfo.ComplianceSubTypeCodes.XCL;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;
			}

			static void GetDefaultsForCostaRica(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
			{
				//TCD
				var configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.CostaRica;
				configuration.SubType = CostaRicaComplianceInfo.ComplianceSubTypeCodes.TCD;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TCR
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.CostaRica;
				configuration.SubType = CostaRicaComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TCR
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.CostaRica;
				configuration.SubType = CostaRicaComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TXI
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.CostaRica;
				configuration.SubType = CostaRicaComplianceInfo.ComplianceSubTypeCodes.TXI;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//XCL
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.CostaRica;
				configuration.SubType = CostaRicaComplianceInfo.ComplianceSubTypeCodes.XCL;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//XCL
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.CostaRica;
				configuration.SubType = CostaRicaComplianceInfo.ComplianceSubTypeCodes.XCL;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;
			}

			static void GetDefaultsForGuatemala(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
			{
				//TCD
				var configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Guatemala;
				configuration.SubType = GuatemalaComplianceInfo.ComplianceSubTypeCodes.TCD;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TCR
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Guatemala;
				configuration.SubType = GuatemalaComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TCR
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Guatemala;
				configuration.SubType = GuatemalaComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TXI
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Guatemala;
				configuration.SubType = GuatemalaComplianceInfo.ComplianceSubTypeCodes.TXI;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//XCL
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Guatemala;
				configuration.SubType = GuatemalaComplianceInfo.ComplianceSubTypeCodes.XCL;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//XCL
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Guatemala;
				configuration.SubType = GuatemalaComplianceInfo.ComplianceSubTypeCodes.XCL;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//XCR
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Guatemala;
				configuration.SubType = GuatemalaComplianceInfo.ComplianceSubTypeCodes.XCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
				configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;
			}

			static void GetDefaultsForHonduras(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
			{
				//TCD
				var configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Honduras;
				configuration.SubType = HondurasComplianceInfo.ComplianceSubTypeCodes.TCD;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TCR
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Honduras;
				configuration.SubType = HondurasComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TCR
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Honduras;
				configuration.SubType = HondurasComplianceInfo.ComplianceSubTypeCodes.TCR;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//TXI
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Honduras;
				configuration.SubType = HondurasComplianceInfo.ComplianceSubTypeCodes.TXI;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//XCL
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Honduras;
				configuration.SubType = HondurasComplianceInfo.ComplianceSubTypeCodes.XCL;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;

				//XCL
				configuration = defaultCollection.AddNew();
				configuration.Country = Constants.CountryCodes.Honduras;
				configuration.SubType = HondurasComplianceInfo.ComplianceSubTypeCodes.XCL;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
				configuration.OrganisationLocation = ZString.Empty;
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ComplianceSubTypeAttributionRuleConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	class ComplianceSubTypeAttributionRuleConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceSubTypeAttributionRuleConfigurationCollection>
	{
		public ComplianceSubTypeAttributionRuleConfigurationRegistryDataType()
		{
		}
	}
}
