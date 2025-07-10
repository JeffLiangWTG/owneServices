using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AccGLHeader.Schema.AG_AccountNum), DescriptionProperty(AccGLHeader.Schema.AG_Description)]
	public class AccGLHeader : AutoAccGLHeader, Integration.IAccGLHeader, IDocManagerSupport, IGLAccount, IEDocsParsingSupport
	{
		public AccGLHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		const int ArbitrarilyChosen4MbMaxLength = 4 * 1024 * 1024;

		public static class Constants
		{
			public static class SectionTypes
			{
				public static class Codes
				{
					public const string TradingStatement = "TS";
					public const string Overheads = "OV";
					public const string ProfitAndLossAppropriation = "AP";
					public const string OwnersEquity = "OE";
					public const string Assets = "AS";
					public const string Liabilities = "LI";
				}

				public static class Descriptions
				{
					public static MultilingualString TradingStatement
					{
						get { return ResString.GetMultilingualString("8d9b56ff-0719-4cf7-886b-3ff01efb96e3", "(1) Trading Statement"); }
					}
					public static MultilingualString Overheads
					{
						get { return ResString.GetMultilingualString("c748b906-f843-4eb1-9c8d-e4c9b7cebf61", "(2) Overheads"); }
					}
					public static MultilingualString ProfitAndLossAppropriation
					{
						get { return ResString.GetMultilingualString("468824ae-9793-4fc8-816f-7135e74a6520", "(3) Profit & Loss Appropriation"); }
					}
					public static MultilingualString OwnersEquity
					{
						get { return ResString.GetMultilingualString("5653e5e3-7500-41a3-a1bf-1feefa50393e", "(4) Owners Equity"); }
					}
					public static MultilingualString Assets
					{
						get { return ResString.GetMultilingualString("d85fe861-9fe0-4275-9c54-082b425d06aa", "(5) Assets"); }
					}
					public static MultilingualString Liabilities
					{
						get { return ResString.GetMultilingualString("96955eed-5bff-4934-9e5a-01b1761c0d18", "(6) Liabilities"); }
					}
				}
			}

			public static IEnumerable<string> AccountTypeListApplicableForSubAccount
			{
				get
				{
					if (accountTypeListApplicableForSubAccount == null)
					{
						accountTypeListApplicableForSubAccount = new List<string>();
						accountTypeListApplicableForSubAccount.Add(Core.Constants.AccountType.ProfitAndLossAccount);
						accountTypeListApplicableForSubAccount.Add(Core.Constants.AccountType.BalanceSheetAccount);
					}
					return accountTypeListApplicableForSubAccount;
				}
			}

			[ThreadStatic]
			static List<string> accountTypeListApplicableForSubAccount;
		}

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		public ZString CurrentFormat
		{
			get { return CurrentFormatFromSingleHeader(this); }
		}

		public ZPropertyInfo CurrentFormatInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentFormat)); }
		}

		public bool IsGLAccountUsedInCompanyLevelRegistry
		{
			get
			{
				return ObjectFactory.Get<IAccounting>().IsGLAccountUsedInCompanyLevelRegistry(PK, CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Select(x => x.ACF_GC_Company).Where(x => x.IsValid).ToArray());
			}
		}

		public bool IsGLAccountUsedInSystemLevelRegistry
		{
			get
			{
				return ObjectFactory.Get<IAccounting>().IsGLAccountUsedInSystemLevelRegistry(PK);
			}
		}

		public bool IsGLAccountUsedForGlobalChargeCode
		{
			get
			{
				AccGlobalChargeCodeCollection cachedValues = Factory.GetCachedValue("GLAccountsSetUpInGlobalChargeCode", delegate
				{
					AccGlobalChargeCodeCollection collection = new AccGlobalChargeCodeCollection(Factory);
					return collection;
				});

				return cachedValues.Any(x => x.AC_AG_AccrualAccount == PK || x.AC_AG_CostAccount == PK || x.AC_AG_RevenueAccount == PK || x.AC_AG_WIPAccount == PK);
			}
		}

		AccGLAccountDescriptor LocalAccountDescriptor
		{
			get
			{
				if (!hasLoadedLocalDescriptor)
				{
					localAccountDescriptor = AccGLAccountDescriptor.GetLocalAccountDescriptor(Factory, PK);
					hasLoadedLocalDescriptor = true;
				}

				return localAccountDescriptor;
			}
		}
		AccGLAccountDescriptor localAccountDescriptor;

		bool hasLoadedLocalDescriptor;

		public ZString LocalAccountNumber
		{
			get
			{
				return LocalAccountDescriptor != null ? LocalAccountDescriptor.AJ_LocalAccountNumber : ZString.Empty;
			}
		}

		public ZString LocalAccountDescription
		{
			get
			{
				return LocalAccountDescriptor != null ? LocalAccountDescriptor.AJ_AccountDescription : ZString.Empty;
			}
		}

		public ZString AlternateAccounts
		{
			get
			{
				var result = string.Empty;

				var gLAccountSelectionAndEntry = AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (gLAccountSelectionAndEntry != Guid.Empty)
				{
					var query = new ZQuery();
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, gLAccountSelectionAndEntry);
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, PK);

					var alternateGLAccounts = Factory.Load<AccAlternateGLAccountAttribute>(query).Select(x => x.AlternateGLAccount).DistinctBy(x => x.PK);
					result = string.Join(", ", alternateGLAccounts.Take(3).Select(x => x.AGA_AccountNum + " - " + x.AGA_Description));
					if (alternateGLAccounts.Count() > 3)
					{
						result += "...";
					}
				}

				return result;
			}
		}

		#region Properties

		public override ZBool AG_IsGlobal
		{
			get
			{
				return base.AG_IsGlobal;
			}
			set
			{
				if (value)
				{
					CompanyFilters.RemoveAndDeleteAll();
				}
				base.AG_IsGlobal = value;
			}
		}

		public override ZBool AG_ControlAccount
		{
			get
			{
				return base.AG_ControlAccount;
			}
			set
			{
				base.AG_ControlAccount = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAG_IsGlobal();
				}
			}
		}

		[MaxLength(ArbitrarilyChosen4MbMaxLength)]
		public override ZString AG_Notes
		{
			get { return base.AG_Notes; }
			set { base.AG_Notes = value; }
		}

		[List("AG_AG_PercentNumList")]
		public override ZGuid AG_AG_PercentNum
		{
			get
			{
				return base.AG_AG_PercentNum;
			}
			set
			{
				base.AG_AG_PercentNum = value;
			}
		}

		[List("AG_AG_ConsolidationNumList")]
		public override ZGuid AG_AG_ConsolidationNum
		{
			get
			{
				return base.AG_AG_ConsolidationNum;
			}
			set
			{
				base.AG_AG_ConsolidationNum = value;
			}
		}

		[List("AG_AG_AlternateNumList")]
		public override ZGuid AG_AG_AlternateNum
		{
			get
			{
				return base.AG_AG_AlternateNum;
			}
			set
			{
				base.AG_AG_AlternateNum = value;
			}
		}

		[List("AG_AG_HeaderDependsOnTotalList")]
		public override ZGuid AG_AG_HeaderDependsOnTotal
		{
			get
			{
				return base.AG_AG_HeaderDependsOnTotal;
			}
			set
			{
				base.AG_AG_HeaderDependsOnTotal = value;
			}
		}

		[List("AG_CashFlowTypeList")]
		public override ZString AG_CashFlowType
		{
			get
			{
				return base.AG_CashFlowType;
			}
			set
			{
				base.AG_CashFlowType = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "_ReadOnly is used via reflection")]
		bool AG_CashFlowType_ReadOnly
		{
			get
			{
				return !IsAccountTypeSupportCashFlow;
			}
		}

		bool IsAccountTypeSupportCashFlow
		{
			get
			{
				return AG_AccountType == Core.Constants.AccountType.BalanceSheetAccount || AG_AccountType == Core.Constants.AccountType.ProfitAndLossAccount;
			}
		}

		[List("AG_DebitCreditList")]
		public override ZString AG_DebitCredit
		{
			get
			{
				return base.AG_DebitCredit;
			}
			set
			{
				base.AG_DebitCredit = value;
			}
		}

		[List("AG_AccountTypeList")]
		public override ZString AG_AccountType
		{
			get { return base.AG_AccountType; }
			set
			{
				base.AG_AccountType = value;
				if (!IsAccountTypeSupportCashFlow)
				{
					AG_CashFlowType = ZString.Empty;
				}

				ReadOnlyGetter.RefreshProperties();
				SetReadonlyDependentValues();
			}
		}

		public override ZString AG_AccountNum
		{
			get { return base.AG_AccountNum; }
			set
			{
				base.AG_AccountNum = value;
				fAG_AG_PercentNumList = null;
			}
		}

		[TranslatableDataField(Schema.TableName, Schema.AG_Description, DataXmlFilePaths.RefAccounting, Type = typeof(AccGLHeader), Asmid = ResString.AssemblyId)]
		public override ZString AG_Description
		{
			get { return base.AG_Description; }
			set { base.AG_Description = value; }
		}

		public MultilingualString AG_DescriptionMultilingual
		{
			get { return GetMultilingual(AG_DescriptionInfo); }
		}

		[List("SectionTypeList")]
		public override ZString AG_Column
		{
			get
			{
				return base.AG_Column;
			}
			set
			{
				base.AG_Column = value;
			}
		}

		public ZString AG_Calc_SubAccountTypes
		{
			get
			{
				var calcSubAccountTypes = new ZStringBuilder();
				foreach (CodeDescriptionPair subAccountTypeItem in AG_SubAccountTypeList)
				{
					var subAccountType = SubAccountTypes.Cast<AccGLHeaderSubAccount>().FirstOrDefault(x => SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(x.ASA_SubClass) == subAccountTypeItem.Code);
					if (subAccountType != null)
					{
						calcSubAccountTypes.Append(Invariant($"{SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(subAccountType.ASA_SubClass)}, "));
					}
				}
				return calcSubAccountTypes.ToString().Trim().Trim(',');
			}
		}

		[List("AG_StatisticalUnitsList")]
		[MaxLength(3)]
		public override ZString AG_StatisticalUnits
		{
			get
			{
				return base.AG_StatisticalUnits;
			}
			set
			{
				base.AG_StatisticalUnits = value;
			}
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("c66e580e-6e91-4c1b-b3de-4fa1a6f6dbbe", "General Ledger Account");
				if (!IsDeleted && !AG_AccountNum.IsEmpty)
				{
					result += " (" + AG_AccountNum + ")";
				}

				return result;
			}
		}

		#region Report Section

		[MaxLength(AccGLHeader.Schema.AG_AccountNumMaxLength + 3)]
		public ZString AG_Calc_AccountNumberWithPrefix
		{
			get { return SectionPrefix + "." + AG_AccountNum; }
		}

		public ZPropertyInfo AG_Calc_AccountNumberWithPrefixInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AG_Calc_AccountNumberWithPrefix));
			}
		}

		public ZInt SectionPrefix
		{
			get { return SectionPrefixes.ContainsKey(AG_Column) ? SectionPrefixes[AG_Column] : ZInt.Zero; }
		}

		public ZPropertyInfo SectionPrefixInfo
		{
			get { return GetZPropertyInfo(nameof(SectionPrefix), "Section Prefix"); }
		}

		public ZString SectionDescription
		{
			get { return SectionTypeList.GetDescriptionFromCode(AG_Column); }
		}

		public ZPropertyInfo SectionDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SectionDescription), "50"); }
		}

		Dictionary<ZString, ZInt> SectionPrefixes
		{
			get
			{
				if (fSectionPrefixes == null)
				{
					fSectionPrefixes = new Dictionary<ZString, ZInt>();
					fSectionPrefixes.Add(Constants.SectionTypes.Codes.TradingStatement, 1);
					fSectionPrefixes.Add(Constants.SectionTypes.Codes.Overheads, 2);
					fSectionPrefixes.Add(Constants.SectionTypes.Codes.ProfitAndLossAppropriation, 3);
					fSectionPrefixes.Add(Constants.SectionTypes.Codes.OwnersEquity, 4);
					fSectionPrefixes.Add(Constants.SectionTypes.Codes.Assets, 5);
					fSectionPrefixes.Add(Constants.SectionTypes.Codes.Liabilities, 6);
				}

				return fSectionPrefixes;
			}
		}

		Dictionary<ZString, ZInt> fSectionPrefixes;

		#endregion

		#region Business Object Overrides

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				if (fNoteTypes == null)
				{
					fNoteTypes = new NoteTypeCollection();

					//Client-Visible
					fNoteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
				}
				return fNoteTypes;
			}
		}

		protected NoteTypeCollection fNoteTypes;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (IsInDatabase)
			{
				if ((ZString)AG_CashFlowTypeInfo.OriginalValue != AG_CashFlowType)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format("Cash Flow Type updated. Previous value: '[{0}]', New value: '({1})'", AG_CashFlowTypeInfo.OriginalValue, AG_CashFlowType));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}

				if (AG_IsGlobal && (ZBool)AG_IsGlobalInfo.OriginalValue != AG_IsGlobal)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format("Global GL Account: Edited as global"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else if (!AG_IsGlobal && CompanyFilters.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format("Company Filters edited: {0}", CompanyFiltersAsString));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}

				if (SubAccountTypes.HasChanges)
				{
					if (SubAccountTypes != null && SubAccountTypes.Count != 0)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, "Sub Account Types updated to " + String.Join(", ", SubAccountTypes.OfType<AccGLHeaderSubAccount>().Select(x => string.Format((NoResString)"[Type={0}, Mandatory={1}]", x.ASA_SubClassDisplayName, x.ASA_IsSubClassValidationRuleMandatory ? "Y" : "N"))));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
					else
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, "Sub Account Types updated to empty");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}

				if (AccountNumInfo.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, "Account Number Updated. Previous Value: '" + AccountNumInfo.OriginalValue + "', New Value: '" + AccountNum + "'");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}

			SubAccountTypes.Resort();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AccGLHeaderFetchStrategy(this);

		#endregion

		#region List Properties

		protected CodeDescriptionPairList fAG_CashFlowTypeList;

		public CodeDescriptionPairList AG_CashFlowTypeList
		{
			get
			{
				if (fAG_CashFlowTypeList == null)
				{
					fAG_CashFlowTypeList = new CodeDescriptionPairList();
					foreach (CashFlowActivityConfiguration cashFlowActivity in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
					{
						fAG_CashFlowTypeList.AddPair(cashFlowActivity.Code, cashFlowActivity.Description);
					}
				}

				return fAG_CashFlowTypeList;
			}
		}

		#region CompanyFilters

		[ChildEditable()]
		public AccGLHeaderCompanyFilterCollection CompanyFilters
		{
			get
			{
				if (fCompanyFilters == null)
				{
					fCompanyFilters = new AccGLHeaderCompanyFilterCollection(this);
					fCompanyFilters.Load();
					RegisterEditableChildObject(fCompanyFilters);
				}
				return fCompanyFilters;
			}
		}
		AccGLHeaderCompanyFilterCollection fCompanyFilters;

		public ZString CompanyFiltersAsString
		{
			get
			{
				var filtersAsString = CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Select(x => x.Company.GC_Code).ToArray();
				return filtersAsString.Any() ? ZString.Join(",", filtersAsString) : (ZString)"ALL";
			}
		}

		#endregion

		#region SubAccountTypes
		[ChildEditable()]
		public AccGLHeaderSubAccountCollection SubAccountTypes
		{
			get
			{
				if (fSubAccountTypes == null)
				{
					fSubAccountTypes = new AccGLHeaderSubAccountCollection(this);
					fSubAccountTypes.Load();
					RegisterEditableChildObject(fSubAccountTypes);
				}
				return fSubAccountTypes;
			}
		}
		AccGLHeaderSubAccountCollection fSubAccountTypes;

		#endregion

		protected CodeDescriptionPairList fAG_DebitCredit_List;

		public CodeDescriptionPairList AG_DebitCreditList
		{
			get
			{
				if (fAG_DebitCredit_List == null)
				{
					fAG_DebitCredit_List = new CodeDescriptionPairList();
					fAG_DebitCredit_List.AddPair("DR", Res.GetString("9a298472-e848-4189-b15f-9106cc979f74", "Debit"));
					fAG_DebitCredit_List.AddPair("CR", Res.GetString("8be10fee-1db5-4109-81c4-3534c5891852", "Credit"));
				}

				return fAG_DebitCredit_List;
			}
		}

		protected CodeDescriptionPairList fAG_AccountType_List;

		public CodeDescriptionPairList AG_AccountTypeList
		{
			get
			{
				if (fAG_AccountType_List == null)
				{
					fAG_AccountType_List = new CodeDescriptionPairList();
					fAG_AccountType_List.AddPair(Core.Constants.AccountType.BalanceSheetAccount, Res.GetString("5a48df50-43ac-489c-8b39-28268d07836a", "Balance Sheet"));
					fAG_AccountType_List.AddPair(Core.Constants.AccountType.ProfitAndLossAccount, Res.GetString("a5142fd7-7dc0-4d78-8f5c-ba77243d7777", "Profit & Loss"));
					fAG_AccountType_List.AddPair(Core.Constants.AccountType.Total, Res.GetString("a1627ec8-bd0c-427d-9452-f94877c8c5a7", "Total Account"));
					fAG_AccountType_List.AddPair(Core.Constants.AccountType.Header, Res.GetString("d5c38361-fd53-49fd-9eea-025e6613bab1", "Header"));
					fAG_AccountType_List.AddPair(Core.Constants.AccountType.Consolidation, Res.GetString("0334fdb4-c630-4eb1-af00-02b8757b1f1c", "Consolidated Account"));
					fAG_AccountType_List.AddPair(Core.Constants.AccountType.Alternate, Res.GetString("fef48114-f945-495f-bc55-1ea290fc92b5", "Alternate Account"));
					fAG_AccountType_List.AddPair(Core.Constants.AccountType.Note, Res.GetString("287DC678-9E66-4DBE-B23D-6DBD597E2938", "Note"));
				}

				return fAG_AccountType_List;
			}
		}

		public CodeDescriptionPairList AG_SubAccountTypeList
		{
			get
			{
				if (ag_SubAccountTypeList == null)
				{
					ag_SubAccountTypeList = new AccountingMasterFilesConstants.SubAccountTypeList();
				}

				return ag_SubAccountTypeList;
			}
		}
		protected CodeDescriptionPairList ag_SubAccountTypeList;

		protected AccGLHeaderCollection fAG_AG_PercentNumList;

		public AccGLHeaderCollection AG_AG_PercentNumList
		{
			get
			{
				if (fAG_AG_PercentNumList == null)
				{
					fAG_AG_PercentNumList = new AccGLHeaderCollection(Factory);
				}

				return fAG_AG_PercentNumList;
			}
		}

		protected AccGLHeaderCollection fAG_AG_ConsolidationNumList;

		public AccGLHeaderCollection AG_AG_ConsolidationNumList
		{
			get
			{
				if (fAG_AG_ConsolidationNumList == null)
				{
					ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AccountType, "CLN");
					fAG_AG_ConsolidationNumList = new AccGLHeaderCollection(Factory, query);
				}

				return fAG_AG_ConsolidationNumList;
			}
		}

		protected AccGLHeaderCollection fAG_AG_AlternateNumList;

		public AccGLHeaderCollection AG_AG_AlternateNumList
		{
			get
			{
				if (fAG_AG_AlternateNumList == null)
				{
					ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AccountType, "ALT");
					fAG_AG_AlternateNumList = new AccGLHeaderCollection(Factory, query);
				}

				return fAG_AG_AlternateNumList;
			}
		}

		protected AccGLHeaderCollection fAG_AG_HeaderDependsOnTotalList;

		public AccGLHeaderCollection AG_AG_HeaderDependsOnTotalList
		{
			get
			{
				if (fAG_AG_HeaderDependsOnTotalList == null)
				{
					ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AccountType, "TTL");
					fAG_AG_HeaderDependsOnTotalList = new AccGLHeaderCollection(Factory, query);
				}

				return fAG_AG_HeaderDependsOnTotalList;
			}
		}

		public CodeDescriptionPairList SectionTypeList
		{
			get
			{
				if (sectionTypeList == null)
				{
					sectionTypeList = BuildSectionTypeList();
				}
				return sectionTypeList;
			}
		}

		CodeDescriptionPairList sectionTypeList;

		public static CodeDescriptionPairList BuildSectionTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.SectionTypes.Codes.TradingStatement, Constants.SectionTypes.Descriptions.TradingStatement);
			result.AddPair(Constants.SectionTypes.Codes.Overheads, Constants.SectionTypes.Descriptions.Overheads);
			result.AddPair(Constants.SectionTypes.Codes.ProfitAndLossAppropriation, Constants.SectionTypes.Descriptions.ProfitAndLossAppropriation);
			result.AddPair(Constants.SectionTypes.Codes.OwnersEquity, Constants.SectionTypes.Descriptions.OwnersEquity);
			result.AddPair(Constants.SectionTypes.Codes.Assets, Constants.SectionTypes.Descriptions.Assets);
			result.AddPair(Constants.SectionTypes.Codes.Liabilities, Constants.SectionTypes.Descriptions.Liabilities);

			return result;
		}

		protected ReadOnlyCodeDescriptionPairList fAG_StatisticalUnits_List;

		public ReadOnlyCodeDescriptionPairList AG_StatisticalUnitsList
		{
			get
			{
				if (fAG_StatisticalUnits_List == null)
				{
					fAG_StatisticalUnits_List = ObjectFactory.Get<IAccounting>().Registry?.NoteGLAccountsStatisticalUnitsofMeasurement(GlbCompany.CurrentCompany.PK.ToGuid()) as ReadOnlyCodeDescriptionPairList;
				}

				return fAG_StatisticalUnits_List;
			}
		}

		#region AlternateChartFormats

		[ChildEditable(true)]
		public AccAlternateGLAccountDissectionCollection AlternateGLAccountDissections
		{
			get
			{
				if (fAlternateGLAccountDissections == null)
				{
					fAlternateGLAccountDissections = new AccAlternateGLAccountDissectionCollection(this);
					fAlternateGLAccountDissections.Load();
					RegisterEditableChildObject(fAlternateGLAccountDissections);
				}
				return fAlternateGLAccountDissections;
			}
		}
		AccAlternateGLAccountDissectionCollection fAlternateGLAccountDissections;

		#endregion

		#endregion

		public override void Delete()
		{
			SubAccountTypes.RemoveAndDeleteAll();
			CompanyFilters.RemoveAndDeleteAll();
			AlternateGLAccountDissections.RemoveAndDeleteAll();
			base.Delete();
		}

		public override bool CanDelete => base.CanDelete && !IsGlHeaderUsedInControlAccountRegistryItems(PK);

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var relatedRegistryNames = GetRelatedRegistryNames(PK);
				MultilingualString result;
				if (relatedRegistryNames == null || relatedRegistryNames.Count == 0)
				{
					result = base.ReasonForNotAbleToDelete;
				}
				else if (relatedRegistryNames.Contains("ElectronicProcessingChargeDisbursementClearingAccount") || relatedRegistryNames.Contains("ElectronicProcessingChargePayableClearingAccount"))
				{
					result = ResString.GetMultilingualString("E2AEE49A-E0D9-4CA5-8216-B50756B1B324", "The GL Account {0} is a system defined GL Account used in Electronic Processing Fee management and cannot be deleted.", AG_AccountNum);
				}
				else
				{
					result = ResString.GetMultilingualString("2ee1cd12-b894-42f8-9a73-84fe95785d9a", "This GL account {0} is used at least once in the Registry. Please choose different registry values before attempting to delete again.", AG_AccountNum);
				}

				return result;
			}
		}

		List<string> GetRelatedRegistryNames(ZGuid accountPK)
		{
			List<string> result = new List<string>();
			if (accountPK != Guid.Empty)
			{
				string query = @"SELECT SD_Name " +
					"FROM dbo.StmData " +
					"WHERE SD_GuidValue = @GuidValue " +
					"AND SD_Name IN ('GL_AR_CONTROL_ACCOUNT', 'GL_AP_CONTROL_ACCOUNT', " +
					"    'GL_AR_SUSPENSE_CONTROL_ACCOUNT', 'GL_AP_SUSPENSE_CONTROL_ACCOUNT', 'GL_EXCHANGE_GAIN_ACCOUNT', 'GL_EXCHANGE_LOSS_ACCOUNT', " +
					"    'GL_AR_DISCOUNT_ACCOUNT', 'GL_AP_DISCOUNT_ACCOUNT', 'GL_OVERPAYMENTS_ACCOUNT', 'GL_ACCRUED_REVENUE_ACCOUNT', 'GL_ACCRUED_COST_ACCOUNT', " +
					"    'GL_GST_INPUT_ACCOUNT', 'GL_GST_OUTPUT_ACCOUNT', 'GL_WHT_INPUT_ACCOUNT', 'GL_WHT_OUTPUT_ACCOUNT', 'GL_BANKCURRENCY_ADJUSTMENT_ACCOUNT', 'GL_FINANCE_CHG_ACCOUNT', " +
					"    'ElectronicProcessingChargeDisbursementClearingAccount', 'ElectronicProcessingChargePayableClearingAccount')";
				using (DbCommand command = Db.Connection.Command(query)) // Required Control Account Check, No access to AccountingConfigurationRegistry
				{
					command.AddParameter("@GuidValue", SqlDbType.UniqueIdentifier, accountPK.ToGuid());
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add((string)reader[0]);
						}
					}
				}
			}
			return result;
		}
			
		bool IsGlHeaderUsedInControlAccountRegistryItems(ZGuid accountPK)
		{
			var relatedRegistryNames = GetRelatedRegistryNames(accountPK);
			return relatedRegistryNames != null && relatedRegistryNames.Count != 0;
		}

		#region Settings

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SetReadonlyDependentValues();

			AG_TotalLevel = 0;
			AG_PrintSequence = 0;
		}

		#endregion

		#region Implementation

		protected void SetReadonlyDependentValues()
		{
			if (AG_ControlAccountInfo.ReadOnly)
			{
				AG_ControlAccount = false;
			}

			if (AG_TotalLevelInfo.ReadOnly)
			{
				AG_TotalLevel = 0;
			}
		}

		protected GLAccountCommonPropertyReadOnlyGetter ReadOnlyGetter
		{
			get
			{
				if (readOnlyGetter == null)
				{
					readOnlyGetter = new AccGLCommonPropertyReadOnlyGetter(this);
				}
				return readOnlyGetter;
			}
		}

		protected GLAccountCommonPropertyReadOnlyGetter readOnlyGetter;

		public static ZString CurrentGLAccountFormat
		{
			get { return ObjectFactory.Get<IAccounting>().GLAccountFormat; }
		}

		ZString CurrentFormatFromSingleHeader(AccGLHeader header)
		{
			return ObjectFactory.Get<IAccounting>().GetGLAccountFormat(header.AG_AccountNum);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.GLAccounts);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion
		#region IGLAccount Members

		public ZPropertyInfo ControlAccountInfo
		{
			get { return AG_ControlAccountInfo; }
		}

		protected bool AG_ControlAccount_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AG_AccountType, IGLAccountSchema.ControlAccountInfo); }
		}

		public ZPropertyInfo PercentNumInfo
		{
			get { return AG_AG_PercentNumInfo; }
		}

		protected bool AG_AG_PercentNum_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AG_AccountType, IGLAccountSchema.PercentNumInfo); }
		}

		public ZPropertyInfo ConsolidationNumInfo
		{
			get { return AG_AG_ConsolidationNumInfo; }
		}

		protected bool AG_AG_ConsolidationNum_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AG_AccountType, IGLAccountSchema.ConsolidationNumInfo); }
		}

		public ZPropertyInfo AlternateNumInfo
		{
			get { return AG_AG_AlternateNumInfo; }
		}

		protected bool AG_AG_AlternateNum_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AG_AccountType, IGLAccountSchema.AlternateNumInfo); }
		}

		public ZPropertyInfo TotalLevelInfo
		{
			get { return AG_TotalLevelInfo; }
		}

		protected bool AG_TotalLevel_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AG_AccountType, IGLAccountSchema.TotalLevelInfo); }
		}

		public ZPropertyInfo HeaderDependsOnTotalInfo
		{
			get { return AG_AG_HeaderDependsOnTotalInfo; }
		}

		protected bool AG_AG_HeaderDependsOnTotal_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AG_AccountType, IGLAccountSchema.HeaderDependsOnTotalInfo); }
		}

		public ZPropertyInfo CarriedForwardInfo
		{
			get { return null; }
		}

		public ZPropertyInfo AccountNumInfo
		{
			get { return AG_AccountNumInfo; }
		}

		public ZPropertyInfo StatisticalUnitsInfo => AG_StatisticalUnitsInfo;

		protected bool AG_StatisticalUnits_ReadOnly => ReadOnlyGetter.ShouldBeReadOnly(AG_AccountType, IGLAccountSchema.StatisticalUnitsInfo);

		public ZString AccountNum
		{
			get { return AG_AccountNum; }
		}

		public ZString AccountNumWithPrefix
		{
			get { return AG_Calc_AccountNumberWithPrefix; }
		}

		public ZString GLAccountFormat
		{
			get { return AccGLHeader.CurrentGLAccountFormat; }
		}

		public ZGuid ConsolidationAccount
		{
			get { return AG_AG_ConsolidationNum; }
		}

		public ZGuid TotalReferenceAccount
		{
			get { return AG_AG_HeaderDependsOnTotal; }
		}

		#endregion

		public bool GetIsSubClassValidationRuleMandatory(ZString subAccountType)
		{
			return SubAccountTypes.Cast<AccGLHeaderSubAccount>().FirstOrDefault(x => x.ASA_SubClass == subAccountType)?.ASA_IsSubClassValidationRuleMandatory ?? false;
		}

		public bool IsAllowedToHaveAttributes()
		{
			if (IsBankAccount() || AccountingMasterFilesUtils.IsNotAllowedForDissectionControlAccount(PK.ToGuid()) || ObjectFactory.Get<IAccounting>().IsNotAllowedForDissectionAttributes(PK))
			{
				return false;
			}

			return true;
		}

		public bool IsBankAccount()
		{
			return Factory.Exists(typeof(AccBankAccount), new ZQuery(AccBankAccountSchema.AB_AG, PK));
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AG_AccountNum = ZGuid.NewZGuid().ToString().Substring(0, AG_AccountNumInfo.MaxLength);
			AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			AG_DebitCredit = Core.Constants.DebitCredit.Debit;
		}
#endif

	}
}
