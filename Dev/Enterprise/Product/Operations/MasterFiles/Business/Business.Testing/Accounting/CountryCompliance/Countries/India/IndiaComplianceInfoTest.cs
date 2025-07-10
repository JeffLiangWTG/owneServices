using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(IndiaComplianceInfo))]
	sealed class IndiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.India;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TXI", "TXC", "TXD", "BSI", "BSC", "BSD", "INV", "CRD", "DBN", "RVI", "RVC", "RVD", "XCI", "XCC", "XCD" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXI", "TXC", "TXD", "BSI", "BSC", "BSD", "INV", "CRD", "DBN", "RVI", "RVC", "RVD", "XCI", "XCC", "XCD" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TXI", "TXC", "TXD", "BSI", "BSC", "BSD", "INV", "CRD", "DBN", "RVI", "RVC", "RVD", "XCI", "XCC", "XCD" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "TXC", "TXD", "BSI", "BSC", "BSD", "INV", "CRD", "DBN", "RVI", "RVC", "RVD", "XCI", "XCC", "XCD" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TXI", "TXC", "TXD", "BSI", "BSC", "BSD", "INV", "CRD", "DBN", "RVI", "RVC", "RVD", "XCI", "XCC", "XCD" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TXC", "TXD", "BSI", "BSC", "BSD", "INV", "CRD", "DBN", "RVI", "RVC", "RVD", "XCI", "XCC", "XCD" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TXC", "TXD", "BSI", "BSC", "BSD", "INV", "CRD", "DBN", "RVI", "RVC", "RVD", "XCI", "XCC", "XCD" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TXI";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Tax Invoice";

		protected override string ExpectedComplianceRules => @"IN,TXI,AR,INV,ATZ,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,TXI,AR,INV,AMT,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,TXC,AR,CRD,ATZ,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,TXC,AR,CRD,AMT,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,TXD,AR,INV,ATZ,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,TXD,AR,INV,AMT,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,BSI,AR,INV,EXT,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,BSC,AR,CRD,EXT,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,BSD,AR,INV,EXT,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,INV,AR,INV,NOT,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,CRD,AR,CRD,NOT,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,DBN,AR,INV,NOT,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,RVI,AR,INV,RVS,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,RVC,AR,CRD,RVS,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,RVD,AR,INV,RVS,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,XCI,AR,INV,EXL,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,XCC,AR,CRD,EXL,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,XCD,AR,INV,EXL,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,TXI,AP,INV,ATZ,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,TXI,AP,INV,AMT,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,TXC,AP,CRD,ATZ,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,TXC,AP,CRD,AMT,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,TXD,AP,INV,ATZ,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,TXD,AP,INV,AMT,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,BSI,AP,INV,EXT,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,BSC,AP,CRD,EXT,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,BSD,AP,INV,EXT,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,INV,AP,INV,NOT,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,CRD,AP,CRD,NOT,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,DBN,AP,INV,NOT,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,RVI,AP,INV,RVS,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,RVC,AP,CRD,RVS,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,RVD,AP,INV,RVS,ALL,ARO,,,,A,Default India Rule Set,,,,,,
IN,XCI,AP,INV,EXL,ALL,OTO,,,,A,Default India Rule Set,,,,,,
IN,XCC,AP,CRD,EXL,ALL,ALL,,,,A,Default India Rule Set,,,,,,
IN,XCD,AP,INV,EXL,ALL,ARO,,,,A,Default India Rule Set,,,,,,";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate => IsProductionSystem ? new ZDate(2021, 01, 01) : new ZDate(2020, 11, 01);

		public override void TestHasExtraTaxInfo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(true, complianceInfo.HasExtraTaxInfo());
		}

		public override void TestGetExtraTax()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("SGST", complianceInfo.GetExtraTaxDescription("QCT"));
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("SGST Amt", complianceInfo.GetExtraTaxOSAmountCaption().ShortCaption);
			AssertEquals("SGST Amount", complianceInfo.GetExtraTaxOSAmountCaption().Caption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("SGST Local", complianceInfo.GetExtraTaxLocalAmountCaption().Caption);
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("GST Amount", complianceInfo.GetTaxOSAmountCaption().ShortCaption);
			AssertEquals("CGST/IGST Amount", complianceInfo.GetTaxOSAmountCaption().Caption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("GST Local", complianceInfo.GetTaxLocalAmountCaption().ShortCaption);
			AssertEquals("CGST/IGST Local", complianceInfo.GetTaxLocalAmountCaption().Caption);
		}

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.India;

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType
			=> new[]
			{
				IndiaComplianceInfo.ComplianceSubTypeCodes.TXI,
				IndiaComplianceInfo.ComplianceSubTypeCodes.TXC,
				IndiaComplianceInfo.ComplianceSubTypeCodes.TXD,
			};

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_India()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(IndiaComplianceInfo.RuleSetCodes.Default, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(1, ruleSet.Count);
			Assert(ruleSet.ContainsCode(IndiaComplianceInfo.RuleSetCodes.Default));
		}

		public void TestGetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceRegistryDefaultProvider;
			AssertEquals(false, complianceInfo.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry());
		}

		public void TestGetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceRegistryDefaultProvider;
			AssertEquals(true, complianceInfo.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry());
		}

		public void TestGetIComplianceSubTypeAndNumberUpdateRules()
		{
			AssertEquals(false, ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(CountryCode)?.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed);
		}

		public void TestUseComplianceOrTransactionNumberForEInvoicingMapping()
		{
			Assert("Use compliance number when transaction date is on or after applies from date",
				IndiaComplianceInfo.UseComplianceNumberForEInvoicingMapping(new ZDate(2021, 08, 09), new ZDate(2021, 08, 09)));
			Assert("Use compliance number when transaction date is on or after applies from date",
				IndiaComplianceInfo.UseComplianceNumberForEInvoicingMapping(new ZDate(2021, 08, 09), new ZDate(2021, 08, 10)));
			Assert("Use compliance number when transaction date is on or after applies from date",
				IndiaComplianceInfo.UseComplianceNumberForEInvoicingMapping(new ZDate(2022, 09, 09), new ZDate(2022, 09, 10)));

			Assert("Do not use compliance number when transaction date is before applies from date",
				!IndiaComplianceInfo.UseComplianceNumberForEInvoicingMapping(new ZDate(2021, 08, 09), new ZDate(2021, 08, 08)));

			Assert("Do not use compliance number when applies from date is empty",
				!IndiaComplianceInfo.UseComplianceNumberForEInvoicingMapping(ZDate.Empty, new ZDate(2021, 08, 08)));
		}

		public void TestUseTransactionNumberForEInvoicingMapping()
		{
			Assert("Do not use transaction number when transaction date is on or after applies from date",
				!IndiaComplianceInfo.UseTransactionNumberForEInvoicingMapping(new ZDate(2021, 08, 09), new ZDate(2021, 08, 09)));
			Assert("Do not use transaction number when transaction date is on or after applies from date",
				!IndiaComplianceInfo.UseTransactionNumberForEInvoicingMapping(new ZDate(2021, 08, 09), new ZDate(2021, 08, 10)));
			Assert("Do not use transaction number when transaction date is on or after applies from date",
				!IndiaComplianceInfo.UseTransactionNumberForEInvoicingMapping(new ZDate(2022, 09, 09), new ZDate(2022, 09, 10)));

			Assert("Use transaction number when transaction date is before applies from date",
				IndiaComplianceInfo.UseTransactionNumberForEInvoicingMapping(new ZDate(2021, 08, 09), new ZDate(2021, 08, 08)));

			Assert("Use transaction number when applies from date is empty",
				IndiaComplianceInfo.UseTransactionNumberForEInvoicingMapping(ZDate.Empty, new ZDate(2021, 08, 08)));
		}

		public void TestUseComplianceOrTransactionNumberForEInvoicingMapping_AreNeverBothSame()
		{
			var dates = new[]
			{
				new ZDate(2021, 08, 09),
				new ZDate(2021, 08, 09),
				new ZDate(2021, 08, 10),
				new ZDate(2021, 08, 11),
				ZDate.Empty,
				ZDate.Today
			};

			foreach (var appliesFromDate in dates)
			{
				foreach (var transactionDate in dates)
				{
					var useTransactionNumber = IndiaComplianceInfo.UseTransactionNumberForEInvoicingMapping(appliesFromDate, transactionDate);
					var useComplianceNumber = IndiaComplianceInfo.UseComplianceNumberForEInvoicingMapping(appliesFromDate, transactionDate);
					AssertNotEquals($"Must use either transaction number or compliance number, but never both. appliesFromDate: {appliesFromDate}, transactionDate: {transactionDate}", useTransactionNumber, useComplianceNumber);
				}
			}
		}

		public void TestCreateComplianceNumberValidationExceptionMessage()
		{
			var message = IndiaComplianceInfo.CreateComplianceNumberValidationExceptionMessage("0001", '0');
			AssertEquals("Generated Compliance Number '0001' cannot start with '0'. Please check your Compliance Number sequence configuration.", message);

			message = IndiaComplianceInfo.CreateComplianceNumberValidationExceptionMessage("\"001", '\"');
			AssertEquals("Generated Compliance Number '\"001' cannot start with '\"'. Please check your Compliance Number sequence configuration.", message);
		}

		public void TestCreateTransactionNumberValidationExceptionMessage()
		{
			var message = IndiaComplianceInfo.CreateTransactionNumberValidationExceptionMessage("0002", '0');
			AssertEquals("Generated Transaction Number '0002' cannot start with '0'. Please check your Transaction Number sequence configuration.", message);

			message = IndiaComplianceInfo.CreateTransactionNumberValidationExceptionMessage("\"002", '\"');
			AssertEquals("Generated Transaction Number '\"002' cannot start with '\"'. Please check your Transaction Number sequence configuration.", message);
		}

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingDisabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

		public override void AssertOrgCusCodePredicateProvider_PlaceOfSupplyTaxRegistration(IOrgCusCodePredicateProvider orgCusCodePredicateProvider)
		{
			AssertEquals(true, orgCusCodePredicateProvider.IncludeForPlaceOfSupplyTaxRegsistration(null));

			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_RN_NKCodeCountry = ZString.Empty;
			cusCode.OK_CodeType = ZString.Empty;
			cusCode.OK_CustomsRegNo = ZString.Empty;
			AssertEquals(true, orgCusCodePredicateProvider.IncludeForPlaceOfSupplyTaxRegsistration(cusCode));

			cusCode.OK_RN_NKCodeCountry = "ᴔᴆ";
			cusCode.OK_CodeType = "β;,";
			cusCode.OK_CustomsRegNo = "@#$";
			AssertEquals(true, orgCusCodePredicateProvider.IncludeForPlaceOfSupplyTaxRegsistration(cusCode));

			cusCode.OK_RN_NKCodeCountry = "IN";
			cusCode.OK_CodeType = "GST";
			cusCode.OK_CustomsRegNo = "URP";
			AssertEquals(false, orgCusCodePredicateProvider.IncludeForPlaceOfSupplyTaxRegsistration(cusCode));
		}

		public override void AssertOrgCusCodePredicateProvider_OrgHeaderTaxRegistration(IOrgCusCodePredicateProvider orgCusCodePredicateProvider)
		{
			AssertEquals(true, orgCusCodePredicateProvider.IncludeForOrgHeaderTaxRegsistration(null));

			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_RN_NKCodeCountry = ZString.Empty;
			cusCode.OK_CodeType = ZString.Empty;
			cusCode.OK_CustomsRegNo = ZString.Empty;
			AssertEquals(true, orgCusCodePredicateProvider.IncludeForOrgHeaderTaxRegsistration(cusCode));

			cusCode.OK_RN_NKCodeCountry = "ᴔᴆ";
			cusCode.OK_CodeType = "β;,";
			cusCode.OK_CustomsRegNo = "@#$";
			AssertEquals(true, orgCusCodePredicateProvider.IncludeForOrgHeaderTaxRegsistration(cusCode));

			cusCode.OK_RN_NKCodeCountry = "IN";
			cusCode.OK_CodeType = "GST";
			cusCode.OK_CustomsRegNo = "URP";
			AssertEquals(false, orgCusCodePredicateProvider.IncludeForOrgHeaderTaxRegsistration(cusCode));
		}
	}
}
