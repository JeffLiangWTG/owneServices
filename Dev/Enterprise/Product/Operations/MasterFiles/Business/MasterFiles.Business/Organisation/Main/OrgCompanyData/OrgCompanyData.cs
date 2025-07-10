using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Organisation.Registry;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ARAPDefaultTaxRecognitionRuleLookups;
using DefaultsOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty("Code")]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgCompanyData : AutoOrgCompanyData, IOrgCompanyData, IDocManagerSupport, IWorkflowTriggerEventSource
	{
		public OrgCompanyData(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(OB_ARClientNumber), ConcurrencyPolicy.Strict);
		}

		#region Schema

		public new class Schema : AutoOrgCompanyData.Schema
		{
			public const string OverrideBankAccountFromDebtorGroup = "OverrideBankAccountFromDebtorGroup";
			public const string ARBankAccountToDisplay = "ARBankAccountToDisplay";
			public const string OB_NoCreditLimitSetLabelIsVisible = "OB_NoCreditLimitSetLabelIsVisible";
			public const string OB_ARCreditLimitNotApprovedLabelIsVisible = "OB_ARCreditLimitNotApprovedLabelIsVisible";
			public const string ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime = "ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime";
			public const string ARTemporaryCreditLimit = "ARTemporaryCreditLimit";
			public const string OB_ARExportAirCollectUplift = "OB_ARExportAirCollectUplift";
			public const string OB_ARExportAirCollectUpliftMinimum = "OB_ARExportAirCollectUpliftMinimum";
			public const string OB_ARExportSeaCollectUplift = "OB_ARExportSeaCollectUplift";
			public const string OB_ARExportSeaCollectUpliftMinimum = "OB_ARExportSeaCollectUpliftMinimum";
			public const string OB_ARImportAirCollectUplift = "OB_ARImportAirCollectUplift";
			public const string OB_ARImportAirCollectUpliftMinimum = "OB_ARImportAirCollectUpliftMinimum";
			public const string OB_ARImportSeaCollectUplift = "OB_ARImportSeaCollectUplift";
			public const string OB_ARImportSeaCollectUpliftMinimum = "OB_ARImportSeaCollectUpliftMinimum";
			public const string OB_AROnGlobalCreditHold = "OB_AROnGlobalCreditHold";
		}

		#endregion

		#region Log Parameters

		public static class LogParameterKeys
		{
			public const string Type = "TYPE";
		}

		public static class LogTypes
		{
			public const string TemporaryCreditLimitAdjustment = "TCLA";
			public const string CreditOnHold = "COH";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		public static class LogReferenceKeys
		{
			public const string GlobalCreditOnHold = "Global Credit On Hold";
			public const string CreditOnHold = "Credit On Hold";
			public const string CreditApprovalGrantedBy = "Credit Approval Granted By";
			public const string CreditApproved = "Credit Approved";
			public const string CreditRating = "Credit Rating";
			public const string CreditLimit = "Credit Limit";
			public const string CreditReviewDue = "Credit Review Due";
			public const string UseSettlementGroupCreditLimit = "Use Settlement Group Credit Limit";
			public const string AgreedPaymentMethod = "Agreed Payment Method";
		}

		#endregion

		public static OrgCompanyData Load(BusinessObjectFactory factory, ZGuid orgHeaderPK, ZGuid companyPK)
		{
			Argument.NotNull(factory, "factory");

			OrgCompanyData result = null;

			if (orgHeaderPK.IsValid && !orgHeaderPK.IsEmpty && companyPK.IsValid && !companyPK.IsEmpty)
			{
				var companyDataQuery = new ZQuery(OrgCompanyDataSchema.OB_OH, orgHeaderPK);
				companyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, companyPK);
				result = factory.LoadTop1<OrgCompanyData>(companyDataQuery);
			}

			return result;
		}

		internal OrgRequiredFields GetRequiredFields()
		{
			OrgRequiredFields fields = new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, false);
			if (!IsDeleted && Header != null)
			{
				if (Header.OH_IsTempAccount)
				{
					if (Header.OH_IsConsignee)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgConsigneeRequiredFields()); }
					if (Header.OH_IsConsignor)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgConsignorRequiredFields()); }
					if (OB_IsCreditor)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgCreditorRequiredFields()); }
					if (OB_IsDebtor)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgDebtorRequiredFields()); }
					if (Header.OH_IsSalesLead)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgSalesRequiredFields()); }
				}
				else
				{
					if (Header.OH_IsBroker)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgBrokerRequiredFields()); }
					if (Header.OH_IsShippingProvider)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgCarrierRequiredFields()); }
					if (Header.OH_IsCompetitor)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgCompetitorRequiredFields()); }
					if (Header.OH_IsConsignee)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgConsigneeRequiredFields()); }
					if (Header.OH_IsConsignor)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgConsignorRequiredFields()); }
					if (Header.OH_IsContainerYard)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgContainerYardRequiredFields()); }
					if (OB_IsCreditor)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgCreditorRequiredFields()); }
					if (Header.OH_IsAirCTO || Header.OH_IsSeaCTO)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgCTORequiredFields()); }
					if (OB_IsDebtor)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgDebtorRequiredFields()); }
					if (Header.OH_IsForwarder)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgForwarderRequiredFields()); }
					if (Header.OH_IsPackDepot)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgPackDepotRequiredFields()); }
					if (Header.OH_IsSalesLead)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgSalesLeadRequiredFields()); }
					if (Header.OH_IsTransportClient)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgTransportClientRequiredFields()); }
					if (Header.OH_IsWarehouseClient)
					{ fields = MergeRequiredFields(fields, Env.Registry.GetOrgWarehouseRequiredFields()); }
				}
			}

			return fields;
		}

		OrgRequiredFields MergeRequiredFields(OrgRequiredFields fields, OrgRequiredFields fieldsToMerge)
		{
			bool address2 = fields.RequireAddress2 || fieldsToMerge.RequireAddress2;
			bool branch = fields.RequireBranch || fieldsToMerge.RequireBranch;
			bool city = fields.RequireCity || fieldsToMerge.RequireCity;
			bool phone = fields.RequirePhoneNumber || fieldsToMerge.RequirePhoneNumber;
			bool gst = fields.RequireBusinessNumber || fieldsToMerge.RequireBusinessNumber;
			bool gstOrPhone = fields.RequirePhoneOrBusinessNumber || fieldsToMerge.RequirePhoneOrBusinessNumber;
			bool fax = fields.RequireFaxNumber || fieldsToMerge.RequireFaxNumber;
			bool email = fields.RequireEmailAddress || fieldsToMerge.RequireEmailAddress;
			bool web = fields.RequireWebAddress || fieldsToMerge.RequireWebAddress;
			bool faxEmailOrWeb = fields.RequireFaxEmailOrWeb || fieldsToMerge.RequireFaxEmailOrWeb;
			bool requireARContact = fields.RequireARContact || fieldsToMerge.RequireARContact;

			return new OrgRequiredFields(address2, branch, city, phone, gst, gstOrPhone, fax, email, web, faxEmailOrWeb, requireARContact);
		}

		public override ZGuid OB_GC
		{
			get
			{
				return base.OB_GC;
			}
			set
			{
				bool hasChanged = base.OB_GC != value;
				base.OB_GC = value;
				if (hasChanged)
				{
					var header = Header;
					if (header != null)
					{
						header.MarkAsNeedingValidation();
						header.Addresses.MarkAsNeedingValidation();
						header.MiscServ.MarkAsNeedingValidation();
					}
				}
			}
		}

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new OrgCompanyDataUniqueIndexFailureHandler(this); }
		}

		static readonly ImmutableHashSet<String> dataEqualsExcludeSet = ImmutableHashSet.Create(OrgCompanyDataSchema.Constants.PK, OrgCompanyDataSchema.Constants.OB_IsValid, OrgCompanyDataSchema.Constants.OB_SystemCreateTimeUtc,
																		OrgCompanyDataSchema.Constants.OB_SystemCreateUser, OrgCompanyDataSchema.Constants.OB_SystemLastEditTimeUtc, OrgCompanyDataSchema.Constants.OB_SystemLastEditUser);

		public bool DataEquals(OrgCompanyData companyData)
		{
			if (companyData == null)
			{
				return false;
			}

			var rowThis = (this as INeedRow).Row;
			var rowCompanyData = (companyData as INeedRow).Row;

			foreach (DataColumn column in rowThis.Table.Columns)
			{
				if (!dataEqualsExcludeSet.Contains(column.ColumnName))
				{
					if (!Object.Equals(rowThis[column], rowCompanyData[column.ColumnName]))
					{
						return false;
					}
				}
			}

			return true;
		}

		public class OrgCompanyDataUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public OrgCompanyDataUniqueIndexFailureHandler(OrgCompanyData companyData)
			{
				this.CompanyData = companyData;
			}

			readonly OrgCompanyData CompanyData;

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return OrgCompanyDataSchema.Constants.Indexes.FK_UC__OB_GC_OB_OH; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var query = new ZQuery(OrgCompanyDataSchema.OB_OH, CompanyData.OB_OH);
				query.AddToFilter(OrgCompanyDataSchema.OB_GC, CompanyData.OB_GC);
				query.AddToFilter(OrgCompanyDataSchema.PK, SQLComparisonOperator.NotEqual, CompanyData.PK);
				query.FetchOnlyFromLocalCache = false;
				query.ReLoadExistingRows = true;
				var companyDataInDB = CompanyData.Factory.LoadTop1<OrgCompanyData>(query);

				if (companyDataInDB == null)
				{
					return;
				}

				var orgCode = CompanyData.Header.OH_Code;

				if (CompanyData.DataEquals(companyDataInDB) || !CompanyData.IsInDatabase)
				{
					using (new DisposableAction(() => CompanyData.SetContext(BusinessContext.NotDeletingOrgCompanyDataIndependentCollectionsOnHandlingUniqueIndexFailure),
												() => CompanyData.RemoveContext(BusinessContext.NotDeletingOrgCompanyDataIndependentCollectionsOnHandlingUniqueIndexFailure)))
					{
						CompanyData.Header.DestroyAndReloadCompanyData(true);
					}

					if (notifier != null)
					{
						notifier.ReportInformation(Res.GetString("95a1183f-4388-4a04-8a5d-6397bc0addef", "While you were working, the organization {0} had its AR/AP information updated. The system will now need to reload this information. Press OK to have this information loaded and then try saving again.", orgCode), Res.GetString("f12945fa-2a95-416b-b4fc-b54713b1f785", "Organization Update Required"));
					}
				}
				else
				{
					notifier.ReportError(Res.GetString("5c9b930e-7866-4b1c-a050-495b470a3093", "While you were editing your data, the organization {0} was modified by another user.\r\nYour changes cannot be saved because they may conflict with the other user's changes.\r\nPlease close and open this form to try again.", orgCode), Res.GetString("f12945fa-2a95-416b-b4fc-b54713b1f785", "Organization Update Required"));
				}
			}

			#endregion
		}

		#endregion

		#region Default Values and Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			OB_GC = GlbCompany.CurrentCompany.PK;
			OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			OB_OG_APCreditorGroup = ObjectFactory.Get<IAccounting>().APAccountGroup;
			OB_OJ_ARDebtorGroup = ObjectFactory.Get<IAccounting>().ARAccountGroup;

			OB_ARCreditApproved = OrganisationsDataRegistry.Instance.DefaultARCreditApproved.Value;
			if (OrganisationsDataRegistry.Instance.UseARSettlementGroupCreditLimit.Value)
			{
				OB_ARUseSettlementGroupCreditLimit = true;
				if (ARTerms.Any())
				{
					ARTerms[0].PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
				}
			}
		}

		#region Default Payables Account

		public void SetDefaultPayablesAccount()
		{
			if (OB_IsCreditor)
			{
				AccBankAccount defaultAcct = null;

				defaultAcct = GetDefaultAccountFromControllingBranch(OB_GB_ControllingBranch) ??
												GetDefaultAccountFromCurrency(APDefltCurrency) ??
												GetDefaultAccountFromCurrency(GlbCompany.CurrentCompany.LocalCurrency);

				OB_AB_APDefaultBankAccount = defaultAcct != null ? defaultAcct.PK : Guid.Empty;
			}
		}

		AccBankAccount GetDefaultAccountFromCurrency(RefCurrency currency)
		{
			AccBankAccount defaultAcct = null;

			if (currency != null)
			{
				if (ControllingBranch != null)
				{
					defaultAcct = GetAccountFromBranchAndCurrency(ControllingBranch, currency, true);
					if (defaultAcct == null)
					{
						defaultAcct = GetAccountFromBranchAndCurrency(ControllingBranch, currency, false);
					}
				}
				if (defaultAcct == null)
				{
					defaultAcct = GetAccountFromBranchAndCurrency(null, currency, true);
					if (defaultAcct == null)
					{
						defaultAcct = GetAccountFromBranchAndCurrency(null, currency, false);
					}
				}
			}

			return defaultAcct;
		}

		AccBankAccount GetAccountFromBranchAndCurrency(GlbBranch branch, RefCurrency currency, ZBool isDefault)
		{
			ZQuery filter = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
			filter.AddToFilter(AccBankAccountSchema.AB_RX_NKAccountCurrency, currency.RX_Code);
			filter.AddToFilter(AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, isDefault);
			if (branch == null)
			{
				filter.AddToFilter(AccBankAccountSchema.AB_GB, null);
			}
			else
			{
				filter.AddToFilter(AccBankAccountSchema.AB_GB, branch.PK);
			}
			return (AccBankAccount)Factory.LoadTop1(typeof(AccBankAccount), filter);
		}

		AccBankAccount GetDefaultAccountFromControllingBranch(ZGuid branchPK)
		{
			AccBankAccount defaultAcct = null;

			if (!branchPK.IsEmpty)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
				query.AddToFilter(AccBankAccountSchema.AB_GB, branchPK);
				query.AddToFilter(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
				//this is assuming that if there are multiple bank accounts with the same branch, we just choose the 1st one
				defaultAcct = Factory.LoadTop1<AccBankAccount>(query);
			}

			return defaultAcct;
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region TransactionHeaders

		public ZBool HasAPTransaction
		{
			get
			{
				return CheckActiveTransactionsExist(false);
			}
		}

		public ZBool HasARTransaction
		{
			get
			{
				return CheckActiveTransactionsExist(true);
			}
		}

		ZBool CheckActiveTransactionsExist(bool isAR)
		{
			if (!Header.IsInDatabase)
			{ return false; }

			var hasActiveTransaction = false;
			var collection = GetActiveTransactionDetails(false);
			foreach (DynamicBusinessObject transacitonDetails in collection)
			{
				if ((isAR && ARTransationTypes.Contains((ZString)transacitonDetails["TransactionType"])) ||
					!isAR && APTransationTypes.Contains((ZString)transacitonDetails["TransactionType"]))
				{
					hasActiveTransaction = true;
					break;
				}
			}
			return hasActiveTransaction;
		}

		public DynamicBusinessObjectCollection GetActiveTransactionDetails(ZBool isDeactivatingOrg)
		{
			var queryBuilder = new ZStringBuilder();
			queryBuilder.Append(@"
SELECT GC_Code as CompanyCode,AH_Ledger as TransactionType
FROM dbo.AccTransactionHeader inner join dbo.GlbCompany
on AccTransactionHeader.AH_GC = GlbCompany.GC_PK
where");

			if (!isDeactivatingOrg)
			{
				queryBuilder.Append(ZString.Format(" AH_GC = {0} AND ", GlbCompany.CurrentCompany.PK.ToSqlGuid()));
			}

			queryBuilder.Append(ZString.Format(@"
(
	(AH_Ledger in ('AP','AR') AND AH_FullyPaidDate IS NULL AND AH_TransactionType <> 'INB')
	OR 
	AH_Ledger in ('UA','PA' ,'IN')
)
AND
AH_OH = {0}
AND
AH_IsCancelled = 0

UNION

SELECT GC_Code as CompanyCode,AL_LineType as TransactionType
FROM dbo.AccTransactionLines inner join dbo.GlbCompany
on AccTransactionLines.AL_GC = GlbCompany.GC_PK
where
AL_LineType in ('ACR','WIP') 
AND
AL_ReverseDate is null 
AND
AL_OH = {0}", Header.PK.ToSqlGuid()));

			if (!isDeactivatingOrg)
			{
				queryBuilder.Append(ZString.Format(" AND AL_GC = {0}", GlbCompany.CurrentCompany.PK.ToSqlGuid()));
			}

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(queryBuilder.ToString());
			return collection;
		}

		public ZString ConvertActiveTransactionCollectionToString(DynamicBusinessObjectCollection activeTransactionCollection)
		{
			var result = new ZStringBuilder();
			var activeTransactionsGroupedByCompany = activeTransactionCollection.GroupBy(x => x["CompanyCode"]);
			foreach (var activeTransactions in activeTransactionsGroupedByCompany)
			{
				var transactionTypes = new ZStringBuilder();
				foreach (var transaction in activeTransactions)
				{
					var fullDescription = ZString.Empty;
					var transactionTypeAbbreviation = transaction["TransactionType"].ToString();
					if (TransactionTypeFullDescription.TryGetValue(transactionTypeAbbreviation, out fullDescription))
					{
						transactionTypes.Append(fullDescription);
					}
					else
					{
						throw new DeveloperNotificationException(string.Format(CultureInfo.InvariantCulture, "Found new transaction type: {0}. Please update the TransactionTypeFullDescription dictionary.",
							transactionTypeAbbreviation));
					}
				}
				result.AppendFormat((NoResString)"	- Company Code: {0}: {1}", activeTransactions.Key.ToString(), transactionTypes.ToStringWithDelimiterBetweenAppends(", "));
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		readonly Dictionary<ZString, ZString> TransactionTypeFullDescription = new Dictionary<ZString, ZString>()
		{
			{ "AR", (NoResString)"Accounts Receivable" },
			{ "AP", (NoResString)"Accounts Payable" },
			{ "ACR", (NoResString)"Accrual" },
			{ "PA", (NoResString)"Pending Allocation" },
			{ "IN", (NoResString)"Incomplete" },
			{ "UA", (NoResString)"Unapproved" },
			{ "WIP", "WIP" },
		};

		readonly List<ZString> APTransationTypes = new List<ZString>() { "AP", "UA", "PA", "IN", "ACR" };
		readonly List<ZString> ARTransationTypes = new List<ZString>() { "AR", "WIP" };

		#endregion

		#region Org Debtor Group

		public OrgDebtorGroupCollection SingleOrgDebtor
		{
			get
			{
				if (fSingleOrgDebtor == null || fSingleOrgDebtor.Count == 0 || fSingleOrgDebtor[0].PK != OB_OJ_ARDebtorGroup)
				{
					ZQuery filter = new ZQuery(OrgDebtorGroupSchema.PK, SQLComparisonOperator.Equal, OB_OJ_ARDebtorGroup);
					var localSingleOrgDebtor = new OrgDebtorGroupCollection(Factory, filter);
					if (OB_OJ_ARDebtorGroup.IsValid)
					{
						localSingleOrgDebtor.Load();
					}
					fSingleOrgDebtor = localSingleOrgDebtor;
					fSingleOrgDebtor.SetReadOnlyIncludingChildren(true);
				}
				return fSingleOrgDebtor;
			}
		}

		OrgDebtorGroupCollection fSingleOrgDebtor;

		#endregion

		#region Organisation

		public OrgHeader Organisation
		{
			get { return (OrgHeader)Factory.Load(typeof(OrgHeader), OB_OH); }
		}

		#endregion

		#region RateTariffLevels

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgRateTariffLevelCollection RateTariffLevels
		{
			get
			{
				if (fRateTariffLevels == null)
				{
					if (Header != null)
					{
						fRateTariffLevels = new OrgRateTariffLevelCollection(Header, OB_GC);
						fRateTariffLevels.LoadRelevant();
						RegisterEditableChildObject(fRateTariffLevels);
						if (Header != null)
						{
							fRateTariffLevels.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyDetailsRatingAndTariffsSecurity);
						}
					}
				}

				return fRateTariffLevels;
			}
		}
		OrgRateTariffLevelCollection fRateTariffLevels;

		void DeleteRateTariffLevels(bool shouldSuspendSettingHasChanges)
		{
			if (IsDeleted || Header == null)
			{
				return;
			}

			var localRateTariffLevels = fRateTariffLevels ?? new OrgRateTariffLevelCollection(Header, OB_GC);

			using (shouldSuspendSettingHasChanges ? localRateTariffLevels.SuspendSettingHasChanges() : null)
			{
				localRateTariffLevels.Load();
				fRateTariffLevels = localRateTariffLevels;
				fRateTariffLevels.RemoveAndDeleteAll();
			}
		}

		#endregion

		#region Rate Fee Charge Levels

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgRateFeeChargeLevelCollection RateFeeChargeLevels
		{
			get
			{
				if (rateFeeChargeLevels == null)
				{
					rateFeeChargeLevels = new OrgRateFeeChargeLevelCollection(Factory, Header);
					RegisterEditableChildObject(rateFeeChargeLevels);

					bool shouldBeReadonly = true;

					if (Header != null)
					{
						shouldBeReadonly = !Header.SecurityProvider.HasModifyDetailsRatingAndTariffsSecurity || !Header.SecurityProvider.HasModifyDetailsSecurity;
					}

					rateFeeChargeLevels.SetReadOnlyIncludingChildren(shouldBeReadonly);
				}
				return rateFeeChargeLevels;
			}
		}
		OrgRateFeeChargeLevelCollection rateFeeChargeLevels;

		#endregion

		#region Rate Commodity Defaulting Rule

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgRateCommodityDefaultingRuleCollection RateCommodityDefaultingRules
		{
			get
			{
				if (rateCommodityDefaultingRules == null)
				{
					rateCommodityDefaultingRules = new OrgRateCommodityDefaultingRuleCollection(Factory, Header);
					RegisterEditableChildObject(rateCommodityDefaultingRules);

					var shouldBeReadonly = true;

					if (Header != null)
					{
						shouldBeReadonly = !Header.SecurityProvider.HasModifyDetailsRatingAndTariffsSecurity || !Header.SecurityProvider.HasModifyDetailsSecurity;
					}

					rateCommodityDefaultingRules.SetReadOnlyIncludingChildren(shouldBeReadonly);
				}
				return rateCommodityDefaultingRules;
			}
		}

		OrgRateCommodityDefaultingRuleCollection rateCommodityDefaultingRules;

		#endregion

		#region Invoice Types

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgInvoiceTypeCollection InvoiceTypes
		{
			get
			{
				if (fInvoiceTypes == null)
				{
					fInvoiceTypes = new OrgInvoiceTypeCollection(this);
					fInvoiceTypes.Load();
					RegisterEditableChildObject(fInvoiceTypes);
					if (Header != null)
					{
						fInvoiceTypes.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyReceivablesInvoiceBatchingSecurity);
					}
				}

				return fInvoiceTypes;
			}
		}
		OrgInvoiceTypeCollection fInvoiceTypes;

		#endregion

		#region AP Account Details

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public AccountDetailsDependentCollection AccountDetailsCollection
		{
			get
			{
				if (fAccountDetailsCollection == null)
				{
					fAccountDetailsCollection = new AccountDetailsDependentCollection(this, Factory);
					fAccountDetailsCollection.Load();
					RegisterEditableChildObject(fAccountDetailsCollection);
					if (Header != null)
					{
						fAccountDetailsCollection.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyPayablesAccountDetailsSecurity);
					}
				}
				return fAccountDetailsCollection;
			}
		}
		AccountDetailsDependentCollection fAccountDetailsCollection;

		#endregion

		#region AR Account Details

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ARAccountDetailsDependentCollection ARAccountDetailsCollection
		{
			get
			{
				if (fARAccountDetailsCollection == null)
				{
					fARAccountDetailsCollection = new ARAccountDetailsDependentCollection(this);
					fARAccountDetailsCollection.Load();
					RegisterEditableChildObject(fARAccountDetailsCollection);
					if (Header != null)
					{
						fARAccountDetailsCollection.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyReceivablesAccountDetailsSecurity);
					}
				}
				return fARAccountDetailsCollection;
			}
		}
		ARAccountDetailsDependentCollection fARAccountDetailsCollection;

		#endregion

		#region Rating Documents Charge Grouping Or Roll up

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public RatingDocumentsChargeGroupingOrRollupCollection RatingDocRollupOrGroups
		{
			get
			{
				if (ratingDocRollupOrGroups == null)
				{
					ratingDocRollupOrGroups = new RatingDocumentsChargeGroupingOrRollupCollection(this);
					ratingDocRollupOrGroups.Load();
					RegisterEditableChildObject(ratingDocRollupOrGroups);
					SetRatingDocRollupOrGroupsDefaults();
				}
				return ratingDocRollupOrGroups;
			}
		}

		RatingDocumentsChargeGroupingOrRollupCollection ratingDocRollupOrGroups;

		void SetRatingDocRollupOrGroupsDefaults()
		{
			if (ratingDocRollupOrGroups.Count == 0)
			{
				using (SuspendSettingHasChanges())
				{
					var newElement = ratingDocRollupOrGroups.AddNew();
					using (newElement.SuspendSettingHasChanges())
					{
						newElement.RCG_OB_CompanyData = this.PK;
						newElement.RCG_Module = DocRollupOrSortModuleList.Codes.All;
						newElement.RCG_JobType = DocRollupOrSortJobTypeList.Codes.All;
						newElement.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
						newElement.RCG_Display = DocRollupOrSortDisplayList.Codes.Default;
						newElement.RCG_Style = DocRollupOrSortStyleList.Codes.Default;
					}
				}
			}
		}

		#endregion

		#region Invoice Settings

		OrgInvoiceRollupOrGroupCollection InvoiceRollupOrGroupsNoAutoCreate
		{
			get
			{
				if (fInvoiceRollupOrGroupsNoAutoCreate == null)
				{
					fInvoiceRollupOrGroupsNoAutoCreate = new OrgInvoiceRollupOrGroupCollection(this);
					fInvoiceRollupOrGroupsNoAutoCreate.Load();
				}
				return fInvoiceRollupOrGroupsNoAutoCreate;
			}
		}

		OrgInvoiceRollupOrGroupCollection fInvoiceRollupOrGroupsNoAutoCreate;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgInvoiceRollupOrGroupCollection InvoiceRollupOrGroups
		{
			get
			{
				if (fInvoiceRollupOrGroups == null)
				{
					fInvoiceRollupOrGroups = new OrgInvoiceRollupOrGroupCollection(this);
					fInvoiceRollupOrGroups.Load();
					RegisterEditableChildObject(fInvoiceRollupOrGroups);
					if (Header != null)
					{
						fInvoiceRollupOrGroups.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyReceivablesChargeGroupingSecurity);
					}

					SetInvoiceGroupingDefaults();
				}
				return fInvoiceRollupOrGroups;
			}
		}
		OrgInvoiceRollupOrGroupCollection fInvoiceRollupOrGroups;

		internal bool InvoiceRollupOrGroupsInitialised
		{
			get { return fInvoiceRollupOrGroups != null; }
		}

		void SetInvoiceGroupingDefaults()
		{
			if (InvoiceRollupOrGroups.Count == 0)
			{
				using (SuspendSettingHasChanges())
				{
					OrgInvoiceRollupOrGroup invoiceRollupOrGroup = InvoiceRollupOrGroups.AddNew();
					using (invoiceRollupOrGroup.SuspendSettingHasChanges())
					{
						invoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
						invoiceRollupOrGroup.PG_GroupOrSubTotal = OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode;
						invoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode;
						invoiceRollupOrGroup.PG_InvoicePostingStyle = OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode;
						invoiceRollupOrGroup.PG_InvoiceLineDisplayOption = OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode;
					}
				}
			}
		}

		#endregion

		#endregion

		#region Properties

		List<ZString> productValueDefaultOptionList => OB_IMProductValueDefaultOptions.Split(',').ToList();

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgCompanyData|ImporterOverride", Caption = "Importer Override")]
		public ZBool ImporterOverride
		{
			get
			{
				if (!importerOverride.HasValue)
				{
					importerOverride = productValueDefaultOptionList.Contains(Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Override);
				}
				return importerOverride.Value;
			}
			set
			{
				if (!importerOverride.HasValue || value != importerOverride.Value)
				{
					if (!value)
					{
						DefaultOptions.RemoveAndDeleteAll();
					}
					importerOverride = value;
					ImporterOverrideInfo.RefreshBinding();
				}
			}
		}
		ZBool? importerOverride;

		public ZPropertyInfo ImporterOverrideInfo => GetZPropertyInfo(nameof(ImporterOverride));

		[ChildEditable(true)]
		public IMProductValueDefaultOptionCollection DefaultOptions
		{
			get
			{
				if (options == null)
				{
					options = new IMProductValueDefaultOptionCollection(this);
					using (options.SuspendSettingHasChanges())
					{
						options.AddProductValueDefaultOptions(productValueDefaultOptionList.Where(x => x != DefaultsOptions.Codes.Override).ToList());
					}
					RegisterEditableChildObject(options);
				}

				return options;
			}
		}
		IMProductValueDefaultOptionCollection options;

		AccCFXUpliftConfigurationCollection accCFXConfigurations;
		[ChildEditable(true)]
		public AccCFXUpliftConfigurationCollection AccCFXConfigurations
		{
			get
			{
				if (accCFXConfigurations == null)
				{
					var branchPk = Env.CurrentCompanyPK == OB_GC ? Env.CurrentBranchPK : ZGuid.Empty;
					var localAccCFXConfigurations = new AccCFXUpliftConfigurationCollection(Factory, OB_GC, branchPk, OB_OH);
					localAccCFXConfigurations.Load();
					accCFXConfigurations = localAccCFXConfigurations;
					RegisterEditableChildObject(accCFXConfigurations);
				}
				return accCFXConfigurations;
			}
		}

		[ChildEditable(true)]
		public AccExchangeRateConfigurationCollection AccARExchangeRateConfigurations
		{
			get
			{
				if (accARExchangeRateConfigurations == null)
				{
					var localAccARExchangeRateConfigurations = new AccExchangeRateConfigurationCollection(Factory, OB_GC, LedgerTypes.AccountsReceivable, OB_OJ_ARDebtorGroup, OB_OH);
					localAccARExchangeRateConfigurations.Load();
					accARExchangeRateConfigurations = localAccARExchangeRateConfigurations;
					RegisterEditableChildObject(accARExchangeRateConfigurations);
				}
				return accARExchangeRateConfigurations;
			}
		}
		AccExchangeRateConfigurationCollection accARExchangeRateConfigurations;

		[ChildEditable(true)]
		public AccExchangeRateConfigurationCollection AccAPExchangeRateConfigurations
		{
			get
			{
				if (accAPExchangeRateConfigurations == null)
				{
					var localAccAPExchangeRateConfigurations = new AccExchangeRateConfigurationCollection(Factory, OB_GC, LedgerTypes.AccountsPayable, OB_OG_APCreditorGroup, OB_OH);
					localAccAPExchangeRateConfigurations.Load();
					accAPExchangeRateConfigurations = localAccAPExchangeRateConfigurations;
					RegisterEditableChildObject(accAPExchangeRateConfigurations);
				}
				return accAPExchangeRateConfigurations;
			}
		}
		AccExchangeRateConfigurationCollection accAPExchangeRateConfigurations;

		[ChildEditable(true)]
		public AccEInvoicingTemplateFileViewCollection EInvoicingTemplateFileConfigurations
		{
			get
			{
				if (eInvoicingTemplateFileConfigurations == null)
				{
					var localEInvoicingTemplateFileConfigurations = new AccEInvoicingTemplateFileViewCollection(Factory, OB_GC, OB_GB_ControllingBranch, OB_OH);
					RegisterEditableChildObject(localEInvoicingTemplateFileConfigurations);
					localEInvoicingTemplateFileConfigurations.Load();
					eInvoicingTemplateFileConfigurations = localEInvoicingTemplateFileConfigurations;
				}
				return eInvoicingTemplateFileConfigurations;
			}
		}
		AccEInvoicingTemplateFileViewCollection eInvoicingTemplateFileConfigurations;

		[ChildEditable(true)]
		public AccCashAdvanceDefaultingConfigurationCollection AccARCashAdvanceConfigurations
		{
			get
			{
				if (accARCashAdvanceConfigurations == null)
				{
					var branchPk = Env.CurrentCompanyPK == OB_GC ? Env.CurrentBranchPK : ZGuid.Empty;
					var localAccARCashAdvanceConfigurations = new AccCashAdvanceDefaultingConfigurationCollection(Factory, OB_GC, branchPk, OB_OH, LedgerTypes.AccountsReceivable);
					localAccARCashAdvanceConfigurations.Load();
					accARCashAdvanceConfigurations = localAccARCashAdvanceConfigurations;
					RegisterEditableChildObject(accARCashAdvanceConfigurations);
				}
				return accARCashAdvanceConfigurations;
			}
		}
		AccCashAdvanceDefaultingConfigurationCollection accARCashAdvanceConfigurations;

		#region OB_OH

		[LightValidationTestExempt]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid OB_OH
		{
			get { return base.OB_OH; }
			set
			{
				if (base.OB_OH != value)
				{
					bool needToDeleteRateTariffLevels = !IsDeleted && !OB_OH.IsEmpty && OB_OH != value;
					if (needToDeleteRateTariffLevels)
					{
						DeleteRateTariffLevels(value == ZGuid.Empty);
					}

					base.OB_OH = value;
					if (Header != null)
					{
						Header.MarkAsNeedingValidation();
						Header.Addresses.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region Credit Agreed Payment Method

		[List("OB_ARCreditAgreedPaymentMethod_List")]
		public override ZString OB_ARCreditAgreedPaymentMethod
		{
			get { return base.OB_ARCreditAgreedPaymentMethod; }
			set { base.OB_ARCreditAgreedPaymentMethod = value; }
		}

		public ReadOnlyCodeDescriptionPairList OB_ARCreditAgreedPaymentMethod_List
		{
			get { return OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList(); }
		}

		[List("OB_APCreditAgreedPaymentMethod_List")]
		public override ZString OB_APCreditAgreedPaymentMethod
		{
			get { return base.OB_APCreditAgreedPaymentMethod; }
			set { base.OB_APCreditAgreedPaymentMethod = value; }
		}

		public ReadOnlyCodeDescriptionPairList OB_APCreditAgreedPaymentMethod_List
		{
			get { return Env.Registry.PayablesCreditAgreedPaymentMethodsList; }
		}

		#endregion

		#region Credit Card Details

		[List("Lookups.OB_ARCreditCardType_List")]
		public override ZString OB_ARCreditCardType
		{
			get { return base.OB_ARCreditCardType; }
			set { base.OB_ARCreditCardType = value; }
		}

		[List("Lookups.OB_ARCreditCardExpire_Month_List")]
		[BusinessObjectTestExclude()]
		[MaxLength(2)]
		public ZString OB_ARCreditCardExpire_Month
		{
			get
			{
				if (OB_ARCreditCardExpire.Length == 4)
				{
					return OB_ARCreditCardExpire.Left(2);
				}
				return "__";
			}
			set
			{
				if (value.IsEmpty)
				{
					value = "__";
				}

				CheckMaximumLength(OB_ARCreditCardExpire_MonthInfo, value);
				OB_ARCreditCardExpire = value.Left(2) + OB_ARCreditCardExpire_Year;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOB_ARCreditCardExpire_Month();
				}
				OB_ARCreditCardExpire_MonthInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OB_ARCreditCardExpire_MonthInfo
		{
			get { return GetZPropertyInfo(nameof(OB_ARCreditCardExpire_Month)); }
		}

		[List("Lookups.OB_ARCreditCardExpire_Year_List")]
		[BusinessObjectTestExclude()]
		[MaxLength(2)]
		public ZString OB_ARCreditCardExpire_Year
		{
			get
			{
				if (OB_ARCreditCardExpire.Length == 4)
				{
					return OB_ARCreditCardExpire.Right(2);
				}
				return "__";
			}
			set
			{
				if (value.IsEmpty)
				{
					value = "__";
				}

				OB_ARCreditCardExpire = OB_ARCreditCardExpire_Month + value.Left(2);

				if (!IsValidationSuspended)
				{
					Validation.ValidateOB_ARCreditCardExpire_Year();
				}
				OB_ARCreditCardExpire_YearInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OB_ARCreditCardExpire_YearInfo
		{
			get { return GetZPropertyInfo(nameof(OB_ARCreditCardExpire_Year)); }
		}

		public override ZString OB_ARCreditCardNum
		{
			get { return base.OB_ARCreditCardNum; }
			set
			{
				if (!value.IsEmpty)
				{
					OB_ARCreditCardType = CardTypeDecider.IdentifyCardType(value);
				}

				base.OB_ARCreditCardNum = value;
			}
		}

		#endregion

		#region Payables (OB_IsCreditor)

		public override ZBool OB_IsCreditor
		{
			get { return base.OB_IsCreditor; }
			set
			{
				if (HasSecurityToChangeCreditor)
				{
					MarkAsNeedingValidation();

					if (value)
					{
						base.OB_IsCreditor = value;
						SetAPTaxApplicable();
						Header.MarkAsNeedingValidation();
						Header.Addresses.MarkAsNeedingValidation();
					}
					else if (!HasAPTransaction)
					{
						base.OB_IsCreditor = value;
						Header.MarkAsNeedingValidation();
						Header.Addresses.MarkAsNeedingValidation();
						RemoveRelatedInvalidZGuidValues("_AP");
					}
					else
					{
						OnCannotModifyAROrAPFlag(LedgerTypes.AccountsPayable);
						OB_IsCreditorInfo.RefreshBinding();
					}
				}
				else
				{
					if (value != OB_IsCreditor)
					{
						OnSecurityAccessDenied(LedgerTypes.AccountsPayable);
						OB_IsCreditorInfo.RefreshBinding();
					}
				}

				OB_OG_APCreditorGroupInfo.RefreshBinding();
			}
		}

		protected bool OB_IsCreditor_ReadOnly
		{
			get { return (!IsDeleted && !HasSecurityToChangeCreditor); }
		}

		bool HasSecurityToChangeCreditor
		{
			get
			{
				bool result = false;
				if (Header != null)
				{
					if (Header.IsInDatabase)
					{
						result = Header.OH_IsTempAccount ? Header.SecurityProvider.HasModifyDetailsOrgTypeTempAPFlag : Header.SecurityProvider.HasModifyDetailsOrgTypeFlagAP;
					}
					else
					{
						result = Header.OH_IsTempAccount ? Header.SecurityProvider.HasNewDetailsOrgTypeTempAPFlag : Header.SecurityProvider.HasNewDetailsOrgTypeFlagAP;
					}
				}
				return result;
			}
		}

		#region APTerms

		[List("Lookups.OB_APPaymentTerms_List")]
		public override ZString OB_APPaymentTerms
		{
			get { return base.OB_APPaymentTerms; }
			set
			{
				base.OB_APPaymentTerms = value;
				if (IsTermWithoutDays(OB_APPaymentTerms))
				{
					OB_APPaymentTermDaysInfo.Value = ZByte.Zero;
				}
			}
		}

		public InvoiceTerm GetAPTerm()
		{
			InvoiceTerm resultTerm = GetAPTermWithoutFallback();
			if (resultTerm.Term == OrgCompanyDataLookups.DefaultInvoiceTerm.Code && Header.APSettlementGroup != null)
			{
				resultTerm = Header.APSettlementGroup.CompanyData.GetAPTermWithoutFallback();
			}
			if (resultTerm.Term == OrgCompanyDataLookups.DefaultInvoiceTerm.Code)
			{
				resultTerm = new InvoiceTerm();
			}
			return resultTerm;
		}

		internal InvoiceTerm GetAPTermWithoutFallback()
		{
			return new InvoiceTerm(OB_APPaymentTerms, Lookups.OB_APPaymentTerms_ListWithoutDefaultValue.GetDescriptionFromCode(OB_APPaymentTerms), OB_APPaymentTermDays);
		}

		protected bool OB_APPaymentTermDays_ReadOnly
		{
			get { return IsTermWithoutDays(OB_APPaymentTerms); }
		}

		bool IsTermWithoutDays(ZString term)
		{
			return AccountingMasterFilesUtils.IsTermWithoutDays(term);
		}

		#endregion

		public override ZString OB_RX_NKAPDefltCurrency
		{
			get { return base.OB_RX_NKAPDefltCurrency; }
			set
			{
				base.OB_RX_NKAPDefltCurrency = value;
				if (Header != null)
				{
					SetDefaultPayablesAccount();
				}
			}
		}

		protected bool OB_APAirlineAccountNumber_ReadOnly
		{
			get { return (Header != null && !Header.OH_IsAirLine); }
		}

		protected bool OB_OG_APCreditorGroup_ReadOnly
		{
			get { return !OB_IsCreditor; }
		}

		#endregion

		#region Receivables (OB_IsDebtor)

		public override ZGuid OB_OJ_ARDebtorGroup
		{
			get { return base.OB_OJ_ARDebtorGroup; }
			set
			{
				base.OB_OJ_ARDebtorGroup = value;
				ARBankAccountToDisplayInfo.RefreshBinding();
			}
		}

		protected bool OB_OJ_ARDebtorGroup_ReadOnly
		{
			get { return !OB_IsDebtor; }
		}

		[List("Lookups.ARPayToAccounts")]
		public ZGuid ARBankAccountToDisplay
		{
			get
			{
				if (OverrideBankAccountFromDebtorGroup)
				{
					return OB_AB_ARPayToAccount;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
			set
			{
				if (OverrideBankAccountFromDebtorGroup)
				{
					OB_AB_ARPayToAccount = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateOB_AB_ARPayToAccount();
				}
				ARBankAccountToDisplayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ARBankAccountToDisplayInfo
		{
			get
			{
				return (OverrideBankAccountFromDebtorGroup)
								? OB_AB_ARPayToAccountInfo
								: GetZPropertyInfo(OrgCompanyData.Schema.ARBankAccountToDisplay);
			}
		}

		protected bool ARBankAccountToDisplay_ReadOnly
		{
			get { return !OverrideBankAccountFromDebtorGroup; }
		}

		public ZBool OverrideBankAccountFromDebtorGroup
		{
			get
			{
				if (ARPayToAccount != null)
				{
					fOverrideBankAccountFromDebtorGroup = true;
				}

				return fOverrideBankAccountFromDebtorGroup;
			}
			set
			{
				fOverrideBankAccountFromDebtorGroup = value;

				if (!value)
				{
					OB_AB_ARPayToAccount = ZGuid.Empty;
				}
				OverrideBankAccountFromDebtorGroupInfo.RefreshBinding();
				ARBankAccountToDisplayInfo.RefreshBinding();
			}
		}

		ZBool fOverrideBankAccountFromDebtorGroup;

		public ZPropertyInfo OverrideBankAccountFromDebtorGroupInfo
		{
			get { return GetZPropertyInfo(OrgCompanyData.Schema.OverrideBankAccountFromDebtorGroup); }
		}

		public override ZBool OB_IsDebtor
		{
			get { return base.OB_IsDebtor; }
			set
			{
				if (HasSecurityToChangeDebtor)
				{
					MarkAsNeedingValidation();

					if (Header == null)
					{
						base.OB_IsDebtor = value;
					}
					else if (value)
					{
						base.OB_IsDebtor = value;
						SetARTaxApplicable();

						Header.MarkAsNeedingValidation();
						Header.Addresses.MarkAsNeedingValidation();
						Header.MiscServ.MarkAsNeedingValidation();

						if (fInvoiceRollupOrGroups != null)
						{
							InvoiceRollupOrGroups.MarkAsNeedingValidation();
						}

						arTerms = new OrgARTermsCollection(this);
						arTerms.DeleteAll();
						arTerms = null;
					}
					else if (!HasARTransaction)
					{
						base.OB_IsDebtor = value;

						Header.MarkAsNeedingValidation();
						Header.Addresses.MarkAsNeedingValidation();

						RemoveRelatedInvalidZGuidValues("_AR");
					}
					else
					{
						OnCannotModifyAROrAPFlag(LedgerTypes.AccountsReceivable);
						OB_IsCreditorInfo.RefreshBinding();
					}
					ARTerms.MarkAsNeedingValidation();
				}
				else
				{
					if (value != OB_IsDebtor)
					{
						OnSecurityAccessDenied(LedgerTypes.AccountsReceivable);
						OB_IsDebtorInfo.RefreshBinding();
					}
				}

				OB_OJ_ARDebtorGroupInfo.RefreshBinding();
			}
		}

		protected bool OB_IsDebtor_ReadOnly
		{
			get { return !IsDeleted && !HasSecurityToChangeDebtor; }
		}

		protected virtual bool HasSecurityToChangeDebtor
		{
			get
			{
				bool result = false;
				if (Header != null)
				{
					if (Header.IsInDatabase)
					{
						result = Header.OH_IsTempAccount ? Header.SecurityProvider.HasModifyDetailsOrgTypeTempARFlag : Header.SecurityProvider.HasModifyDetailsOrgTypeFlagAR;
					}
					else
					{
						result = Header.OH_IsTempAccount ? Header.SecurityProvider.HasNewDetailsOrgTypeTempARFlag : Header.SecurityProvider.HasNewDetailsOrgTypeFlagAR;
					}
				}
				return result;
			}
		}

		#region ARTerms

		[ChildEditable]
		[ActionFieldFollow(true)]
		public OrgARTermsCollection ARTerms
		{
			get
			{
				if (arTerms == null)
				{
					arTerms = new OrgARTermsCollection(this);
					if (ARTerms.Count == 0)
					{
						LoadDefaultARTermsFromRegistry();
					}

					arTerms.SetReadOnlyIncludingChildren(CreditDetails_ReadOnly);

					RegisterEditableChildObject(arTerms);
				}
				return arTerms;
			}
		}
		OrgARTermsCollection arTerms;

		public void LoadDefaultARTermsFromRegistry()
		{
			ARTermsCollection registryValue = OrganisationRegistry.Instance.TermsAndTermDays.Value;

			if (registryValue.Count > 0)
			{
				OB_ARTreatDisbursementsAsStandardValue = ((ARTerms)registryValue.First()).TreatDisbursementsAsStandardValue;
			}

			foreach (ARTerms registryARTerms in registryValue)
			{
				OrgARTerms orgARTerms = Factory.New<OrgARTerms>();
				using (orgARTerms.GetValidationSuspender())
				using (orgARTerms.SuspendSettingHasChanges())
				{
					orgARTerms.PY_JobType = registryARTerms.JobType;
					orgARTerms.PY_TransportMode = registryARTerms.TransportMode;
					orgARTerms.PY_Direction = registryARTerms.Direction;
					if (!registryARTerms.BranchPK.IsEmpty)
					{
						orgARTerms.PY_GB_Branch = registryARTerms.BranchPK;
					}
					if (!registryARTerms.DeptPK.IsEmpty)
					{
						orgARTerms.PY_GE_Department = registryARTerms.DeptPK;
					}
					orgARTerms.PY_InvoiceClass = registryARTerms.InvoiceClass;
					orgARTerms.PY_InvoiceTerm = registryARTerms.InvoiceTerm;
					orgARTerms.PY_InvoiceDays = registryARTerms.InvoiceDays;

					foreach (ARTermsCycle registryARTermsCycle in registryARTerms.ARTermsCycles)
					{
						OrgARTermsCycle orgARTermsCycle = Factory.New<OrgARTermsCycle>();
						using (orgARTermsCycle.GetValidationSuspender())
						using (orgARTermsCycle.SuspendSettingHasChanges())
						{
							orgARTermsCycle.P5_ToDay = registryARTermsCycle.ToDay;
							orgARTermsCycle.P5_PaymentDay = registryARTermsCycle.PaymentDay;
						}
						orgARTerms.ARTermsCycles.Add(orgARTermsCycle);
					}

					foreach (ARPaymentCycle registryARPaymentCycle in registryARTerms.ARPaymentCycles)
					{
						OrgARPaymentCycle orgARPaymentCycle = Factory.New<OrgARPaymentCycle>();
						using (orgARPaymentCycle.GetValidationSuspender())
						using (orgARPaymentCycle.SuspendSettingHasChanges())
						{
							orgARPaymentCycle.P5_ToDay = registryARPaymentCycle.ToDay;
							orgARPaymentCycle.P5_PaymentDay = registryARPaymentCycle.PaymentDay;
						}
						orgARTerms.ARPaymentCycles.Add(orgARPaymentCycle);
					}

					ARTerms.Add(orgARTerms);
				}
			}
		}

		public OrgARTerms LoadARTermForAllInvoiceTypes()
		{
			return CreateOrLoadARTerm(OrgARTermsLookups.InvoiceTypes.All.Code);
		}

		public OrgARTerms CreateOrLoadDisbursementARTerm()
		{
			return CreateOrLoadARTerm(OrgARTermsLookups.InvoiceTypes.DSB.Code);
		}

		public OrgARTerms CreateOrLoadARTerm(string invoiceType)
		{
			OrgARTerms resultTerm = null;
			if (!string.IsNullOrEmpty(invoiceType))
			{
				IEnumerable<OrgARTerms> existingTerms = ARTerms.Find(new ZQuery(OrgARTermsSchema.PY_InvoiceClass, invoiceType));
				if (!existingTerms.Any())
				{
					resultTerm = ARTerms.AddNew();
					resultTerm.PY_InvoiceClass = invoiceType;
				}
				else
				{
					resultTerm = existingTerms.First();
				}
			}

			return resultTerm;
		}

		public InvoiceTerm GetDisbursementARTerm(JobInvoicingConsumerType jobType, ZString direction, ZString transportMode, ZGuid branchPK, ZGuid deptPK, string invoiceTermToSearchWithFallback = null)
		{
			return GetARTerm(jobType, direction, transportMode, branchPK, deptPK, OrgARTermsLookups.InvoiceTypes.DSB.Code, invoiceTermToSearchWithFallback);
		}

		public InvoiceTerm GetARTerm(JobInvoicingConsumerType jobType, ZString direction, ZString transportMode, ZGuid branchPK, ZGuid deptPK
			, string invoiceType = null, string invoiceTermToSearchWithFallback = null)
		{
			var resultTerm = GetARTermWithoutFallback(jobType, direction, transportMode, branchPK, deptPK, invoiceType, invoiceTermToSearchWithFallback);
			if (resultTerm.Term == OrgARTermsLookups.DefaultInvoiceTerm.Code && Header.ARSettlementGroup != null)
			{
				resultTerm = Header.ARSettlementGroup.CompanyData.GetARTermWithoutFallback(jobType, direction, transportMode, branchPK, deptPK, resultTerm.InvoiceClass, invoiceTermToSearchWithFallback);
			}
			if (resultTerm.Term == OrgARTermsLookups.DefaultInvoiceTerm.Code)
			{
				resultTerm = new InvoiceTerm();
			}
			return resultTerm;
		}

		[SuppressMessage("Microsoft.Design", "CA1021")]
		public bool TryToGetTheOnlyOrgARTerm(out InvoiceTerm invoiceTerm, bool isDisbursementTerm)
		{
			bool result = false;

			var arTerm = GetOrgARTerms(x => x.IsDisbursementTerm == isDisbursementTerm);
			var defaultARTerm = GetOrgARTerms(x => x.IsDefaultTerm);

			if (!arTerm.Any() && defaultARTerm.Any())
			{
				result = true;
				invoiceTerm = new InvoiceTerm(defaultARTerm.First());
			}
			else if (arTerm.Any() && arTerm.Count() == 1)
			{
				result = true;
				invoiceTerm = new InvoiceTerm(arTerm.First());
			}
			else
			{
				invoiceTerm = new InvoiceTerm();
			}
			return result;
		}

		public string GetOrgARTermsAsCSV(string separator = ",", Func<OrgARTerms, bool> filter = null)
		{
			string result = string.Empty;
			var termsText = new List<string>();
			var arTermsList = GetOrgARTerms(filter, x => termsText.Add(x.ShortFullText));

			if (arTermsList.Any())
			{
				result = string.Join(separator, termsText.OrderByDescending(x => x));
			}
			return result;
		}

		public string GetOrgARTermsAsLongText(Func<OrgARTerms, bool> filter = null)
		{
			string result = string.Empty;
			var termsText = new List<string>();
			var arTermsList = GetOrgARTerms(filter, x => termsText.Add(Res.GetString("cfff5afb-5a92-4a54-a6c2-2a4d8983b6a5", "{0}. {1}", (termsText.Count + 1), x.LongFullText)));

			if (arTermsList.Any())
			{
				result = string.Join(System.Environment.NewLine, termsText.OrderBy(x => x));
			}
			return result;
		}

		public IEnumerable<OrgARTerms> GetOrgARTerms(Func<OrgARTerms, bool> filter = null)
		{
			return GetOrgARTerms(filter: filter, termsText: null);
		}

		public bool IsCreditApprovedAndNotOnHold
		{
			get { return OB_ARCreditApproved && !OB_AROnCreditHold; }
		}

		public bool IsIncludedInTparReport => OB_APPrintContractorForm;

		bool IsCreditOnHold
		{
			get
			{
				var result = OB_AROnCreditHold;

				if (!result)
				{
					var globalCreditGroupParent = Organisation.MiscServ.ARGlobalCreditGroup ?? Organisation;
					result = globalCreditGroupParent.MiscServ.OM_ARGlobalOnCreditHold;
				}

				return result;
			}
		}

		internal InvoiceTerm GetARTermWithoutFallback(JobInvoicingConsumerType jobType, ZString direction, ZString transportMode, ZGuid branchPK, ZGuid deptPK
			, string invoiceType = null, string invoiceTermToSearchWithFallback = null)
		{
			if (IsCreditOnHold)
			{
				return OrganisationRegistry.Instance.UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold.Value
					? GetBestMatchedARTerm()
					: CreateInvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, OrganisationRegistry.Instance.OnHoldTerms.Value.TermDays);
			}
			else if (IsCreditApprovedAndNotOnHold)
			{
				return GetBestMatchedARTerm();
			}
			else
			{
				return CreateInvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, 0);
			}

			InvoiceTerm GetBestMatchedARTerm()
			{
				var resultTerm = GetBestMatchedARterms(jobType, direction, transportMode, branchPK, deptPK, invoiceType, invoiceTermToSearchWithFallback);
				return resultTerm != null ? new InvoiceTerm(resultTerm) : new InvoiceTerm();
			}

			InvoiceTerm CreateInvoiceTerm(string terms, byte termDays)
			{
				var description = new ARInvoiceTermsList().GetDescriptionFromCode(terms);
				return new InvoiceTerm(terms, description, termDays);
			}
		}

		IEnumerable<OrgARTerms> GetOrgARTerms(Func<OrgARTerms, bool> filter, Action<OrgARTerms.ToStringConverter> termsText)
		{
			List<OrgARTerms> arTermsList = new List<OrgARTerms>();

			if (ARTerms != null)
			{
				foreach (OrgARTerms arTerm in (filter != null ? ARTerms.Where(filter) : ARTerms))
				{
					if (arTerm.PY_InvoiceTerm == OrgARTermsLookups.DefaultInvoiceTerm.Code && Header.ARSettlementGroup != null)
					{
						var sgARTerm = Header.ARSettlementGroup.CompanyData.GetBestMatchedARterms(arTerm.JobType, arTerm.PY_Direction, arTerm.PY_TransportMode, arTerm.PY_GB_Branch, arTerm.PY_GE_Department, arTerm.PY_InvoiceClass, null);
						arTermsList.Add(sgARTerm);

						if (termsText != null && sgARTerm != null)
						{
							termsText(new OrgARTerms.ToStringConverter(arTerm.Lookups, arTerm.JobType?.Code, arTerm.PY_Direction, arTerm.PY_TransportMode,
																		arTerm.Branch, arTerm.Department,
																		arTerm.PY_InvoiceClass,
																		sgARTerm.PY_InvoiceTerm, sgARTerm.PY_InvoiceDays.ToZInt(),
																		sgARTerm.IsTermWithoutDays, InvoiceTerm.GetIsTermWitMonths(sgARTerm.PY_InvoiceTerm)));
						}
					}
					else
					{
						arTermsList.Add(arTerm);
						if (termsText != null)
						{
							termsText(new OrgARTerms.ToStringConverter(arTerm));
						}
					}
				}
			}

			return arTermsList;
		}

		public OrgARTerms GetBestMatchedARterms(JobInvoicingConsumerType jobType, ZString direction, ZString transportMode, ZGuid branchPK, ZGuid deptPK, string invoiceType = null, string invoiceTermToSearchWithFallback = null)
		{
			var arTermsList = new List<OrgARTerms>();
			if (invoiceType == OrgARTermsLookups.InvoiceTypes.DSB.Code)
			{
				arTermsList.AddRange(ARTerms.Where(x => x.IsDefaultTerm));
				arTermsList.AddRange(ARTerms.Where(x => x.IsDisbursementTerm));
			}
			else
			{
				arTermsList.AddRange(ARTerms.OfType<OrgARTerms>());
			}

			var ranker = new ColumnValueRanker();
			if (!string.IsNullOrWhiteSpace(invoiceTermToSearchWithFallback))
			{
				ranker.Add(OrgARTermsSchema.PY_InvoiceTerm, GetInvoiceTermToSearchFallBackCode(invoiceTermToSearchWithFallback));
			}
			ranker.Add(OrgARTermsSchema.PY_JobType, GetJobTypeFallBackCode(jobType));
			ranker.Add(OrgARTermsSchema.PY_Direction, GetDirectionFallBackCode(direction));
			ranker.Add(OrgARTermsSchema.PY_TransportMode, GetTransportModeFallBackCode(transportMode));
			ranker.Add(OrgARTermsSchema.PY_GE_Department, GetGuidFallBackPK(deptPK));
			ranker.Add(OrgARTermsSchema.PY_GB_Branch, GetGuidFallBackPK(branchPK));
			ranker.Add(OrgARTermsSchema.PY_InvoiceClass, GetInvoiceTypeFallBack(invoiceType));

			var matchedARTerms = ranker.GetBestMatch(arTermsList);
			OrgARTerms resultTerm = null;

			if (matchedARTerms != null)
			{
				foreach (OrgARTerms term in matchedARTerms)
				{
					if (string.IsNullOrEmpty(invoiceTermToSearchWithFallback)
							|| term.PY_InvoiceTerm == invoiceTermToSearchWithFallback
							|| term.PY_InvoiceTerm == OrgARTermsLookups.DefaultInvoiceTerm.Code)
					{
						resultTerm = term;
						break;
					}
				}
			}
			return resultTerm;
		}

		IZType[] GetInvoiceTermToSearchFallBackCode(ZString invoiceTermToSearchWithFallback)
		{
			List<IZType> result = new List<IZType>();
			if (!string.IsNullOrWhiteSpace(invoiceTermToSearchWithFallback))
			{
				result.Add(invoiceTermToSearchWithFallback);
			}
			result.Add((ZString)OrgARTermsLookups.DefaultInvoiceTerm.Code);
			return result.ToArray();
		}

		IZType[] GetJobTypeFallBackCode(JobInvoicingConsumerType jobType)
		{
			List<IZType> result = new List<IZType>();
			if (jobType != null)
			{
				result.Add(new ZString(jobType.Code));
			}
			result.Add((ZString)JobTypeDirectionAndTransportInfoProvider.All);
			result.Add(ZString.Empty);
			return result.ToArray();
		}

		IZType[] GetDirectionFallBackCode(ZString direction)
		{
			List<IZType> result = new List<IZType>();
			if (!string.IsNullOrWhiteSpace(direction) && direction != JobTypeDirectionAndTransportInfoProvider.All)
			{
				result.Add(direction);
			}
			result.Add((ZString)JobTypeDirectionAndTransportInfoProvider.All);
			result.Add(ZString.Empty);
			return result.ToArray();
		}

		IZType[] GetTransportModeFallBackCode(ZString transportMode)
		{
			List<IZType> result = new List<IZType>();
			if (!string.IsNullOrWhiteSpace(transportMode) && transportMode != JobTypeDirectionAndTransportInfoProvider.All)
			{
				result.Add(transportMode);
			}
			result.Add((ZString)JobTypeDirectionAndTransportInfoProvider.All);
			result.Add(ZString.Empty);
			return result.ToArray();
		}

		IZType[] GetGuidFallBackPK(ZGuid pk)
		{
			List<IZType> result = new List<IZType>();
			if (pk.IsValid)
			{
				result.Add(pk);
			}
			result.Add(ZGuid.Empty);
			return result.ToArray();
		}

		IZType[] GetInvoiceTypeFallBack(ZString invoiceType)
		{
			List<IZType> result = new List<IZType>();

			if (!string.IsNullOrWhiteSpace(invoiceType) && invoiceType != JobTypeDirectionAndTransportInfoProvider.All)
			{
				result.Add(invoiceType);
				if (InvoiceTypeCalculationProvider.IsDeferredInvoiceType(invoiceType))
				{
					result.Add(new ZString(InvoiceTypeCalculationProvider.ConvertDeferredInvoiceTypeToNonDeferredOne(invoiceType)));
				}
				if (InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(invoiceType))
				{
					result.Add(new ZString(OrgARTermsLookups.InvoiceTypes.DSB.Code));
				}
			}
			result.Add((ZString)OrgARTermsLookups.InvoiceTypes.All.Code);
			result.Add(ZString.Empty);
			return result.ToArray();
		}

		#endregion

		#endregion

		#region Buyers Consol Invoicing Style

		public ZString EffectiveBuyersConsolInvoicingStyle
		{
			get { return OB_ARBuyersConsolInvoicingStyle == OrgCompanyDataLookups.BuyersConsolInvoicingStyleList.Default ? (ZString)OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.Value : OB_ARBuyersConsolInvoicingStyle; }
		}

		#endregion

		#region Shippers Consol Invoicing Style

		public ZString EffectiveShippersConsolInvoicingStyle
		{
			get { return OB_ARShippersConsolInvoicingStyle == OrgCompanyDataLookups.ShippersConsolInvoicingStyleList.Default ? (ZString)OrganisationsDataRegistry.Instance.ShippersConsolInvoicingStyle.Value : OB_ARShippersConsolInvoicingStyle; }
		}

		#endregion

		#region IsImportAirUpliftValid

		protected ZBool fIsImportAirUpliftValid = true;
		public ZBool IsImportAirUpliftValid
		{
			get
			{
				return fIsImportAirUpliftValid;
			}
			set
			{
				if (fIsImportAirUpliftValid != value)
				{
					fIsImportAirUpliftValid = value;
					IsImportAirUpliftValidInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsImportAirUpliftValidInfo
		{
			get { return GetZPropertyInfo(nameof(IsImportAirUpliftValid)); }
		}

		#endregion

		#region IsExportAirUpliftValid

		protected ZBool fIsExportAirUpliftValid = true;
		public ZBool IsExportAirUpliftValid
		{
			get { return fIsExportAirUpliftValid; }
			set
			{
				if (fIsExportAirUpliftValid != value)
				{
					fIsExportAirUpliftValid = value;
					IsExportAirUpliftValidInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsExportAirUpliftValidInfo
		{
			get { return GetZPropertyInfo(nameof(IsExportAirUpliftValid)); }
		}

		#endregion

		#region IsImportSeaUpliftValid

		public ZBool IsImportSeaUpliftValid
		{
			get
			{
				return fIsImportSeaUpliftValid;
			}
			set
			{
				if (fIsImportSeaUpliftValid != value)
				{
					fIsImportSeaUpliftValid = value;
					IsImportSeaUpliftValidInfo.RefreshBinding();
				}
			}
		}
		ZBool fIsImportSeaUpliftValid = true;

		public ZPropertyInfo IsImportSeaUpliftValidInfo
		{
			get { return GetZPropertyInfo(nameof(IsImportSeaUpliftValid)); }
		}

		#endregion

		#region IsExportSeaUpliftValid

		public ZBool IsExportSeaUpliftValid
		{
			get { return fIsExportSeaUpliftValid; }
			set
			{
				if (fIsExportSeaUpliftValid != value)
				{
					fIsExportSeaUpliftValid = value;
					IsExportSeaUpliftValidInfo.RefreshBinding();
				}
			}
		}
		ZBool fIsExportSeaUpliftValid = true;

		public ZPropertyInfo IsExportSeaUpliftValidInfo
		{
			get { return GetZPropertyInfo(nameof(IsExportSeaUpliftValid)); }
		}

		#endregion

		#region OB_NoCreditLimitSetLabelIsVisible

		public ZBool OB_NoCreditLimitSetLabelIsVisible
		{
			get { return OB_ARCreditLimit.IsEmpty; }
		}

		public ZPropertyInfo OB_NoCreditLimitSetLabelIsVisibleInfo
		{
			get { return GetZPropertyInfo(Schema.OB_NoCreditLimitSetLabelIsVisible); }
		}

		public ZBool OB_ARGlobalCreditApprovedLabelIsVisible
		{
			get
			{
				var miscServ = Organisation.MiscServ;
				var isApproved = miscServ.OM_ARGlobalCreditApproved;

				if (!isApproved && miscServ.IsMemberOfGlobalCreditGroup)
				{
					isApproved = miscServ.ARGlobalCreditGroup.MiscServ.OM_ARGlobalCreditApproved;
				}

				return !isApproved;
			}
		}

		public ZBool OB_OnARGlobalCreditHoldLabelIsVisible
		{
			get
			{
				var miscServ = Organisation.MiscServ;
				var result = miscServ.OM_ARGlobalOnCreditHold;

				if (!result && miscServ.IsMemberOfGlobalCreditGroup)
				{
					result = miscServ.ARGlobalCreditGroup.MiscServ.OM_ARGlobalOnCreditHold;
				}

				return result;
			}
		}

		public ZString OB_NoGlobalCreditLimitSetLabelValue
		{
			get
			{
				var result = string.Empty;
				var miscServ = Organisation.MiscServ;

				if (miscServ.OM_OH_ARGlobalCreditGroup.IsEmpty)
				{
					if (miscServ.OM_ARGlobalCreditLimit > 0)
					{
						result = Res.GetString("3fd87d67-10d9-43e4-b406-2558fa649ada", "This Organization is the Global Credit Group. Credit Limit set.");
					}
					else
					{
						result = Res.GetString("bb0de457-9d93-4511-828d-0ab784244e92", "No Global Credit Limit Set.");
					}
				}
				else
				{
					result = Res.GetString("6482706c-fb16-4a68-9b8a-1a52212014d5", "Use Global Credit Group’s Credit Limit");
				}

				return result;
			}
		}

		#endregion

		#region OB_ARCreditLimitNotApprovedLabelIsVisible

		public ZBool OB_ARCreditLimitNotApprovedLabelIsVisible
		{
			get { return !OB_ARCreditApproved; }
		}

		public ZPropertyInfo OB_ARCreditLimitNotApprovedLabelIsVisibleInfo
		{
			get { return GetZPropertyInfo(Schema.OB_ARCreditLimitNotApprovedLabelIsVisible); }
		}

		#endregion

		#region OB_VATConfigLabel

		public string OB_VATConfigLabel
		{
			get { return Res.GetString("C56D1079-095B-4B39-92E9-7378A196CACE", "{0} Recognition and reporting Basis Override", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription); }
		}

		#endregion

		#region OB_ARVATConfig

		[List("Lookups.OB_VATConfigList")]
		public override ZString OB_ARVATConfig
		{
			get { return base.OB_ARVATConfig; }
			set
			{
				bool wasChanged = OB_ARVATConfig != value;
				base.OB_ARVATConfig = value;
				if (wasChanged && Header != null && Header.IsInDatabase && !Factory.IsInTransaction && OB_IsDebtor)
				{
					Header.OnShowMessage(Res.GetString("5E08EA65-9A25-4692-9A58-D76C8DDC35A4", "Changing Debtor Tax Configuration"),
						Res.GetString("28B11112-3E30-4F39-921E-81F09C02AB3C", "Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Debtor will NOT change."));
				}
			}
		}

		[List("Lookups.OB_ARCreateVATComplianceDocumentOnPostingList")]
		public override ZString OB_ARCreateVATComplianceDocumentOnPosting
		{
			get => base.OB_ARCreateVATComplianceDocumentOnPosting;
			set => base.OB_ARCreateVATComplianceDocumentOnPosting = value;
		}

		[List("Lookups.OB_APCreateVATComplianceDocumentOnPostingList")]
		public override ZString OB_APCreateVATComplianceDocumentOnPosting
		{
			get => base.OB_APCreateVATComplianceDocumentOnPosting;
			set => base.OB_APCreateVATComplianceDocumentOnPosting = value;
		}

		public ZBool IsAPPCDSetting => OB_APCreateVATComplianceDocumentOnPosting == Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;

		public ZBool IsARRCCSetting => OB_ARCreateVATComplianceDocumentOnPosting == Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;

		public ZBool IsAPRCCSetting => OB_APCreateVATComplianceDocumentOnPosting == Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;

		public ZBool IsARTaxApplicable
		{
			get { return OB_ARVATConfig != AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code; }
		}

		public void SetARTaxApplicable(bool isApplicable)
		{
			if (isApplicable != IsARTaxApplicable)
			{
				if (isApplicable)
				{
					if (Organisation == null || Organisation.CountryCode.IsEmpty)
					{
						OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
					}
					else
					{
						OB_ARVATConfig = GetARDefaultTaxRecognitionCodeFromRegistry(Organisation.UNLOCO);
					}
				}
				else
				{
					OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
				}
			}
		}

		internal void RedefaultTaxRecognitionForAR(RefUNLOCO unLOCO)
		{
			if (unLOCO == null)
			{
				OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			}
			else
			{
				OB_ARVATConfig = GetARDefaultTaxRecognitionCodeFromRegistry(unLOCO);
			}
		}

		string GetARDefaultTaxRecognitionCodeFromRegistry(RefUNLOCO unLOCO)
		{
			var result = string.Empty;
			var (loginCompanyCountryRule, organizationCountryRule) = CalculateLoginCompanyAndOrganizationCountryRule(unLOCO);
			var matchedRule = AccountingMasterFilesRegistry.Instance.ARDefaultTaxRecognitionRule.GetValueWithoutFallback(OB_GC.ToGuid(), Guid.Empty, Guid.Empty)
				.Cast<ARAPDefaultTaxRecognitionRule>()
				.FirstOrDefault(x => x.LoginCompanyCountryRuleCode == loginCompanyCountryRule && x.OrganizationCountryRuleCode == organizationCountryRule);
			if (matchedRule != null)
			{
				result = matchedRule.TaxRecognitionCode;
			}
			return result;
		}

		(ZString, ZString) CalculateLoginCompanyAndOrganizationCountryRule(RefUNLOCO unLOCO)
		{
			var isLoginCompanyInEU = Company.Country != null && Company.Country.IsPartOfEuropeanUnion;
			var loginCompanyCountryRule = string.Empty;
			var organizationCountryRule = string.Empty;

			if (isLoginCompanyInEU)
			{
				loginCompanyCountryRule = LoginCompanyCountryRuleCode.IEU;
				organizationCountryRule = (unLOCO != null && unLOCO.IsInEU) ? OrganizationCountryRuleCode.IEU : OrganizationCountryRuleCode.OEU;
			}
			else
			{
				var isOrganizationHasSameCountryAsLoginCompany = (unLOCO != null && unLOCO.RL_RN_NKCountryCode == Company.GC_RN_NKCountryCode);
				loginCompanyCountryRule = LoginCompanyCountryRuleCode.OEU;
				organizationCountryRule = isOrganizationHasSameCountryAsLoginCompany ? OrganizationCountryRuleCode.SAL : OrganizationCountryRuleCode.DTL;
			}

			return (loginCompanyCountryRule, organizationCountryRule);
		}

		#endregion

		#region OB_ARCreditRating

		[List("Lookups.OB_ARCreditRating_List")]
		public override ZString OB_ARCreditRating
		{
			get { return base.OB_ARCreditRating; }
			set { base.OB_ARCreditRating = value; }
		}
		#endregion

		#region OB_APVATConfig

		[List("Lookups.OB_VATConfigList")]
		public override ZString OB_APVATConfig
		{
			get { return base.OB_APVATConfig; }
			set
			{
				bool wasChanged = OB_APVATConfig != value;
				base.OB_APVATConfig = value;
				if (wasChanged && Header != null && Header.IsInDatabase && !Factory.IsInTransaction && OB_IsCreditor)
				{
					Header.OnShowMessage(Res.GetString("E9C2820B-F570-409E-9D06-78F72D9A70AD", "Changing Creditor Tax Configuration"),
						Res.GetString("5517F238-2B58-483C-BF64-8235A12B78A2", "Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Creditor will NOT change."));
				}
			}
		}

		public ZBool IsAPTaxApplicable
		{
			get { return OB_APVATConfig != AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code; }
		}

		public void RedefaultTaxRecognitionForAP(RefUNLOCO unLOCO)
		{
			if (unLOCO != null)
			{
				OB_APVATConfig = GetAPDefaultTaxRecognitionCodeFromRegistry(unLOCO);
			}
			// when unLOCO is missing, there is no existing logic to assign a default value.
			//it's the safest way to leave as it is rather than assign a random default value
		}

		string GetAPDefaultTaxRecognitionCodeFromRegistry(RefUNLOCO unLOCO)
		{
			var result = string.Empty;
			var (loginCompanyCountryRule, organizationCountryRule) = CalculateLoginCompanyAndOrganizationCountryRule(unLOCO);
			var matchedRule = AccountingMasterFilesRegistry.Instance.APDefaultTaxRecognitionRule.GetValueWithoutFallback(OB_GC.ToGuid(), Guid.Empty, Guid.Empty)
				.Cast<ARAPDefaultTaxRecognitionRule>()
				.FirstOrDefault(x => x.LoginCompanyCountryRuleCode == loginCompanyCountryRule && x.OrganizationCountryRuleCode == organizationCountryRule);
			if (matchedRule != null)
			{
				result = matchedRule.TaxRecognitionCode;
			}
			return result;
		}

		public void SetAPTaxApplicable(bool isApplicable)
		{
			SetAPVATConfig(isApplicable, () =>
			{
#if DEBUG
				if (Globals.IsTest && (Organisation == null || Organisation.CountryCode.IsEmpty))
				{
					OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
					return;
				}
#endif
				OB_APVATConfig = GetAPDefaultTaxRecognitionCodeFromRegistry(Organisation.UNLOCO);
			});
		}

		public void SetAPTaxApplicableIgnoringRegistrySetting(bool isApplicable)
		{
			SetAPVATConfig(isApplicable, () => { OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code; });
		}

		void SetAPVATConfig(bool isApplicable, Action setValueAction)
		{
			if (isApplicable != IsAPTaxApplicable)
			{
				if (isApplicable)
				{
					setValueAction();
				}
				else
				{
					OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
				}
			}
		}

		#endregion

		#region OB_APCategory

		[List("Lookups.OB_APCategory_List")]
		public override ZString OB_APCategory
		{
			get { return base.OB_APCategory; }
			set { base.OB_APCategory = value; }
		}

		#endregion

		#region OB_ARConsolidatedAccountingCategory

		[List("Header.MiscServ.OM_ARConsolidatedAccountingCategory_List")]
		public override ZString OB_ARConsolidatedAccountingCategory
		{
			get { return base.OB_ARConsolidatedAccountingCategory; }
			set { base.OB_ARConsolidatedAccountingCategory = value; }
		}

		#endregion

		#region OB_ARBuyersConsolInvoicingStyle
		[List("Lookups.BuyersConsolInvoicingStyles")]
		public override ZString OB_ARBuyersConsolInvoicingStyle
		{
			get
			{
				return base.OB_ARBuyersConsolInvoicingStyle;
			}
			set
			{
				base.OB_ARBuyersConsolInvoicingStyle = value;
			}
		}
		#endregion

		#region OB_ARShippersConsolInvoicingStyle
		[List("Lookups.ShippersConsolInvoicingStyles")]
		public override ZString OB_ARShippersConsolInvoicingStyle
		{
			get
			{
				return base.OB_ARShippersConsolInvoicingStyle;
			}
			set
			{
				base.OB_ARShippersConsolInvoicingStyle = value;
			}
		}
		#endregion

		#region OB_ARGoodsOwnership
		[List("Lookups.OB_GoodsOwnership_List")]
		public override ZString OB_ARGoodsOwnership
		{
			get { return base.OB_ARGoodsOwnership; }
			set { base.OB_ARGoodsOwnership = value; }
		}

		#endregion

		#region OB_APExternalCreditorCode

		public override ZString OB_APExternalCreditorCode
		{
			get
			{
				if (base.OB_APExternalCreditorCode.IsEmpty && Header != null)
				{
					return Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ExternalCreditorAccountCode);
				}
				return base.OB_APExternalCreditorCode;
			}
			set
			{
				base.OB_APExternalCreditorCode = value;
				OB_APExternalCreditorCodeInfo.RefreshBinding();
			}
		}

		#endregion

		#region OB_ARExternalDebtorCode

		public override ZString OB_ARExternalDebtorCode
		{
			get
			{
				if (base.OB_ARExternalDebtorCode.IsEmpty && Header != null)
				{
					return Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ExternalDebtorAccountCode);
				}
				return base.OB_ARExternalDebtorCode;
			}
			set
			{
				base.OB_ARExternalDebtorCode = value;
				OB_ARExternalDebtorCodeInfo.RefreshBinding();
			}
		}

		#endregion

		#region OB_ARClientNumber

		protected bool OB_ARClientNumber_ReadOnly => true;

		#endregion

		#region OB_CusPaidBy

		[List("Lookups.OB_CusPaidByList")]
		public override ZString OB_CusPaidBy
		{
			get { return base.OB_CusPaidBy; }
			set { base.OB_CusPaidBy = value; }
		}

		#endregion

		#region OB_RateSecurityGroup

		[List("Lookups.OB_RateSecurityGroup_List")]
		public override ZString OB_RateSecurityGroup
		{
			get { return base.OB_RateSecurityGroup; }
			set { base.OB_RateSecurityGroup = value; }
		}

		#endregion

		#region Code

		public ZString Code => Organisation?.OH_Code ?? ZString.Empty;

		#endregion

		#region ControllingBranch
		[List("Lookups.ControllingBranches")]
		public override ZGuid OB_GB_ControllingBranch
		{
			get
			{
				return base.OB_GB_ControllingBranch;
			}
			set
			{
				base.OB_GB_ControllingBranch = value;
				SetDefaultPayablesAccount();
			}
		}

		#endregion

		#region Warehouse

		#region OB_ARWarehouseRatingPeriod
		[List("Lookups.WarehouseRatingPeriods")]
		public override ZString OB_ARWarehouseRatingPeriod
		{
			get
			{
				return base.OB_ARWarehouseRatingPeriod;
			}
			set
			{
				base.OB_ARWarehouseRatingPeriod = value;
			}
		}

		public ZString GetWarehouseRatingPeriod()
		{
			if (OB_ARWarehouseRatingPeriod == Core.Constants.StorageCalculationPeriods.Default)
			{
				return Env.Registry.Rating.StorageCalculationPeriod;
			}
			else
			{
				return OB_ARWarehouseRatingPeriod;
			}
		}

		#endregion

		#region OB_ARWhsStorageCalcMethod
		[List("Lookups.WarehouseStorageCalculationMethods")]
		public override ZString OB_ARWhsStorageCalcMethod
		{
			get { return base.OB_ARWhsStorageCalcMethod; }
			set
			{
				base.OB_ARWhsStorageCalcMethod = value;
			}
		}

		public ZBool IsWhsSplitMonthBilling
		{
			get { return OB_ARWhsStorageCalcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling; }
		}

		public ZPropertyInfo IsWhsSplitMonthBillingInfo
		{
			get { return GetZPropertyInfo(nameof(IsWhsSplitMonthBilling)); }
		}

		#endregion

		#region OB_WhsClientFreeStorageDays

		public override ZByte OB_WhsClientFreeStorageDays
		{
			get
			{
				return OB_WhsOverrideFreeStorage ? base.OB_WhsClientFreeStorageDays : (ZByte)RatingDataRegistry.Instance.WarehouseClientFreeStorageDays.Value;
			}
			set
			{
				if (OB_WhsClientFreeStorageDays != value)
				{
					base.OB_WhsClientFreeStorageDays = value;
					OB_WhsOverrideFreeStorage = true;
				}
			}
		}

		protected bool OB_WhsClientFreeStorageDays_ReadOnly
		{
			get { return !OB_WhsOverrideFreeStorage; }
		}

		#endregion

		#region OB_WhsAutoCreateAndRatePeriodicInvoice

		public override ZBool OB_WhsAutoCreateAndRatePeriodicInvoice
		{
			get => base.OB_WhsAutoCreateAndRatePeriodicInvoice;
			set
			{
				base.OB_WhsAutoCreateAndRatePeriodicInvoice = value;
				OB_WhsAutoPostPeriodicInvoice = value;
				if (!value)
				{
					OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = false;
				}
			}
		}

		#endregion

		#region OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock

		[ReadOnlyMember(nameof(OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockReadOnly))]
		public override ZBool OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock
		{
			get => base.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock;
			set => base.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = value;
		}

		bool OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockReadOnly => !OB_WhsAutoCreateAndRatePeriodicInvoice;

		#endregion

		#region OB_WhsAutoPostPeriodicInvoice

		[ReadOnlyMember(nameof(OB_WhsAutoPostPeriodicInvoiceReadOnly))]
		public override ZBool OB_WhsAutoPostPeriodicInvoice
		{
			get => base.OB_WhsAutoPostPeriodicInvoice;
			set
			{
				base.OB_WhsAutoPostPeriodicInvoice = value;
				OB_WhsAutoDeliverPeriodicInvoice = value;
			}
		}

		bool OB_WhsAutoPostPeriodicInvoiceReadOnly => !OB_WhsAutoCreateAndRatePeriodicInvoice;

		#endregion

		#region OB_WhsAutoDeliverPeriodicInvoice

		[ReadOnlyMember(nameof(OB_WhsAutoDeliverPeriodicInvoiceReadOnly))]
		public override ZBool OB_WhsAutoDeliverPeriodicInvoice
		{
			get => base.OB_WhsAutoDeliverPeriodicInvoice;
			set => base.OB_WhsAutoDeliverPeriodicInvoice = value;
		}
		bool OB_WhsAutoDeliverPeriodicInvoiceReadOnly => !OB_WhsAutoPostPeriodicInvoice;

		#endregion

		#region OB_ARYardStorageRatingPeriod

		[List("Lookups.WarehouseRatingPeriods")]
		public override ZString OB_ARYardStorageRatingPeriod
		{
			get => base.OB_ARYardStorageRatingPeriod;
			set => base.OB_ARYardStorageRatingPeriod = value;
		}

		public ZString YardStorageRatingPeriod
			=> OB_ARYardStorageRatingPeriod == Core.Constants.StorageCalculationPeriods.Default
				? Env.Registry.Rating.StorageCalculationPeriod
				: OB_ARYardStorageRatingPeriod;

		#endregion

		#region OB_ARYardStorageCalcMethod

		[List("Lookups.YardStorageCalculationMethods")]
		public override ZString OB_ARYardStorageCalcMethod
		{
			get => base.OB_ARYardStorageCalcMethod;
			set => base.OB_ARYardStorageCalcMethod = value;
		}

		#endregion

		#endregion

		#region Credit Limit

		protected bool OB_ARCreditLimit_ReadOnly
		{
			get { return OB_ARUseSettlementGroupCreditLimit; }
		}

		public int RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit
		{
			get
			{
				int result = -1;

				if (Header != null)
				{
					result = Header.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit();
				}

				return result;
			}
		}

		public int RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck()
		{
			return RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheckCore() % GlobalFlag;
		}

		public bool RequiredAuthorizationForCreditControlledDocumentDeliveryDueToGlobalCreditCheck => RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheckCore() > GlobalFlag;

		int RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheckCore()
		{
			return Factory.GetCachedValue("CreditControlledDocumentDeliveryDueToCreditCheck" + PK, () =>
			{
				var result = 0;

				if (Header != null)
				{
					result = GetRequiredAuthorizationForLocalCreditControlledDocumentDeliveryDueToCreditCheck();
					if (result < AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly))
					{
						var miscServ = Header.MiscServ;
						if (Header.CompanyData.OB_IsDebtor && miscServ.GlobalCreditCurrency != null && miscServ.ARGlobalCreditApproved)
						{
							var globalApprovalLevel = GetRequiredAuthorizationForGlobalCreditControlledDocumentDeliveryDueToCreditCheck();
							result = globalApprovalLevel > result ? globalApprovalLevel + GlobalFlag : result;
						}
					}
				}

				return result;
			});
		}

		const int GlobalFlag = 100;

		int GetRequiredAuthorizationForLocalCreditControlledDocumentDeliveryDueToCreditCheck()
		{
			var result = 0;
			if (!OB_ARDoNotCheckOverdueInvoicesStatus)
			{
				result = GetRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck(GetConfiguration(), GetOrgCreditControlledDocumentsApprovalBalancesAndDueDays());
			}
			return result;
		}

		int GetRequiredAuthorizationForGlobalCreditControlledDocumentDeliveryDueToCreditCheck()
		{
			var result = 0;
			Dictionary<InvoiceType, OutStanding> balancesAndDueDays;
			if (!TryGetOrgGlobalCreditControlledDocumentsApprovalBalancesAndDueDays(out balancesAndDueDays))
			{
				result = AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.MissingExRate);
			}
			else if (Organisation.MiscServ == null || !Organisation.MiscServ.OM_GlobalDoNotCheckOverdueInvoicesStatus)
			{
				result = GetRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck(GetGlobalConfiguration(), balancesAndDueDays);
			}

			return result;
		}

		int GetRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck(Dictionary<ApprovalLevel, List<ICreditControlledDocumentsCheckConfiguration>> configuration, Dictionary<InvoiceType, OutStanding> balancesAndDueDays)
		{
			var result = 0;

			if (configuration.Any())
			{
				var configLevels = new List<ApprovalLevel>(configuration.Keys);

				var maxLevel = configLevels.Max();
				configLevels.Remove(maxLevel);

				var currentlyMatchedSettings = new ICreditControlledDocumentsCheckConfiguration[] { null, null, null };

				var isMaxlevelApplicable = DoBalancesAndDueDaysMatchSettings(balancesAndDueDays, configuration[maxLevel], ref currentlyMatchedSettings);
				result = isMaxlevelApplicable ? (int)maxLevel : result;

				if (configLevels.Count > 0 && !isMaxlevelApplicable)
				{
					while (configLevels.Count >= 1)
					{
						var minLevel = configLevels.Min();
						configLevels.Remove(minLevel);
						DoBalancesAndDueDaysMatchSettings(balancesAndDueDays, configuration[minLevel], ref currentlyMatchedSettings);
					}

					var validSettings = currentlyMatchedSettings.Where(x => x != null);
					result = validSettings.Any() ? validSettings.Max(x => AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(x.AuthorisationRequirement)) : 0;
				}
			}

			return result;
		}

		bool DoBalancesAndDueDaysMatchSettings(Dictionary<InvoiceType, OutStanding> balancesAndDueDays, List<ICreditControlledDocumentsCheckConfiguration> settings, ref ICreditControlledDocumentsCheckConfiguration[] currentMatchedSettings)
		{
			var result = false;

			foreach (var setting in settings)
			{
				var invoiceType = GetInvoiceTypeEnum(setting.InvoiceType);
				var amount = balancesAndDueDays[invoiceType].Amount;
				var numberOfDaysOverdue = balancesAndDueDays[invoiceType].NumberOfDaysOverdue;

				if (amount == 0 || numberOfDaysOverdue == 0)
				{
					continue;
				}

				if (setting.Range == AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo)
				{
					if (currentMatchedSettings[(int)invoiceType] == null || setting.Amount < currentMatchedSettings[(int)invoiceType].Amount || setting.NumberOfDaysOverdue < currentMatchedSettings[(int)invoiceType].NumberOfDaysOverdue)
					{
						if (amount <= setting.Amount && numberOfDaysOverdue <= setting.NumberOfDaysOverdue)
						{
							currentMatchedSettings[(int)invoiceType] = setting;
							result = true;
						}
					}
				}
				else if (setting.Range == AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above)
				{
					if (amount > setting.Amount || numberOfDaysOverdue > setting.NumberOfDaysOverdue)
					{
						currentMatchedSettings[(int)invoiceType] = setting;
						result = true;
					}
				}
			}

			return result;
		}

		enum ApprovalLevel
		{
			NoApprovalRequired = 0,
			FirstApprovalRequiredOnly = 1,
			SecondApprovalRequiredOnly = 2,
			ThirdApprovalRequiredOnly = 3
		}

		enum InvoiceType
		{
			ALL = 0,
			DSB = 1,
			NDB = 2,
		}

		Dictionary<ApprovalLevel, List<ICreditControlledDocumentsCheckConfiguration>> GetConfiguration()
		{
			return GetConfiguration(ObjectFactory.Get<IAccounting>().GetCreditControlledDocumentsCheckConfiguration());
		}

		Dictionary<ApprovalLevel, List<ICreditControlledDocumentsCheckConfiguration>> GetGlobalConfiguration()
		{
			return GetConfiguration(ObjectFactory.Get<IAccounting>().GetGlobalCreditControlledDocumentsCheckConfiguration());
		}

		Dictionary<ApprovalLevel, List<ICreditControlledDocumentsCheckConfiguration>> GetConfiguration(ICreditControlledDocumentsCheckConfiguration[] configuration)
		{
			var result = new Dictionary<ApprovalLevel, List<ICreditControlledDocumentsCheckConfiguration>>();

			if (configuration != null)
			{
				foreach (var item in configuration)
				{
					var key = ApprovalLevel.NoApprovalRequired;
					switch (item.AuthorisationRequirement)
					{
						case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired:
							key = ApprovalLevel.NoApprovalRequired;
							break;
						case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly:
							key = ApprovalLevel.FirstApprovalRequiredOnly;
							break;
						case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly:
							key = ApprovalLevel.SecondApprovalRequiredOnly;
							break;
						case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly:
							key = ApprovalLevel.ThirdApprovalRequiredOnly;
							break;
					}

					List<ICreditControlledDocumentsCheckConfiguration> settings = null;
					if (!result.TryGetValue(key, out settings))
					{
						settings = new List<ICreditControlledDocumentsCheckConfiguration>();
					}

					settings.Add(item);

					result[key] = settings;
				}

				foreach (var key in result.Keys.ToArray())
				{
					result[key] = new List<ICreditControlledDocumentsCheckConfiguration>(result[key].OrderByDescending(x => x.Amount));
				}
			}

			return result;
		}

		InvoiceType GetInvoiceTypeEnum(ZString invoiceType)
		{
			var result = InvoiceType.ALL;

			if (invoiceType == CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code)
			{
				result = InvoiceType.ALL;
			}
			else if (invoiceType == CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code)
			{
				result = InvoiceType.DSB;
			}
			else if (invoiceType == CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB.Code)
			{
				result = InvoiceType.NDB;
			}

			return result;
		}

		Dictionary<InvoiceType, OutStanding> GetOrgCreditControlledDocumentsApprovalBalancesAndDueDays()
		{
			var result = new Dictionary<InvoiceType, OutStanding>();
			var query = new DynamicBusinessObjectCollection(Factory);
			var command = new StringBuilder("SELECT * FROM OrgCreditControlledDocumentsApprovalBalances(@OrgPK, @CompanyPK, @LocalDateTime)");
			var param = new ZSqlParameterCollection();
			param.Add("@OrgPK", OB_OH, OrgHeaderSchema.PK);
			param.Add("@CompanyPK", OB_GC, GlbCompanySchema.PK);
			param.Add("@LocalDateTime", ZDateTime.Now, CargoWise.Schema.Schema.GenericDateTimeColumn);

			query.Load(command.ToString(), param);

			result[InvoiceType.ALL] = new OutStanding(new ZDecimal(query[0]["SumAllAmount"]), new ZInt(query[0]["SumAllDueDays"]));
			result[InvoiceType.DSB] = new OutStanding(new ZDecimal(query[0]["SumDsbAmount"]), new ZInt(query[0]["SumDsbDueDays"]));
			result[InvoiceType.NDB] = new OutStanding(new ZDecimal(query[0]["SumNotDsbAmount"]), new ZInt(query[0]["SumNotDsbDueDays"]));
			return result;
		}

		bool TryGetOrgGlobalCreditControlledDocumentsApprovalBalancesAndDueDays(out Dictionary<InvoiceType, OutStanding> orgGlobalCreditControlledDocumentsApprovalBalancesAndDueDays)
		{
			var success = false;
			orgGlobalCreditControlledDocumentsApprovalBalancesAndDueDays = new Dictionary<InvoiceType, OutStanding>();

			var result = new DynamicBusinessObjectCollection(Factory);

			result.Load("EXEC OrgGlobalCreditControlledDocumentsApprovalBalances @OrgPK, @LocalDateTime, @GlobalCreditCurrencyCode", new ZSqlParameterCollection()
			{
				ZSqlParameter.New("@OrgPK", OB_OH, OrgHeaderSchema.PK),
				ZSqlParameter.New("@LocalDateTime", ZDateTime.Now, CargoWise.Schema.Schema.GenericDateTimeColumn),
				ZSqlParameter.New("@GlobalCreditCurrencyCode", Header.MiscServ.GlobalCreditCurrency.RX_Code, RefCurrencySchema.RX_Code)
			});

			if (result.Count == 1)
			{
				success = !new ZBool(result[0]["InvalidGlobalCreditCurrencyOrMissingExRate"]);
				orgGlobalCreditControlledDocumentsApprovalBalancesAndDueDays[InvoiceType.ALL] = new OutStanding(new ZDecimal(result[0]["SumAllAmount"]), new ZInt(result[0]["MaxAllDueDays"]));
				orgGlobalCreditControlledDocumentsApprovalBalancesAndDueDays[InvoiceType.DSB] = new OutStanding(new ZDecimal(result[0]["SumDsbAmount"]), new ZInt(result[0]["MaxDsbDueDays"]));
				orgGlobalCreditControlledDocumentsApprovalBalancesAndDueDays[InvoiceType.NDB] = new OutStanding(new ZDecimal(result[0]["SumNotDsbAmount"]), new ZInt(result[0]["MaxNotDsbDueDays"]));
			}

			return success;
		}

		class OutStanding
		{
			public ZDecimal Amount;
			public ZInt NumberOfDaysOverdue;

			public OutStanding(ZDecimal amount, ZInt numberOfDaysOverdue)
			{
				Amount = amount;
				NumberOfDaysOverdue = numberOfDaysOverdue;
			}
		}

		#endregion

		#region Credit On Hold

		public int GetCreditOnHoldMaxLevel(SecurityCore security)
		{
			return security.OrgCreditOnHoldThirdLevel.IsAllowed ? 3 :
									security.OrgCreditOnHoldSecondLevel.IsAllowed ? 2 :
									security.OrgCreditOnHoldFirstLevel.IsAllowed ? 1 : 0;
		}

		public int GetCreditOnHoldAuthorizationLevel()
		{
			int authorizationLevel = 0;
			IGlbStaff staffSetLocalCreditOnHold = null;
			IGlbStaff staffSetGlobalCreditOnHold = null;

			if (Header.MiscServ.OM_AROnCreditHold)
			{
				staffSetLocalCreditOnHold = CreditOnHoldChecker.GetLastUserSettingLocalCreditOnHold(Logs);
			}

			if (Header.MiscServ.OM_ARGlobalOnCreditHold)
			{
				staffSetGlobalCreditOnHold = CreditOnHoldChecker.GetLastUserSettingGlobalCreditHold(Header.MiscServ.Logs);
			}

			if (Header.MiscServ.IsMemberOfGlobalCreditGroup && Header.MiscServ.ARGlobalCreditGroup.MiscServ.OM_ARGlobalOnCreditHold)
			{
				staffSetGlobalCreditOnHold = CreditOnHoldChecker.GetLastUserSettingGlobalCreditHold(Header.MiscServ.ARGlobalCreditGroup.MiscServ.Logs);
			}

			if (staffSetLocalCreditOnHold != null)
			{
				authorizationLevel = GetCreditOnHoldMaxLevel(new SecurityCore(null, staffSetLocalCreditOnHold.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK));
			}

			if (staffSetGlobalCreditOnHold != null)
			{
				var globalAuthorizationLevel = GetCreditOnHoldMaxLevel(new SecurityCore(null, staffSetGlobalCreditOnHold.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK));

				authorizationLevel = Math.Max(authorizationLevel, globalAuthorizationLevel);
			}

			return authorizationLevel == 0 ? 3 : authorizationLevel;
		}

		#endregion

		#region OB_ARUseSettlementGroupCreditLimit

		public override ZBool OB_ARUseSettlementGroupCreditLimit
		{
			get
			{
				return base.OB_ARUseSettlementGroupCreditLimit;
			}
			set
			{
				if (value && Header != null && Header.PK != ARSettlementGroupPK && !ARSettlementGroupPK.IsEmpty)
				{
					OB_ARCreditLimit = 0m;
					OB_ARTemporaryCreditLimitIncrease = 0m;
				}
				base.OB_ARUseSettlementGroupCreditLimit = value;
			}
		}
		#endregion

		#region AccountFeeSettings
		public AccountFeeSettings AccountFeeSettings
		{
			get
			{
				if (fAccountFeeSettings == null)
				{
					fAccountFeeSettings = new AccountFeeSettings(new ZGuid(Env.CurrentCompany.PK), this);
					fAccountFeeSettings.PopulateFields();
				}
				RegisterEditableChildObject(fAccountFeeSettings);
				return fAccountFeeSettings;
			}
		}
		AccountFeeSettings fAccountFeeSettings;
		#endregion

		[ChildEditable(true)]
		public AccAROrgTaxConfigurationCollection AROrgTaxConfigurations
		{
			get
			{
				if (arOrgTaxConfigurations == null)
				{
					arOrgTaxConfigurations = new AccAROrgTaxConfigurationCollection(this);
					RegisterEditableChildObject(arOrgTaxConfigurations);
				}
				return arOrgTaxConfigurations;
			}
		}
		AccAROrgTaxConfigurationCollection arOrgTaxConfigurations;

		[ChildEditable(true)]
		public AccAPOrgTaxConfigurationCollection APOrgTaxConfigurations
		{
			get
			{
				if (apOrgTaxConfigurations == null)
				{
					apOrgTaxConfigurations = new AccAPOrgTaxConfigurationCollection(this);
					RegisterEditableChildObject(apOrgTaxConfigurations);
				}
				return apOrgTaxConfigurations;
			}
		}
		AccAPOrgTaxConfigurationCollection apOrgTaxConfigurations;

		#endregion

		#region AP / AR Information Security Access

		#region Cannot Modify AR or AP Flag

		public delegate void CannotModifyAROrAPFlagEventHandler(object sender, CannotModifyAROrAPFlagEventArgs e);

		public event CannotModifyAROrAPFlagEventHandler CannotModifyAROrAPFlag;

		void OnCannotModifyAROrAPFlag(ZString aROrAPCode)
		{
			if (CannotModifyAROrAPFlag != null)
			{
				CannotModifyAROrAPFlag(this, new CannotModifyAROrAPFlagEventArgs(aROrAPCode));
			}
		}

		public class CannotModifyAROrAPFlagEventArgs : EventArgs
		{
			public CannotModifyAROrAPFlagEventArgs(ZString aROrAPCode)
			{
				this.AROrAPCode = aROrAPCode;
			}

			public ZString AROrAPCode;
		}

		#endregion

		public event CannotModifyAROrAPFlagEventHandler SecurityAccessDenied;

		void OnSecurityAccessDenied(ZString aROrAPCode)
		{
			if (SecurityAccessDenied != null)
			{
				SecurityAccessDenied(this, new CannotModifyAROrAPFlagEventArgs(aROrAPCode));
			}
		}

		void RemoveRelatedInvalidZGuidValues(ZString orgTypeDescriptor)
		{
			foreach (ZPropertyInfo info in ZPropertyInfoHash)
			{
				if (info.Name.IndexOf(orgTypeDescriptor) != -1 && ValueIsInvalidZGuid(info))
				{
					this[info.Name] = ZGuid.Empty;
				}
			}
		}

		bool ValueIsInvalidZGuid(ZPropertyInfo info)
		{
			return this[info.Name].GetType() == typeof(ZGuid) && !((ZGuid)this[info.Name]).IsValid;
		}

		#endregion

		#region Tax Details

		internal void SetTaxApplicable()
		{
			SetARTaxApplicable();
			SetAPTaxApplicable();
		}

		void SetARTaxApplicable()
		{
			SetARTaxApplicable(GlbCompany.CurrentCompany.GC_IsGSTRegistered);
			OB_ARWHTApplicable = false;
		}

		void SetAPTaxApplicable()
		{
			if (Organisation != null && Organisation.UNLOCO != null &&
							Organisation.UNLOCO.Country != null &&
									 ((Organisation.UNLOCO.RL_RN_NKCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
							|| (Organisation.UNLOCO.Country.IsPartOfEuropeanUnion && GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion)))
			{
				SetAPTaxApplicable(GlbCompany.CurrentCompany.GC_IsGSTRegistered);
			}
			else if (!DefaultPayablesGSTApplicableIfNecessary())
			{
				SetAPTaxApplicable(GlbCompany.CurrentCompany.GC_IsGSTRegistered);
			}

			OB_APWHTApplicable = false;
		}

		public bool DefaultPayablesGSTApplicableIfNecessary()
		{
			var result = false;
			if (OB_IsDebtor && OB_IsCreditor && Header != null && Header.OH_IsForwarder && IsARTaxApplicable)
			{
				//This is a special case, we should not read the value from the registry, instead we should keep the existing logic
				SetAPTaxApplicableIgnoringRegistrySetting(GlbCompany.CurrentCompany.GC_IsGSTRegistered);
				result = true;
			}
			return result;
		}

		#endregion

		#region Deleting

		public override void Delete()
		{
			if (!this.HasContext(BusinessContext.NotDeletingOrgCompanyDataIndependentCollectionsOnHandlingUniqueIndexFailure))
			{
				DeleteIndependentCollections();
			}
			InvoiceTypes.RemoveAndDeleteAll();
			AccountDetailsCollection.RemoveAndDeleteAll();
			ARAccountDetailsCollection.RemoveAndDeleteAll();
			InvoiceRollupOrGroupsNoAutoCreate.RemoveAndDeleteAll();
			ARTerms.DeleteAll();
			OrgTaxConfigurationsCleanUp();
			base.Delete();
		}

		void OrgTaxConfigurationsCleanUp()
		{
			foreach (var aRConfiguration in AROrgTaxConfigurations)
			{
				aRConfiguration.TaxRates.DeleteAll();
			}
			AROrgTaxConfigurations.DeleteAll();

			foreach (var aPConfiguration in APOrgTaxConfigurations)
			{
				aPConfiguration.TaxRates.DeleteAll();
			}
			APOrgTaxConfigurations.DeleteAll();
		}

		void AccCFXConfigsCleanUp()
		{
			var ohLevelConfigs = AccCFXConfigurations.Where(c => c.Level == AccCFXConfigurationLevelEnum.Organisation).ToList();

			foreach (var cfg in ohLevelConfigs)
			{
				AccCFXConfigurations.RemoveAndDelete(cfg);
			}

			AccCFXConfigurations.RemoveAll();
		}

		void AccEInvoicingTemplateConfigsCleanUp()
		{
			var orgLevelConfigs = EInvoicingTemplateFileConfigurations.Where(c => c.Level == AccEInvoicingTemplateFileLevelEnum.Organisation).ToList();

			foreach (var cfg in orgLevelConfigs)
			{
				EInvoicingTemplateFileConfigurations.RemoveAndDelete(cfg);
			}

			EInvoicingTemplateFileConfigurations.RemoveAll();
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CompanySpecificOrg);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			SetInvoiceGroupingDefaults();

			GenerateARClientNumber();

			if (OB_ARCreditApprovedInfo.HasChanges)
			{
				if (OB_ARCreditApproved)
				{
					Logs.AddNew(Events.CreditApprovalGranted, string.Format("Credit Control and Settlement - {0}: {1} - {2}", LogReferenceKeys.CreditApprovalGrantedBy, GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_LoginName));
				}
				else
				{
					Logs.AddNew(Events.CreditControlsModified, string.Format("Credit Control and Settlement - {0}: {1}", LogReferenceKeys.CreditApproved, OB_ARCreditApproved));
				}
			}
			if (OB_ARCreditRatingInfo.HasChanges)
			{
				Logs.AddNew(Events.CreditControlsModified, string.Format("Credit Control and Settlement - {0}: {1}", LogReferenceKeys.CreditRating, OB_ARCreditRating));
			}
			if (OB_ARCreditLimitInfo.HasChanges)
			{
				Logs.AddNew(Events.CreditControlsModified, string.Format("Credit Control and Settlement - {0}: {1:c}", LogReferenceKeys.CreditLimit, OB_ARCreditLimit));
			}
			if (OB_AROnCreditHoldInfo.HasChanges)
			{
				var typeParameter = new KeyValuePair<string, string>(LogParameterKeys.Type, LogTypes.CreditOnHold);
				Logs.AddNew(Events.CreditControlsModified, string.Format("Credit Control and Settlement - {0}: {1}", LogReferenceKeys.CreditOnHold, OB_AROnCreditHold), typeParameter);
			}
			if (OB_ARAccountAndCreditReviewDueInfo.HasChanges)
			{
				Logs.AddNew(Events.CreditControlsModified, string.Format("Credit Control and Settlement - {0}: {1:d}", LogReferenceKeys.CreditReviewDue, OB_ARAccountAndCreditReviewDue));
			}
			if (OB_ARUseSettlementGroupCreditLimitInfo.HasChanges)
			{
				Logs.AddNew(Events.CreditControlsModified, string.Format("Credit Control and Settlement - {0}: {1}", LogReferenceKeys.UseSettlementGroupCreditLimit, OB_ARUseSettlementGroupCreditLimit));
			}
			if (OB_ARCreditAgreedPaymentMethodInfo.HasChanges)
			{
				Logs.AddNew(Events.CreditControlsModified, string.Format("Credit Control and Settlement - {0}: {1}", LogReferenceKeys.AgreedPaymentMethod, OB_ARCreditAgreedPaymentMethod));
			}
			if (OB_ARTemporaryCreditLimitIncreaseInfo.HasChanges || OB_ARTemporaryCreditLimitIncreaseExpiryInfo.HasChanges)
			{
				var typeParameter = new KeyValuePair<string, string>(LogParameterKeys.Type, LogTypes.TemporaryCreditLimitAdjustment);

				Logs.AddNew(Events.CreditControlsModified, string.Format("Credit Limit Adjustment of: {0} to: {1}, Expiry: {2:yyyy-MM-dd HH:mm} UTC",
						OB_ARTemporaryCreditLimitIncrease.ToString(LocalDecimals),
						ARTemporaryCreditLimit.ToString(LocalDecimals),
						OB_ARTemporaryCreditLimitIncreaseExpiry),
						typeParameter);
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (!IsInDatabase || OB_ARTemporaryCreditLimitIncreaseInfo.HasChanges)
			{
				CalculateAndSetTemporaryCreditLimitIncreaseExpiry();
			}

			if (DefaultOptions.HasChanges)
			{
				OB_IMProductValueDefaultOptions = DefaultOptions.GetDefaultOptionsString();
			}
		}

		#endregion

		#region Log Based Credit Control Dates

		public ZDateTime CreditControlEditedDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				StmALog[] logs = Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModifiedCode));
				if (logs.Any())
				{
					result = logs.Max(p => p.SL_EventTime);
				}
				return result;
			}
		}

		public ZDateTime CreditApprovedDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				StmALog[] logs = Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditApprovalGrantedCode));
				if (logs.Any())
				{
					result = logs.Max(p => p.SL_EventTime);
				}
				return result;
			}
		}

		#endregion

		#region GlobalCredit

		[DecimalPlaces(nameof(LocalDecimals))]
		[ReadOnly(true)]
		public ZDecimal CreditOutStandingBalance
		{
			get
			{
				Header?.FillGlobalCreditGroupChilds();
				return creditOutstandingBalance;
			}
			set
			{
				creditOutstandingBalance = value;
			}
		}
		ZDecimal creditOutstandingBalance;

		public ZBool OverCreditLimit => !IsUnlimitedCreditLimit && CreditOutStandingBalance > ARTemporaryCreditLimit;

		public ZBool OverARCreditLimit
		{
			get
			{
				return Header?.CreditChecker.DoesExceedCreditLimit() ?? false;
			}
		}

		bool IsUnlimitedCreditLimit => OB_ARCreditApproved && ARTemporaryCreditLimit == 0m;

		[ReadOnly(true)]
		[List("Lookups.Headers")]
		public ZGuid ARSettlementGroupPK
		{
			get
			{
				return Header == null || Header.IsUsingGlobalCreditGroupChildren ? arSettlementGroupPK : Header.ARSettlementGroupPK;
			}
			set
			{
				if (Header?.IsUsingGlobalCreditGroupChildren ?? false)
				{
					arSettlementGroupPK = value;
				}
				else
				{
					Header.ARSettlementGroupPK = value;
				}
			}
		}
		ZGuid arSettlementGroupPK;

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			string propertyName = property.Name;
			if (Header != null)
			{
				if (propertyName == OrgCompanyDataSchema.OB_GB_ControllingBranch.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyDetailsNameAndAddressSecurity;
				}
				else if (propertyName == OrgCompanyDataSchema.OB_IMUsedBondedWhs.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyWarehouseSecurity;
				}
				else if (propertyName == OrgCompanyDataSchema.OB_ARAutoUpdateRates.Name ||
										propertyName == OrgCompanyDataSchema.OB_RateSecurityGroup.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyDetailsRatingAndTariffsSecurity;
				}
				else if (propertyName == Schema.OB_ARImportAirCollectUplift ||
										propertyName == Schema.OB_ARImportAirCollectUpliftMinimum ||
										propertyName == Schema.OB_ARImportSeaCollectUplift ||
										propertyName == Schema.OB_ARImportSeaCollectUpliftMinimum)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesCurrencyUpliftSecurity;
				}
				else if (propertyName == Schema.OB_ARExportAirCollectUplift ||
										propertyName == Schema.OB_ARExportAirCollectUpliftMinimum ||
										propertyName == Schema.OB_ARExportSeaCollectUplift ||
										propertyName == Schema.OB_ARExportSeaCollectUpliftMinimum)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesCurrencyUpliftSecurity;
				}
				else if (propertyName == OrgCompanyDataSchema.OB_AREftCustomsPaymentMethod.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyConsigneeDetailsSecurity && !Header.SecurityProvider.HasModifyConsignorDetailsSecurity;
				}
				else if (propertyName.Contains("_CR"))
				{
					if (propertyName == OrgCompanyDataSchema.OB_CRIsShipsAgencyPrincipal.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyCarrierSecuritySea;
					}
					else
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyCarrierSecurity;
					}
				}
				else if (propertyName.Contains("_AP") || propertyName.Contains("_NKAP"))
				{
					if (propertyName == OrgCompanyDataSchema.OB_OG_APCreditorGroup.Name ||
							propertyName == OrgCompanyDataSchema.OB_APCategory.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesCreditorDetailsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_RX_NKAPDefltCurrency.Name ||
							propertyName == OrgCompanyDataSchema.OB_AB_APDefaultBankAccount.Name ||
							propertyName == OrgCompanyDataSchema.OB_AC_APDefaultChargeCode.Name ||
							propertyName == OrgCompanyDataSchema.OB_AC_APDefaultChargeCode.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesDefaultsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_APCreditLimit.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesCreditDetailsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_APPaymentTerms.Name ||
							propertyName == OrgCompanyDataSchema.OB_APPaymentTermDays.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesPaymentTermsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_APVATConfig.Name ||
							propertyName == OrgCompanyDataSchema.OB_APWHTApplicable.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesTaxDetailsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_APPayInvoiceAfterPostingDefault.Name ||
							propertyName == OrgCompanyDataSchema.OB_APCostsSelfBilled.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesOtherDetailsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_APQualityAssured.Name ||
							propertyName == OrgCompanyDataSchema.OB_APQualityAssuredCheckedDate.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesQualityAssuranceSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_APExternalCreditorCode.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesExternalCreditorSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_APTransactionCreationRestriction.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasOrgPayablesModifyConfigTransCreationRestrictionSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_APAirlineAccountNumber.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyCarrierSecurityAir;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_OCT_APTaxTemplate.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesTaxConfigurationTemplateSecurity;
					}
					else
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyPayablesSecurity;
					}
				}
				else if (propertyName.Contains("_AR") || propertyName.Contains("_NKAR") ||
										propertyName == OrgCompanyData.Schema.OverrideBankAccountFromDebtorGroup ||
										propertyName == OrgCompanyData.Schema.ARBankAccountToDisplay)
				{
					if (propertyName == OrgCompanyDataSchema.OB_OJ_ARDebtorGroup.Name ||
							propertyName == OrgCompanyData.Schema.OverrideBankAccountFromDebtorGroup ||
							propertyName == OrgCompanyData.Schema.ARBankAccountToDisplay ||
							propertyName == OrgCompanyDataSchema.OB_ARCategory.Name ||
							propertyName == OrgCompanyDataSchema.OB_RX_NKARDDefltCurrency.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesAccountDetailsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARCreditRating.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARCreditApproved.Name ||
							propertyName == OrgCompanyDataSchema.OB_AROnCreditHold.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARAccountAndCreditReviewDue.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARCreditAgreedPaymentMethod.Name)
					{
						shouldBeReadOnly = !Env.Security.OrgReceivablesModifyCreditControl.IsAllowed;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARCreditLimit.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARUseSettlementGroupCreditLimit.Name)
					{
						shouldBeReadOnly = CreditDetails_ReadOnly;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARVATConfig.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARWHTApplicable.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARDontShowTaxOnDocs.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARVATSplitPaymentApplicable.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARGoodsOwnership.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesTaxDetailsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARQualityAssured.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARQualityAssuredCheckedDate.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesQualityAssuranceSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARExternalDebtorCode.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesExternalDebtorSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARTreatDisbursementsAsStandardValue.Name)
					{
						shouldBeReadOnly = CreditDetails_ReadOnly;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARReceiptInvoiceAfterPostingDefault.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARCombinedStatementInvoice.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesInvoiceDetailsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARBuyersConsolInvoicingStyle.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesBuyersConsolInvoicingSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARIncludeInwardsWhsConsolidatedInvoice.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARIncludeOutwardsWhsConsolidatedInvoice.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARWarehouseRatingPeriod.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARWhsStorageCalcMethod.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyWarehouseSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARCreditCardType.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARCreditCardNum.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARCreditCardHolder.Name ||
							propertyName == OrgCompanyDataSchema.OB_ARCreditCardExpire.Name ||
							propertyName == "OB_ARCreditCardExpire_Month" ||
							propertyName == "OB_ARCreditCardExpire_Year" ||
							propertyName == OrgCompanyDataSchema.OB_ARCreditCardAdditionalInfo.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesCreditCardDetailsSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARTransactionCreationRestriction.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_ARApplicableSurcharges.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasOrgReceivablesModifySurchargeConfigurationSecurity;
					}
					else if (propertyName == OrgCompanyDataSchema.OB_OCT_ARTaxTemplate.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesTaxConfigurationTemplateSecurity;
					}
					else
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesSecurity;
					}
				}
				else if (propertyName == OrgCompanyDataSchema.OB_WhsClientFreeStorageDays.Name ||
						propertyName == OrgCompanyDataSchema.OB_WhsOverrideFreeStorage.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyReceivablesSecurity;
				}
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		internal bool CreditDetails_ReadOnly
		{
			get
			{
				bool canModifyCreditControl = Env.Security.OrgReceivablesModifyCreditControl.IsAllowed;
				bool canModifyCreditControlOrPaymentTerms = Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed || Env.Security.OrgReceivablesModifyCreditControl.IsAllowed;

				return (!OB_ARCreditApproved && !canModifyCreditControlOrPaymentTerms) ||
						 (OB_ARCreditApproved && !canModifyCreditControl);
			}
		}

		#endregion

		#region Credit Limit Temporary Increase

		#region ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime

		public ZDateTime ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime
		{
			get
			{
				return OB_ARTemporaryCreditLimitIncreaseExpiry.ToLocalBranchTime();
			}
		}

		public ZPropertyInfo ARTemporaryCreditLimitIncreaseExpiryLocalBranchTimeInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime, x => OB_ARTemporaryCreditLimitIncreaseExpiryInfo);
			}
		}

		#endregion

		#region TemporaryCreditLimitInEffect

		public bool TemporaryCreditLimitInEffect
		{
			get
			{
				return
						OB_ARTemporaryCreditLimitIncrease > 0M &&
						OB_ARTemporaryCreditLimitIncreaseExpiry > ZDateTime.UtcNow &&
						!ObjectFactory.Get<IAccounting>().UseWebServiceForCreditLimit;
			}
		}

		#endregion

		#region ARTemporaryCreditLimit

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ARTemporaryCreditLimit
		{
			get
			{
				return OB_ARCreditLimit + (TemporaryCreditLimitInEffect ? OB_ARTemporaryCreditLimitIncrease : ZDecimal.Zero);
			}
		}

		public ZPropertyInfo ARTemporaryCreditLimitInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.ARTemporaryCreditLimit);
			}
		}

		#endregion

		#region OB_ARCreditLimit

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal OB_ARCreditLimit
		{
			get
			{
				return base.OB_ARCreditLimit;
			}
			set
			{
				base.OB_ARCreditLimit = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOB_ARTemporaryCreditLimitIncrease();
					Header.MiscServ.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region OB_ARCreditApproved

		public override ZBool OB_ARCreditApproved
		{
			get
			{
				return base.OB_ARCreditApproved;
			}
			set
			{
				base.OB_ARCreditApproved = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOB_ARTemporaryCreditLimitIncrease();
				}
			}
		}

		#endregion

		#region OB_ARTemporaryCreditLimitIncrease

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal OB_ARTemporaryCreditLimitIncrease
		{
			get
			{
				return base.OB_ARTemporaryCreditLimitIncrease;
			}
			set
			{
				if (base.OB_ARTemporaryCreditLimitIncrease != value)
				{
					base.OB_ARTemporaryCreditLimitIncrease = value;
					CalculateAndSetTemporaryCreditLimitIncreaseExpiry();
					if (!IsValidationSuspended)
					{
						Header.MiscServ.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected bool OB_ARTemporaryCreditLimitIncrease_ReadOnly
		{
			get { return OB_ARUseSettlementGroupCreditLimit || !Env.Security.OrgReceivablesModifyCreditControl.IsAllowed; }
		}

		void CalculateAndSetTemporaryCreditLimitIncreaseExpiry()
		{
			var setting = TemporaryCreditLimitIncreaseHelper.Instance.GetSettingForProposedIncrease(OB_ARCreditLimit, OB_ARTemporaryCreditLimitIncrease);

			if (setting != null)
			{
				OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.Today.AddDays(setting.DaysToExpiry).AddMinutes(-1).ToUniversalBranchTime();
			}
			else
			{
				OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.Empty;
			}
		}

		#endregion

		#region Transaction Creation Restriction

		[List("Lookups.OB_APTransactionCreationRestrictionList")]
		public override ZString OB_APTransactionCreationRestriction
		{
			get
			{
				return base.OB_APTransactionCreationRestriction;
			}
		}

		[List("Lookups.OB_ARTransactionCreationRestrictionList")]
		public override ZString OB_ARTransactionCreationRestriction
		{
			get
			{
				return base.OB_ARTransactionCreationRestriction;
			}
		}

		#endregion

		#endregion

		#region Decimals

		public int LocalDecimals => Company.GetLocalDecimals();

		#endregion

		#region Applicable Invoice Types

		public Dictionary<ZString, OrgInvoiceType> GetApplicableInvoiceTypes(ZString transportMode, ZString serviceDirection, ZString serviceLevel, bool useCache, params ZString[] jobTypes)
		{
			Dictionary<ZString, OrgInvoiceType> result = null;
			string invoiceTypeCacheKey = GetInvoiceTypesCacheKey(transportMode, serviceDirection, serviceLevel, jobTypes);

			if (!useCache || (!string.IsNullOrEmpty(invoiceTypeCacheKey) &&
				(!InvoiceTypeCache.TryGetValue(invoiceTypeCacheKey, out result) || result.Values.Any(x => x.IsDeleted))))
			{
				if (serviceDirection == OrgConstants.ServiceDirection.Code.Unknown)
				{
					serviceDirection = "";
				}
				result = LoadApplicableInvoiceTypes(transportMode, serviceDirection, serviceLevel, jobTypes);
				if (useCache)
				{
					InvoiceTypeCache.Remove(invoiceTypeCacheKey);
					InvoiceTypeCache[invoiceTypeCacheKey] = result;
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		Dictionary<ZString, OrgInvoiceType> LoadApplicableInvoiceTypes(ZString transportMode, ZString serviceDirection, ZString serviceLevel, params ZString[] jobTypes)
		{
			Dictionary<ZString, OrgInvoiceType> result = new Dictionary<ZString, OrgInvoiceType>();

			if (jobTypes != null && InvoiceTypes != null)
			{
				foreach (var jobType in jobTypes)
				{
					foreach (OrgInvoiceType invoiceType in InvoiceTypes)
					{
						if ((string.Equals(invoiceType.PI_Module, jobType, StringComparison.OrdinalIgnoreCase) || string.Equals(invoiceType.PI_Module, "ALL", StringComparison.OrdinalIgnoreCase))
							&& (string.Equals(invoiceType.PI_ServiceDirection, serviceDirection, StringComparison.OrdinalIgnoreCase) || string.Equals(invoiceType.PI_ServiceDirection, "ALL", StringComparison.OrdinalIgnoreCase))
							&& (string.Equals(invoiceType.PI_TransportMode, transportMode, StringComparison.OrdinalIgnoreCase) || string.Equals(invoiceType.PI_TransportMode, "ALL", StringComparison.OrdinalIgnoreCase))
							&& (string.Equals(invoiceType.PI_RS_NKServiceLevel, serviceLevel, StringComparison.OrdinalIgnoreCase) || invoiceType.PI_RS_NKServiceLevel.IsEmpty))
						{
							if (!result.ContainsKey(jobType))
							{
								result.Add(jobType, invoiceType);
							}
							else
							{
								//Keep the most specific one
								if ((string.Equals(result[jobType].PI_Module, "ALL", StringComparison.OrdinalIgnoreCase) && string.Equals(invoiceType.PI_Module, jobType, StringComparison.OrdinalIgnoreCase))
									|| ((string.Equals(result[jobType].PI_ServiceDirection, "ALL", StringComparison.OrdinalIgnoreCase) || result[jobType].PI_ServiceDirection == "") && string.Equals(invoiceType.PI_ServiceDirection, serviceDirection, StringComparison.OrdinalIgnoreCase))
									|| ((string.Equals(result[jobType].PI_TransportMode, "ALL", StringComparison.OrdinalIgnoreCase) || result[jobType].PI_TransportMode == "") && string.Equals(invoiceType.PI_TransportMode, transportMode, StringComparison.OrdinalIgnoreCase))
									|| (result[jobType].PI_RS_NKServiceLevel.IsEmpty && string.Equals(invoiceType.PI_RS_NKServiceLevel, serviceLevel, StringComparison.OrdinalIgnoreCase)))
								{
									result[jobType] = invoiceType;
								}
							}
						}
					}
				}
			}
			return result;
		}

		string GetInvoiceTypesCacheKey(ZString transportMode, ZString serviceDirection, ZString serviceLevel, params ZString[] jobTypes)
		{
			return string.Concat(this.PK, transportMode, serviceDirection, serviceLevel, string.Join("_", jobTypes.OrderBy(x => x).ToArray()));
		}

		Dictionary<string, Dictionary<ZString, OrgInvoiceType>> InvoiceTypeCache
		{
			get { return Factory.GetCachedValue(GetMainDictionaryKey(), () => new Dictionary<string, Dictionary<ZString, OrgInvoiceType>>()); }
		}

		ZString GetMainDictionaryKey()
		{
			return "MasterFileOrgCompanyData" + GlbCompany.CurrentCompany.PK.ToStringKey() + "InvoiceTypeDictionary";
		}

#if DEBUG

		public void ClearInvoiceTypeCache_ForTestOnly()
		{
			Factory.ClearCachedValue<Dictionary<string, Dictionary<ZString, OrgInvoiceType>>>(GetMainDictionaryKey());
		}

		public void IncreaseOutstandingBalance(decimal amount)
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = Header.PK;
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_OutstandingAmount = amount;
			invoice.AH_InvoiceAmount = amount;
			invoice.AH_OSTotal = amount;
			invoice.AH_IsCancelled = false;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			invoice.AH_IsCancelled = false;

			Factory.Save();
		}

#endif

		#endregion

		#region Default E-Payment Reason

		public ZString GetDefaultEPaymentReason(ZString paymentProvider, ZString paymentCurrency)
		{
			var paymentMethod = GetPaymentMethodForProvider(paymentProvider);
			var matchingAccountDetail = AccountDetailsCollection.GetAccountDetails(paymentMethod, paymentCurrency);
			return matchingAccountDetail?.A1_EPaymentReasonCode ?? ZString.Empty;
		}

		ZString GetPaymentMethodForProvider(ZString paymentProvider)
		{
			var paymentMethod = ZString.Empty;
			if (!paymentProvider.IsEmpty)
			{
				switch (paymentProvider)
				{
					case EPaymentProviderCodes.Codes.OFX:
						paymentMethod = EPaymentMethods.EPaymentViaOFX;
						break;
					default:
						throw new NotImplementedException($"E-Payment Provider [{paymentProvider}] is not mapped to its Payment Method. Please add the mapping here.");
				}
			}
			return paymentMethod;
		}

		#endregion

		#region IWorkflowTriggerEventSource

		public IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				if (Header != null)
				{
					list.Add(Header);
				}
				return list;
			}
		}

		public IGlbCompany JobHeaderCompany => Company;

		#endregion

		public void GenerateARClientNumber()
		{
			if (OB_IsDebtor && OB_ARClientNumber.IsEmpty)
			{
				OB_ARClientNumber = Env.NumberFountains.OrgARClientNumber(OB_GC.ToGuid()).GetNextFormatted(Factory);
			}
		}

		void DeleteIndependentCollections()
		{
			AccCFXConfigsCleanUp();
			AccEInvoicingTemplateConfigsCleanUp();
			RateFeeChargeLevels.DeleteAll();
			RateCommodityDefaultingRules.DeleteAll();
		}
	}
}
