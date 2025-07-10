using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class ARTransactionFilterBusinessObject : AutoARTransactionFilterBusinessObject
	{
		public ARTransactionFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values and Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SetDefaults();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetDefaults();
		}

		void SetDefaults()
		{
			AH_NumberFilter = AccountingUtils.NumberFilterTypes.Common;
			AH_DateFilter = AccountingUtils.DateFilterTypes.All;
			FilterOperator = SQLComparisonOperator.Equal;
		}

		#endregion

		#region Lookups

		#region AH_OHList
		protected OrgHeaderCollection fAH_OHList;
		public OrgHeaderCollection AH_OHList
		{
			get
			{
				if (fAH_OHList == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					query.AddSubQuery(subQuery, JoinCondition.And);

					fAH_OHList = new OrgHeaderCollection(new BusinessObjectFactory(), query);
				}
				return fAH_OHList;
			}
		}
		#endregion

		#region AH_GBList
		protected GlbBranchCollection fAH_GBList;
		public GlbBranchCollection AH_GBList
		{
			get
			{
				if (fAH_GBList == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					fAH_GBList = new GlbBranchCollection(Factory, filter);
				}
				return fAH_GBList;
			}
		}
		#endregion

		#region AH_GEList
		protected GlbDepartmentCollection fAH_GEList;
		public GlbDepartmentCollection AH_GEList
		{
			get
			{
				if (fAH_GEList == null)
				{
					fAH_GEList = new GlbDepartmentCollection(Factory);
				}
				return fAH_GEList;
			}
		}
		#endregion

		#region AH_RXList
		protected RefCurrencyCollection fAH_RXList;
		public RefCurrencyCollection AH_RXList
		{
			get
			{
				if (fAH_RXList == null)
				{
					fAH_RXList = new RefCurrencyCollection(Factory);
				}
				return fAH_RXList;
			}
		}
		#endregion

		#region AH_JHList
		protected JobHeaderCollection fAH_JHList;
		public JobHeaderCollection AH_JHList
		{
			get
			{
				if (fAH_JHList == null)
				{
					fAH_JHList = new JobHeaderCollection(Factory);
				}
				return fAH_JHList;
			}
		}
		#endregion

		#region OrgSettlementGroup_List

		public OrganisationsFindBoxCollection OrgSettlementGroup_List
		{
			get
			{
				return new OrganisationsFindBoxCollection(Factory);
			}
		}

		#endregion

		#region TransactionTypeList
		protected CodeDescriptionPairList fTransactionTypeList;
		public virtual CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new CodeDescriptionPairList();
					fTransactionTypeList.AddPair("ALL", Res.GetString("7e6a9e70-cb1c-458e-aa5b-6b0f0eef26ec", "All Transactions"));
					fTransactionTypeList.AddPair("ADJ", Res.GetString("affd7807-41bb-472f-b8c2-5971da954d89", "Adjustment Note"));
					fTransactionTypeList.AddPair("CTR", Res.GetString("1dc47251-d317-40d2-88b9-c37fb68ab353", "Contra"));
					fTransactionTypeList.AddPair("CRD", Res.GetString("24bf8928-5f3a-466a-ba9e-be3b8741df9a", "Credit Note"));
					fTransactionTypeList.AddPair("DSC", Res.GetString("91688b59-f5bc-4f49-93a0-287292c79bbb", "Discount"));
					fTransactionTypeList.AddPair("EXX", Res.GetString("15db6e10-6406-46b3-ae73-65134bb423ff", "Exchange Difference"));
					fTransactionTypeList.AddPair("INV", Res.GetString("bfbb6e7b-24b9-4a2d-8b37-3c9a075d2448", "Invoice"));
					fTransactionTypeList.AddPair("JNL", Res.GetString("83a08098-f8e6-4036-932e-96d0142147ec", "Journal"));
					fTransactionTypeList.AddPair("OVP", Res.GetString("2747ee22-ef63-440c-b28d-f73f65e1e711", "Overpayment"));
					fTransactionTypeList.AddPair("PAY", Res.GetString("99c685fe-2e36-4eda-929a-a6faa7c1b353", "Payment"));
					fTransactionTypeList.AddPair("REC", Res.GetString("7cfce960-17cb-4c88-a480-29f27ef85b6d", "Receipt"));
					fTransactionTypeList.AddPair("TRF", Res.GetString("1ba09304-5073-41dc-aa86-ee1c232665a4", "Transfer"));
				}
				return fTransactionTypeList;
			}
		}
		#endregion

		#region PaymentStatusList
		protected CodeDescriptionPairList fPaymentStatusList;
		public CodeDescriptionPairList PaymentStatusList
		{
			get
			{
				if (fPaymentStatusList == null)
				{
					fPaymentStatusList = new CodeDescriptionPairList();
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.All, Res.GetString("724cb60b-e9cf-444c-9532-441201e5ea31", "Display all transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.Unpaid, Res.GetString("33088977-bbe4-4e67-953a-d653df2796e7", "Display unpaid transactions"));
				}
				return fPaymentStatusList;
			}
		}
		#endregion

		#region AH_NumberFilter_List

		public override ZQueryProviderCodeDescriptionListBase AH_NumberFilter_List
		{
			get
			{
				if (fAH_NumberFilter_List == null)
				{
					fAH_NumberFilter_List = new ZQueryProviderCodeDescriptionList();
					fAH_NumberFilter_List.AddEmptySelection();

					fAH_NumberFilter_List.Add(AccountingUtils.NumberFilterTypes.JobNumber,
						ResString.GetMultilingualString("a50e787c-654d-4e28-9b56-ea955d8ec817", "Job #"), SQLComparisonOperator.Equal, new AddToQueryDelegate(AddJobNumberToFilter));

					fAH_NumberFilter_List.Add(AccountingUtils.NumberFilterTypes.TransactionNumber,
						ResString.GetMultilingualString("b2e83d31-eb7a-4fe3-a3e1-965df0211416", "Transaction #"), SQLComparisonOperator.Equal, new AddToQueryDelegate(AddTransactionNumberToFilter));

					fAH_NumberFilter_List.Add(AccountingUtils.NumberFilterTypes.ConsolidationNumber,
						ResString.GetMultilingualString("dd0a5951-dbc3-4913-a43c-5697d4cc9720", "Job Invoice #"), SQLComparisonOperator.Equal, new AddToQueryDelegate(AddJobInvoiceNumberToFilter));

					fAH_NumberFilter_List.Add(AccountingUtils.NumberFilterTypes.ChequeReferenceNumber,
						ResString.GetMultilingualString("2cff05b8-8853-46b4-9f9d-450db81bf1ba", "Check/Reference #"), SQLComparisonOperator.Equal, new AddToQueryDelegate(AddChequeNumberToFilter));

					fAH_NumberFilter_List.Add(AccountingUtils.NumberFilterTypes.DepositBatchNumber,
						ResString.GetMultilingualString("5b4b9c63-d60c-476b-b960-156daf1e2472", "Deposit Batch #"), SQLComparisonOperator.Equal, new AddToQueryDelegate(AddDepositBatchNumberToFilter));

					fAH_NumberFilter_List.Add(AccountingUtils.NumberFilterTypes.DDRBatchNumber,
						ResString.GetMultilingualString("d6aeaad3-9453-4db8-a616-31f0c9420413", "DDR Batch #"), SQLComparisonOperator.Equal, new AddToQueryDelegate(AddDDRBatchNumberToFilter));

					fAH_NumberFilter_List.AddQueryProviderCompositionForAll(1);

					fAH_NumberFilter_List.AddQueryProviderComposition(AccountingUtils.NumberFilterTypes.Common,
						ResString.GetMultilingualString("74ba5c5b-d111-4942-a127-65996a29a3ba", "Common"), 2,
						NumberFilterCommonItems.ToArray());
				}
				return fAH_NumberFilter_List;
			}
		}
		ZQueryProviderCodeDescriptionList fAH_NumberFilter_List;

		protected virtual IReadOnlyCollection<string> NumberFilterCommonItems
		{
			get { return new string[] { AccountingUtils.NumberFilterTypes.TransactionNumber, AccountingUtils.NumberFilterTypes.ConsolidationNumber }; }
		}

		#endregion

		#region AH_DateFilter_List

		public override ZQueryProviderCodeDescriptionListBase AH_DateFilter_List
		{
			get
			{
				if (fAH_DateFilter_List == null)
				{
					fAH_DateFilter_List = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
					fAH_DateFilter_List.AddEmptySelection();

					fAH_DateFilter_List.Add(AccountingUtils.DateFilterTypes.PostDate, ResString.GetMultilingualString("a2a9e9dc-6396-47d5-8765-5806c916094a", "Post Date"),
						AccTransactionHeaderSchema.AH_PostDate, AccTransactionHeaderSchema.AH_PostDate);

					fAH_DateFilter_List.Add(AccountingUtils.DateFilterTypes.TransactionDate, ResString.GetMultilingualString("9cb65699-04e2-4554-a812-c042d1003a6e", "Transaction Date"),
						AccTransactionHeaderSchema.AH_InvoiceDate, AccTransactionHeaderSchema.AH_InvoiceDate);

					fAH_DateFilter_List.Add(AccountingUtils.DateFilterTypes.DueDate, ResString.GetMultilingualString("cfd3826c-03cc-4ed1-8226-f9f9fee2edbd", "Due Date"),
						AccTransactionHeaderSchema.AH_DueDate, AccTransactionHeaderSchema.AH_DueDate);

					fAH_DateFilter_List.AddQueryProviderCompositionForAll(1);
				}
				return fAH_DateFilter_List;
			}
		}
		ZQueryProviderCodeDescriptionListWith2FilterArguments fAH_DateFilter_List;

		#endregion

		#region DebtorGroupCollection

		public OrgDebtorGroupCollection DebtorGroupCollection
		{
			get
			{
				if (fDebtorGroupCollection == null)
				{
					fDebtorGroupCollection = new OrgDebtorGroupCollection(Factory);
				}
				return fDebtorGroupCollection;
			}
		}

		OrgDebtorGroupCollection fDebtorGroupCollection;

		#endregion

		#endregion

		#region Filter Overrides

		protected virtual bool FilterByCurrentCompany
		{
			get { return true; }
		}

		#region Number Filters

		#region AddJobNumberToFilter

		protected void AddJobNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, FilterOperator, value);
			}
		}

		#endregion

		#region AddTransactionNumberToFilter

		protected void AddTransactionNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				query.AddToFilter(GetTransactionNumberFilter((ZString)value));
			}
		}

		protected virtual ZQuery GetTransactionNumberFilter(ZString transactionNumber)
		{
			return new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, FilterOperator, transactionNumber);
		}

		#endregion

		#region	AddChequeNumberToFilter

		public void AddChequeNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, FilterOperator, value);
			}
		}

		#endregion

		#region AddJobInvoiceNumberToFilter

		public void AddJobInvoiceNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, FilterOperator, value);
			}
		}

		#endregion

		#region AddDepositBatchNumberToFilter

		public void AddDepositBatchNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.Receipt);
				query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, FilterOperator, value);
			}
		}

		#endregion

		#region AddDDRBatchNumberToFilter

		public void AddDDRBatchNumberToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.Payment);
				query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, FilterOperator, value);
			}
		}

		#endregion

		#endregion

		public override ZQuery Filter
		{
			get
			{
				ZQuery query;
				if (AH_Number.IsEmpty || !PaymentStatusPanelFilter.IsEmpty)
				{
					query = base.Filter;
				}
				else
				{
					query = new ZQuery();
					query.AddToFilter(NumberFilterControlFilter);
				}
				query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, LedgerTypes.AccountsReceivable);
				return query;
			}
		}

		#region TransactionTypePanelFilter

		protected override ZQuery TransactionTypePanelFilter
		{
			get
			{
				if (AH_TransactionType != "ALL")
				{
					return base.TransactionTypePanelFilter;
				}
				else
				{
					return new ZQuery();
				}
			}
		}

		#endregion

		#region PostPeriodFilter

		protected override ZQuery PostPeriodPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				if (!PostPeriod.IsEmpty)
				{
					AccountingPeriodCalculator calculator = new AccountingPeriodCalculator(Factory);
					if (calculator.IsPeriodValid(PostPeriod))
					{
						ZDateTime periodStartDate = calculator.GetFirstDayForPeriod(PostPeriod);
						ZDateTime periodEndDate = calculator.GetLastDayForPeriod(PostPeriod);
						query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualTo, periodStartDate);
						query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, periodEndDate);
					}
				}
				return query;
			}
		}

		#endregion

		#region CreditorDebtorGroupFilterPanelFilter

		internal protected override ZQuery CreditorDebtorGroupFilterPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				if (!DebtorCreditorGroup.IsEmpty && DebtorCreditorGroup.IsValid)
				{
					ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
					ZDBOnlySubQuery orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_OJ_ARDebtorGroup, DebtorCreditorGroup);
					orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					transactionHeaderQuery.AddSubQuery(AccTransactionHeaderSchema.AH_OH, orgCompanyDataQuery, JoinCondition.And);
					query.AddToFilter(transactionHeaderQuery, JoinCondition.And);
				}
				return query;
			}
		}

		#endregion

		#region PaymentStatusPanelFilter

		protected override ZQuery PaymentStatusPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				if (PaymentStatus == "UNPAID")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.Equal, null);
				}
				return query;
			}
		}

		#endregion

		#region TransactionAmountPanelFilter

		protected override ZQuery TransactionAmountPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				ZBool useTransactionAmountFilter = !TransactionFromAmount.IsEmpty || !TransactionToAmount.IsEmpty;
				if (useTransactionAmountFilter)
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.GreaterThanOrEqualTo, TransactionFromAmount);
				}
				if (useTransactionAmountFilter)
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.LessThanOrEqualTo, TransactionToAmount);
				}
				return query;
			}
		}

		#endregion

		#region SettlementGroupPanelFilter

		protected override ZQuery SettlementGroupPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				if (!SettlementGroup.IsEmpty)
				{
					ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
					ZDBOnlySubQuery orgRelatedPartyQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
					orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ARSettlementGroup);
					orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, SettlementGroup);
					orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
					transactionHeaderQuery.AddSubQuery(AccTransactionHeaderSchema.AH_OH, orgRelatedPartyQuery, JoinCondition.And);
					query.AddToFilter(transactionHeaderQuery, JoinCondition.And);
				}
				return query;
			}
		}

		#endregion

		#endregion

		#region Validation

		public override void ValidateAH_TransactionType()
		{
			base.ValidateAH_TransactionType();
			CheckAH_TransactionType();
		}

		protected virtual void CheckAH_TransactionType()
		{
			ListValidation.ErrorIfInvalidCode(AH_TransactionTypeInfo, TransactionTypeList);
		}

		public override void ValidatePaymentStatus()
		{
			base.ValidatePaymentStatus();
			ListValidation.ErrorIfInvalidCode(PaymentStatusInfo, PaymentStatusList);
		}

		public override void ValidateTransactionFromAmount()
		{
			TransactionFromAmountInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(TransactionFromAmountInfo, AccTransactionHeaderSchema.AH_OSTotal.Precision, AccTransactionHeaderSchema.AH_OSTotal.Scale);
		}

		public override void ValidateTransactionToAmount()
		{
			TransactionToAmountInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(TransactionToAmountInfo, AccTransactionHeaderSchema.AH_OSTotal.Precision, AccTransactionHeaderSchema.AH_OSTotal.Scale);
		}

		#endregion
	}

	public abstract class AutoARTransactionFilterBusinessObject : FilterBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string TableName = "ARTransactionFilterBusinessObject";
			public const string PK = "PK";

			public const string AH_DateFilter = "AH_DateFilter";
			public const string AH_FromDate = "AH_FromDate";
			public const string AH_GB = "AH_GB";
			public const string AH_GE = "AH_GE";
			public const string AH_IsDisbursement = "AH_IsDisbursement";
			public const string AH_Number = "AH_Number";
			public const string AH_NumberFilter = "AH_NumberFilter";
			public const string AH_OH = "AH_OH";
			public const string AH_RX = "AH_RX";
			public const string AH_ToDate = "AH_ToDate";
			public const string AH_TransactionType = "AH_TransactionType";
			public const string DebtorCreditorGroup = "DebtorCreditorGroup";
			public const string FilterOperator = "FilterOperator";
			public const string PaymentStatus = "PaymentStatus";
			public const string PostPeriod = "PostPeriod";
			public const string SettlementGroup = "SettlementGroup";
			public const string TransactionFromAmount = "TransactionFromAmount";
			public const string TransactionToAmount = "TransactionToAmount";
		}

		#endregion

		protected AutoARTransactionFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region PK

		public override string PK_ColumnName
		{
			get { return Schema.PK; }
		}

		#endregion

		#region Properties

		#region AH_DateFilter

		public virtual ZString AH_DateFilter
		{
			get { return new ZString(BizOInternals.GetValueFromRowSafely(AH_DateFilterInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(AH_DateFilterInfo, value);
				SetQueryProviderParameterPropertyValue(AH_DateFilterInfo, value, Schema.AH_DateFilter, Schema.AH_FromDate, Schema.AH_ToDate);
				if (!IsValidationSuspended)
				{
					ValidateAH_DateFilter();
				}
			}
		}

		public virtual void ValidateAH_DateFilter()
		{
			AH_DateFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(AH_DateFilterInfo, AH_DateFilter_List);
		}
		public abstract ZQueryProviderCodeDescriptionListBase AH_DateFilter_List { get; }

		public virtual ZPropertyInfo AH_DateFilterInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_DateFilter); }
		}

		#endregion

		#region AH_FromDate

		public virtual ZDateTime AH_FromDate
		{
			get { return new ZDateTime(BizOInternals.GetValueFromRowSafely(AH_FromDateInfo)); }
			set
			{
				SetPropertyValue(AH_FromDateInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_FromDate();
				}
			}
		}

		public virtual void ValidateAH_FromDate()
		{
			AH_FromDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(AH_FromDateInfo);
		}

		public virtual ZPropertyInfo AH_FromDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_FromDate); }
		}

		#endregion

		#region AH_GB

		public virtual ZGuid AH_GB
		{
			get { return new ZGuid(BizOInternals.GetValueFromRowSafely(AH_GBInfo)); }
			set
			{
				SetPropertyValue(AH_GBInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_GB();
				}
			}
		}

		public virtual void ValidateAH_GB()
		{
			AH_GBInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AH_GBInfo);
		}

		public virtual ZPropertyInfo AH_GBInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_GB); }
		}

		#endregion

		#region AH_GE

		public virtual ZGuid AH_GE
		{
			get { return new ZGuid(BizOInternals.GetValueFromRowSafely(AH_GEInfo)); }
			set
			{
				SetPropertyValue(AH_GEInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_GE();
				}
			}
		}

		public virtual void ValidateAH_GE()
		{
			AH_GEInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AH_GEInfo);
		}

		public virtual ZPropertyInfo AH_GEInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_GE); }
		}

		#endregion

		#region AH_IsDisbursement

		public virtual ZBool AH_IsDisbursement
		{
			get { return new ZBool(BizOInternals.GetValueFromRowSafely(AH_IsDisbursementInfo)); }
			set
			{
				SetPropertyValue(AH_IsDisbursementInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_IsDisbursement();
				}
			}
		}

		public virtual void ValidateAH_IsDisbursement()
		{
			AH_IsDisbursementInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo AH_IsDisbursementInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_IsDisbursement); }
		}

		#endregion

		#region AH_Number

		public virtual ZString AH_Number
		{
			get { return new ZString(BizOInternals.GetValueFromRowSafely(AH_NumberInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(AH_NumberInfo, value);
				SetPropertyValue(AH_NumberInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_Number();
				}
			}
		}

		public virtual void ValidateAH_Number()
		{
			AH_NumberInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo AH_NumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_Number); }
		}

		#endregion

		#region AH_NumberFilter

		public virtual ZString AH_NumberFilter
		{
			get { return new ZString(BizOInternals.GetValueFromRowSafely(AH_NumberFilterInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(AH_NumberFilterInfo, value);
				SetQueryProviderParameterPropertyValue(AH_NumberFilterInfo, value, Schema.AH_NumberFilter, Schema.AH_Number);
				if (!IsValidationSuspended)
				{
					ValidateAH_NumberFilter();
				}
			}
		}

		public virtual void ValidateAH_NumberFilter()
		{
			AH_NumberFilterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(AH_NumberFilterInfo, AH_NumberFilter_List);
		}
		public abstract ZQueryProviderCodeDescriptionListBase AH_NumberFilter_List { get; }

		public virtual ZPropertyInfo AH_NumberFilterInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_NumberFilter); }
		}

		#endregion

		#region AH_OH

		public virtual ZGuid AH_OH
		{
			get { return new ZGuid(BizOInternals.GetValueFromRowSafely(AH_OHInfo)); }
			set
			{
				SetPropertyValue(AH_OHInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_OH();
				}
			}
		}

		public virtual void ValidateAH_OH()
		{
			AH_OHInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AH_OHInfo);
		}

		public virtual ZPropertyInfo AH_OHInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_OH); }
		}

		#endregion

		#region AH_RX

		public virtual ZGuid AH_RX
		{
			get { return new ZGuid(BizOInternals.GetValueFromRowSafely(AH_RXInfo)); }
			set
			{
				SetPropertyValue(AH_RXInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_RX();
				}
			}
		}

		public virtual void ValidateAH_RX()
		{
			AH_RXInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AH_RXInfo);
		}

		public virtual ZPropertyInfo AH_RXInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_RX); }
		}

		#endregion

		#region AH_ToDate

		public virtual ZDateTime AH_ToDate
		{
			get { return new ZDateTime(BizOInternals.GetValueFromRowSafely(AH_ToDateInfo)); }
			set
			{
				SetPropertyValue(AH_ToDateInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_ToDate();
				}
			}
		}

		public virtual void ValidateAH_ToDate()
		{
			AH_ToDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(AH_ToDateInfo);
		}

		public virtual ZPropertyInfo AH_ToDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_ToDate); }
		}

		#endregion

		#region AH_TransactionType

		public virtual ZString AH_TransactionType
		{
			get { return new ZString(BizOInternals.GetValueFromRowSafely(AH_TransactionTypeInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(AH_TransactionTypeInfo, value);
				SetPropertyValue(AH_TransactionTypeInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateAH_TransactionType();
				}
			}
		}

		public virtual void ValidateAH_TransactionType()
		{
			AH_TransactionTypeInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo AH_TransactionTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_TransactionType); }
		}

		#endregion

		#region DebtorCreditorGroup

		public virtual ZGuid DebtorCreditorGroup
		{
			get { return new ZGuid(BizOInternals.GetValueFromRowSafely(DebtorCreditorGroupInfo)); }
			set
			{
				SetPropertyValue(DebtorCreditorGroupInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateDebtorCreditorGroup();
				}
			}
		}

		public virtual void ValidateDebtorCreditorGroup()
		{
			DebtorCreditorGroupInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(DebtorCreditorGroupInfo);
		}

		public virtual ZPropertyInfo DebtorCreditorGroupInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.DebtorCreditorGroup); }
		}

		#endregion

		#region FilterOperator

		public virtual SQLComparisonOperator FilterOperator
		{
			get { return ((SQLComparisonOperator)BizOInternals.GetValueFromRowSafely(FilterOperatorInfo)); }
			set
			{
				SetPropertyValue(FilterOperatorInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateFilterOperator();
				}
			}
		}

		public virtual void ValidateFilterOperator()
		{
			FilterOperatorInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo FilterOperatorInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.FilterOperator); }
		}

		#endregion

		#region PaymentStatus

		public virtual ZString PaymentStatus
		{
			get { return new ZString(BizOInternals.GetValueFromRowSafely(PaymentStatusInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(PaymentStatusInfo, value);
				SetPropertyValue(PaymentStatusInfo, value);
				if (!IsValidationSuspended)
				{
					ValidatePaymentStatus();
				}
			}
		}

		public virtual void ValidatePaymentStatus()
		{
			PaymentStatusInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo PaymentStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.PaymentStatus); }
		}

		#endregion

		#region PostPeriod

		public virtual ZInt PostPeriod
		{
			get { return new ZInt(BizOInternals.GetValueFromRowSafely(PostPeriodInfo)); }
			set
			{
				SetPropertyValue(PostPeriodInfo, value);
				if (!IsValidationSuspended)
				{
					ValidatePostPeriod();
				}
			}
		}

		public virtual void ValidatePostPeriod()
		{
			PostPeriodInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo PostPeriodInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.PostPeriod); }
		}

		#endregion

		#region SettlementGroup

		public virtual ZGuid SettlementGroup
		{
			get { return new ZGuid(BizOInternals.GetValueFromRowSafely(SettlementGroupInfo)); }
			set
			{
				SetPropertyValue(SettlementGroupInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateSettlementGroup();
				}
			}
		}

		public virtual void ValidateSettlementGroup()
		{
			SettlementGroupInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(SettlementGroupInfo);
		}

		public virtual ZPropertyInfo SettlementGroupInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.SettlementGroup); }
		}

		#endregion

		#region TransactionFromAmount

		public virtual ZDecimal TransactionFromAmount
		{
			get { return new ZDecimal(BizOInternals.GetValueFromRowSafely(TransactionFromAmountInfo)); }
			set
			{
				SetPropertyValue(TransactionFromAmountInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateTransactionFromAmount();
				}
			}
		}

		public virtual void ValidateTransactionFromAmount()
		{
			TransactionFromAmountInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(TransactionFromAmountInfo, 0, 0);
		}

		public virtual ZPropertyInfo TransactionFromAmountInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.TransactionFromAmount); }
		}

		#endregion

		#region TransactionToAmount

		public virtual ZDecimal TransactionToAmount
		{
			get { return new ZDecimal(BizOInternals.GetValueFromRowSafely(TransactionToAmountInfo)); }
			set
			{
				SetPropertyValue(TransactionToAmountInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateTransactionToAmount();
				}
			}
		}

		public virtual void ValidateTransactionToAmount()
		{
			TransactionToAmountInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(TransactionToAmountInfo, 0, 0);
		}

		public virtual ZPropertyInfo TransactionToAmountInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.TransactionToAmount); }
		}

		#endregion

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DataRow row = ((IBusinessObjectInternals)this).Row;
			row[Schema.AH_DateFilter] = "";
			row[Schema.AH_FromDate] = DBNull.Value;
			row[Schema.AH_GB] = DBNull.Value;
			row[Schema.AH_GE] = DBNull.Value;
			row[Schema.AH_IsDisbursement] = false;
			row[Schema.AH_Number] = "";
			row[Schema.AH_NumberFilter] = "";
			row[Schema.AH_OH] = DBNull.Value;
			row[Schema.AH_RX] = DBNull.Value;
			row[Schema.AH_ToDate] = DBNull.Value;
			row[Schema.AH_TransactionType] = "";
			row[Schema.DebtorCreditorGroup] = DBNull.Value;
			row[Schema.FilterOperator] = SQLComparisonOperator.NotSpecified;
			row[Schema.PaymentStatus] = "";
			row[Schema.PostPeriod] = 0;
			row[Schema.SettlementGroup] = DBNull.Value;
			row[Schema.TransactionFromAmount] = 0;
			row[Schema.TransactionToAmount] = 0;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateAH_DateFilter();
			ValidateAH_FromDate();
			ValidateAH_GB();
			ValidateAH_GE();
			ValidateAH_IsDisbursement();
			ValidateAH_Number();
			ValidateAH_NumberFilter();
			ValidateAH_OH();
			ValidateAH_RX();
			ValidateAH_ToDate();
			ValidateAH_TransactionType();
			ValidateDebtorCreditorGroup();
			ValidateFilterOperator();
			ValidatePaymentStatus();
			ValidatePostPeriod();
			ValidateSettlementGroup();
			ValidateTransactionFromAmount();
			ValidateTransactionToAmount();

			base.RunPreSaveValidationCore(); // call RunPreSaveValidation() on all children then fire OnNotificationsChanged()
		}

		#endregion
		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateReadOnlyOnQueryDeciderParameter(Schema.AH_DateFilter, Schema.AH_FromDate, Schema.AH_ToDate);
			UpdateReadOnlyOnQueryDeciderParameter(Schema.AH_NumberFilter, Schema.AH_Number);
		}

		#region Filters

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				query.AddToFilter(SettlementGroupPanelFilter);
				query.AddToFilter(TransactionTypePanelFilter);
				query.AddToFilter(PostPeriodPanelFilter);
				query.AddToFilter(TransactionAmountPanelFilter);
				query.AddToFilter(PaymentStatusPanelFilter);
				query.AddToFilter(CreditorDebtorGroupFilterPanelFilter);
				query.AddToFilter(DateTimeFilterControlFilter);
				query.AddToFilter(NumberFilterControlFilter);
				if (AH_IsDisbursement)
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes);
				}
				RefCurrency currency = Factory.Load<RefCurrency>(AH_RX);
				AddIfNotEmpty(query, AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, SQLComparisonOperator.Equal, currency != null ? currency.RX_Code : ZString.Empty);
				AddIfNotEmpty(query, AccTransactionHeaderSchema.AH_GE, SQLComparisonOperator.Equal, AH_GE);
				AddIfNotEmpty(query, AccTransactionHeaderSchema.AH_GB, SQLComparisonOperator.Equal, AH_GB);
				AddIfNotEmpty(query, AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.Equal, AH_OH);

				return query;
			}
		}

		protected virtual ZQuery SettlementGroupPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery TransactionTypePanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				AddIfNotEmpty(query, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, AH_TransactionType);

				return query;
			}
		}

		protected virtual ZQuery PostPeriodPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery TransactionAmountPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery PaymentStatusPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		internal protected virtual ZQuery CreditorDebtorGroupFilterPanelFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				return query;
			}
		}

		protected virtual ZQuery DateTimeFilterControlFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				AddQueryProviderFilter(query, Schema.AH_DateFilter, "AH_DateFilter_List", SQLComparisonOperator.GreaterThanOrEqualTo, Schema.AH_FromDate, 0); // This code is auto-generated
				AddQueryProviderFilter(query, Schema.AH_DateFilter, "AH_DateFilter_List", SQLComparisonOperator.LessThanOrEqualToDatePartOnly, Schema.AH_ToDate, 1); // This code is auto-generated

				return query;
			}
		}

		protected virtual ZQuery NumberFilterControlFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				AddQueryProviderFilter(query, Schema.AH_NumberFilter, "AH_NumberFilter_List", SQLComparisonOperator.Contains, Schema.AH_Number, 0); // This code is auto-generated

				return query;
			}
		}

		#endregion
	}
}
