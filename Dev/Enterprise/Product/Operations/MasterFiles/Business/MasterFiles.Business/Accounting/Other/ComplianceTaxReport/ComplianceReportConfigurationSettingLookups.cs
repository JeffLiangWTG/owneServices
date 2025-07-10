using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Registry.Business
{
	public class ComplianceReportConfigurationSettingLookups : ZLookups, IComplianceSubTypeListAdditionalDataProvider
	{
		public ComplianceReportConfigurationSettingLookups(ComplianceReportConfigurationSetting parent)
			: base(parent)
		{
			BizOFactory = new BusinessObjectFactory();
		}

		protected new ComplianceReportConfigurationSetting Parent
		{
			get { return (ComplianceReportConfigurationSetting)base.Parent; }
		}

		protected override BusinessObjectFactory Factory
		{
			get
			{
				return base.Factory ?? factory ?? (factory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory factory;

		#region SubTypeList

		public CodeDescriptionPairList SubTypeList => Lists.GetSubTypeList(Parent.Country);

		#endregion

		#region LedgerTypeList

		public CodeDescriptionPairList LedgerTypeList => Lists.GetLedgerTypeList(this);

		public static class PseudoLedgerCodes
		{
			public const string CashBasisTax = "CT";
		}

		#endregion

		#region InvoiceTypeList

		public CodeDescriptionPairList InvoiceTypeList => Lists.GetInvoiceTypeList(this);

		#endregion

		#region OrganisationCategoryList

		public CodeDescriptionPairList OrganisationCategoryList => new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory);

		#endregion

		#region CountryList

		public RefCountryCollection CountryList => Lists.GetCountryList(Factory);

		#endregion

		#region Locations

		public CodeDescriptionPairList Locations
		{
			get
			{
				return AccountingTaxLocations.GetLocations(Factory);
			}
		}

		#endregion

		public CodeDescriptionPairList TaxInvoiceRuleList => Lists.GetTaxInvoiceRuleList(this);

		public CodeDescriptionPairList OriginalRuleList => Lists.OriginalRuleList;

		#region Tax Registration Type List

		public CodeDescriptionPairList TaxRegistrationTypeList => Lists.GetTaxRegistrationTypeList(this, Parent.Country);

		#endregion

		#region Disbursement Rule List

		public CodeDescriptionPairList DisbursementRuleList => Lists.DisbursementRuleList;

		#endregion

		#region Self Billing Rule List

		public CodeDescriptionPairList SelfBillingRuleList => Lists.SelfBillingRuleList;

		#endregion

		public CodeDescriptionPairList OrganisationLocationList => Lists.GetOrganisationLocationList(Parent.Country, BizOFactory);

		#region Tax Registration Location Rule List

		public CodeDescriptionPairList TaxRegistrationLocationRuleList => Lists.GetTaxRegistrationLocationRuleList(Parent.Country, BizOFactory);

		#endregion

		#region VAT Group List

		public CodeDescriptionPairList VATGroupList => Lists.VATGroupList;

		#endregion

		#region Reporting Date List

		public CodeDescriptionPairList ReportingDateList => Lists.GetReportingDateList(Parent.LedgerType);

		#endregion

		#region IComplianceSubTypeListDataProvider

		CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalTaxInvoiceRuleList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(TaxInvoiceRuleCodes.All, TaxInvoiceRuleDescriptions.All);
			return result;
		}

		CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalLedgerTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(LedgerTypes.CashBook, Res.GetString("61865029-1a89-455b-b820-e674ddac59be", "Cash Book"));

			if (Parent != null && Parent.ParentCollection != null && Parent.ParentCollection.ParentConfiguration != null &&
				(Parent.ParentCollection.ParentConfiguration.ReportBaseTablePrefix == ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.AllTransactions
				|| Parent.ParentCollection.ParentConfiguration.ReportBaseTablePrefix == ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData))
			{
				result.AddPair(LedgerTypes.General, Res.GetString("60c87810-1730-4a51-a81a-c6960ce73145", "General Ledger"));
				result.AddPair(LedgerTypes.JobCosting, Res.GetString("bd5a9958-97bb-4303-a3c1-53768bdbf5f4", "Job Costing"));
				result.AddPair(PseudoLedgerCodes.CashBasisTax, Res.GetString("bdc1e53b-43bb-4fc3-91e8-947c05e1128e", "Cash Basis Tax"));
			}
			return result;
		}

		CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalInvoiceTypeList()
		{
			var result = new CodeDescriptionPairList();
			if (Parent != null)
			{
				switch (Parent.LedgerType)
				{
					case LedgerTypes.AccountsReceivable:
					case LedgerTypes.AccountsPayable:
						result.AddPair(TransactionTypes.Contra, Res.GetString("0b70cb20-4638-4570-90d3-5766e04f99e8", "Contra"));
						result.AddPair(TransactionTypes.CreditNote, Res.GetString("0ecfc358-4864-4393-92c7-84f55213dd12", "Credit Note"));
						result.AddPair(TransactionTypes.AdjustmentNote, Res.GetString("d24188ae-381a-4e0c-8222-11e63992204f", "Adjustment Note"));
						result.AddPair(TransactionTypes.Discount, Res.GetString("b2473150-0c1d-40f5-abca-06d83ce718f4", "Discount"));
						result.AddPair(TransactionTypes.ExchangeDifference, Res.GetString("136adc97-a0c9-4a91-bd1d-03a9a2d33f84", "Exchange Rate Difference"));
						result.AddPair(TransactionTypes.Invoice, Res.GetString("38b1adc0-eae1-4ca1-aa81-075a92945ea2", "Invoice"));
						result.AddPair(TransactionTypes.Journal, Res.GetString("ee79c111-4787-4bdc-9803-0848a45979f6", "Journal"));
						result.AddPair(TransactionTypes.Overpayment, Res.GetString("3ae01fa0-ced5-447a-8b2e-1cceb3884b0d", "Overpayment"));
						result.AddPair(TransactionTypes.Payment, Res.GetString("85a41818-8e6f-4dee-b7ae-c9a41a2a1d24", "Payment"));
						result.AddPair(TransactionTypes.Receipt, Res.GetString("2076cbc9-61a9-4774-85f0-7edef032208e", "Receipt"));
						result.AddPair(TransactionTypes.Transfer, Res.GetString("61afeb01-6424-4604-945c-f12bf1a0f7c3", "Transfer"));
						result.AddPair(TransactionLineTypes.Cost, Res.GetString("490ee0c1-1f7d-4b1a-aa26-3157563947e3", "Cost"));
						result.AddPair(TransactionLineTypes.Revenue, Res.GetString("d852fb30-b684-460a-bfe0-d87591e19c2b", "Revenue"));
						break;
					case LedgerTypes.CashBook:
						result.AddPair(TransactionTypes.ExchangeDifference, Res.GetString("136adc97-a0c9-4a91-bd1d-03a9a2d33f84", "Exchange Rate Difference"));
						result.AddPair(TransactionTypes.Transfer, Res.GetString("61afeb01-6424-4604-945c-f12bf1a0f7c3", "Transfer"));
						result.AddPair(TransactionTypes.DirectPayment, Res.GetString("3917df93-9ea0-41ff-8449-7f3ab023c45a", "Direct Payment"));
						result.AddPair(TransactionTypes.DirectReceipt, Res.GetString("696ac178-32e9-4672-aa5c-2a2903cd1e6c", "Direct Receipt"));
						break;
					case LedgerTypes.General:
						result.AddPair(TransactionTypes.GLAutoJournal, Res.GetString("85118d86-5e41-4f14-860d-0b8a05a6c8cf", "Auto Journal"));
						result.AddPair(TransactionTypes.GLReversingJournal, Res.GetString("cf2fa4d7-1e77-4744-9305-8770d159e32e", "Reversing Journal"));
						result.AddPair(TransactionTypes.GLStandardJournal, Res.GetString("5b8f7970-aa74-4d90-9c89-345699b0d263", "Standard Journal"));
						break;
					case LedgerTypes.JobCosting:
						result.AddPair(TransactionTypes.JobRevenueJournal, Res.GetString("85e02122-4c27-4c07-bae2-a819e1fdd50f", "Job Revenue Journal"));
						result.AddPair(TransactionTypes.Journal, Res.GetString("ee79c111-4787-4bdc-9803-0848a45979f6", "Journal"));
						result.AddPair(TransactionLineTypes.Accrual, Res.GetString("97e1acfa-c230-40c6-bfaa-7baea0da8501", "Accrual"));
						result.AddPair(TransactionLineTypes.Revenue, Res.GetString("60d056fe-2afc-46bb-9235-5034037870a6", "Revenue Recognition"));
						result.AddPair(TransactionLineTypes.WIP, Res.GetString("d5666f9a-9946-4754-85e2-20cb9ad82151", "Work In Progress"));
						result.AddPair(PseudoTransactionTypeCodes.ReversedAccrual, Res.GetString("b6152e13-5275-495e-8d2c-c7e995a63b8a", "Reversed Accrual"));
						result.AddPair(PseudoTransactionTypeCodes.ReversedWIP, Res.GetString("2288EEA6-11F8-417A-8E7B-15E2FD267441", "Reversed WIP"));
						break;
					case PseudoLedgerCodes.CashBasisTax:
						result.AddPair(PseudoTransactionTypeCodes.CashBasisTax, Res.GetString("44b784ae-f5bc-449c-8022-760bbd996b10", "Cash Basis Tax Realized Records"));
						break;
				}
			}
			return result;
		}

		CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalTaxRegistrationTypeList() => new CodeDescriptionPairList();

		#endregion

		public static class PseudoTransactionTypeCodes
		{
			public const string ReversedAccrual = "RAC";
			public const string ReversedWIP = "RWI";
			public const string CashBasisTax = "CBT";
		}

		readonly BusinessObjectFactory BizOFactory;
	}
}
