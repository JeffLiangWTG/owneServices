using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	public class ComplianceReportConfigurationRegistryItem : StronglyTypedRegistryItem<ComplianceReportConfigurationCollection>
	{
		public ComplianceReportConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new ComplianceReportConfigurationRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		public static ImmutableList<string> CountryCodesForServiceTask => ImmutableList.Create(
				Constants.CountryCodes.Australia,
				Constants.CountryCodes.France,
				Constants.CountryCodes.Germany,
				Constants.CountryCodes.Italy,
				Constants.CountryCodes.Norway,
				Constants.CountryCodes.Poland,
				Constants.CountryCodes.Portugal,
				Constants.CountryCodes.Taiwan,
				Constants.CountryCodes.UnitedKingdom
			);

		class ComplianceReportConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public ComplianceReportConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new ComplianceReportConfigurationRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var factory = new BusinessObjectFactory();
				var countryCode = companyPK != Guid.Empty && companyPK != Env.CurrentCompanyPK
					? factory.Load<GlbCompany>(companyPK)?.Country?.RN_Code.ToString()
					: Env.CurrentCompany.Country?.Code;

				var defaultCollection = new ComplianceReportConfigurationCollection(factory);
				defaultCollection.SuspendValidation();

				switch (countryCode)
				{
					case Constants.CountryCodes.Australia:
						GetDefaultCollectionForAustralia(companyPK, defaultCollection);
						break;
					case Constants.CountryCodes.France:
						GetDefaultCollectionForFrance(defaultCollection);
						break;
					case Constants.CountryCodes.Germany:
						GetDefaultCollectionForGermany(defaultCollection);
						break;
					case Constants.CountryCodes.Italy:
						GetDefaultCollectionForItaly(defaultCollection);
						break;
					case Constants.CountryCodes.Norway:
						GetDefaultCollectionForNorway(defaultCollection);
						break;
					case Constants.CountryCodes.Poland:
						GetDefaultCollectionForPoland(defaultCollection);
						break;
					case Constants.CountryCodes.Portugal:
						GetDefaultCollectionForPortugal(defaultCollection);
						break;
					case Constants.CountryCodes.Taiwan:
						GetDefaultCollectionForTaiwan(defaultCollection);
						break;
					case Constants.CountryCodes.UnitedKingdom:
						GetDefaultCollectionForUnitedKingdom(defaultCollection);
						break;
				}

				defaultCollection.ResumeValidation();
				return defaultCollection;
			}

			void GetDefaultCollectionForUnitedKingdom(ComplianceReportConfigurationCollection collection)
			{
				var reportConfig = collection.AddNew();
				reportConfig.Country = Constants.CountryCodes.UnitedKingdom;
				reportConfig.ReportCode = "MTD";
				reportConfig.ReportTitle = Res.GetString("e8c69128-6385-4b2d-b822-1b757117f943", "UK VAT Return");
				reportConfig.TaxRegistrationType = "VAT";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
				reportConfig.IncludeQueuedForPreviousPeriod = true;

				AddSettingForAll(reportConfig, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
				AddSettingForAll(reportConfig, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
				AddSettingForAll(reportConfig, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
				AddSettingForAll(reportConfig, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote);
				AddSettingForAll(reportConfig, LedgerTypes.CashBook, TransactionTypes.DirectPayment);
				AddSettingForAll(reportConfig, LedgerTypes.CashBook, TransactionTypes.DirectReceipt);
			}

			void GetDefaultCollectionForNorway(ComplianceReportConfigurationCollection collection)
			{
				var reportConfig = collection.AddNew();
				reportConfig.Country = Constants.CountryCodes.Norway;
				reportConfig.ReportCode = "SAF";
				reportConfig.ReportTitle = Res.GetString("12bf3cf4-c451-49b3-a03a-2d69734626c8", "NORWAY SAF-T");
				reportConfig.TaxRegistrationType = "MVA";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBookWithoutGrouping;
			}

			void GetDefaultCollectionForPortugal(ComplianceReportConfigurationCollection collection)
			{
				var reportConfigSAF = collection.AddNew();
				reportConfigSAF.Country = Constants.CountryCodes.Portugal;
				reportConfigSAF.ReportCode = "SAF";
				reportConfigSAF.ReportTitle = Res.GetString("307ab145-d42f-46cd-8665-19593e54bd09", "SAFT (PT) File");
				reportConfigSAF.TaxRegistrationType = "IVA";
				reportConfigSAF.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
				reportConfigSAF.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
				reportConfigSAF.ReportLineGrouping = ReportLineGroupingListCodes.DayBookWithoutGrouping;

				var reportConfigSAT = collection.AddNew();
				reportConfigSAT.Country = Constants.CountryCodes.Portugal;
				reportConfigSAT.ReportCode = "SAT";
				reportConfigSAT.ReportTitle = Res.GetString("f5ab63a5-bc42-4366-ac09-e28c547cdb9c", "SAFT (PT) File with transactions only");
				reportConfigSAT.TaxRegistrationType = "IVA";
				reportConfigSAT.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
				reportConfigSAT.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
				reportConfigSAT.ReportLineGrouping = ReportLineGroupingListCodes.NoGrouping;
				reportConfigSAT.IsDefaultReportType = true;

				AddSetting(reportConfigSAT, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.SBI);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.SBC);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.SBD);

				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.TCR);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.TCD);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.XCL);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.XCR);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.TDM);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.TCM);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.TXM);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.LCD);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.LCR);
				AddSetting(reportConfigSAT, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.LTX);

				void AddSetting(ComplianceReportConfiguration config, string ledger, string invoiceType, string subType = "")
				{
					var setting = config.Settings.AddNew();
					setting.LedgerType = ledger;
					setting.InvoiceType = invoiceType;
					setting.ComplianceSubType = subType;
					setting.OriginalRule = OriginalRuleCodes.AllTransactions;
					setting.DisbursementRule = DisbursementRuleCodes.AllTransactions;
					setting.TaxInvoiceRule = TaxInvoiceRuleCodes.All;
					setting.ReportingDate = ComplianceSubTypeCodesAndLists.ReportingDateCodes.InvoiceDate;
				}
			}

			void GetDefaultCollectionForItaly(ComplianceReportConfigurationCollection collection)
			{
				var reportConfig = collection.AddNew();
				reportConfig.Country = Constants.CountryCodes.Italy;
				reportConfig.ReportCode = "EST";
				reportConfig.ReportTitle = (NoResString)"Esterometro";
				reportConfig.TaxRegistrationType = "IVA";
				reportConfig.RepCountryRegistrationCode = "COD";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;

				void AddSubTypeSetting(string subType)
				{
					var setting = reportConfig.Settings.AddNew();
					setting.ComplianceSubType = subType;
				}

				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.APS);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INT);

				reportConfig = collection.AddNew();
				reportConfig.Country = Constants.CountryCodes.Italy;
				reportConfig.ReportCode = "ARC";
				reportConfig.ReportTitle = (NoResString)"Registro IVA ARC";
				reportConfig.TaxRegistrationType = "IVA";
				reportConfig.RepCountryRegistrationCode = "COD";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
				reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.ComplianceSubType;

				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INT);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.ARS);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.ARN);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.ARI);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.ARE);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.APS);

				reportConfig = collection.AddNew();
				reportConfig.Country = Constants.CountryCodes.Italy;
				reportConfig.ReportCode = "APC";
				reportConfig.ReportTitle = (NoResString)"Registro IVA APC";
				reportConfig.TaxRegistrationType = "IVA";
				reportConfig.RepCountryRegistrationCode = "COD";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
				reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.ComplianceSubType;

				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INT);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.APV);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.APS);
				AddSubTypeSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.API);

				reportConfig = collection.AddNew();
				reportConfig.Country = Constants.CountryCodes.Italy;
				reportConfig.ReportCode = "LBG";
				reportConfig.ReportTitle = (NoResString)"Libro Giornale";
				reportConfig.TaxRegistrationType = "IVA";
				reportConfig.RepCountryRegistrationCode = "COD";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBook;
				reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.ComplianceSubType;

				reportConfig = collection.AddNew();
				reportConfig.Country = Constants.CountryCodes.Italy;
				reportConfig.ReportCode = "LIQ";
				reportConfig.ReportTitle = (NoResString)"Liquidazione IVA";
				reportConfig.TaxRegistrationType = "IVA";
				reportConfig.RepCountryRegistrationCode = "COD";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
				reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.LedgerAscendinging;
				reportConfig.IsDefaultReportType = true;

				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
			}

			void GetDefaultCollectionForTaiwan(ComplianceReportConfigurationCollection collection)
			{
				var reportConfig = collection.AddNew();
				reportConfig.Country = Constants.CountryCodes.Taiwan;
				reportConfig.ReportCode = "TXT";
				reportConfig.ReportTitle = Res.GetString("fc7862dc-5e85-4d0d-aa2b-e1076e72bac2", "Purchases and Sales GUI");
				reportConfig.TaxRegistrationType = "VAT";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
				reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.FormatCodeAndDocumentNumber;

				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE);
				AddSetting(reportConfig, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TSX);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TSD);
				AddSetting(reportConfig, LedgerTypes.AccountsPayable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXS);

				var reportConfig1 = collection.AddNew();
				reportConfig1.Country = Constants.CountryCodes.Taiwan;
				reportConfig1.ReportCode = "T02";
				reportConfig1.ReportTitle = Res.GetString("905bbd19-8576-4de5-825f-78ac52481fc9", "Zero Rated Sales GUI");
				reportConfig1.TaxRegistrationType = "VAT";
				reportConfig1.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig1.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
				reportConfig1.ReportLineOrdering = ReportLineOrderingListCodes.ComplianceDocumentNumber;

				AddSetting(reportConfig1, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AddSetting(reportConfig1, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AddSetting(reportConfig1, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AddSetting(reportConfig1, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AddSetting(reportConfig1, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AddSetting(reportConfig1, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AddSetting(reportConfig1, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AddSetting(reportConfig1, LedgerTypes.AccountsReceivable, TaiwanComplianceInfo.ComplianceSubTypeCodes.ZNG, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);

				void AddSetting(ComplianceReportConfiguration config, string ledger, string subType, string taxInvoice = "")
				{
					var setting = config.Settings.AddNew();
					setting.LedgerType = ledger;
					setting.ComplianceSubType = subType;
					setting.TaxInvoiceRule = taxInvoice;
				}
			}

			void GetDefaultCollectionForAustralia(Guid companyPK, ComplianceReportConfigurationCollection collection)
			{
				if (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty))
				{
					var reportConfig = collection.AddNew();
					reportConfig.Country = Constants.CountryCodes.Australia;
					reportConfig.ReportCode = "TPR";
					reportConfig.ReportTitle = Res.GetString("1058724c-cfcd-4acf-8553-a26f38a3f074", "TPAR - Taxable Payments Annual Report");
					reportConfig.TaxRegistrationType = "ABN";
					reportConfig.ReportPeriodicity = ReportPeriodicityCodes.ComplianceFinancialYear;
					reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
					reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TransactionPayments;
					reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.Organisation;
					reportConfig.ReportAmountsRoundingType = ReportAmountsRoundingTypeListCodes.Rounding;
					reportConfig.ReportAmountsRoundingTruncating = 2;

					var setting = reportConfig.Settings.AddNew();
					setting.LedgerType = LedgerTypes.AccountsPayable;
					setting.InvoiceType = TransactionTypes.Invoice;
					setting.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
					setting.DisbursementRule = DisbursementRuleCodes.AllTransactions;
					setting.TaxInvoiceRule = TaxInvoiceRuleCodes.All;
					setting = reportConfig.Settings.AddNew();
					setting.LedgerType = LedgerTypes.AccountsPayable;
					setting.InvoiceType = TransactionTypes.CreditNote;
					setting.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
					setting.DisbursementRule = DisbursementRuleCodes.AllTransactions;
					setting.TaxInvoiceRule = TaxInvoiceRuleCodes.All;

					reportConfig = collection.AddNew();
					reportConfig.Country = Constants.CountryCodes.Australia;
					reportConfig.ReportCode = "PTS";
					reportConfig.ReportTitle = Res.GetString("976ef91c-dec7-4e10-a1f3-f231d7826f4b", "PTRS - Payments to Reportable Small Businesses");
					reportConfig.TaxRegistrationType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
					reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
					reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
					reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable;
					reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.Organisation;
					reportConfig.ReportAmountsRoundingType = ReportAmountsRoundingTypeListCodes.Rounding;
					reportConfig.ReportAmountsRoundingTruncating = 2;

					reportConfig = collection.AddNew();
					reportConfig.Country = Constants.CountryCodes.Australia;
					reportConfig.ReportCode = "TCP";
					reportConfig.ReportTitle = Res.GetString("407b4ed8-e645-4e6f-b5f3-6252304dc25b", "PTRS - Payments to ALL Creditors");
					reportConfig.TaxRegistrationType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
					reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
					reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
					reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.PaymentTimesAll;
					reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.Organisation;
					reportConfig.ReportAmountsRoundingType = ReportAmountsRoundingTypeListCodes.Rounding;
					reportConfig.ReportAmountsRoundingTruncating = 2;

					reportConfig = collection.AddNew();
					reportConfig.Country = Constants.CountryCodes.Australia;
					reportConfig.ReportCode = "PTR";
					reportConfig.ReportTitle = Res.GetString("78c7fb4b-0e5b-4456-b52d-13f652371343", "PTRS - Payments to Reportable Small Businesses (Legacy)");
					reportConfig.TaxRegistrationType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
					reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
					reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
					reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable;
					reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.Organisation;
					reportConfig.ReportAmountsRoundingType = ReportAmountsRoundingTypeListCodes.Rounding;
					reportConfig.ReportAmountsRoundingTruncating = 2;

					reportConfig = collection.AddNew();
					reportConfig.Country = Constants.CountryCodes.Australia;
					reportConfig.ReportCode = "PTA";
					reportConfig.ReportTitle = Res.GetString("004c02ad-c8fe-43d0-8d8e-1841629c1a02", "PTRS - Payments to ALL Creditors (Legacy)");
					reportConfig.TaxRegistrationType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
					reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
					reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
					reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.PaymentTimesAll;
					reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.Organisation;
					reportConfig.ReportAmountsRoundingType = ReportAmountsRoundingTypeListCodes.Rounding;
					reportConfig.ReportAmountsRoundingTruncating = 2;
				}
			}

			void GetDefaultCollectionForGermany(ComplianceReportConfigurationCollection collection)
			{
				var visibleReports = AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)
					.Cast<CodeDescriptionBool>().Where(v => v.Bool);
				ComplianceReportConfiguration reportConfig;

				if (visibleReports.Any(v => v.Code == "UVA"))
				{
					reportConfig = collection.AddNew();
					reportConfig.Country = Constants.CountryCodes.Germany;
					reportConfig.ReportCode = "UVA";
					reportConfig.ReportTitle = (NoResString)"Umsatzsteuervoranmeldung";
					reportConfig.TaxRegistrationType = "UST";
					reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
					reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
					reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode;
				}

				if (visibleReports.Any(v => v.Code == "U11"))
				{
					reportConfig = collection.AddNew();
					reportConfig.Country = Constants.CountryCodes.Germany;
					reportConfig.ReportCode = "U11";
					reportConfig.ReportTitle = "Umsatzsteuer-Sondervorauszahlung";
					reportConfig.TaxRegistrationType = "UST";
					reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
					reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
					reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.ReportSubCodeAndTransactionHeader;
				}

				if (visibleReports.Any(v => v.Code == "ZMD"))
				{
					reportConfig = collection.AddNew();
					reportConfig.Country = Constants.CountryCodes.Germany;
					reportConfig.ReportCode = "ZMD";
					reportConfig.ReportTitle = (NoResString)"Zusammenfassende Meldung";
					reportConfig.TaxRegistrationType = "UST";
					reportConfig.ReportPeriodicity = ReportPeriodicityCodes.MonthlyQuarterlyYearly;
					reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
					reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode;
				}

				reportConfig = collection.AddNew();
				reportConfig.Country = Constants.CountryCodes.Germany;
				reportConfig.ReportCode = "IDE";
				reportConfig.ReportTitle = (NoResString)"IDEA Tax Audit Export";
				reportConfig.TaxRegistrationType = "UST";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBookWithPresentation;
			}

			void GetDefaultCollectionForFrance(ComplianceReportConfigurationCollection collection)
			{
				var featureControlManager = ObjectFactory.Get<IFeatureControlManager>();
				var eReportingEnabled = featureControlManager.GetFeatureData(LicenceFeatureCodeList.Codes.AccountingFrenchEReporting) != null;

				var fecReportConfig = collection.AddNew();
				fecReportConfig.Country = Constants.CountryCodes.France;
				fecReportConfig.ReportCode = "FEC";
				fecReportConfig.ReportTitle = (NoResString)"Fichier des Écritures Comptables";
				fecReportConfig.TaxRegistrationType = "TVA";
				fecReportConfig.ReportPeriodicity = ReportPeriodicityCodes.RangeAccountingPeriod;
				fecReportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;

				if (eReportingEnabled)
				{
					var earReportConfig = collection.AddNew();
					earReportConfig.Country = Constants.CountryCodes.France;
					earReportConfig.ReportCode = "EAR";
					earReportConfig.ReportTitle = (NoResString)"FR E-Reporting Sales";
					earReportConfig.TaxRegistrationType = "TVA";
					earReportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
					earReportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;

					AddSettingForAll(earReportConfig, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice).OrganisationLocation = AccChargeTaxOverride.AllCountriesExceptLoginCountry;
					AddSettingForAll(earReportConfig, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote).OrganisationLocation = AccChargeTaxOverride.AllCountriesExceptLoginCountry;
					AddSettingForAll(earReportConfig, LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote).OrganisationLocation = AccChargeTaxOverride.AllCountriesExceptLoginCountry;
				}
			}

			void GetDefaultCollectionForPoland(ComplianceReportConfigurationCollection collection)
			{
				var config = collection.AddNew();
				config.Country = Constants.CountryCodes.Poland;
				config.ReportCode = "JPK";
				config.ReportTitle = (NoResString)"JPK (Jednolity Plik Kontrolny) Monthly File";
				config.TaxRegistrationType = "PTU";
				config.RepCountryRegistrationCode = "PTU";
				config.ReportPeriodicity = ReportPeriodicityCodes.CalendarMonth;
				config.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
				config.ReportLineOrdering = ReportLineOrderingListCodes.LedgerDescendinging;
				config.ReportAmountsRoundingType = ReportAmountsRoundingTypeListCodes.Rounding;
				config.ReportAmountsRoundingTruncating = 2;

				addSetting(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, ReportingDateCodes.EarliestTaxDate);
				addSetting(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, ReportingDateCodes.EarliestTaxDate);
				addSetting(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, ReportingDateCodes.DocumentReceivedDate);
				addSetting(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, ReportingDateCodes.DocumentReceivedDate);

				void addSetting(string ledgerType, string invoiceType, string reportingDate)
				{
					var setting = config.Settings.AddNew();
					setting.LedgerType = ledgerType;
					setting.InvoiceType = invoiceType;
					setting.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
					setting.OriginalRule = OriginalRuleCodes.AllTransactions;
					setting.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
					setting.ReportingDate = reportingDate;
				}
			}
		}

		#region Implementation

		static ComplianceReportConfigurationSetting AddSettingForAll(ComplianceReportConfiguration reportConfig, string ledger, string invoiceType)
		{
			return AddSetting(reportConfig, ledger, invoiceType, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.All);
		}

		static ComplianceReportConfigurationSetting AddSetting(ComplianceReportConfiguration reportConfig,
			string ledger,
			string invoiceType,
			string originalRule = "",
			string disbursementRule = "",
			string taxInvoiceRule = "")
		{
			var setting = reportConfig.Settings.AddNew();
			setting.LedgerType = ledger;
			setting.InvoiceType = invoiceType;
			setting.OriginalRule = originalRule;
			setting.DisbursementRule = disbursementRule;
			setting.TaxInvoiceRule = taxInvoiceRule;
			return setting;
		}

		#endregion
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.ComplianceReportConfigurationRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class ComplianceReportConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceReportConfigurationCollection>
	{
		public ComplianceReportConfigurationRegistryDataType()
		{
		}
	}
}
