using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class IndonesiaComplianceInfo : CountryComplianceInfo, IComplianceSubTypeCodeProvider, IComplianceSubTypeDependencyConfigurationProvider, IComplianceSubTypeRulesWithMultipleRuleSetProvider, IComplianceSubTypeTaxInvoiceRulePrecedenceProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Indonesia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.IndonesiaCodeTypes.PPN;
		protected override string GetConsumptionTaxCode() => OrgCusCode.IndonesiaCodeTypes.PPN;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.IndonesiaCodeTypes.PPN;
		protected override string GetRecipientLocalBusinessRegNumberCodeType() => OrgCusCode.IndonesiaCodeTypes.NIT;
		protected override string GetRecipientLocalBusinessRegHeading() => (NoResString)"CLIENT NITKU";
		protected override string GetRecipientTaxIDHeading() => (NoResString)"CLIENT NPWP";
		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.BKP,
				ComplianceSubTypes.JKP,
				ComplianceSubTypes.T01,
				ComplianceSubTypes.T02,
				ComplianceSubTypes.T03,
				ComplianceSubTypes.T04,
				ComplianceSubTypes.T05,
				ComplianceSubTypes.T06,
				ComplianceSubTypes.T07,
				ComplianceSubTypes.T08,
				ComplianceSubTypes.T09,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string JKP = "JKP";
			public const string BKP = "BKP";
			public const string T01 = "T01";
			public const string T02 = "T02";
			public const string T03 = "T03";
			public const string T04 = "T04";
			public const string T05 = "T05";
			public const string T06 = "T06";
			public const string T07 = "T07";
			public const string T08 = "T08";
			public const string T09 = "T09";
		}

		public static class ComplianceSubTypeTaxStatusCode
		{
			public const string T01 = "01";
			public const string T02 = "02";
			public const string T03 = "03";
			public const string T04 = "04";
			public const string T05 = "05";
			public const string T06 = "06";
			public const string T07 = "07";
			public const string T08 = "08";
			public const string T09 = "09";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|TXI", "ID Govt Tax Invoice");
			public static MultilingualString JKP => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|JKP", "Export of Taxable Services");
			public static MultilingualString BKP => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|BKP", "Export of Intangible Taxable Goods");
			public static MultilingualString T01 => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|T01", "ID Govt Tax Invoice Code '01'");
			public static MultilingualString T02 => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|T02", "ID Govt Tax Invoice Code '02'");
			public static MultilingualString T03 => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|T03", "ID Govt Tax Invoice Code '03'");
			public static MultilingualString T04 => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|T04", "ID Govt Tax Invoice Code '04'");
			public static MultilingualString T05 => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|T05", "ID Govt Tax Invoice Code '05'");
			public static MultilingualString T06 => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|T06", "ID Govt Tax Invoice Code '06'");
			public static MultilingualString T07 => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|T07", "ID Govt Tax Invoice Code '07'");
			public static MultilingualString T08 => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|T08", "ID Govt Tax Invoice Code '08'");
			public static MultilingualString T09 => ResString.GetMultilingualString("IDComplianceSubTypeCodeList|T09", "ID Govt Tax Invoice Code '09'");
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "Faktur Pajak";
			public const string JKP = "Ekspor Jasa Kena Pajak";
			public const string BKP = "Ekspor Barang Kena Pajak Tidak Berwujud";
			public const string T01 = "Faktur Pajak Kode Transaksi '01'";
			public const string T02 = "Faktur Pajak Kode Transaksi '02'";
			public const string T03 = "Faktur Pajak Kode Transaksi '03'";
			public const string T04 = "Faktur Pajak Kode Transaksi '04'";
			public const string T05 = "Faktur Pajak Kode Transaksi '05'";
			public const string T06 = "Faktur Pajak Kode Transaksi '06'";
			public const string T07 = "Faktur Pajak Kode Transaksi '07'";
			public const string T08 = "Faktur Pajak Kode Transaksi '08'";
			public const string T09 = "Faktur Pajak Kode Transaksi '09'";
		}

		static class ComplianceSubTypeInternalImplementationNote
		{
			public const string TXI = "Used in both Receivables and Payables to sub-classify invoice (INV) transactions containing an amount of VAT.";
			public const string JKP = "This sub type is used when export service is zero rated with reference to Regulation Number 32/PMK.010/2019. No attribution rule has been set as we are not 100% clear when export service can be zero rated. No document has been provided as we do not have sample of actual sales invoice containing zero rated charges and corresponding notification document provided to us.";
			public const string BKP = "This sub type is used when export of intangible good is zero rated with reference to Regulation Number 32/PMK.010/2019. No attribution rule has been set as we are not 100% clear when export of intangible good can be zero rated. No document has been provided as we do not have sample of actual sales invoice containing zero rated charges and corresponding notification document provided to us.";
			public const string T01 = "Used for submission of BKP / JKP which is owed VAT and VAT is collected by the PKP seller who submits BKP / JKP. E.g. Local Customer";
			public const string T02 = "Used for submission of BKP / JKP to the collector of government treasurer VAT whose VAT is collected by the government treasurer VAT collector. E.g. VAT Collector - Government Treasurer";
			public const string T03 = "Used for submitting BKP / JKP to other VAT collectors (other than government treasurers) whose VAT is collected by other VAT collectors (other than government treasurers). E.g. VAT Collector - Beside Government";
			public const string T04 = "Used for submission of BKP or JKP using DPP of other values, the VAT is collected by the PKP of the seller who submits BKP / JKP.";
			public const string T05 = "Used for submission of BKP or JKP using certain amounts, the VAT is collected by the PKP of the seller who submits BKP / JKP.  E.g. Local Customer - Freight Charge";
			public const string T06 = "Used for other submissions for which the VAT is levied by PKP sellers who hand over BKP / JKP as well as surrenders to private overseas passport holders as referred to in article 16 E of the Value Added Tax Law. E.g. Individual Foreigner.";
			public const string T07 = "Used for submission of BKP / JKP that gets PPN facilities not collected / borne by the government (DTP). E.g. VAT is borned by Government";
			public const string T08 = "Used for submission of BKP / JKP where facilities are exempt from VAT imposition. E.g. NGO, Local Customer who has VAT Exempt Facility.";
			public const string T09 = "Used for the transfer of assets article 16 D in which the VAT is levied by the PKP seller who submits the BKP. E.g. Disposal of Assets.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplementationNote.TXI);

			public static ComplianceSubType BKP => new ComplianceSubType(ComplianceSubTypeCodes.BKP, () => ComplianceSubTypeDescriptions.BKP, () => ComplianceSubTypeLocalDescriptions.BKP, () => ComplianceSubTypeInternalImplementationNote.BKP);

			public static ComplianceSubType JKP => new ComplianceSubType(ComplianceSubTypeCodes.JKP, () => ComplianceSubTypeDescriptions.JKP, () => ComplianceSubTypeLocalDescriptions.JKP, () => ComplianceSubTypeInternalImplementationNote.JKP);

			public static ComplianceSubType T01 => new ComplianceSubType(ComplianceSubTypeCodes.T01, () => ComplianceSubTypeDescriptions.T01, () => ComplianceSubTypeLocalDescriptions.T01, () => ComplianceSubTypeInternalImplementationNote.T01, taxStatusCode: ComplianceSubTypeTaxStatusCode.T01);

			public static ComplianceSubType T02 => new ComplianceSubType(ComplianceSubTypeCodes.T02, () => ComplianceSubTypeDescriptions.T02, () => ComplianceSubTypeLocalDescriptions.T02, () => ComplianceSubTypeInternalImplementationNote.T02, taxStatusCode: ComplianceSubTypeTaxStatusCode.T02);

			public static ComplianceSubType T03 => new ComplianceSubType(ComplianceSubTypeCodes.T03, () => ComplianceSubTypeDescriptions.T03, () => ComplianceSubTypeLocalDescriptions.T03, () => ComplianceSubTypeInternalImplementationNote.T03, taxStatusCode: ComplianceSubTypeTaxStatusCode.T03);

			public static ComplianceSubType T04 => new ComplianceSubType(ComplianceSubTypeCodes.T04, () => ComplianceSubTypeDescriptions.T04, () => ComplianceSubTypeLocalDescriptions.T04, () => ComplianceSubTypeInternalImplementationNote.T04, taxStatusCode: ComplianceSubTypeTaxStatusCode.T04);

			public static ComplianceSubType T05 => new ComplianceSubType(ComplianceSubTypeCodes.T05, () => ComplianceSubTypeDescriptions.T05, () => ComplianceSubTypeLocalDescriptions.T05, () => ComplianceSubTypeInternalImplementationNote.T05, taxStatusCode: ComplianceSubTypeTaxStatusCode.T05);

			public static ComplianceSubType T06 => new ComplianceSubType(ComplianceSubTypeCodes.T06, () => ComplianceSubTypeDescriptions.T06, () => ComplianceSubTypeLocalDescriptions.T06, () => ComplianceSubTypeInternalImplementationNote.T06, taxStatusCode: ComplianceSubTypeTaxStatusCode.T06);

			public static ComplianceSubType T07 => new ComplianceSubType(ComplianceSubTypeCodes.T07, () => ComplianceSubTypeDescriptions.T07, () => ComplianceSubTypeLocalDescriptions.T07, () => ComplianceSubTypeInternalImplementationNote.T07, taxStatusCode: ComplianceSubTypeTaxStatusCode.T07);

			public static ComplianceSubType T08 => new ComplianceSubType(ComplianceSubTypeCodes.T08, () => ComplianceSubTypeDescriptions.T08, () => ComplianceSubTypeLocalDescriptions.T08, () => ComplianceSubTypeInternalImplementationNote.T08, taxStatusCode: ComplianceSubTypeTaxStatusCode.T08);

			public static ComplianceSubType T09 => new ComplianceSubType(ComplianceSubTypeCodes.T09, () => ComplianceSubTypeDescriptions.T09, () => ComplianceSubTypeLocalDescriptions.T09, () => ComplianceSubTypeInternalImplementationNote.T09, taxStatusCode: ComplianceSubTypeTaxStatusCode.T09);
		}

		#endregion

		#region IComplianceSubTypeDependencyConfigurationProvider

		void IComplianceSubTypeDependencyConfigurationProvider.GetDefaults(ComplianceSubTypeDependencyConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.T01;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.T02;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.T03;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.T04;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.T05;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.T06;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.T07;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.T08;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.T09;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TXI, RuleSetDescriptions.TXI);
			rulesetList.AddPair(RuleSetCodes.T01T04, RuleSetDescriptions.T01T04);
			rulesetList.AddPair(RuleSetCodes.T01T05, RuleSetDescriptions.T01T05);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.TXI;
		}

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.TXI)
			{
				AddComplianceSubTypeAttributionRulesForDefault(collection);
			}
			else if (ruleSetCode == RuleSetCodes.T01T04)
			{
				AddComplianceSubTypeAttributionRulesForRuleTwo(collection);
			}
			else if (ruleSetCode == RuleSetCodes.T01T05)
			{
				AddComplianceSubTypeAttributionRulesForRuleThree(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode)) //this is valid for UT and CountryComplianceInfoDisplayForm
			{
				AddComplianceSubTypeAttributionRulesForDefault(collection);
				AddComplianceSubTypeAttributionRulesForRuleTwo(collection);
				AddComplianceSubTypeAttributionRulesForRuleThree(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("IndonesiaComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefault(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Indonesia;
			configuration.SubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXI;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI;
		}

		void AddComplianceSubTypeAttributionRulesForRuleTwo(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Indonesia;
			configuration.SubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.T01;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.SpecificTaxIDs;
			configuration.TaxIDCode = (NoResString)"PPN, CAPPPN";
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.T01T04;
			configuration.RuleSetDescription = RuleSetDescriptions.T01T04;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Indonesia;
			configuration.SubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.T04;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.SpecificTaxIDs;
			configuration.TaxIDCode = "PPN1";
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.T01T04;
			configuration.RuleSetDescription = RuleSetDescriptions.T01T04;
		}

		void AddComplianceSubTypeAttributionRulesForRuleThree(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Indonesia;
			configuration.SubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.T01;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.SpecificTaxIDs;
			configuration.TaxIDCode = (NoResString)"PPN, CAPPPN";
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.T01T05;
			configuration.RuleSetDescription = RuleSetDescriptions.T01T05;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Indonesia;
			configuration.SubType = IndonesiaComplianceInfo.ComplianceSubTypeCodes.T05;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.SpecificTaxIDs;
			configuration.TaxIDCode = "PPN1";
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.T01T05;
			configuration.RuleSetDescription = RuleSetDescriptions.T01T05;
		}

		#region SuppressResourceStringsCheckRegion

		public static class RuleSetCodes
		{
			public const string TXI = "1";
			public const string T01T04 = "2";
			public const string T01T05 = "3";
		}

		static class RuleSetDescriptions
		{
			public const string TXI = "TXI";
			public const string T01T04 = "T01,T04";
			public const string T01T05 = "T01,T05";
		}

		#endregion

		#endregion

		#region IComplianceSubTypeTaxInvoiceRulePrecedenceProvider

		ZString[] IComplianceSubTypeTaxInvoiceRulePrecedenceProvider.ComplianceSubTypeTaxInvoiceRulePrecedenceList()
		{
			return new ZString[]
			{
				TaxInvoiceRuleCodes.SpecificTaxIDs,
				TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs,
				TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly,
				TaxInvoiceRuleCodes.ContainsAnAmountOfTax,
				TaxInvoiceRuleCodes.ContainsNoTaxIDs,
				TaxInvoiceRuleCodes.All,
				ZString.Empty
			};
		}

		#endregion
	}
}
