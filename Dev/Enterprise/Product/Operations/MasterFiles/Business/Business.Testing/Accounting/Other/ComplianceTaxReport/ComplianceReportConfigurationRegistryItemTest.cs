using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportConfigurationRegistryItem))]
	sealed class ComplianceReportConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceReportConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<ComplianceReportConfigurationCollection, ComplianceReportConfigurationCollection> GetNewRegistryItem()
		{
			return new ComplianceReportConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}

		public void TestDefaultValueForTW()
		{
			AssertCollectionContains("List of countries having default value for CRQ contains Taiwan", Constants.CountryCodes.Taiwan, ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var registryItem = GetNewRegistryItem();
				var defaultValues = registryItem.DefaultValue;
				AssertEquals(2, defaultValues.Count);

				AssertEquals(Constants.CountryCodes.Taiwan, defaultValues[0].Country);
				AssertEquals("TXT", defaultValues[0].ReportCode);
				AssertEquals("Purchases and Sales GUI", defaultValues[0].ReportTitle);
				AssertEquals("VAT", defaultValues[0].TaxRegistrationType);
				AssertEquals(ReportPeriodicityCodes.DateRange, defaultValues[0].ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.ComplianceDocumentHeader, defaultValues[0].ReportBaseTablePrefix);
				AssertEquals(ReportLineOrderingListCodes.FormatCodeAndDocumentNumber, defaultValues[0].ReportLineOrdering);

				var setting = defaultValues[0].Settings;
				AssertEquals(23, setting.Count);
				AssertSettings(setting[0], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[1], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[2], TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[3], TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[4], TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[5], TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[6], TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[7], TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[8], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[9], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, LedgerTypes.AccountsReceivable);
				AssertSettings(setting[10], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI, LedgerTypes.AccountsPayable);
				AssertSettings(setting[11], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP, LedgerTypes.AccountsPayable);
				AssertSettings(setting[12], TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI, LedgerTypes.AccountsPayable);
				AssertSettings(setting[13], TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC, LedgerTypes.AccountsPayable);
				AssertSettings(setting[14], TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, LedgerTypes.AccountsPayable);
				AssertSettings(setting[15], TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, LedgerTypes.AccountsPayable);
				AssertSettings(setting[16], TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE, LedgerTypes.AccountsPayable);
				AssertSettings(setting[17], TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD, LedgerTypes.AccountsPayable);
				AssertSettings(setting[18], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, LedgerTypes.AccountsPayable);
				AssertSettings(setting[19], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, LedgerTypes.AccountsPayable);
				AssertSettings(setting[20], TaiwanComplianceInfo.ComplianceSubTypeCodes.TSX, LedgerTypes.AccountsPayable);
				AssertSettings(setting[21], TaiwanComplianceInfo.ComplianceSubTypeCodes.TSD, LedgerTypes.AccountsPayable);
				AssertSettings(setting[22], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXS, LedgerTypes.AccountsPayable);

				AssertEquals(Constants.CountryCodes.Taiwan, defaultValues[1].Country);
				AssertEquals("T02", defaultValues[1].ReportCode);
				AssertEquals("Zero Rated Sales GUI", defaultValues[1].ReportTitle);
				AssertEquals("VAT", defaultValues[1].TaxRegistrationType);
				AssertEquals(ReportPeriodicityCodes.DateRange, defaultValues[1].ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.ComplianceDocumentHeader, defaultValues[1].ReportBaseTablePrefix);
				AssertEquals(ReportLineOrderingListCodes.ComplianceDocumentNumber, defaultValues[1].ReportLineOrdering);

				setting = defaultValues[1].Settings;
				AssertEquals(8, setting.Count);
				AssertSettings(setting[0], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI, LedgerTypes.AccountsReceivable, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AssertSettings(setting[1], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP, LedgerTypes.AccountsReceivable, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AssertSettings(setting[2], TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI, LedgerTypes.AccountsReceivable, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AssertSettings(setting[3], TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC, LedgerTypes.AccountsReceivable, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AssertSettings(setting[4], TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, LedgerTypes.AccountsReceivable, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AssertSettings(setting[5], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, LedgerTypes.AccountsReceivable, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AssertSettings(setting[6], TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, LedgerTypes.AccountsReceivable, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
				AssertSettings(setting[7], TaiwanComplianceInfo.ComplianceSubTypeCodes.ZNG, LedgerTypes.AccountsReceivable, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount);
			}

			void AssertSettings(ComplianceReportConfigurationSetting setting, string subType, string ledger, string taxInvoice = "")
			{
				AssertEquals(subType, setting.ComplianceSubType);
				AssertEquals(ledger, setting.LedgerType);
				AssertEquals(taxInvoice, setting.TaxInvoiceRule);
			}
		}

		public void TestDefaultValueForPT()
		{
			AssertCollectionContains("List of countries having default value for CRQ contains Portugal", Constants.CountryCodes.Portugal, ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var registryItem = GetNewRegistryItem();
				var defaultValues = registryItem.DefaultValue;
				AssertEquals(2, defaultValues.Count);

				AssertEquals(Constants.CountryCodes.Portugal, defaultValues[0].Country);
				AssertEquals("SAF", defaultValues[0].ReportCode);
				AssertEquals("SAFT (PT) File", defaultValues[0].ReportTitle);
				AssertEquals("IVA", defaultValues[0].TaxRegistrationType);
				AssertEquals(ReportPeriodicityCodes.AccountingPeriod, defaultValues[0].ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.AllTransactions, defaultValues[0].ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.DayBookWithoutGrouping, defaultValues[0].ReportLineGrouping);
				AssertEquals(0, defaultValues[0].Settings.Count);
				Assert(!defaultValues[0].IsDefaultReportType);

				AssertEquals(Constants.CountryCodes.Portugal, defaultValues[1].Country);
				AssertEquals("SAT", defaultValues[1].ReportCode);
				AssertEquals("SAFT (PT) File with transactions only", defaultValues[1].ReportTitle);
				AssertEquals("IVA", defaultValues[1].TaxRegistrationType);
				AssertEquals(ReportPeriodicityCodes.AccountingPeriod, defaultValues[1].ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, defaultValues[1].ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.NoGrouping, defaultValues[1].ReportLineGrouping);
				Assert("SAT is the default report type for Portugal", defaultValues[1].IsDefaultReportType);

				var settings = defaultValues[1].Settings;
				AssertEquals(14, settings.Count);
				AssertSettings(settings[0], LedgerTypes.AccountsPayable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.SBI);
				AssertSettings(settings[1], LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.SBC);
				AssertSettings(settings[2], LedgerTypes.AccountsPayable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.SBD);

				AssertSettings(settings[3], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI);
				AssertSettings(settings[4], LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.TCR);
				AssertSettings(settings[5], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.TCD);
				AssertSettings(settings[6], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.XCL);
				AssertSettings(settings[7], LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.XCR);
				AssertSettings(settings[8], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.TDM);
				AssertSettings(settings[9], LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.TCM);
				AssertSettings(settings[10], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.TXM);
				AssertSettings(settings[11], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.LCD);
				AssertSettings(settings[12], LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, PortugalComplianceInfo.ComplianceSubTypeCodes.LCR);
				AssertSettings(settings[13], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, PortugalComplianceInfo.ComplianceSubTypeCodes.LTX);
			}

			void AssertSettings(ComplianceReportConfigurationSetting setting, string ledger, string invoiceType, string subType = "")
			{
				AssertEquals(subType, setting.ComplianceSubType);
				AssertEquals(ledger, setting.LedgerType);
				AssertEquals(invoiceType, setting.InvoiceType);
				AssertEquals(OriginalRuleCodes.AllTransactions, setting.OriginalRule);
				AssertEquals(DisbursementRuleCodes.AllTransactions, setting.DisbursementRule);
				AssertEquals(TaxInvoiceRuleCodes.All, setting.TaxInvoiceRule);
				AssertEquals(string.Empty, setting.TaxRegistrationType);
				AssertEquals(string.Empty, setting.OrganisationLocation);
				AssertEquals(ComplianceSubTypeCodesAndLists.ReportingDateCodes.InvoiceDate, setting.ReportingDate);
			}
		}

		public void TestDefaultValueForAU()
		{
			AssertCollectionContains("List of countries having default for CRQ value contains Australia", Constants.CountryCodes.Australia, ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				AssertEquals("AU Compliance Reporting is disabled by default", false, AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.Value);
				var registryItem = GetNewRegistryItem();
				var defaultValues = registryItem.DefaultValue;
				AssertEquals("Expect no reports", 0, defaultValues.Count);

				using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.Instance.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					registryItem = GetNewRegistryItem();
					defaultValues = registryItem.DefaultValue;
					AssertEquals("Expect 5 report", 5, defaultValues.Count);

					var report = defaultValues.OfType<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == "TPR");
					AssertNotNull("TPR report", report);
					AssertEquals("Country", Constants.CountryCodes.Australia, report.Country);
					AssertEquals("ReportCode", "TPR", report.ReportCode);
					AssertEquals("ReportTitle", "TPAR - Taxable Payments Annual Report", report.ReportTitle);
					AssertEquals("TaxRegistrationType", "ABN", report.TaxRegistrationType);
					AssertEquals("ReportPeriodicity", ReportPeriodicityCodes.ComplianceFinancialYear, report.ReportPeriodicity);
					AssertEquals("ReportBaseTablePrefix", ReportBaseTablePrefixListCodes.TransactionHeader, report.ReportBaseTablePrefix);
					AssertEquals("ReportLineGrouping", ReportLineGroupingListCodes.TransactionPayments, report.ReportLineGrouping);
					AssertEquals("ReportLineOrdering", ReportLineOrderingListCodes.Organisation, report.ReportLineOrdering);
					AssertEquals("GoodsServiceType", ZString.Empty, report.GoodsServiceType);
					AssertEquals("ReportAmountsRoundingType", ReportAmountsRoundingTypeListCodes.Rounding, report.ReportAmountsRoundingType);
					AssertEquals("ReportAmountsRoundingTruncating", 2, report.ReportAmountsRoundingTruncating);

					var settings = report.Settings.OfType<ComplianceReportConfigurationSetting>();
					AssertEquals("Expect two settings on report", 2, settings.Count());

					var invSetting = settings.FirstOrDefault(x => x.InvoiceType == TransactionTypes.Invoice);
					AssertNotNull("AP INV", invSetting);
					AssertSetting(invSetting);

					var crdSetting = settings.FirstOrDefault(x => x.InvoiceType == TransactionTypes.CreditNote);
					AssertNotNull("AP CRD", crdSetting);
					AssertSetting(crdSetting);

					report = defaultValues.OfType<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == "PTR");
					AssertNotNull("PTR report", report);
					AssertEquals("Country", Constants.CountryCodes.Australia, report.Country);
					AssertEquals("ReportCode", "PTR", report.ReportCode);
					AssertEquals("ReportTitle", "PTRS - Payments to Reportable Small Businesses (Legacy)", report.ReportTitle);
					AssertEquals("TaxRegistrationType", "ABN", report.TaxRegistrationType);
					AssertEquals("ReportPeriodicity", ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
					AssertEquals("ReportBaseTablePrefix", ReportBaseTablePrefixListCodes.TransactionHeader, report.ReportBaseTablePrefix);
					AssertEquals("ReportLineGrouping", ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable, report.ReportLineGrouping);
					AssertEquals("ReportLineOrdering", ReportLineOrderingListCodes.Organisation, report.ReportLineOrdering);
					AssertEquals("GoodsServiceType", ZString.Empty, report.GoodsServiceType);
					AssertEquals("ReportAmountsRoundingType", ReportAmountsRoundingTypeListCodes.Rounding, report.ReportAmountsRoundingType);
					AssertEquals("ReportAmountsRoundingTruncating", 2, report.ReportAmountsRoundingTruncating);
					Assert("Expect no settings on report", !report.Settings.OfType<ComplianceReportConfigurationSetting>().Any());

					report = defaultValues.OfType<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == "PTA");
					AssertNotNull("PTA report", report);
					AssertEquals("Country", Constants.CountryCodes.Australia, report.Country);
					AssertEquals("ReportCode", "PTA", report.ReportCode);
					AssertEquals("ReportTitle", "PTRS - Payments to ALL Creditors (Legacy)", report.ReportTitle);
					AssertEquals("TaxRegistrationType", "ABN", report.TaxRegistrationType);
					AssertEquals("ReportPeriodicity", ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
					AssertEquals("ReportBaseTablePrefix", ReportBaseTablePrefixListCodes.TransactionHeader, report.ReportBaseTablePrefix);
					AssertEquals("ReportLineGrouping", ReportLineGroupingListCodes.PaymentTimesAll, report.ReportLineGrouping);
					AssertEquals("ReportLineOrdering", ReportLineOrderingListCodes.Organisation, report.ReportLineOrdering);
					AssertEquals("GoodsServiceType", ZString.Empty, report.GoodsServiceType);
					AssertEquals("ReportAmountsRoundingType", ReportAmountsRoundingTypeListCodes.Rounding, report.ReportAmountsRoundingType);
					AssertEquals("ReportAmountsRoundingTruncating", 2, report.ReportAmountsRoundingTruncating);
					Assert("Expect no settings on report", !report.Settings.OfType<ComplianceReportConfigurationSetting>().Any());

					report = defaultValues.OfType<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == "PTS");
					AssertNotNull("PTS report", report);
					AssertEquals("Country", Constants.CountryCodes.Australia, report.Country);
					AssertEquals("ReportCode", "PTS", report.ReportCode);
					AssertEquals("ReportTitle", "PTRS - Payments to Reportable Small Businesses", report.ReportTitle);
					AssertEquals("TaxRegistrationType", "ABN", report.TaxRegistrationType);
					AssertEquals("ReportPeriodicity", ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
					AssertEquals("ReportBaseTablePrefix", ReportBaseTablePrefixListCodes.TransactionHeader, report.ReportBaseTablePrefix);
					AssertEquals("ReportLineGrouping", ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable, report.ReportLineGrouping);
					AssertEquals("ReportLineOrdering", ReportLineOrderingListCodes.Organisation, report.ReportLineOrdering);
					AssertEquals("GoodsServiceType", ZString.Empty, report.GoodsServiceType);
					AssertEquals("ReportAmountsRoundingType", ReportAmountsRoundingTypeListCodes.Rounding, report.ReportAmountsRoundingType);
					AssertEquals("ReportAmountsRoundingTruncating", 2, report.ReportAmountsRoundingTruncating);
					Assert("Expect no settings on report", !report.Settings.OfType<ComplianceReportConfigurationSetting>().Any());

					report = defaultValues.OfType<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == "TCP");
					AssertNotNull("TCP report", report);
					AssertEquals("Country", Constants.CountryCodes.Australia, report.Country);
					AssertEquals("ReportCode", "TCP", report.ReportCode);
					AssertEquals("ReportTitle", "PTRS - Payments to ALL Creditors", report.ReportTitle);
					AssertEquals("TaxRegistrationType", "ABN", report.TaxRegistrationType);
					AssertEquals("ReportPeriodicity", ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
					AssertEquals("ReportBaseTablePrefix", ReportBaseTablePrefixListCodes.TransactionHeader, report.ReportBaseTablePrefix);
					AssertEquals("ReportLineGrouping", ReportLineGroupingListCodes.PaymentTimesAll, report.ReportLineGrouping);
					AssertEquals("ReportLineOrdering", ReportLineOrderingListCodes.Organisation, report.ReportLineOrdering);
					AssertEquals("GoodsServiceType", ZString.Empty, report.GoodsServiceType);
					AssertEquals("ReportAmountsRoundingType", ReportAmountsRoundingTypeListCodes.Rounding, report.ReportAmountsRoundingType);
					AssertEquals("ReportAmountsRoundingTruncating", 2, report.ReportAmountsRoundingTruncating);
					Assert("Expect no settings on report", !report.Settings.OfType<ComplianceReportConfigurationSetting>().Any());

					void AssertSetting(ComplianceReportConfigurationSetting setting)
					{
						AssertEquals("ComplianceSubType", ZString.Empty, setting.ComplianceSubType);
						AssertEquals("LedgerType", LedgerTypes.AccountsPayable, setting.LedgerType);
						AssertEquals("OriginalRule", OriginalRuleCodes.OriginalTransactionOnly, setting.OriginalRule);
						AssertEquals("DisbursementRule", DisbursementRuleCodes.AllTransactions, setting.DisbursementRule);
						AssertEquals("TaxInvoiceRule", TaxInvoiceRuleCodes.All, setting.TaxInvoiceRule);
					}
				}
			}
		}

		public void TestDefaultValueForDE()
		{
			AssertCollectionContains("List of countries having default value for CRQ contains Germany", Constants.CountryCodes.Germany, ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AccountingMasterFilesRegistryTest.EnableAllGermanComplianceReports();
				var registryItem = GetNewRegistryItem();
				var defaultValues = registryItem.DefaultValue;
				AssertEquals("Number of expected reports", 4, defaultValues.Count);

				var report = defaultValues[0];
				AssertEquals(Constants.CountryCodes.Germany, report.Country);
				AssertEquals("UVA", report.ReportCode);
				AssertEquals("Umsatzsteuervoranmeldung", report.ReportTitle);
				AssertEquals("UST", report.TaxRegistrationType);
				AssertEquals(ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, report.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode, report.ReportLineGrouping);
				AssertEquals(ZString.Empty, report.ReportLineOrdering);
				AssertEquals(ZString.Empty, report.GoodsServiceType);
				AssertEquals(ZString.Empty, report.ReportAmountsRoundingType);
				AssertEquals(0, report.ReportAmountsRoundingTruncating);
				AssertEquals(ZString.Empty, report.AmountThresholdLevel);
				AssertEquals(0m, report.ExTaxAmountThreshold);
				AssertEquals(0m, report.TaxAmountThreshold);
				AssertEquals(ZGuid.Empty, report.RecipientOrgPK);
				AssertEquals(false, report.IncludeQueuedForPreviousPeriod);
				AssertEquals(false, report.IsDefaultReportType);

				report = defaultValues[1];
				AssertEquals(Constants.CountryCodes.Germany, report.Country);
				AssertEquals("U11", report.ReportCode);
				AssertEquals("Umsatzsteuer-Sondervorauszahlung", report.ReportTitle);
				AssertEquals("UST", report.TaxRegistrationType);
				AssertEquals(ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, report.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.ReportSubCodeAndTransactionHeader, report.ReportLineGrouping);
				AssertEquals(ZString.Empty, report.ReportLineOrdering);
				AssertEquals(ZString.Empty, report.GoodsServiceType);
				AssertEquals(ZString.Empty, report.ReportAmountsRoundingType);
				AssertEquals(0, report.ReportAmountsRoundingTruncating);
				AssertEquals(ZString.Empty, report.AmountThresholdLevel);
				AssertEquals(0m, report.ExTaxAmountThreshold);
				AssertEquals(0m, report.TaxAmountThreshold);
				AssertEquals(ZGuid.Empty, report.RecipientOrgPK);
				AssertEquals(false, report.IncludeQueuedForPreviousPeriod);
				AssertEquals(false, report.IsDefaultReportType);

				report = defaultValues[2];
				AssertEquals(Constants.CountryCodes.Germany, report.Country);
				AssertEquals("ZMD", report.ReportCode);
				AssertEquals("Zusammenfassende Meldung", report.ReportTitle);
				AssertEquals("UST", report.TaxRegistrationType);
				AssertEquals(ReportPeriodicityCodes.MonthlyQuarterlyYearly, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, report.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode, report.ReportLineGrouping);
			}
		}

		public void TestDefaultValueForIT()
		{
			AssertCollectionContains("List of countries having default value for CRQ contains Italy", Constants.CountryCodes.Italy, ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var registryItem = GetNewRegistryItem();
				var defaultValues = registryItem.DefaultValue;
				AssertEquals("Expected five reports", 5, defaultValues.Count);

				var report = defaultValues[0];
				AssertEquals(Constants.CountryCodes.Italy, report.Country);
				AssertEquals("EST", report.ReportCode);
				AssertEquals("Esterometro", report.ReportTitle);
				AssertEquals("IVA", report.TaxRegistrationType);
				AssertEquals("COD", report.RepCountryRegistrationCode);
				AssertEquals(ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, report.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.TaxReporting, report.ReportLineGrouping);
				AssertEquals(ZString.Empty, report.ReportLineOrdering);
				AssertEquals(ZString.Empty, report.GoodsServiceType);
				AssertEquals(ZString.Empty, report.ReportAmountsRoundingType);
				AssertEquals(0, report.ReportAmountsRoundingTruncating);
				AssertEquals(ZString.Empty, report.AmountThresholdLevel);
				AssertEquals(0m, report.ExTaxAmountThreshold);
				AssertEquals(0m, report.TaxAmountThreshold);
				AssertEquals(ZGuid.Empty, report.RecipientOrgPK);
				AssertEquals(false, report.IncludeQueuedForPreviousPeriod);
				AssertEquals(false, report.IsDefaultReportType);

				var repSettings = report.Settings.OfType<ComplianceReportConfigurationSetting>();
				AssertEquals("Expect two settings for EST report", 2, repSettings.Count());
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.APS);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INT);

				report = defaultValues[1];
				AssertEquals(Constants.CountryCodes.Italy, report.Country);
				AssertEquals("ARC", report.ReportCode);
				AssertEquals("Registro IVA ARC", report.ReportTitle);
				AssertEquals("IVA", report.TaxRegistrationType);
				AssertEquals("COD", report.RepCountryRegistrationCode);
				AssertEquals(ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, report.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.TaxReporting, report.ReportLineGrouping);
				AssertEquals(ReportLineOrderingListCodes.ComplianceSubType, report.ReportLineOrdering);
				AssertEquals(ZString.Empty, report.GoodsServiceType);
				AssertEquals(ZString.Empty, report.ReportAmountsRoundingType);
				AssertEquals(0, report.ReportAmountsRoundingTruncating);
				AssertEquals(ZString.Empty, report.AmountThresholdLevel);
				AssertEquals(0m, report.ExTaxAmountThreshold);
				AssertEquals(0m, report.TaxAmountThreshold);
				AssertEquals(ZGuid.Empty, report.RecipientOrgPK);
				AssertEquals(false, report.IncludeQueuedForPreviousPeriod);
				AssertEquals(false, report.IsDefaultReportType);

				repSettings = report.Settings.OfType<ComplianceReportConfigurationSetting>();
				AssertEquals("Expect seven settings for ARC report", 7, repSettings.Count());
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.APS);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.ARE);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.ARI);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.ARN);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.ARS);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INT);

				report = defaultValues[2];
				AssertEquals(Constants.CountryCodes.Italy, report.Country);
				AssertEquals("APC", report.ReportCode);
				AssertEquals("Registro IVA APC", report.ReportTitle);
				AssertEquals("IVA", report.TaxRegistrationType);
				AssertEquals("COD", report.RepCountryRegistrationCode);
				AssertEquals(ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, report.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.TaxReporting, report.ReportLineGrouping);
				AssertEquals(ReportLineOrderingListCodes.ComplianceSubType, report.ReportLineOrdering);
				AssertEquals(ZString.Empty, report.GoodsServiceType);
				AssertEquals(ZString.Empty, report.ReportAmountsRoundingType);
				AssertEquals(0, report.ReportAmountsRoundingTruncating);
				AssertEquals(ZString.Empty, report.AmountThresholdLevel);
				AssertEquals(0m, report.ExTaxAmountThreshold);
				AssertEquals(0m, report.TaxAmountThreshold);
				AssertEquals(ZGuid.Empty, report.RecipientOrgPK);
				AssertEquals(false, report.IncludeQueuedForPreviousPeriod);
				AssertEquals(false, report.IsDefaultReportType);

				repSettings = report.Settings.OfType<ComplianceReportConfigurationSetting>();
				AssertEquals("Expect five settings for APC report", 5, repSettings.Count());
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.API);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.APS);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.APV);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INI);
				AssertSetting(ItalyComplianceInfo.ComplianceSubTypeCodes.INT);

				report = defaultValues[3];
				AssertEquals(Constants.CountryCodes.Italy, report.Country);
				AssertEquals("LBG", report.ReportCode);
				AssertEquals("Libro Giornale", report.ReportTitle);
				AssertEquals("IVA", report.TaxRegistrationType);
				AssertEquals("COD", report.RepCountryRegistrationCode);
				AssertEquals(ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.AllTransactions, report.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.DayBook, report.ReportLineGrouping);
				AssertEquals(ReportLineOrderingListCodes.ComplianceSubType, report.ReportLineOrdering);
				AssertEquals(ZString.Empty, report.GoodsServiceType);
				AssertEquals(ZString.Empty, report.ReportAmountsRoundingType);
				AssertEquals(0, report.ReportAmountsRoundingTruncating);
				AssertEquals(ZString.Empty, report.AmountThresholdLevel);
				AssertEquals(0m, report.ExTaxAmountThreshold);
				AssertEquals(0m, report.TaxAmountThreshold);
				AssertEquals(ZGuid.Empty, report.RecipientOrgPK);
				AssertEquals(false, report.IncludeQueuedForPreviousPeriod);
				AssertEquals(false, report.IsDefaultReportType);

				AssertEquals("Expect no settings for LBG report", 0, report.Settings.Count);

				report = defaultValues[4];
				AssertEquals(Constants.CountryCodes.Italy, report.Country);
				AssertEquals("LIQ", report.ReportCode);
				AssertEquals("Liquidazione IVA", report.ReportTitle);
				AssertEquals("IVA", report.TaxRegistrationType);
				AssertEquals("COD", report.RepCountryRegistrationCode);
				AssertEquals(ReportPeriodicityCodes.DateRange, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, report.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.TaxReporting, report.ReportLineGrouping);
				AssertEquals(ReportLineOrderingListCodes.LedgerAscendinging, report.ReportLineOrdering);
				AssertEquals(ZString.Empty, report.GoodsServiceType);
				AssertEquals(ZString.Empty, report.ReportAmountsRoundingType);
				AssertEquals(0, report.ReportAmountsRoundingTruncating);
				AssertEquals(ZString.Empty, report.AmountThresholdLevel);
				AssertEquals(0m, report.ExTaxAmountThreshold);
				AssertEquals(0m, report.TaxAmountThreshold);
				AssertEquals(ZGuid.Empty, report.RecipientOrgPK);
				AssertEquals(false, report.IncludeQueuedForPreviousPeriod);
				AssertEquals(true, report.IsDefaultReportType);

				repSettings = report.Settings.OfType<ComplianceReportConfigurationSetting>();
				AssertEquals("Expect six settings for Liquidazione IVA", 6, repSettings.Count());
				AssertSetting("", LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AssertSetting("", LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AssertSetting("", LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AssertSetting("", LedgerTypes.AccountsPayable, TransactionTypes.Invoice, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AssertSetting("", LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);
				AssertSetting("", LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID);

				void AssertSetting(string subType, string ledger = "", string invoiceType = "", string originalRule = "", string disbursementRule = "", string taxInvoice = "")
				{
					var setting = repSettings.FirstOrDefault(x => x.ComplianceSubType == subType && x.LedgerType == ledger && x.InvoiceType == invoiceType && x.OriginalRule == originalRule && x.DisbursementRule == disbursementRule && x.TaxInvoiceRule == taxInvoice);
					AssertNotNull($"IT - incorrect Settings, must contain SubType = '{subType}' - LedgerType = '{ledger}' - InvoiceType = '{invoiceType}' - OriginalRule = '{originalRule}' - DisbursementRule = '{disbursementRule}' - TaxInvoiceRule = '{taxInvoice}'.", setting);

					AssertEquals($"IT - TaxRegistrationLocationRule: {subType} - incorrect TaxRegistrationLocationRule", string.Empty, setting.TaxRegistrationLocationRule);
					AssertEquals($"IT - OrganisationLocation: {subType} - incorrect OrganisationLocation", string.Empty, setting.OrganisationLocation);
				}
			}
		}

		public void TestDefaultValueForNO()
		{
			AssertCollectionContains("List of countries having default value for CRQ contains Norway", Constants.CountryCodes.Norway, ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Norway))
			{
				var registryItem = GetNewRegistryItem();
				var defaultValues = registryItem.DefaultValue;
				AssertEquals("Expected only one report", 1, defaultValues.Count);

				var report = defaultValues[0];
				AssertEquals(Constants.CountryCodes.Norway, report.Country);
				AssertEquals("SAF", report.ReportCode);
				AssertEquals("NORWAY SAF-T", report.ReportTitle);
				AssertEquals("MVA", report.TaxRegistrationType);
				AssertEquals(ReportPeriodicityCodes.AccountingPeriod, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.AllTransactions, report.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.DayBookWithoutGrouping, report.ReportLineGrouping);
				AssertEquals(ZString.Empty, report.ReportLineOrdering);
				AssertEquals(ZString.Empty, report.GoodsServiceType);
				AssertEquals(ZString.Empty, report.ReportAmountsRoundingType);
				AssertEquals(0, report.ReportAmountsRoundingTruncating);
				AssertEquals(ZString.Empty, report.AmountThresholdLevel);
				AssertEquals(0m, report.ExTaxAmountThreshold);
				AssertEquals(0m, report.TaxAmountThreshold);
				AssertEquals(ZGuid.Empty, report.RecipientOrgPK);
				AssertEquals(false, report.IncludeQueuedForPreviousPeriod);
			}
		}

		public void TestDefaultValueForPL()
		{
			AssertCollectionContains("List of countries having default value for CRQ contains Poland", Constants.CountryCodes.Poland, ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Poland))
			{
				var registryItem = GetNewRegistryItem();
				var defaultValues = registryItem.DefaultValue;
				AssertEquals("Expected only one report", 1, defaultValues.Count);

				var report = defaultValues[0];
				AssertEquals(Constants.CountryCodes.Poland, report.Country);
				AssertEquals("JPK", report.ReportCode);
				AssertEquals("JPK (Jednolity Plik Kontrolny) Monthly File", report.ReportTitle);
				AssertEquals("PTU", report.TaxRegistrationType);
				AssertEquals("PTU", report.RepCountryRegistrationCode);
				AssertEquals(ReportPeriodicityCodes.CalendarMonth, report.ReportPeriodicity);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, report.ReportBaseTablePrefix);
				AssertEquals(ZString.Empty, report.ReportLineGrouping);
				AssertEquals(ReportLineOrderingListCodes.LedgerDescendinging, report.ReportLineOrdering);
				AssertEquals(ZString.Empty, report.GoodsServiceType);
				AssertEquals(ReportAmountsRoundingTypeListCodes.Rounding, report.ReportAmountsRoundingType);
				AssertEquals(2, report.ReportAmountsRoundingTruncating);
				AssertEquals(ZString.Empty, report.AmountThresholdLevel);
				AssertEquals(0m, report.ExTaxAmountThreshold);
				AssertEquals(0m, report.TaxAmountThreshold);
				AssertEquals(ZGuid.Empty, report.RecipientOrgPK);
				AssertEquals(false, report.IncludeQueuedForPreviousPeriod);

				var settings = report.Settings.OfType<ComplianceReportConfigurationSetting>();
				AssertEquals("Expect four settings on report", 4, settings.Count());

				assertSetting(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, expectedReportingDate: ReportingDateCodes.EarliestTaxDate);
				assertSetting(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, expectedReportingDate: ReportingDateCodes.EarliestTaxDate);
				assertSetting(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, expectedReportingDate: ReportingDateCodes.DocumentReceivedDate);
				assertSetting(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, expectedReportingDate: ReportingDateCodes.DocumentReceivedDate);

				void assertSetting(ZString ledgerType, ZString invoiceType, ZString expectedReportingDate)
				{
					var msg = $"{ledgerType} {invoiceType}";
					var setting = settings.FirstOrDefault(x => x.LedgerType == ledgerType && x.InvoiceType == invoiceType);
					AssertNotNull(msg, setting);

					AssertEquals(msg + " ComplianceSubType", ZString.Empty, setting.ComplianceSubType);
					AssertEquals(msg + " OriginalRule", OriginalRuleCodes.AllTransactions, setting.OriginalRule);
					AssertEquals(msg + " DisbursementRule", DisbursementRuleCodes.NonDisbursementOnly, setting.DisbursementRule);
					AssertEquals(msg + " TaxInvoiceRule", TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID, setting.TaxInvoiceRule);
					AssertEquals(msg + " TaxRegistrationType", ZString.Empty, setting.TaxRegistrationType);
					AssertEquals(msg + " OrganisationLocation", ZString.Empty, setting.OrganisationLocation);
					AssertEquals(msg + " TaxRegistrationLocationRule", ZString.Empty, setting.TaxRegistrationLocationRule);
					AssertEquals(msg + " ReportingDate", expectedReportingDate, setting.ReportingDate);
				}
			}
		}

		public void TestDefaultValueForFR()
		{
			AssertCollectionContains("List of countries having default value for CRQ contains France", Constants.CountryCodes.France, ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var registryItem = GetNewRegistryItem();
				var defaultValues = registryItem.DefaultValue;
				AssertEquals("Expected only one report", 1, defaultValues.Count);

				Assert_FR_FEC_Report(defaultValues[0]);
			}
		}

		public void TestDefaultForFR_EReportingEnabled()
		{
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.AccountingFrenchEReporting, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			using (ObjectFactory.Substitute(featureControlMock.Object))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
				{
					var registryItem = GetNewRegistryItem();
					var defaultValues = registryItem.DefaultValue;
					AssertEquals("Expected two reports", 2, defaultValues.Count);

					Assert_FR_FEC_Report(defaultValues[0]);
					Assert_FR_EAR_Report(defaultValues[1]);
				}
			}
		}

		public void TestDefaultValueForGB()
		{
			AssertCollectionContains("List of countries having default value for CRQ contains United Kingdom", Constants.CountryCodes.UnitedKingdom, ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask);
		}

		public void TestDefaultValueForAllCountries()
		{
			var defaultedCountries = ComplianceReportConfigurationRegistryItem.CountryCodesForServiceTask;

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.Instance.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var countriesCollection = new RefCountryCollection(new BusinessObjectFactory());
				foreach (var cc in countriesCollection.Select(rc => rc.Code))
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(cc))
					{
						var registryItem = GetNewRegistryItem();
						AssertEquals($"Expected reports for {cc}", defaultedCountries.Contains(cc), registryItem.DefaultValue.Any());
					}
				}
			}
		}

		void Assert_FR_FEC_Report(ComplianceReportConfiguration fecReport)
		{
			AssertEquals(Constants.CountryCodes.France, fecReport.Country);
			AssertEquals("FEC", fecReport.ReportCode);
			AssertEquals("Fichier des Écritures Comptables", fecReport.ReportTitle);
			AssertEquals("TVA", fecReport.TaxRegistrationType);
			AssertEquals(ReportPeriodicityCodes.RangeAccountingPeriod, fecReport.ReportPeriodicity);
			AssertEquals(ReportBaseTablePrefixListCodes.AllTransactions, fecReport.ReportBaseTablePrefix);

			var fecSettings = fecReport.Settings.OfType<ComplianceReportConfigurationSetting>();
			AssertEquals("Expect no settings on FEC report", 0, fecSettings.Count());
		}

		void Assert_FR_EAR_Report(ComplianceReportConfiguration earReport)
		{
			AssertEquals(Constants.CountryCodes.France, earReport.Country);
			AssertEquals("EAR", earReport.ReportCode);
			AssertEquals("FR E-Reporting Sales", earReport.ReportTitle);
			AssertEquals("TVA", earReport.TaxRegistrationType);
			AssertEquals(ReportPeriodicityCodes.DateRange, earReport.ReportPeriodicity);
			AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, earReport.ReportBaseTablePrefix);

			var earSettings = earReport.Settings.OfType<ComplianceReportConfigurationSetting>();
			AssertEquals("Expect three settings on EAR report", 3, earSettings.Count());

			assertSetting(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			assertSetting(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			assertSetting(LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote);

			void assertSetting(ZString ledgerType, ZString invoiceType)
			{
				var msg = $"{ledgerType} {invoiceType}";
				var setting = earSettings.FirstOrDefault(x => x.LedgerType == ledgerType && x.InvoiceType == invoiceType);
				AssertNotNull(msg, setting);

				AssertEquals(msg + " OriginalRule", OriginalRuleCodes.AllTransactions, setting.OriginalRule);
				AssertEquals(msg + " DisbursementRule", DisbursementRuleCodes.AllTransactions, setting.DisbursementRule);
				AssertEquals(msg + " TaxInvoiceRule", TaxInvoiceRuleCodes.All, setting.TaxInvoiceRule);
				AssertEquals(msg + " OrganisationLocationRule", AccChargeTaxOverride.AllCountriesExceptLoginCountry, setting.OrganisationLocation);
			}
		}
	}
}
