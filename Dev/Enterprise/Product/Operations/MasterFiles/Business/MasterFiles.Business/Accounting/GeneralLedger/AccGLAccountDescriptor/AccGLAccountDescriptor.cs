using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AutoAccGLAccountDescriptor.Schema.AJ_LocalAccountNumber), DescriptionProperty(AutoAccGLAccountDescriptor.Schema.AJ_AccountDescription)]
	public class AccGLAccountDescriptor : AutoAccGLAccountDescriptor, Integration.IAccGLAccountDescriptor, IDocManagerSupport, IGLAccount
	{
		public const string ReportTypeCOA = "COA";

		public new class Schema : AutoAccGLAccountDescriptor.Schema
		{
			public const string ParentGLHeaderPK = "ParentGLHeaderPK";
		}

		public AccGLAccountDescriptor(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (AJ_DebitCredit.IsEmpty)
			{
				AJ_DebitCredit = Constants.DebitCredit.Debit;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		public bool IsValidCharForChineseGLAccount(char accountChar)
		{
			return char.IsDigit(accountChar) || accountChar == '.' || char.IsControl(accountChar);
		}

		#region Report Setup

		[ChildEditable]
		public ReportConfigurationPivotCollection ReportConfigurationPivotCollection
		{
			get
			{
				if (fReportConfigPivotCollection == null)
				{
					fReportConfigPivotCollection = new ReportConfigurationPivotCollection(Factory, this);
					RegisterEditableChildObject(fReportConfigPivotCollection);
				}
				return fReportConfigPivotCollection;
			}
		}

		public ZBool ReportSetupVisible
		{
			get
			{
				return IsInDatabase &&
						!(ParentGLHeaderPK.IsEmpty || !ParentGLHeaderPK.IsValid) &&
						((AJ_Language == SharedConstants.Languages.ChineseSimplified && AJ_RN_NKCountryOfCompliance == Constants.CountryCodes.China) ||
						AccountingMasterFilesRegistry.Instance.EnableReportSetup.Value) &&
						AJ_ReportCategory != AccountTypeComboBoxConstants.Note;
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		ReportConfigurationPivotCollection fReportConfigPivotCollection;

		#endregion

		#region YJ_AG

		[ChildEditable]
		[ZArchitecture.Business.ActionFieldFollow(true)]
		public AccGLDescriptorPivotCollection AccGLDescriptorPivotCOACollection
		{
			get
			{
				if (fAccGLDescriptorPivotCOACollection == null)
				{
					fAccGLDescriptorPivotCOACollection = new AccGLDescriptorPivotCollection(Factory, this);
					RegisterEditableChildObject(fAccGLDescriptorPivotCOACollection);
				}
				return fAccGLDescriptorPivotCOACollection;
			}
		}

		AccGLDescriptorPivotCollection fAccGLDescriptorPivotCOACollection;

		public AccGLDescriptorPivot AccGLDescriptorPivotCOA
		{
			get
			{
				return IsCOADescriptor ? AccGLDescriptorPivotCOACollection.GetAccGLDescriptorPivot() : null;
			}
		}

		protected ZBool IsCOADescriptor
		{
			get { return AJ_ReportType == ReportTypeCOA; }
		}

		[List("ParentGLHeaderPK_List")]
		public ZGuid ParentGLHeaderPK
		{
			get
			{
				return AccGLDescriptorPivotCOA != null ? AccGLDescriptorPivotCOA.YJ_AG : Guid.Empty;
			}
			set
			{
				if (IsCOADescriptor && ParentGLHeaderPK != value)
				{
					if (ReportConfigurationPivotCollection.Count > 0 && value.IsEmpty)
					{
						ReportConfigurationPivotCollection.DeleteAll();
					}

					AccGLDescriptorPivotCOACollection.MapToGLHeader(value);
					AccGLDescriptorPivotCOACollection.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateParentGLHeaderPK();
					}

					ParentGLHeaderPKInfo.RefreshBinding();

					if (!ParentGLHeaderPKInfo.HasErrors() && AccGLDescriptorPivotCOA != null)
					{
						ReportConfigurationPivotCollection.UpdateGLHeader(value);

						if (AJ_ReportCategory == AccountTypeComboBoxConstants.ProfitAndLossAccount ||
							AJ_ReportCategory == AccountTypeComboBoxConstants.BalanceSheetAccount)
						{
							if (AJ_DebitCredit.IsEmpty && ParentGLHeader != null)
							{
								AJ_DebitCredit = ParentGLHeader.AG_DebitCredit;
								AJ_DebitCreditInfo.RefreshBinding();
							}
						}
					}
				}
			}
		}

		public ZPropertyInfo ParentGLHeaderPKInfo
		{
			get { return GetZPropertyInfo(Schema.ParentGLHeaderPK); }
		}

		public AccGLHeader ParentGLHeader
		{
			get { return (AccGLHeader)Factory.Load(typeof(AccGLHeader), ParentGLHeaderPK); }
		}

		#endregion

		public override void Delete()
		{
			if (IsCOADescriptor && ReportConfigurationPivotCollection.Count > 0)
			{
				ReportConfigurationPivotCollection.DeleteAll();
			}
			if (AccGLDescriptorPivotCOA != null)
			{
				AccGLDescriptorPivotCOACollection.DeleteAll();
			}
			base.Delete();
		}

		protected void UpdateCommonPropertiesOnRelatedDescriptors()
		{
			if (IsCOADescriptor && AccGLDescriptorPivotCOA != null && IsInDatabase && ReportConfigurationPivotCollection.Count > 0)
			{
				foreach (AccGLAccountDescriptor descriptor in
					ReportConfigurationPivotCollection.Select(pivot => pivot.GLAccountDescriptor).Where(descriptor => descriptor != null))
				{
					if (descriptor.AJ_LocalAccountNumber != AJ_LocalAccountNumber)
					{
						descriptor.AJ_LocalAccountNumber = AJ_LocalAccountNumber;
					}

					if (descriptor.AJ_AccountDescription != AJ_AccountDescription)
					{
						descriptor.AJ_AccountDescription = AJ_AccountDescription;
					}

					if (descriptor.AJ_AJ_AlternativeNum != AJ_AJ_AlternativeNum)
					{
						descriptor.AJ_AJ_AlternativeNum = AJ_AJ_AlternativeNum;
					}

					if (descriptor.AJ_AJ_CarriedForwardAccount != AJ_AJ_CarriedForwardAccount)
					{
						descriptor.AJ_AJ_CarriedForwardAccount = AJ_AJ_CarriedForwardAccount;
					}

					if (descriptor.AJ_AJ_ConsolidationNum != AJ_AJ_ConsolidationNum)
					{
						descriptor.AJ_AJ_ConsolidationNum = AJ_AJ_ConsolidationNum;
					}

					if (descriptor.AJ_AJ_HeaderDependsOnTotal != AJ_AJ_HeaderDependsOnTotal)
					{
						descriptor.AJ_AJ_HeaderDependsOnTotal = AJ_AJ_HeaderDependsOnTotal;
					}

					if (descriptor.AJ_AJ_PercentNum != AJ_AJ_PercentNum)
					{
						descriptor.AJ_AJ_PercentNum = AJ_AJ_PercentNum;
					}

					if (descriptor.AJ_DebitCredit != AJ_DebitCredit)
					{
						descriptor.AJ_DebitCredit = AJ_DebitCredit;
					}

					if (descriptor.AJ_PrintSequence != AJ_PrintSequence)
					{
						descriptor.AJ_PrintSequence = AJ_PrintSequence;
					}

					if (descriptor.AJ_RN_NKCountryOfCompliance != AJ_RN_NKCountryOfCompliance)
					{
						descriptor.AJ_RN_NKCountryOfCompliance = AJ_RN_NKCountryOfCompliance;
					}

					if (descriptor.AJ_TotalLevel != AJ_TotalLevel)
					{
						descriptor.AJ_TotalLevel = AJ_TotalLevel;
					}
				}
			}
		}

		#region Get Local Account Descriptor

		public static AccGLAccountDescriptor GetLocalAccountDescriptor(BusinessObjectFactory factory, ZGuid pk)
		{
			return GetLocalAccountDescriptor(factory, pk, GlbStaff.CurrentUser.GS_WorkingLanguage);
		}

		public static AccGLAccountDescriptor GetLocalAccountDescriptor(BusinessObjectFactory factory, ZGuid pk, ZString language)
		{
			AccGLAccountDescriptor result = null;
			var pivotQuery = new ZQuery(AccGLDescriptorPivotSchema.YJ_AG, pk);
			var pivotPKs = factory.Load<AccGLDescriptorPivot>(pivotQuery).Select(x => x.YJ_AJ);
			if (pivotPKs.Any())
			{
				var query = new ZQuery(AccGLAccountDescriptorSchema.PK, pivotPKs);
				query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, ReportTypeCOA);
				query.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				query.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, language);

				var descriptor = factory.LoadTop1<AccGLAccountDescriptor>(query);
				if (descriptor != null)
				{
					result = descriptor;
				}
			}

			return result;
		}

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AJ_ReportType = ReportTypeCOA;
			ReadOnlyGetter.RefreshProperties();
		}

		#endregion

		#region List Properties

		#region Language List

		public CodeDescriptionPairList AJ_Language_List
		{
			get { return fAJ_Language_List ?? (fAJ_Language_List = new CodeDescriptionPairList(OLookUpEditType.GLLanguage)); }
		}

		CodeDescriptionPairList fAJ_Language_List;

		#endregion

		#region Debit Credit List

		public CodeDescriptionPairList AJ_DebitCredit_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.DebitCredit); }
		}

		#endregion

		#region AJ_GLAccountType_List

		public virtual CodeDescriptionPairList AJ_GLAccountType_List
		{
			get
			{
				return fAJ_GLAccountType_List ??
						 (fAJ_GLAccountType_List = new CodeDescriptionPairList(OLookUpEditType.GLAccountDescriptorType));
			}
		}

		protected CodeDescriptionPairList fAJ_GLAccountType_List;

		#endregion

		#region ParentGLHeaderPK_List

		public AccGLHeaderCollection ParentGLHeaderPK_List
		{
			get { return fParentGLHeaderPK_List ?? (fParentGLHeaderPK_List = new AccGLHeaderCollection(Factory, new ZQuery(AGFilter))); }
		}

		protected AccGLHeaderCollection fParentGLHeaderPK_List;

		protected ZQuery AGFilter
		{
			get
			{
				if (fAGFilter == null)
				{
					SetParentAccountFilterCore();
				}
				return fAGFilter;
			}
		}

		protected ZQuery fAGFilter;

		#endregion

		#region AJ_CarriedFwdAccountList

		public AccGLAccountDescriptorCollection AJ_CarriedFwdAccountList
		{
			get
			{
				return fAJ_CarriedFwdAccountList ??
						 (fAJ_CarriedFwdAccountList = GetAccountListByType(AccountTypeComboBoxConstants.CarriedForwardAccount));
			}
		}

		protected AccGLAccountDescriptorCollection fAJ_CarriedFwdAccountList;

		#endregion

		#region AJ_ConsolidatedAccountList

		public AccGLAccountDescriptorCollection AJ_ConsolidatedAccountList
		{
			get
			{
				return fAJ_ConsolidatedAccountList ??
						 (fAJ_ConsolidatedAccountList = GetAccountListByType(AccountTypeComboBoxConstants.Consolidation));
			}
		}

		protected AccGLAccountDescriptorCollection fAJ_ConsolidatedAccountList;

		#endregion

		#region AJ_AlternateAccountList

		public AccGLAccountDescriptorCollection AJ_AlternateAccountList
		{
			get
			{
				return fAJ_AlternateAccountList ??
						 (fAJ_AlternateAccountList = GetAccountListByType(AccountTypeComboBoxConstants.Alternate));
			}
		}

		protected AccGLAccountDescriptorCollection fAJ_AlternateAccountList;

		#endregion

		#region AJ_Percent List

		public AccGLAccountDescriptorCollection AJ_PercentOfList
		{
			get
			{
				if (fAJ_PercentOfList == null)
				{
					ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.GreaterThan, AccountNum);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, AJ_Language);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, AJ_RN_NKCountryOfCompliance);
					fAJ_PercentOfList = new AccGLAccountDescriptorCollection(Factory, new ZQuery(filter));
				}
				return fAJ_PercentOfList;
			}
		}

		protected AccGLAccountDescriptorCollection fAJ_PercentOfList;

		#endregion

		#region AJ_Total Reference List

		public AccGLAccountDescriptorCollection AJ_TotalReferenceList
		{
			get
			{
				if (fAJ_TotalReferenceList == null)
				{
					ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, AccountTypeComboBoxConstants.Total);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, ReportTypeCOA);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, AJ_Language);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, AJ_RN_NKCountryOfCompliance);
					filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.GreaterThan, AccountNum);
					fAJ_TotalReferenceList = new AccGLAccountDescriptorCollection(Factory, new ZQuery(filter));
				}
				return fAJ_TotalReferenceList;
			}
		}

		protected AccGLAccountDescriptorCollection fAJ_TotalReferenceList;

		#endregion

		#endregion

		#region Overriden Properties

		#region AJ_AJ_HeaderDependsOnTotal
		[List("AJ_TotalReferenceList")]
		public override ZGuid AJ_AJ_HeaderDependsOnTotal
		{
			get
			{
				return base.AJ_AJ_HeaderDependsOnTotal;
			}
			set
			{
				base.AJ_AJ_HeaderDependsOnTotal = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}
		#endregion

		#region AJ_AJ_PercentNum
		[List("AJ_PercentOfList")]
		public override ZGuid AJ_AJ_PercentNum
		{
			get
			{
				return base.AJ_AJ_PercentNum;
			}
			set
			{
				base.AJ_AJ_PercentNum = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}
		#endregion

		#region AJ_AJ_AlternativeNum
		[List("AJ_AlternateAccountList")]
		public override ZGuid AJ_AJ_AlternativeNum
		{
			get
			{
				return base.AJ_AJ_AlternativeNum;
			}
			set
			{
				base.AJ_AJ_AlternativeNum = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}

		#endregion

		#region AJ_AJ_ConsolidationNum
		[List("AJ_ConsolidatedAccountList")]
		public override ZGuid AJ_AJ_ConsolidationNum
		{
			get
			{
				return base.AJ_AJ_ConsolidationNum;
			}
			set
			{
				base.AJ_AJ_ConsolidationNum = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}
		#endregion

		#region AJ_AJ_CarriedForwardAccount
		[List("AJ_CarriedFwdAccountList")]
		public override ZGuid AJ_AJ_CarriedForwardAccount
		{
			get
			{
				return base.AJ_AJ_CarriedForwardAccount;
			}
			set
			{
				base.AJ_AJ_CarriedForwardAccount = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}
		#endregion

		#region AJ_DebitCredit
		[List("AJ_DebitCredit_List")]
		public override ZString AJ_DebitCredit
		{
			get
			{
				return base.AJ_DebitCredit;
			}
			set
			{
				base.AJ_DebitCredit = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}
		#endregion

		public override ZString AJ_LocalAccountNumber
		{
			get
			{
				return base.AJ_LocalAccountNumber;
			}
			set
			{
				if (ReportSetupVisible && AccGLDescriptorPivotCOA != null && ReportConfigurationPivotCollection != null)
				{
					UpdateCommonPropertiesOnRelatedDescriptors();
				}

				base.AJ_LocalAccountNumber = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}

		public override ZString AJ_AccountDescription
		{
			get
			{
				return base.AJ_AccountDescription;
			}
			set
			{
				base.AJ_AccountDescription = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}

		public override ZShort AJ_PrintSequence
		{
			get
			{
				return base.AJ_PrintSequence;
			}
			set
			{
				base.AJ_PrintSequence = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}

		[RelatedBusinessObject("CountryOfCompliance")]
		[List("Lookups.CountryOfCompliances")]
		public override ZString AJ_RN_NKCountryOfCompliance
		{
			get
			{
				return base.AJ_RN_NKCountryOfCompliance;
			}
			set
			{
				base.AJ_RN_NKCountryOfCompliance = value;
				ResetBoundAccountList();
				ClearLinkingAccount();
			}
		}

		public override ZShort AJ_TotalLevel
		{
			get
			{
				return base.AJ_TotalLevel;
			}
			set
			{
				base.AJ_TotalLevel = value;
				UpdateCommonPropertiesOnRelatedDescriptors();
			}
		}

		public override ZString AJ_ReportType
		{
			get { return base.AJ_ReportType; }
			set
			{
				base.AJ_ReportType = value;
				ParentGLHeaderPK = Guid.Empty;
				ParentGLHeaderPKInfo.RefreshBinding();
			}
		}

		#region AJ_ReportCategory

		[List("AJ_GLAccountType_List")]
		public override ZString AJ_ReportCategory
		{
			get { return base.AJ_ReportCategory; }
			set
			{
				base.AJ_ReportCategory = value;

				ParentGLHeaderPK = Guid.Empty;

				SetParentAccountFilter();
				ReadOnlyGetter.RefreshProperties();
				ParentGLHeaderPKInfo.RefreshBinding();
			}
		}

		protected void SetParentAccountFilter()
		{
			fParentGLHeaderPK_List = null;
			SetParentAccountFilterCore();
		}

		void SetParentAccountFilterCore()
		{
			switch (AJ_ReportCategory)
			{
				case AccountTypeComboBoxConstants.BalanceSheetAccount:
					fAGFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.BalanceSheetAccount);
					break;
				case AccountTypeComboBoxConstants.ProfitAndLossAccount:
					fAGFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.ProfitAndLossAccount);
					break;
				case AccountTypeComboBoxConstants.Note:
					fAGFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.Note);
					break;
				default:
					fAGFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.BalanceSheetAccount);
					fAGFilter.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, AccountTypeComboBoxConstants.ProfitAndLossAccount);
					break;
			}
		}

		#endregion

		#region AJ_Language

		[List("AJ_Language_List")]
		public override ZString AJ_Language
		{
			get { return base.AJ_Language; }
			set
			{
				base.AJ_Language = value;

				ClearLocalAccountNumberIfLanguageIsChineseAndThereAreInvalidCharacters();
				ResetBoundAccountList();
				ClearLinkingAccount();
			}
		}

		void ClearLocalAccountNumberIfLanguageIsChineseAndThereAreInvalidCharacters()
		{
			foreach (char accountChar in AJ_LocalAccountNumber)
			{
				if ((AJ_Language == Constants.Languages.ChineseSimplified || AJ_Language == Constants.Languages.ChineseTraditional) &&
					!IsValidCharForChineseGLAccount(accountChar))
				{
					AJ_LocalAccountNumber = "";
					break;
				}
			}
		}

		protected void ClearLinkingAccount()
		{
			ParentGLHeaderPK = Guid.Empty;
			ParentGLHeaderPKInfo.RefreshBinding();
			AJ_AJ_AlternativeNumInfo.ClearValue();
			AJ_AJ_CarriedForwardAccountInfo.ClearValue();
			AJ_AJ_HeaderDependsOnTotalInfo.ClearValue();
			AJ_AJ_ConsolidationNumInfo.ClearValue();
			AJ_AJ_PercentNumInfo.ClearValue();
		}

		protected void ResetBoundAccountList()
		{
			ParentGLHeaderPK = Guid.Empty;
			ParentGLHeaderPKInfo.RefreshBinding();
			fAJ_CarriedFwdAccountList = null;
			fAJ_AlternateAccountList = null;
			fAJ_TotalReferenceList = null;
			fAJ_ConsolidatedAccountList = null;
			fAJ_PercentOfList = null;
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("D8102DFD-8704-4D0C-9CE2-C6B020AD173F", "General Ledger Multi-Language Mapping - {0}", CalculateShortcutName());

		#endregion

		#region Implementation

		protected AccGLAccountDescriptorCollection GetAccountListByType(ZString accountType)
		{
			ZQuery query = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, accountType);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, ReportTypeCOA);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, AJ_Language);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, AJ_RN_NKCountryOfCompliance);
			query.AddToFilter(AccGLAccountDescriptorSchema.PK, SQLComparisonOperator.NotEqual, PK);
			AccGLAccountDescriptorCollection listToReturn = new AccGLAccountDescriptorCollection(Factory, query);
			return listToReturn;
		}

		protected virtual bool ParentGLHeaderPK_ReadOnly
		{
			get
			{
				switch (AJ_ReportCategory)
				{
					case AccountTypeComboBoxConstants.Header:
					case AccountTypeComboBoxConstants.Total:
					case AccountTypeComboBoxConstants.CarriedForwardAccount:
					case AccountTypeComboBoxConstants.Consolidation:
					case AccountTypeComboBoxConstants.Alternate:
						return true;
					default:
						return false;
				}
			}
		}

		protected void ClearSelectedFields()
		{
			switch (AJ_ReportCategory)
			{
				case AccountTypeComboBoxConstants.BalanceSheetAccount:
					AJ_TotalLevel = (ZShort)0;
					AJ_AJ_CarriedForwardAccount = ZGuid.Empty;
					break;

				case AccountTypeComboBoxConstants.ProfitAndLossAccount:
					AJ_TotalLevel = (ZShort)0;
					AJ_AJ_CarriedForwardAccount = ZGuid.Empty;
					break;

				case AccountTypeComboBoxConstants.Total:
					ParentGLHeaderPK = ZGuid.Empty;
					break;

				case AccountTypeComboBoxConstants.Header:
					ParentGLHeaderPK = ZGuid.Empty;
					AJ_TotalLevel = (ZShort)0;
					AJ_AJ_CarriedForwardAccount = ZGuid.Empty;
					break;

				case AccountTypeComboBoxConstants.CarriedForwardAccount:
					ParentGLHeaderPK = ZGuid.Empty;
					AJ_TotalLevel = (ZShort)0;
					AJ_AJ_CarriedForwardAccount = ZGuid.Empty;
					break;
			}
		}

		#endregion

		#region IGLAccount Members

		public ZPropertyInfo ControlAccountInfo
		{
			get { return null; }
		}

		public ZPropertyInfo PercentNumInfo
		{
			get { return AJ_AJ_PercentNumInfo; }
		}

		protected virtual bool AJ_AJ_PercentNum_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AJ_ReportCategory, IGLAccountSchema.PercentNumInfo); }
		}

		public ZPropertyInfo ConsolidationNumInfo
		{
			get { return AJ_AJ_ConsolidationNumInfo; }
		}

		protected virtual bool AJ_AJ_ConsolidationNum_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AJ_ReportCategory, IGLAccountSchema.ConsolidationNumInfo); }
		}

		public ZPropertyInfo AlternateNumInfo
		{
			get { return AJ_AJ_AlternativeNumInfo; }
		}

		protected virtual bool AJ_AJ_AlternativeNum_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AJ_ReportCategory, IGLAccountSchema.AlternateNumInfo); }
		}

		public ZPropertyInfo TotalLevelInfo
		{
			get { return AJ_TotalLevelInfo; }
		}

		protected virtual bool AJ_TotalLevel_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AJ_ReportCategory, IGLAccountSchema.TotalLevelInfo); }
		}

		public ZPropertyInfo HeaderDependsOnTotalInfo
		{
			get { return AJ_AJ_HeaderDependsOnTotalInfo; }
		}

		protected virtual bool AJ_AJ_HeaderDependsOnTotal_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AJ_ReportCategory, IGLAccountSchema.HeaderDependsOnTotalInfo); }
		}

		public ZPropertyInfo CarriedForwardInfo
		{
			get { return AJ_AJ_CarriedForwardAccountInfo; }
		}

		protected virtual bool AJ_AJ_CarriedForwardAccount_ReadOnly
		{
			get { return ReadOnlyGetter.ShouldBeReadOnly(AJ_ReportCategory, IGLAccountSchema.CarriedForwardInfo); }
		}

		public ZPropertyInfo AccountNumInfo
		{
			get { return AJ_LocalAccountNumberInfo; }
		}

		public ZPropertyInfo StatisticalUnitsInfo => null;

		public ZString AccountNum
		{
			get { return AJ_LocalAccountNumber; }
		}

		public ZString AccountNumWithPrefix
		{
			get { return AccountNum; }
		}

		public ZString GLAccountFormat
		{
			get { return "XXXX.XXX"; }
		}

		public ZGuid ConsolidationAccount
		{
			get { return AJ_AJ_ConsolidationNum; }
		}

		public ZGuid TotalReferenceAccount
		{
			get { return AJ_AJ_HeaderDependsOnTotal; }
		}

		protected GLAccountCommonPropertyReadOnlyGetter ReadOnlyGetter
		{
			get { return new GLAccountCommonPropertyReadOnlyGetter(this); }
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
	}
}
