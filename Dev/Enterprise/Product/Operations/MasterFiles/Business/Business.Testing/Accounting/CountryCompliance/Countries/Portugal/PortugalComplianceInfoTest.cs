using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	[TestedType(typeof(PortugalComplianceInfo))]
	sealed class PortugalComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Portugal;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "CBC", "CBD", "CBI", "LCD", "LCR", "LTX", "PCD", "PCR", "PTX", "SBC", "SBD", "SBI", "TCD", "TCM", "TCR", "TDM", "TXI", "TXM", "XCL", "XCR", "XPC", "XPI" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "CBC", "CBD", "CBI", "LCD", "LCR", "LTX", "TCD", "TCM", "TCR", "TDM", "TXI", "TXM", "XCL", "XCR" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "PCD", "PCR", "PTX", "SBC", "SBD", "SBI", "XPC", "XPI" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "CBD", "CBI", "LCD", "LTX", "TCD", "TDM", "TXI", "TXM", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "PCD", "PTX", "SBD", "SBI", "XPI" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "CBC", "LCR", "TCM", "TCR", "XCR" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "PCR", "SBC", "XPC" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TXI";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Fatura";

		protected override string ExpectedComplianceRules => @"PT,TCD,AR,INV,TID,ALL,ARO,,,,1,Default Portugal Rule Set,,,,,,
PT,TCR,AR,CRD,TID,ALL,ARO,,,,1,Default Portugal Rule Set,,,,,,
PT,TXI,AR,INV,TID,ALL,OTO,,,,1,Default Portugal Rule Set,,,,,,
PT,XCL,AR,INV,EXL,ALL,ALL,,,,1,Default Portugal Rule Set,,,,,,
PT,SBI,AP,INV,TID,ALL,OTO,,,,1,Default Portugal Rule Set,SBI,,,,,
PT,SBC,AP,CRD,TID,ALL,ARO,,,,1,Default Portugal Rule Set,SBI,,,,,
PT,SBD,AP,INV,TID,ALL,ARO,,,,1,Default Portugal Rule Set,SBI,,,,,";

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> {
			{ "CBC", LedgerOfUse.AR }, { "CBD", LedgerOfUse.AR }, { "CBI", LedgerOfUse.AR }, { "LCD", LedgerOfUse.AR },
			{ "LCR", LedgerOfUse.AR }, { "LTX", LedgerOfUse.AR }, { "PCD", LedgerOfUse.AP }, { "PCR", LedgerOfUse.AP },
			{ "PTX", LedgerOfUse.AP }, { "SBC", LedgerOfUse.AP }, { "SBD", LedgerOfUse.AP }, { "SBI", LedgerOfUse.AP },
			{ "TCD", LedgerOfUse.AR }, { "TCM", LedgerOfUse.AR }, { "TCR", LedgerOfUse.AR }, { "TDM", LedgerOfUse.AR }, { "TXI", LedgerOfUse.AR },
			{ "TXM", LedgerOfUse.AR }, { "XCL", LedgerOfUse.AR }, { "XCR", LedgerOfUse.AR }, { "XPC", LedgerOfUse.AP },
			{ "XPI", LedgerOfUse.AP }
		};

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse> {
			{ "CBC", TransactionTypeOfUse.CRD }, { "CBD", TransactionTypeOfUse.INV }, { "CBI", TransactionTypeOfUse.INV }, { "LCD", TransactionTypeOfUse.INV },
			{ "LCR", TransactionTypeOfUse.CRD }, { "LTX", TransactionTypeOfUse.INV }, { "PCD", TransactionTypeOfUse.INV }, { "PCR", TransactionTypeOfUse.CRD },
			{ "PTX", TransactionTypeOfUse.INV }, { "SBC", TransactionTypeOfUse.CRD }, { "SBD", TransactionTypeOfUse.INV }, { "SBI", TransactionTypeOfUse.INV },
			{ "TCD", TransactionTypeOfUse.INV }, { "TCM", TransactionTypeOfUse.CRD }, { "TCR", TransactionTypeOfUse.CRD }, { "TDM", TransactionTypeOfUse.INV }, { "TXI", TransactionTypeOfUse.INV },
			{ "TXM", TransactionTypeOfUse.INV }, { "XCL", TransactionTypeOfUse.INV }, { "XCR", TransactionTypeOfUse.CRD }, { "XPC", TransactionTypeOfUse.CRD },
			{ "XPI", TransactionTypeOfUse.INV }
		};

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "M00", Description = (NoResString)"N\u00e3o Isen\u00e7\u00e3o", Bool = false },
				new CodeDescriptionBoolRelatedItem() { Code = "M01", Description = (NoResString)"Artigo 16.\u00ba n.\u00ba 6 al\u00ednea c) do CIVA", Bool = true, RelatedItemCode = "M01" },
				new CodeDescriptionBoolRelatedItem() { Code = "M02", Description = (NoResString)"Artigo 6.\u00ba do Decreto\u2010Lei n.\u00ba 198/90, de 19 de Junho", Bool = true, RelatedItemCode = "M02" },
				new CodeDescriptionBoolRelatedItem() { Code = "M03", Description = (NoResString)"Exigibilidade de caixa", Bool = false, RelatedItemCode = "M03" },
				new CodeDescriptionBoolRelatedItem() { Code = "M04", Description = (NoResString)"Isento Artigo 13.\u00ba do CIVA", Bool = true, RelatedItemCode = "M04" },
				new CodeDescriptionBoolRelatedItem() { Code = "M05", Description = (NoResString)"Isento Artigo 14.\u00ba do CIVA", Bool = true, RelatedItemCode = "M05" },
				new CodeDescriptionBoolRelatedItem() { Code = "M06", Description = (NoResString)"Isento Artigo 15.\u00ba do CIVA", Bool = true, RelatedItemCode = "M06" },
				new CodeDescriptionBoolRelatedItem() { Code = "M07", Description = (NoResString)"Isento Artigo 9.\u00ba do CIVA", Bool = true, RelatedItemCode = "M07" },
				new CodeDescriptionBoolRelatedItem() { Code = "M08", Description = (NoResString)"IVA \u2013 Autoliquida\u00e7\u00e3o", Bool = false, RelatedItemCode = "M08" },
				new CodeDescriptionBoolRelatedItem() { Code = "M09", Description = (NoResString)"IVA \u2010 n\u00e3o confere direito a dedu\u00e7\u00e3o", Bool = true, RelatedItemCode = "M09" },
				new CodeDescriptionBoolRelatedItem() { Code = "M10", Description = (NoResString)"IVA \u2013 Regime de isen\u00e7\u00e3o", Bool = true, RelatedItemCode = "M10" },
				new CodeDescriptionBoolRelatedItem() { Code = "M11", Description = (NoResString)"Regime particular do tabaco", Bool = false, RelatedItemCode = "M11" },
				new CodeDescriptionBoolRelatedItem() { Code = "M12", Description = (NoResString)"Regime da margem de lucro - Ag\u00eancias de Viagens", Bool = false, RelatedItemCode = "M12" },
				new CodeDescriptionBoolRelatedItem() { Code = "M13", Description = (NoResString)"Regime da margem de lucro \u2013 Bens em segunda m\u00e3o", Bool = false, RelatedItemCode = "M13" },
				new CodeDescriptionBoolRelatedItem() { Code = "M14", Description = (NoResString)"Regime da margem de lucro - Objetos de arte", Bool = false, RelatedItemCode = "M14" },
				new CodeDescriptionBoolRelatedItem() { Code = "M15", Description = (NoResString)"Regime da margem de lucro - Objetos de cole\u00e7\u00e3o e antiguidades", Bool = false, RelatedItemCode = "M15" },
				new CodeDescriptionBoolRelatedItem() { Code = "M16", Description = (NoResString)"Isento Artigo 14.\u00ba do RITI", Bool = true, RelatedItemCode = "M16" },
				new CodeDescriptionBoolRelatedItem() { Code = "M19", Description = (NoResString)"Outras isen\u00e7\u00f5es", Bool = true, RelatedItemCode = "M19" },
				new CodeDescriptionBoolRelatedItem() { Code = "M20", Description = (NoResString)"IVA - regime forfet\u00e1rio", Bool = true, RelatedItemCode = "M20" },
				new CodeDescriptionBoolRelatedItem() { Code = "M21", Description = (NoResString)"IVA – n\u00e3o confere direito \u00e0 dedu\u00e7\u00e3o (ou express\u00e3o similar)", Bool = true, RelatedItemCode = "M21" },
				new CodeDescriptionBoolRelatedItem() { Code = "M25", Description = (NoResString)"Mercadorias \u00e0 consigna\u00e7\u00e3o", Bool = true, RelatedItemCode = "M25" },
				new CodeDescriptionBoolRelatedItem() { Code = "M30", Description = (NoResString)"IVA - autoliquida\u00e7\u00e3o", Bool = true, RelatedItemCode = "M30" },
				new CodeDescriptionBoolRelatedItem() { Code = "M31", Description = (NoResString)"IVA - autoliquida\u00e7\u00e3o", Bool = true, RelatedItemCode = "M31" },
				new CodeDescriptionBoolRelatedItem() { Code = "M32", Description = (NoResString)"IVA - autoliquida\u00e7\u00e3o", Bool = true, RelatedItemCode = "M32" },
				new CodeDescriptionBoolRelatedItem() { Code = "M33", Description = (NoResString)"IVA - autoliquida\u00e7\u00e3o", Bool = true, RelatedItemCode = "M33" },
				new CodeDescriptionBoolRelatedItem() { Code = "M40", Description = (NoResString)"IVA - autoliquida\u00e7\u00e3o", Bool = true, RelatedItemCode = "M40" },
				new CodeDescriptionBoolRelatedItem() { Code = "M41", Description = (NoResString)"IVA - autoliquida\u00e7\u00e3o", Bool = true, RelatedItemCode = "M41" },
				new CodeDescriptionBoolRelatedItem() { Code = "M42", Description = (NoResString)"IVA - autoliquida\u00e7\u00e3o", Bool = true, RelatedItemCode = "M42" },
				new CodeDescriptionBoolRelatedItem() { Code = "M43", Description = (NoResString)"IVA - autoliquida\u00e7\u00e3o", Bool = true, RelatedItemCode = "M43" },
				new CodeDescriptionBoolRelatedItem() { Code = "M99", Description = (NoResString)"N\u00e3o sujeito; n\u00e3o tributado (ou similar)", Bool = true, RelatedItemCode = "M99" }
			};

		protected override string ExpectedComplianceVersionNo
		{
			get
			{
				var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
				return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", currentVersionNumber.Major, currentVersionNumber.Minor, currentVersionNumber.Release);
			}
		}

		public void TestGetInvoiceType()
		{
			var complianceSubTypeCodes = ReflectionExtensions.GetConstantValues(typeof(PortugalComplianceInfo.ComplianceSubTypeCodes));

			foreach (var complianceSubType in complianceSubTypeCodes)
			{
				var invoiceType = PortugalComplianceInfo.GetInvoiceType(complianceSubType);

				if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TCD
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.SBD
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LCD
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TDM
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.PCD
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.CBD)
				{
					AssertEquals("ND", invoiceType);
				}
				else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TCR
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TCM
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.SBC
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.XCR
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LCR
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.PCR
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.XPC
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.CBC)
				{
					AssertEquals("NC", invoiceType);
				}
				else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TXI
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.XCL
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TXM
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.SBI
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LTX
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.PTX
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.XPI
					|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.CBI)
				{
					AssertEquals("FT", invoiceType);
				}
			}
		}

		protected override string ExpectedComplianceSequencePrefixErrorMessage => "Series Prefix must contain alphanumeric characters only and no spaces";

		protected override string ExpectedComplianceSequencePrefixRegex => "^[0-9a-zA-Z]*$";

		public void TestDefaultMandatoryComplianceNumberSequenceConfigurationCode()
		{
			var defaultConfigInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(CountryCode).DefaultMandatoryComplianceNumberSequenceConfigurationCode;

			AssertEquals("MPC", defaultConfigInfo.Code);
			AssertEquals("Mandatory Portugal Configuration", defaultConfigInfo.Description);
		}

		protected override ZString ExpectedDefaultEInvoicingSubmitPivotStatus => string.Empty;

		protected override ZString ExpectedDefaultEInvoicingPivotPendingStatusDescription => string.Empty;

		protected override ZDate ExpectedEInvoicingComplianceDate => ZDate.Empty;

		public void TestMandatoryComplianceNumberSequenceConfiguration()
		{
			var configs = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(CountryCode).GetMandatoryComplianceNumberSequenceConfiguration();

			AssertEquals("Only one config", 1, configs.Length);
			AssertEquals("MPC", configs[0].Code);
			AssertEquals("Mandatory Portugal Configuration", configs[0].Description);

			var includedElements = configs[0].Elements.Cast<ComplianceNumberSequenceCustomisationElement>().Where(x => x.Include).OrderBy(y => y.Order).ToArray();
			AssertEquals("Config has 5 elements included", 5, includedElements.Length);
			AssertEquals(ComplianceNumberSequenceCustomisationElement.ElementNames.ComplianceSubType, includedElements[0].ElementName);
			AssertEquals(ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace, includedElements[1].ElementName);
			AssertEquals(ComplianceNumberSequenceCustomisationElement.ElementNames.SeriesPrefix, includedElements[2].ElementName);
			AssertEquals(ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1, includedElements[3].ElementName);
			AssertEquals(ComplianceNumberSequenceCustomisationElement.ElementNames.SequenceNumber, includedElements[4].ElementName);
			AssertEquals("custom element is a slash", "/", includedElements[3].DigitCode);

			Assert("No duplicates elements", !configs[0].Elements.Cast<ComplianceNumberSequenceCustomisationElement>().GroupBy(x => x.ElementName).Any(g => g.Count() > 1));
		}

		public void TestGetSequenceNumber()
		{
			var complianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(CountryCode);
			AssertEquals("000005", complianceInfo.GetSequenceNumber("TXI /000005"));
			AssertEquals("000085", complianceInfo.GetSequenceNumber("TCR Prefix/000085"));
			AssertEquals("990", complianceInfo.GetSequenceNumber("TCR 0258/990"));
			AssertEquals("0005230", complianceInfo.GetSequenceNumber("TCD PR8/0005230"));
			AssertEquals("empty transaction reference", "", complianceInfo.GetSequenceNumber(""));
			AssertEquals("invalid transaction reference format", "", complianceInfo.GetSequenceNumber("00100056"));
		}

		public void TestGetTaxCountryRegion()
		{
			AssertEquals("PT", PortugalComplianceInfo.GetTaxCountryRegion(string.Empty));
			AssertEquals("PT", PortugalComplianceInfo.GetTaxCountryRegion("Unkwown"));
			AssertEquals("PT", PortugalComplianceInfo.GetTaxCountryRegion("CAPIVA"));
			AssertEquals("PT", PortugalComplianceInfo.GetTaxCountryRegion("NOTREPORT"));
			AssertEquals("PT", PortugalComplianceInfo.GetTaxCountryRegion("EXCLUDE"));
			AssertEquals("PT-AC", PortugalComplianceInfo.GetTaxCountryRegion("IVA18"));
			AssertEquals("PT-MA", PortugalComplianceInfo.GetTaxCountryRegion("IVA22"));
		}

		public void TestGetTaxType()
		{
			AssertEquals("IVA", PortugalComplianceInfo.GetTaxType(string.Empty));
			AssertEquals("IVA", PortugalComplianceInfo.GetTaxType("Unkwown"));
			AssertEquals("IVA", PortugalComplianceInfo.GetTaxType("CAPIVA"));
			AssertEquals("IVA", PortugalComplianceInfo.GetTaxType("IVA18"));
			AssertEquals("IVA", PortugalComplianceInfo.GetTaxType("IVA22"));
			AssertEquals("NS", PortugalComplianceInfo.GetTaxType("NOTREPORT"));
			AssertEquals("NS", PortugalComplianceInfo.GetTaxType("EXCLUDE"));
		}

		public void TestIsTaxRegistrationNumber()
		{
			AssertEquals(true, PortugalComplianceInfo.IsTaxRegistrationNumber(Core.Constants.CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA));
			AssertEquals(false, PortugalComplianceInfo.IsTaxRegistrationNumber(Core.Constants.CountryCodes.Australia, OrgCusCode.CodeTypes.IVA));
			AssertEquals(false, PortugalComplianceInfo.IsTaxRegistrationNumber(Core.Constants.CountryCodes.Portugal, OrgCusCode.CodeTypes.AgentCode));
		}

		public void TestUnknownData()
		{
			AssertEquals("Desconhecido", PortugalComplianceInfo.UnknownData);
		}

		public void TestPortugalAccountCodeForMissingRegistrationNumber()
		{
			AssertEquals("Consumidor final", PortugalComplianceInfo.PortugalAccountCodeForMissingRegistrationNumber);
		}

		public void TestIsPrintingAuthorizationNumberLengthValid()
		{
			var complianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(CountryCode);
			var complianceSequenceValidatorProvider = complianceInfo as IComplianceSequenceValidationProvider;

			var validNumericValue = "23456789";
			AssertEquals($"PrintingAuthorizationNumber {validNumericValue} is of valid length", expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberLengthValid(validNumericValue));

			var validAlphabeticValue = "BCDFGHJK";
			AssertEquals($"PrintingAuthorizationNumber {validAlphabeticValue} is of valid length", expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberLengthValid(validAlphabeticValue));

			var validAlphaNumericValue = "BCDFGH23";
			AssertEquals($"PrintingAuthorizationNumber {validAlphaNumericValue} is of valid length", expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberLengthValid(validAlphaNumericValue));

			var invalidLengthValue = "2345678";
			AssertEquals($"PrintingAuthorizationNumber {invalidLengthValue} is of invalid length", expected: false, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberLengthValid(invalidLengthValue));
		}

		public void TestIsPrintingAuthorizationNumberFormatValid()
		{
			var complianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(CountryCode);
			var complianceSequenceValidatorProvider = complianceInfo as IComplianceSequenceValidationProvider;

			var validNumericValue = "23456789";
			AssertEquals($"PrintingAuthorizationNumber {validNumericValue} is of valid format", expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberFormatValid(validNumericValue));

			var validAlphabeticValue = "BCDFGHJK";
			AssertEquals($"PrintingAuthorizationNumber {validAlphabeticValue} is of valid format", expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberFormatValid(validAlphabeticValue));

			var validAlphaNumericValue = "BCDFGH23";
			AssertEquals($"PrintingAuthorizationNumber {validAlphaNumericValue} is of valid format", expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberFormatValid(validAlphaNumericValue));

			var validPrefix = "BCDFGHJ";

			var invalidLetters = new string[] { "0", "1", "A", "E", "I", "O", "U" };

			foreach (var invalidLetter in invalidLetters)
			{
				var invalidValue = validPrefix + invalidLetter;
				AssertEquals($"PrintingAuthorizationNumber '{invalidValue}' is of invalid format", expected: false, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberFormatValid(invalidValue));
			}
		}

		public override void AssertShouldShowOriginalInvoiceReferenceFields(IOriginalInvoiceReference originalInvoiceReference)
		{
			var ledgerTypes = ReflectionExtensions.GetConstantValues(typeof(LedgerTypes));
			var transactionTypes = ReflectionExtensions.GetConstantValues(typeof(TransactionTypes));

			foreach (var ledger in ledgerTypes)
			{
				foreach (var transactionType in transactionTypes)
				{
					var expectedResult = ((ledger == LedgerTypes.AccountsReceivable || ledger == LedgerTypes.AccountsPayable) && transactionType == TransactionTypes.CreditNote)
						|| (ledger == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.Invoice);
					AssertEquals($"Original Reference fields are shown only for AR INV, AR CRD  and AP CRD for Portugal companies. Not for {ledger} {transactionType}",
						expectedResult, originalInvoiceReference.ShouldShowOriginalInvoiceReferenceFields(ledger, transactionType));
				}
			}
		}

		public override void AssertShouldShowOriginalInvoiceReferenceDatesFields(IOriginalInvoiceReference originalInvoiceReference)
		{
			var ledgerTypes = typeof(LedgerTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			var transactionTypes = typeof(TransactionTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();

			foreach (var ledger in ledgerTypes)
			{
				foreach (var transactionType in transactionTypes)
				{
					var expectedResult = ledger == LedgerTypes.AccountsReceivable && (transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.Invoice);
					AssertEquals($"Original Reference Dates fields are shown only for AR INV and AR CRD for Portugal companies. Not for {ledger} {transactionType}",
						expectedResult, originalInvoiceReference.ShouldShowOriginalInvoiceReferenceDatesFields(ledger, transactionType));
				}
			}
		}

		public override void AssertShouldShowOriginalInvoiceReferenceReasonFields(IOriginalInvoiceReference originalInvoiceReference)
		{
			var ledgerTypes = typeof(LedgerTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			var transactionTypes = typeof(TransactionTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();

			foreach (var ledger in ledgerTypes)
			{
				foreach (var transactionType in transactionTypes)
				{
					var expectedResult = ledger == LedgerTypes.AccountsReceivable && (transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.Invoice);
					AssertEquals($"Reason fields are shown only for AR INV and AR CRD for Portugal companies. Not for {ledger} {transactionType}",
						expectedResult, originalInvoiceReference.ShouldShowOriginalInvoiceReferenceReasonFields(ledger, transactionType));
				}
			}
		}

		public override void AssertGetAreAllOriginalInvoiceReferenceFieldsEnabled(IOriginalInvoiceReference originalInvoiceReference)
		{
			var ledgerTypes = typeof(LedgerTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			var transactionTypes = typeof(TransactionTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();

			foreach (var ledger in ledgerTypes)
			{
				foreach (var transactionType in transactionTypes)
				{
					foreach (var complianceSubType in ExpectedComplianceSubTypes)
					{
						var expectedResult = ((ledger == LedgerTypes.AccountsReceivable || ledger == LedgerTypes.AccountsPayable) && transactionType == TransactionTypes.CreditNote)
							|| ((ledger == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.Invoice)
								&& (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LCD
									|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TCD
									|| complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TDM));
						AssertEquals($"ALl original Reference fields are enabled only for AR CRD, AP CRD and AR INV when compliance sub type is LCD, TDM or TCD for Portugal companies. Not for {ledger} {transactionType} with compliance sub type {complianceSubType}",
							expectedResult, originalInvoiceReference.GetAreAllOriginalInvoiceReferenceFieldsEnabled(ledger, transactionType, complianceSubType));
					}
				}
			}
		}

		public override void AssertGetAreOriginalTransactionReferenceFieldsMandatory(IOriginalInvoiceReference originalInvoiceReference)
		{
			var ledgerTypes = typeof(LedgerTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			var transactionTypes = typeof(TransactionTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();

			foreach (var ledger in ledgerTypes)
			{
				foreach (var transactionType in transactionTypes)
				{
					var expectedResult = ledger == LedgerTypes.AccountsReceivable && (transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.Invoice);
					AssertEquals($"Original TRansaction Reference fields are mandatory only for AR INV and AR CRD for Portugal companies. Not for {ledger} {transactionType}",
						expectedResult, originalInvoiceReference.GetAreOriginalTransactionReferenceFieldsMandatory(ledger, transactionType));
				}
			}
		}

		public override void AssertGetDocOriginalReferenceReason(IOriginalInvoiceReference originalInvoiceReference)
		{
			AssertEquals(string.Empty, originalInvoiceReference.GetDocOriginalReferenceReason(string.Empty, ZDate.Invalid, ZDate.Invalid));
			AssertEquals(string.Empty, originalInvoiceReference.GetDocOriginalReferenceReason(string.Empty, ZDate.Empty, ZDate.Empty));
			AssertEquals(string.Empty, originalInvoiceReference.GetDocOriginalReferenceReason(string.Empty, ZDate.BrettsBirthday, ZDate.Empty));
			AssertEquals(string.Empty, originalInvoiceReference.GetDocOriginalReferenceReason(string.Empty, ZDate.Empty, ZDate.BrettsBirthday));
			AssertEquals("reason", originalInvoiceReference.GetDocOriginalReferenceReason("reason", ZDate.Empty, ZDate.Empty));
			AssertEquals("1971-09-16 - 1971-09-18", originalInvoiceReference.GetDocOriginalReferenceReason(string.Empty, ZDate.BrettsBirthday.AddDays(-2), ZDate.BrettsBirthday));
			AssertEquals("1971-09-16 - 1971-09-18", originalInvoiceReference.GetDocOriginalReferenceReason("reason", ZDate.BrettsBirthday.AddDays(-2), ZDate.BrettsBirthday));
		}

		public override void AssertIsTransactionAuthorizationNumberEnabled(ITransactionAuthorizationNumber transactionAuthorizationNumber)
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			Assert("should be false when the registry returns no value", !transactionAuthorizationNumber.IsTransactionAuthorizationNumberEnabled(companyPK, new ZDateTime(2050, 1, 1)));
			AccountingMasterFilesRegistry.Instance.TransactionAuthorizationNumberDate.SetValue(companyPK, Guid.Empty, Guid.Empty, new DateTime(2022, 2, 15));
			Assert("should be true when invoice date greater than registry date", transactionAuthorizationNumber.IsTransactionAuthorizationNumberEnabled(companyPK, new ZDateTime(2022, 2, 16)));
			Assert("should be true when invoice date equals registry date", transactionAuthorizationNumber.IsTransactionAuthorizationNumberEnabled(companyPK, new ZDateTime(2022, 2, 15)));
			Assert("should be false when invoice date lower than registry date", !transactionAuthorizationNumber.IsTransactionAuthorizationNumberEnabled(companyPK, new ZDateTime(2022, 2, 14)));
		}

		public override void AssertGetTransactionAuthorizationNumberLabel(ITransactionAuthorizationNumber transactionAuthorizationNumber)
		{
			AssertEquals("ATCUD", transactionAuthorizationNumber.GetTransactionAuthorizationNumberLabel());
		}

		public override void AssertGetDefaultTransactionAuthorizationNumberFeatureEnabledStartDate(ITransactionAuthorizationNumber transactionAuthorizationNumber)
		{
			AssertEquals(new ZDate(2023, 1, 1), transactionAuthorizationNumber.GetDefaultTransactionAuthorizationNumberFeatureEnabledStartDate());
		}

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingDisabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

		protected override string ExpectedDefaultValueForComplianceNumberAllocationDateRegistry =>
			AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code;

		protected override IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry()
			=> new Dictionary<(string code, bool isEInvoicingEnabled), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true),  "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, false), "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, true),  "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, true),  string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, false), "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, true),  "Cannot change default value for country/region 'PT'." },
			};

		protected override IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry()
			=> new Dictionary<(string code, bool isEInvoicingEnabled), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true),  "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, false), "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, true),  "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, true),  string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, false), "Cannot change default value for country/region 'PT'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, true),  "Cannot change default value for country/region 'PT'." },
			};
	}
}
