using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoAccComplianceSequence.Schema.XD_Description)]
	[SystemDefinedValues]
	public partial class AccComplianceSequence : AutoAccComplianceSequence, IDocManagerSupport, ITemplateCopyable, IDocumentSupportable, IWorkflowProvider, IEDocsParsingSupport
	{
		public AccComplianceSequence(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public abstract new class Schema : AutoAccComplianceSequence.Schema
		{
			public const string XD_Calc_NextNumberString = "XD_Calc_NextNumberString";
			public const string XD_Calc_StartNumberString = "XD_Calc_StartNumberString";
			public const string XD_Calc_EndNumberString = "XD_Calc_EndNumberString";
			public const string XD_Calc_DocumentTitle = "XD_Calc_DocumentTitle";
			public const string XD_LockBy = "XD_LockBy";
			public const string XD_Calc_SequenceClassDescription = "XD_Calc_SequenceClassDescription";
		}

		#region Taiwan Related

		ZBool IsInTaiwanCompany => Company != null && Company.GC_RN_NKCountryCode == CountryCodes.Taiwan;

		public ZByte LengthOfMaxNumberDigitsForTW => 8;

		#endregion

		#region Portugal Related

		ZBool IsInPortugalCompany => Company != null && Company.GC_RN_NKCountryCode == CountryCodes.Portugal;

		#endregion

		#region Property Override

		[List("Lookups.XD_SequenceClass_List")]
		public override ZString XD_SequenceClass
		{
			get
			{
				return base.XD_SequenceClass;
			}
			set
			{
				base.XD_SequenceClass = value;
			}
		}

		public ZString XD_Calc_SequenceClassDescription => Lookups.XD_SequenceClass_List.GetDescriptionFromCode(XD_SequenceClass);

		public ZPropertyInfo XD_Calc_SequenceClassDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.XD_Calc_SequenceClassDescription); }
		}

		public static ZString FindParentSubTypeInRegistry(ZString subType)
		{
			var complianceSubTypeDependencySetup = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.Value;
			return complianceSubTypeDependencySetup == null ? ZString.Empty : complianceSubTypeDependencySetup.FindParentSubTypeInCollection(GlbCompany.CurrentCompany.Country.Code, subType);
		}

		[List("Lookups.Printers")]
		public override ZGuid XD_SQ_DocumentPrintQueue
		{
			get
			{
				return base.XD_SQ_DocumentPrintQueue;
			}
			set
			{
				base.XD_SQ_DocumentPrintQueue = value;
			}
		}

		[List("Lookups.ComplianceInvoiceDocumentMenus")]
		public override ZGuid XD_SU_MenuItem
		{
			get
			{
				return base.XD_SU_MenuItem;
			}
			set
			{
				base.XD_SU_MenuItem = value;
				if (value.IsEmpty)
				{
					XD_MaxChargesPerTransaction = 0;
					XD_RollupBehaviourWhenMaxExceeded = ZString.Empty;
				}
			}
		}

		[List("Lookups.PrintingBranches")]
		public override ZGuid XD_GB_BranchOwner
		{
			get
			{
				return base.XD_GB_BranchOwner;
			}
			set
			{
				base.XD_GB_BranchOwner = value;
			}
		}

		[List("Lookups.PrintingDepartments")]
		public override ZGuid XD_GE_Department
		{
			get
			{
				return base.XD_GE_Department;
			}
			set
			{
				base.XD_GE_Department = value;
			}
		}

		[List("Lookups.XD_ComplianceNumberFormat_List")]
		public override ZString XD_NumberFormat
		{
			get => base.XD_NumberFormat;
			set => base.XD_NumberFormat = value;
		}

		[List("Lookups.XD_RollupBehaviourType_List")]
		public override ZString XD_RollupBehaviourWhenMaxExceeded
		{
			get
			{
				return base.XD_RollupBehaviourWhenMaxExceeded;
			}
			set
			{
				base.XD_RollupBehaviourWhenMaxExceeded = value;
				if (value == ComplianceRollupBehaviourType.MultiPageNoLimitation)
				{
					XD_MaxChargesPerTransaction = 0;
				}
			}
		}

		public bool IsAllocated => IsInDatabase && (XD_NextNumber > XD_StartNumber);

		public ZBool IsInFinalisedDate
		{
			get
			{
				var helper = ObjectFactory.Get<IComplianceReportComplianceDocumentFinaliser>("IComplianceReportComplianceDocumentFinaliser", Factory);

				return helper.IsInFinalisedRange(XD_SequenceClass, XD_StartDate, XD_ExpiryDate, LedgerTypes.AccountsReceivable);
			}
		}

		ZBool DateReadOnly
		{
			get
			{
				var result = IsInDatabase && !XD_ExpiryDateInfo.HasChanges && !XD_StartDateInfo.HasChanges && IsInFinalisedDate;

				return IsInPortugalCompany ? IsInDatabase && XD_NextNumber > 1 : result;
			}
		}

		bool XD_ExpiryDate_ReadOnly => DateReadOnly;

		bool XD_StartDate_ReadOnly => DateReadOnly;

		protected bool XD_SequenceClass_ReadOnly => IsInTaiwanCompany && IsAllocated;

		virtual protected bool XD_Prefix_ReadOnly => IsAllocated;

		protected bool XD_MaximumNumberDigits_ReadOnly => IsAllocated;

		protected bool XD_GB_BranchOwner_ReadOnly
		{
			get { return !AllocationStrategy.IsBranchApplicable; }
		}

		protected bool XD_GE_Department_ReadOnly
		{
			get { return !AllocationStrategy.IsDepartmentApplicable; }
		}

		public bool IsComplianceNumberAllocationDateMandatory => AccountingMasterFilesRegistry.Instance.GetComplianceNumberAllocationDateRegistryValue(ComplianceNumberAllocationDateValidLedgerEnum.AR, XD_GC_Company) != ComplianceNumberAllocationDateOptions.NoControl.Code;

		#endregion

		#region Calculated Fields

		[BusinessObjectTestExclude]
		[MaxLength(nameof(StartNumberMaxLength))]
		public virtual ZString XD_Calc_StartNumberString
		{
			get
			{
				ZString startNumber = XD_StartNumber.ToStringTrimZeros(0);
				startNumber = startNumber.PadLeft(XD_MaximumNumberDigits, '0');
				return startNumber;
			}
			set
			{
				ZDecimal numberParsed;
				if (ZDecimal.TryParse(value, out numberParsed))
				{
					XD_StartNumber = numberParsed;
					if (!IsInDatabase)
					{
						XD_NextNumber = XD_StartNumber;
					}
				}
				else
				{
					XD_StartNumber = 0;
					XD_NextNumber = 0;
				}
				XD_Calc_StartNumberStringInfo.RefreshBinding();
			}
		}

		public ZWrappedPropertyInfo XD_Calc_StartNumberStringInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.XD_Calc_StartNumberString, x => XD_StartNumberInfo); }
		}

		protected bool XD_Calc_StartNumberString_ReadOnly => IsInDatabase;

		int StartNumberMaxLength => AccComplianceSequenceSchema.XD_StartNumber.Precision;

		[BusinessObjectTestExclude]
		[MaxLength(nameof(EndNumberMaxLength))]
		public virtual ZString XD_Calc_EndNumberString
		{
			get
			{
				ZString endNumber = XD_EndNumber.ToStringTrimZeros(0);
				endNumber = endNumber.PadLeft(XD_MaximumNumberDigits, '0');
				return endNumber;
			}
			set
			{
				ZDecimal numberParsed;
				if (ZDecimal.TryParse(value, out numberParsed))
				{
					XD_EndNumber = numberParsed;
				}
				else
				{
					XD_EndNumber = 0;
				}
				XD_Calc_EndNumberStringInfo.RefreshBinding();
			}
		}

		public ZWrappedPropertyInfo XD_Calc_EndNumberStringInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.XD_Calc_EndNumberString, x => XD_EndNumberInfo); }
		}

		protected bool XD_Calc_EndNumberString_ReadOnly
		{
			get { return IsInDatabase; }
		}

		int EndNumberMaxLength => AccComplianceSequenceSchema.XD_EndNumber.Precision;

		[BusinessObjectTestExclude]
		public ZString XD_Calc_NextNumberString
		{
			get
			{
				ZString nextNumber = XD_NextNumber.ToStringTrimZeros(0);
				nextNumber = nextNumber.PadLeft(XD_MaximumNumberDigits, '0');
				return nextNumber;
			}
		}

		public ZPropertyInfo XD_Calc_NextNumberStringInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.XD_Calc_NextNumberString, x => XD_NextNumberInfo); }
		}

		[BusinessObjectTestExclude]
		public ZString XD_Calc_DocumentTitle
		{
			get
			{
				return GetDocumentTitle(XD_SequenceClass, GlbCompany.CurrentCompany.Country.Code);
			}
		}

		public ZPropertyInfo XD_Calc_DocumentTitleInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.XD_Calc_DocumentTitle, x => XD_SequenceClassInfo); }
		}

		public static ZString GetDocumentTitle(ZString subType, ZString countryCode) => AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(countryCode).GetDescriptionFromCode(subType);

		#region Allocation Level

		[List("Lookups.XD_AllocationLevel_List")]
		public override ZString XD_AllocationLevel
		{
			get
			{
				return base.XD_AllocationLevel;
			}
			set
			{
				if (base.XD_AllocationLevel != value)
				{
					base.XD_AllocationLevel = value;

					AllocationStrategy = AllocationLevelStrategyFactory.Create(XD_AllocationLevel);

					if (!AllocationStrategy.IsDepartmentApplicable)
					{
						if (!XD_GE_Department.IsEmpty)
						{
							XD_GE_Department = ZGuid.Empty;
						}
					}
					if (!AllocationStrategy.IsBranchApplicable)
					{
						if (!XD_GB_BranchOwner.IsEmpty)
						{
							XD_GB_BranchOwner = ZGuid.Empty;
						}
					}
					XD_GB_BranchOwnerInfo.RefreshBinding();
					XD_GE_DepartmentInfo.RefreshBinding();
				}
			}
		}

		public IAllocationLevelStrategy AllocationStrategy
		{
			get
			{
				if (allocationStrategy == null)
				{
					allocationStrategy = AllocationLevelStrategyFactory.Create(XD_AllocationLevel);
				}
				return allocationStrategy;
			}
			private set
			{
				allocationStrategy = value;
			}
		}

		IAllocationLevelStrategy allocationStrategy;

		#endregion

		protected bool XD_MaxChargesPerTransaction_ReadOnly
		{
			get
			{
				return XD_RollupBehaviourWhenMaxExceeded == ComplianceRollupBehaviourType.MultiPageNoLimitation || XD_SU_MenuItem.IsEmpty;
			}
		}

		protected bool XD_RollupBehaviourWhenMaxExceeded_ReadOnly
		{
			get { return XD_SU_MenuItem.IsEmpty; }
		}

		public ZString PrefixExcludesNumberPart
		{
			get
			{
				int lastNonNumericCharPos = LastNonNumericCharPositionInPrefix();
				if (lastNonNumericCharPos == XD_Prefix.Length)
				{
					return XD_Prefix;
				}
				else
				{
					return XD_Prefix.Substring(0, lastNonNumericCharPos);
				}
			}
		}

		public ZString StartNumberIncludingPrefixNumberPart
		{
			get
			{
				int lastNonNumericCharPos = LastNonNumericCharPositionInPrefix();
				if (lastNonNumericCharPos == XD_Prefix.Length)
				{
					return XD_Calc_StartNumberString;
				}
				else
				{
					return XD_Prefix.Substring(lastNonNumericCharPos) + XD_Calc_StartNumberString;
				}
			}
		}

		public ZString EndNumberIncludingPrefixNumberPart
		{
			get
			{
				int lastNonNumericCharPos = LastNonNumericCharPositionInPrefix();
				if (lastNonNumericCharPos == XD_Prefix.Length)
				{
					return XD_Calc_EndNumberString;
				}
				else
				{
					return XD_Prefix.Substring(lastNonNumericCharPos) + XD_Calc_EndNumberString;
				}
			}
		}

		int LastNonNumericCharPositionInPrefix()
		{
			int lastNonNumericCharPos = 0;
			int currentPos = 1;
			foreach (char c in XD_Prefix)
			{
				if (!char.IsDigit(c))
				{
					lastNonNumericCharPos = currentPos;
				}
				currentPos++;
			}
			return lastNonNumericCharPos;
		}

		public ZString VoidedNumbersLogsAsHTML
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				foreach (var log in Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.VoidingComplianceSequenceNo.Code)).OrderBy(x => x.SL_EventTime))
				{
					result.AppendLine(log.SL_EventTime.ToString() + " - " + log.SL_Reference);
				}

				return result.ToString().Replace("\r\n", "<br />");
			}
		}

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			LogEvents();
		}

		[SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		protected virtual void LogEvents()
		{
			if (IsInDatabase && XD_NextNumberInfo.HasChanges)
			{
				ZString fromNumber = XD_NextNumberInfo.OriginalValue.ToString();
				ZString toNumber = (XD_NextNumber - 1m).ToString(CultureInfo.InvariantCulture);
				Logs.AddNew(Events.VoidingComplianceSequenceNo, string.Format(CultureInfo.InvariantCulture, "sequence number is voided from {0} to {1}", fromNumber, toNumber));
			}
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override string CanReactivate()
		{
			var error = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetComplianceSequencePresentationProvider().CanReactivate(this);
			if (!error.IsEmpty)
			{
				return error;
			}

			return base.CanReactivate();
		}

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("12e55627-9229-4b65-8ce4-2a9c2e654875", "Compliance Invoice Book");
			}
		}

		protected override AccComplianceSequenceValidation GetNewValidation()
		{
			var complianceInfo = CountryComplianceFactory.GetICountryComplianceInfo(Company?.GC_RN_NKCountryCode ?? ZString.Empty) as IComplianceSequenceValidationProvider;
			return complianceInfo?.GetAccComplianceSequenceValidation(this) ?? base.GetNewValidation();
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XD_GC_Company = GlbCompany.CurrentCompany.PK;
			XD_MaxChargesPerTransaction = (ZByte)AccountingMasterFilesRegistry.Instance.MaximumNumberOfChargesToPrintPerComplianceDocument.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty);
			XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;

			if (IsInTaiwanCompany)
			{
				XD_MaximumNumberDigits = LengthOfMaxNumberDigitsForTW;
			}

			var defaultConfig = DefaultMandatoryComplianceNumberSequenceConfiguration;
			if (defaultConfig != null)
			{
				XD_NumberFormat = defaultConfig.Code;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, DocManagerCodes.ComplianceSequence);
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

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			var result = Factory.New<AccComplianceSequence>();
			result.CopyPersistentValuesFrom(this);
			if (IsDigitalSignatureApplicable)
			{
				result.XD_StartNumber = this.XD_NextNumber;
			}
			return result;
		}

		bool IsDigitalSignatureApplicable => Company.Country.SupportDocumentSigning;

		#endregion

		public DocumentSupporter DocumentSupporter
		{
			get { return new AccComplianceSequenceDocumentSupporter(this); }
		}

		#region IWorkflowSupporter

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new AccComplianceSequenceProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return new AccComplianceSequenceWorkflowDescriptor().Code; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		#endregion

		public bool IsConfiguredAsPrePrintedSequence
		{
			get
			{
				return !(XD_RollupBehaviourWhenMaxExceeded.IsEmpty || XD_RollupBehaviourWhenMaxExceeded == ComplianceRollupBehaviourType.MultiPageNoLimitation);
			}
		}

		public CodeDescriptionPair DefaultMandatoryComplianceNumberSequenceConfiguration
			=> ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(Company?.GC_RN_NKCountryCode ?? string.Empty)?.DefaultMandatoryComplianceNumberSequenceConfigurationCode;

		#region AddOn fields

		public ZString LockBy => LockByBizo?.GS_Code ?? ZString.Empty;

		GlbStaff LockByBizo => Factory.Load<GlbStaff>(XD_LockBy);

		public ZPropertyInfo LockByInfo
		{
			get { return GetZPropertyInfo(nameof(LockBy)); }
		}

		[ResourceStringData("6b877057-7f6b-4c68-a526-491c466be34b", Caption = "Lock By")]
		public ZGuid XD_LockBy
		{
			get
			{
				return this.GetSystemDefinedValue<ZGuid>(Schema.XD_LockBy);
			}
			set
			{
				this.SetSystemDefinedValue(Schema.XD_LockBy, AddOnColumnDataType.Codes.Guid, value);
				XD_LockByInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo XD_LockByInfo
		{
			get { return GetZPropertyInfo(Schema.XD_LockBy); }
		}
		#endregion

		#region Find Sequence Book

		public bool CheckOverlappingBooks()
		{
			var query = GetLoadSequenceBookQuery(XD_SequenceClass, XD_AllocationLevel, XD_StartDate, XD_ExpiryDate, this, true, default, default, default);
			query.AddToFilter(AccComplianceSequenceSchema.PK, SQLComparisonOperator.NotEqual, PK);
			return Factory.Exists(typeof(AccComplianceSequence), query);
		}

		public static AccComplianceSequence[] FindSuitableSequenceBook(ZString subType, ZString allocationLevel, ZDateTime startDate, ZDateTime expireDate, ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK)
		{
			var query = GetLoadSequenceBookQuery(subType, allocationLevel, startDate, expireDate, default, default, companyPK, branchPK, departmentPK);
			return new BusinessObjectFactory().Load<AccComplianceSequence>(query).Where(x => !x.IsInFinalisedDate).ToArray();
		}

		static ZQuery GetLoadSequenceBookQuery(ZString subType, ZString allocationLevel, ZDateTime startDate, ZDateTime expireDate, AccComplianceSequence parentSequence = null, bool isForOverlapValidation = false, ZGuid companyPK = default, ZGuid branchPK = default, ZGuid departmentPK = default)
		{
			ZQuery query = isForOverlapValidation ? new ZQuery() : new ZDBOnlyQuery(typeof(AccComplianceSequence));

			query.AddToFilter(AccComplianceSequenceSchema.XD_SequenceClass, subType);
			query.AddToFilter(AccComplianceSequenceSchema.XD_GC_Company, parentSequence?.XD_GC_Company ?? (!companyPK.IsDefault ? companyPK : GlbCompany.CurrentCompany.PK));
			query.AddToFilter(AccComplianceSequenceSchema.XD_IsActive, ZBool.True);

			IAllocationLevelStrategy allocationStrategy;
			if (parentSequence != null)
			{
				allocationStrategy = parentSequence.AllocationStrategy;
			}
			else
			{
				allocationStrategy = AllocationLevelStrategyFactory.Create(allocationLevel, branchPK, departmentPK);
			}
			allocationStrategy.CreateMatchFilter(parentSequence, query);

			if (!expireDate.IsEmpty && expireDate.IsValid)
			{
				var startDateDateFilter = new ZQuery(AccComplianceSequenceSchema.XD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, expireDate);
				startDateDateFilter.AddToFilter(JoinCondition.Or, AccComplianceSequenceSchema.XD_StartDate, DBNull.Value);
				query.AddToFilter(startDateDateFilter);
			}

			if (!startDate.IsEmpty && startDate.IsValid)
			{
				var expireDateFilter = new ZQuery(AccComplianceSequenceSchema.XD_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate.Date.ToDateTime());
				expireDateFilter.AddToFilter(JoinCondition.Or, AccComplianceSequenceSchema.XD_ExpiryDate, DBNull.Value);
				query.AddToFilter(expireDateFilter);
			}

			return query;
		}

		#endregion

		#region Compliance SubType Sequence Allocation

		public ZString GetNextNumber(IComplianceNumberSequence complianceNumberSequence, ZDateTime updateDateTime, string complianceNumberAllocationDateOption = "", ZDateTime? complianceNumberAllocationDate = null)
		{
			return GetNextNumberInfo(complianceNumberSequence, updateDateTime, complianceNumberAllocationDateOption, complianceNumberAllocationDate).NextNumberWithPrefix;
		}

		public (ZString NextNumber, ZString NextNumberWithPrefix) GetNextNumberInfo(IComplianceNumberSequence complianceNumberSequence, ZDateTime updateDateTime, string complianceNumberAllocationDateOption = "", ZDateTime? complianceNumberAllocationDate = null)
		{
			var nextNumberAsString = ZString.Empty;
			var nextNumberAsStringWithPrefix = ZString.Empty;

			int nextNumber = GetNextNumberFromComplianceSubTypeSequenceWithLock(updateDateTime);
			if (nextNumber > 0)
			{
				nextNumberAsString = nextNumber.ToString(CultureInfo.InvariantCulture).PadLeft(XD_MaximumNumberDigits, '0');
				if (XD_NumberFormat == AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code)
				{
					nextNumberAsStringWithPrefix = XD_Prefix + nextNumberAsString;
				}
				else
				{
					var collection = AccountingMasterFilesRegistry.Instance.ComplianceNumberSequenceConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (collection != null && collection.Count > 0)
					{
						var configuration = collection.GetComplianceNumberSequenceConfigurationByCode(XD_NumberFormat);
						nextNumberAsStringWithPrefix = configuration.GetFormatedComplianceDocumentNumber(complianceNumberSequence, nextNumberAsString, XD_Prefix);
					}
				}
			}

			return (nextNumberAsString, nextNumberAsStringWithPrefix);
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "complex query")]
		public int GetNextNumberFromComplianceSubTypeSequenceWithLock(ZDateTime updateDateTime)
		{
			string selectOrInsertSql = @"
					DECLARE @CurrentValue int
					DECLARE @MaxValue int
					DECLARE @ExpiryDateTime DateTime
					DECLARE @StartDate Date
					SELECT @CurrentValue = XD_NextNumber, @MaxValue = XD_EndNumber, @ExpiryDateTime = XD_ExpiryDate, @StartDate = XD_StartDate FROM dbo.AccComplianceSequence WITH(UPDLOCK, ROWLOCK)
					WHERE XD_PK = @PK AND XD_IsActive = 1

					IF (@CurrentValue IS NULL)  
						RAISERROR(@msgSequenceFullOrExpired, 16, 1)
					ELSE
					 BEGIN
						IF ((@ExpiryDateTime IS NOT NULL AND @updateDateTime > @ExpiryDateTime) OR (@StartDate IS NOT NULL AND @updateDateTime < @StartDate))
							RAISERROR(@msgSequenceFullOrExpired, 16, 1)
						ELSE
							IF (@CurrentValue > @MaxValue)  
								RAISERROR(@msgSequenceFullOrExpired, 16, 1)
							ELSE
								BEGIN
									UPDATE dbo.AccComplianceSequence
									SET 
										XD_IsActive = (CASE 
														WHEN @CurrentValue = @MaxValue THEN @False
														ELSE XD_IsActive
														END),
										XD_ExpiryDate = (CASE 
														WHEN @CurrentValue = @MaxValue THEN @updateDateTime
														ELSE XD_ExpiryDate
														END),
										XD_NextNumber = @CurrentValue + 1,
										XD_SystemLastEditTimeUtc = GETUTCDATE(),
										XD_SystemLastEditUser = @SystemLastEditUser
									WHERE XD_PK = @PK

									SELECT @CurrentValue
								END
					 END";

			var dbConn = ((IDbConnected)Factory).Connection;

			var command = dbConn.Command(selectOrInsertSql);
			command.AddParameterBasedOnDbColumn("@PK", PK.ToGuid(), AccComplianceSequenceSchema.PK);
			command.AddParameter("@updateDateTime", SqlDbType.DateTime, updateDateTime.Date.ToDateTime());
			command.AddParameterBasedOnDbColumn("@True", 1, AccComplianceSequenceSchema.XD_IsActive);
			command.AddParameterBasedOnDbColumn("@False", 0, AccComplianceSequenceSchema.XD_IsActive);
			command.AddParameter("@msgSequenceFullOrExpired", SqlDbType.NVarChar, ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage);
			command.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
			//Command.CommandTimeout = ???;	// default timeout is 300, i.e. AllocationComplianceSequenceBusyException will be thrown after 5 minutes

			try
			{
				object resultObject = command.ExecuteScalar();
				int result = 0;

				if (resultObject != null)
				{
					int.TryParse(resultObject.ToString(), out result);
				}
				return result;
			}
			catch (SqlException e)
			{
				if (string.Compare(e.Message, ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage, StringComparison.OrdinalIgnoreCase) == 0)
				{
					throw new AllocationComplianceSequenceFullException();
				}
				else
				{
					DbErrorType complianceSequenceNumberErrorType = new DbErrorHandler(e, dbConn).ExceptionType;

					if (complianceSequenceNumberErrorType == DbErrorType.TimeoutExpired || complianceSequenceNumberErrorType == DbErrorType.DeadlockError)
					{
						throw new AllocationComplianceSequenceBusyException();
					}
				}

				throw;
			}
		}

		public bool PrintingAuthorizationNumberHasBeenUsedInTransactionHeaderReference
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new ZString[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote });
				query.AddToFilter(AccTransactionHeaderSchema.AH_XD_ComplianceBook, PK);

				var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
				subQuery.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return Factory.Exists(typeof(AccTransactionHeader), query);
			}
		}

		#endregion
	}

	#region Compliance Sequence Related Exceptions

	public static class ComplianceSequenceNumberAllocationErrorMessages
	{
		public static string FailedToFindComplianceSequenceMessage
		{
			get { return Res.GetString("8EB90889-E0D2-4867-825C-596E3F8C1CFB", "Please check your Compliance Invoice Book Setups. \r\n A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist."); }
		}
		public static string CannotAllocateNumberToSequenceConfiguredAsPrePrinted
		{
			get { return Res.GetString("2c4b038e-8295-4c32-b1d4-21602a8f3d26", "Some compliance sequence numbers were not allocated because the compliance book/s used have pre-printed invoice configurations. The compliance sequence number will be allocated when you print the document."); }
		}
		public static string MultipleComplianceSequenceFoundMessage
		{
			get { return Res.GetString("b00db571-f311-41aa-9df1-fb2a11e7e988", "Please check your Compliance Invoice Book Setups. \r\n {0} was unable to determine which Compliance Book should to use when allocating numbers. \r\n There should only be one Active Compliance Book per Sub Type and Branch at any one time.", BrandingFactory.Instance.ProductName); }
		}
		public static string FailedToFindSequenceFromSequenceNumberMessage
		{
			get { return Res.GetString("681a4998-175c-49fd-9940-68d9305dfa2d", "Could not identify the original Compliance Invoice Book.\r\n {0} could not identify the Compliance Invoice Book that was used to assign this transaction a Compliance Number.", BrandingFactory.Instance.ProductName); }
		}
		public static string FailedToFindSequenceDueToReferenceNotEndWithNumberMessage
		{
			get { return Res.GetString("c524aa4a-6d40-452c-8671-6e540a8a79f5", "Could not identify the original Compliance Invoice Book.\r\n {0} could not identify the Compliance Invoice Book as the transaction reference does not end with a valid number.", BrandingFactory.Instance.ProductName); }
		}
		public static string ComplianceSequenceIsFullOrExpiredExceptionMessage
		{
			get { return Res.GetString("71d741a1-6a23-4be0-95a1-48b16b97008b", "Please check your Compliance Book Setups.\r\n Compliance Numbers could not be allocated.\r\n All suitable Compliance Books for the relevant criteria are fully used or expired (e.g. Sub Type, Allocation Level, Branch, Active Status, Start Date, Expiry Date etc."); }
		}
		public static string NoComplianceInvoicesToPrintLBDMessage
		{
			get { return Res.GetString("BC0C3DC4-35F0-44F3-A160-E044AADE9A3B", "Please configure appropriate Compliance Invoice Books for your Login Company, Branch or Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)"); }
		}
		public static string NoComplianceInvoicesToPrintHBDMessage
		{
			get { return Res.GetString("8022E1EB-BB2C-4832-9BE6-85CFF04B6E9A", "Please configure appropriate Compliance Invoice Books for your Login Company, Transaction Header Branch or Transaction Header Branch and Department through the Compliance Sequences module. \r\n Compliance Books for the relevant criteria do not exist (e.g. Sub-Type, Allocation Level, Branch, Active Status, Start / Expiry Date, Post Date etc.)"); }
		}
		public static string ComplianceSequenceIsBusyExceptionMessage
		{
			get { return Res.GetString("2f7e1832-9983-4f63-9a18-d08ede6eefb8", "Please try this action again.\r\n Compliance Numbers could not be allocated because the necessary Compliance Book was busy allocating numbers for another user.\r\n Please try this action again."); }
		}
		public static string ComplianceSequenceIsFullValidationMessage
		{
			get { return Res.GetString("f0962ad0-ccdf-42df-a01a-31bb3f1d88fa", "Please check your Compliance Book Setups.\r\n The Last Number of the Compliance Book has been used.\r\n A new Compliance Invoice Book needs to be activated."); }
		}
		public static string OnePrinterCannotHandleMultipleComplianceSequenceTypesMessage
		{
			get { return Res.GetString("c6e986ae-902d-49dc-a000-aa850464d7ab", "Please check the Printer Setups configured against your Compliance Invoice Books.\r\n When printing to pre-printed pre-numbered paper stock, Active Compliance books cannot share the same printer."); }
		}
		public static string UnableToAllocateNumberDueToSubtypeMissingMessage
		{
			get { return Res.GetString("a36ef7f5-18f0-425a-bb54-bfd935241e81", "Compliance Numbers cannot be allocated. \r\n This transaction does not have a recognized Compliance Sub Type"); }
		}
		public static string GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(string complianceNumberAllocationDateOption)
		{
			if (complianceNumberAllocationDateOption == ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				return Res.GetString("a1da51c8-0014-4e09-b066-a7fa8bfd572c", "Compliance Numbers cannot be allocated.\r\n Last posted transaction with the same Compliance Sub Type {0} has Invoice Date = {1}, that is greater than the current one(s).");
			}
			else if (complianceNumberAllocationDateOption == ComplianceNumberAllocationDateOptions.PostDate.Code)
			{
				return Res.GetString("984D7811-FE63-418C-B790-233720859020", "Compliance Numbers cannot be allocated.\r\n Last posted transaction with the same Compliance Sub Type {0} has Post Date = {1}, that is greater than the current one(s).");
			}
			else
			{
				return Res.GetString("0c26ba04-f666-4f2e-86c4-a828d7dc9af4", "Compliance Numbers cannot be allocated.\r\n Last posted transaction with the same Compliance Sub Type {0} has allocation date = {1}, that is greater than the current one(s).");
			}
		}
		public static string GetUnableToAllocateNumberDueToSparseComplianceBookMessage(string complianceNumberAllocationDateOption)
		{
			if (complianceNumberAllocationDateOption == ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				return Res.GetString("33a72097-6775-4526-90fa-2c24bf21e8d6", "Compliance Numbers cannot be allocated.\r\n There is some transaction with the same Compliance Sub Type {0} in earlier Invoice Date and Compliance Number empty.\r\n Please allocate Compliance Number to all transactions with Invoice Date < {1}.");
			}
			else if (complianceNumberAllocationDateOption == ComplianceNumberAllocationDateOptions.PostDate.Code)
			{
				return Res.GetString("22CF371F-CA09-4A3A-AB40-AE171C1F558E", "Compliance Numbers cannot be allocated.\r\n There is some transaction with the same Compliance Sub Type {0} in earlier Post Date and Compliance Number empty.\r\n Please allocate Compliance Number to all transactions with Post Date < {1}.");
			}
			else
			{
				return Res.GetString("b4aca91b-fde6-4776-8053-d21c83798974", "Compliance Numbers cannot be allocated.\r\n There is some transaction with the same Compliance Sub Type {0} in earlier allocation date and Compliance Number empty.\r\n Please allocate Compliance Number to all transactions with allocation date < {1}.");
			}
		}
		public static string UnableToPrintDueToAtLeastOneInvoicePrintedAlreadyMessage
		{
			get { return Res.GetString("51884316-924d-491e-b924-aead430bdab9", "Please change your selection.\r\n Compliance Numbers cannot be allocated because at least one selected transaction has already been printed. Please amend your selection"); }
		}
		public static string UserQuestionsAreNotConfiguredProperlyExceptionMessage
		{
			get { return Res.GetString("9fe413e7-6960-4e42-b333-ef4105339ac6", "User questions are not configured for the compliance number allocation process."); }
		}
		public static string ComplianceSequenceHasNoDocumentMenuDefinedExceptionMessage
		{
			get { return Res.GetString("d1787360-bec7-44bc-adc5-a6fcbf8f7eef", "No Compliance Invoice Document will be Printed.\r\n This Compliance Book is not configured for printing.\r\n If you want to print a Compliance Document please amend your Compliance Book setups and nominate an appropriate document menu for printing."); }
		}

		public static string EmptyComplianceSubTypeExceptionMessage
		{
			get { return Res.GetString("683bdf26-0512-46b1-a365-80b563fab33c", "You cannot post invoices with a blank compliance sub-type. Please check the registry setting at Accounting > Government Compliance Invoice Document > Disallow posting transactions with empty compliance subtype."); }
		}

		public static string InvoiceDateLessThanPreviousExceptionMessage
		{
			get { return Res.GetString("1235962a-38b1-4ff6-8b71-e3d6127356a1", "Invoice date must be equal or higher than previous document."); }
		}

		public static string HasNonCMTChargeZeroAmountLineException
		{
			get { return Res.GetString("E838319A-5A50-47CB-82D5-1D5280DB84C4", "Compliance Number cannot be allocated to this transaction as it contains a Zero Amount Non-Comment Charge Line."); }
		}

		public static string PostDateLessThanPreviousExceptionMessage
		{
			get { return Res.GetString("3553e262-7eee-4c18-8bad-a0ff8ed319c4", "Post date must be equal or higher than previous document."); }
		}

		public static string InvoiceDateGreaterThanPostDateExceptionMessage
		{
			get { return Res.GetString("eabdce46-8535-490e-b150-cbbcd049f8f3", "Invoice date must be equal or lower than post date."); }
		}

		public static string ComplianceNumberExceedMaximumLengthExceptionMessage
		{
			get { return Res.GetString("7ba9a249-8a75-465e-a5b0-f15d47d38540", "The length of generated compliance number('{0}') has exceeded the total length of {1} characters.\r\n Please update the compliance invoice book with a number format that will not exceed the length limit."); }
		}

		public static string APBulkInvoiceRelatedExceptionMessage
		{
			get { return Res.GetString("0E900A97-26E5-4DFB-9D4F-6CDB5B66A10D", "Some invoices have Compliance Sequence errors"); }
		}
	}

	[Serializable]
	public abstract class ComplianceSequenceRelatedException : Exception
	{
		public ComplianceSequenceRelatedException()
		{ }

		protected ComplianceSequenceRelatedException(MultilingualString message)
			: base(message.ToString())
		{ }

#if NETFRAMEWORK
		protected ComplianceSequenceRelatedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public abstract ZString UserFriendlyMessage
		{
			get;
		}
	}

	[Serializable]
	public class ComplianceNumberExceedMaximumLengthException : ComplianceSequenceRelatedException
	{
		public ComplianceNumberExceedMaximumLengthException()
		{ }

		public ComplianceNumberExceedMaximumLengthException(ZString complianceNumber)
		{
			this.complianceNumber = complianceNumber;
		}

#if NETFRAMEWORK
		protected ComplianceNumberExceedMaximumLengthException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage => string.Format(CultureInfo.CurrentCulture, ComplianceSequenceNumberAllocationErrorMessages.ComplianceNumberExceedMaximumLengthExceptionMessage, complianceNumber, AccTransactionHeaderSchema.AH_TransactionReference.MaxLength);

		readonly ZString complianceNumber;
	}

	[Serializable]
	public class AllocationComplianceSequenceBusyException : ComplianceSequenceRelatedException
	{
		public AllocationComplianceSequenceBusyException()
		{ }

#if NETFRAMEWORK
		protected AllocationComplianceSequenceBusyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsBusyExceptionMessage;
			}
		}
	}

	[Serializable]
	public class AllocationComplianceSequenceFullException : ComplianceSequenceRelatedException
	{
		public AllocationComplianceSequenceFullException()
		{ }

#if NETFRAMEWORK
		protected AllocationComplianceSequenceFullException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage;
			}
		}
	}

	[Serializable]
	public class FailedToFindComplianceSequenceException : ComplianceSequenceRelatedException
	{
		public FailedToFindComplianceSequenceException()
		{ }

#if NETFRAMEWORK
		protected FailedToFindComplianceSequenceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage;
			}
		}
	}

	[Serializable]
	public class CannotAllocateSequenceNumberToPrePrintedSequenceWhenNotPrintingException : ComplianceSequenceRelatedException
	{
		public CannotAllocateSequenceNumberToPrePrintedSequenceWhenNotPrintingException()
		{ }

#if NETFRAMEWORK
		protected CannotAllocateSequenceNumberToPrePrintedSequenceWhenNotPrintingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.CannotAllocateNumberToSequenceConfiguredAsPrePrinted;
			}
		}
	}

	[Serializable]
	public class MultipleComplianceSequenceFoundException : ComplianceSequenceRelatedException
	{
		public MultipleComplianceSequenceFoundException()
		{ }

#if NETFRAMEWORK
		protected MultipleComplianceSequenceFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.MultipleComplianceSequenceFoundMessage;
			}
		}
	}

	[Serializable]
	public class FailedToFindComplianceSequenceFromSequenceNumberException : ComplianceSequenceRelatedException
	{
		public FailedToFindComplianceSequenceFromSequenceNumberException()
		{ }

#if NETFRAMEWORK
		protected FailedToFindComplianceSequenceFromSequenceNumberException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.FailedToFindSequenceFromSequenceNumberMessage;
			}
		}
	}

	[Serializable]
	public class FailedToFindSequenceDueToReferenceNotEndWithNumberException : ComplianceSequenceRelatedException
	{
		public FailedToFindSequenceDueToReferenceNotEndWithNumberException()
		{ }

#if NETFRAMEWORK
		protected FailedToFindSequenceDueToReferenceNotEndWithNumberException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.FailedToFindSequenceDueToReferenceNotEndWithNumberMessage;
			}
		}
	}

	[Serializable]
	public class NoComplianceInvoicesToPrintLBDException : ComplianceSequenceRelatedException
	{
		public NoComplianceInvoicesToPrintLBDException()
		{ }

#if NETFRAMEWORK
		protected NoComplianceInvoicesToPrintLBDException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.NoComplianceInvoicesToPrintLBDMessage;
			}
		}
	}

	[Serializable]
	public class NoComplianceInvoicesToPrintHBDException : ComplianceSequenceRelatedException
	{
		public NoComplianceInvoicesToPrintHBDException()
		{ }

#if NETFRAMEWORK
		protected NoComplianceInvoicesToPrintHBDException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.NoComplianceInvoicesToPrintHBDMessage;
			}
		}
	}

	[Serializable]
	public class OnePrinterCannotHandleMultipleComplianceSequenceTypesException : ComplianceSequenceRelatedException
	{
		public OnePrinterCannotHandleMultipleComplianceSequenceTypesException()
		{ }

#if NETFRAMEWORK
		protected OnePrinterCannotHandleMultipleComplianceSequenceTypesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.OnePrinterCannotHandleMultipleComplianceSequenceTypesMessage;
			}
		}
	}

	[Serializable]
	public class UnableToAllocateNumberDueToSubtypeMissingException : ComplianceSequenceRelatedException
	{
		public UnableToAllocateNumberDueToSubtypeMissingException()
		{ }

#if NETFRAMEWORK
		protected UnableToAllocateNumberDueToSubtypeMissingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.UnableToAllocateNumberDueToSubtypeMissingMessage;
			}
		}
	}

	[Serializable]
	public class UnableToAllocateNumberDueToPostDateEarlierThanLastDateUsedException : ComplianceSequenceRelatedException
	{
		public UnableToAllocateNumberDueToPostDateEarlierThanLastDateUsedException()
		{ }

#if NETFRAMEWORK
		protected UnableToAllocateNumberDueToPostDateEarlierThanLastDateUsedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public UnableToAllocateNumberDueToPostDateEarlierThanLastDateUsedException(string subType, ZDateTime lastDateUsed, string complianceNumberAllocationDateOption)
		{
			this.complianceNumberAllocationDateOption = complianceNumberAllocationDateOption;
			this.subType = subType;
			lastDateUsedFormatted = lastDateUsed.ToShortDateString();
		}

#if NETFRAMEWORK
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(subType), subType);
			info.AddValue(nameof(lastDateUsedFormatted), lastDateUsedFormatted);
			info.AddValue(nameof(complianceNumberAllocationDateOption), complianceNumberAllocationDateOption);
		}
#endif

		public override ZString UserFriendlyMessage => string.Format(CultureInfo.CurrentCulture, ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(complianceNumberAllocationDateOption), subType, lastDateUsedFormatted);

		readonly string complianceNumberAllocationDateOption;
		readonly string subType;
		readonly string lastDateUsedFormatted;
	}

	[Serializable]
	public class UnableToAllocateNumberDueToSparseComplianceBookException : ComplianceSequenceRelatedException
	{
		public UnableToAllocateNumberDueToSparseComplianceBookException()
		{ }

#if NETFRAMEWORK
		protected UnableToAllocateNumberDueToSparseComplianceBookException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public UnableToAllocateNumberDueToSparseComplianceBookException(string subType, ZDateTime allocationDate, string complianceNumberAllocationDateOption)
		{
			this.complianceNumberAllocationDateOption = complianceNumberAllocationDateOption;
			this.subType = subType;
			allocationDateFormatted = allocationDate.ToShortDateString();
		}

#if NETFRAMEWORK
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(subType), subType);
			info.AddValue(nameof(allocationDateFormatted), allocationDateFormatted);
			info.AddValue(nameof(complianceNumberAllocationDateOption), complianceNumberAllocationDateOption);
		}
#endif

		public override ZString UserFriendlyMessage => string.Format(CultureInfo.CurrentCulture, ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(complianceNumberAllocationDateOption), subType, allocationDateFormatted);

		readonly string complianceNumberAllocationDateOption;
		readonly string subType;
		readonly string allocationDateFormatted;
	}

	[Serializable]
	public class UnableToPrintDueToAtLeastOneInvoicePrintedAlreadyException : ComplianceSequenceRelatedException
	{
		public UnableToPrintDueToAtLeastOneInvoicePrintedAlreadyException()
		{ }

#if NETFRAMEWORK
		protected UnableToPrintDueToAtLeastOneInvoicePrintedAlreadyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.UnableToPrintDueToAtLeastOneInvoicePrintedAlreadyMessage;
			}
		}
	}

	[Serializable]
	public class IndonesianConstraintFailedException : ComplianceSequenceRelatedException
	{
		public IndonesianConstraintFailedException()
		{ }

		public IndonesianConstraintFailedException(string message)
		{
			userFriendlyMessage = Res.GetString("1211d1dc-808c-4d23-adbe-b252c03c5a6f", "Fail to meet Indonesia constraint - {0}", message);
		}

#if NETFRAMEWORK
		protected IndonesianConstraintFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return userFriendlyMessage;
			}
		}
		readonly ZString userFriendlyMessage;
	}

	[Serializable]
	public class UserQuestionsAreNotConfiguredProperlyException : ComplianceSequenceRelatedException
	{
		public UserQuestionsAreNotConfiguredProperlyException()
		{ }

#if NETFRAMEWORK
		protected UserQuestionsAreNotConfiguredProperlyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.UserQuestionsAreNotConfiguredProperlyExceptionMessage;
			}
		}
	}

	[Serializable]
	public class GovernmentInvoiceMenuNotFoundException : ComplianceSequenceRelatedException
	{
		public GovernmentInvoiceMenuNotFoundException()
		{ }

#if NETFRAMEWORK
		protected GovernmentInvoiceMenuNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public GovernmentInvoiceMenuNotFoundException(ZString subType)
		{
			userFriendlyMessage = Res.GetString("9361ca12-b3fb-4e9f-ac94-db9c54808a56", "Government Invoice Menu can not be found for compliance subtype {0}.", subType);
		}

		public override ZString UserFriendlyMessage
		{
			get
			{
				return userFriendlyMessage;
			}
		}
		readonly ZString userFriendlyMessage;
	}

	[Serializable]
	public class DefaultGovernmentInvoiceMenuNotFoundException : ComplianceSequenceRelatedException
	{
		public DefaultGovernmentInvoiceMenuNotFoundException()
		{ }

#if NETFRAMEWORK
		protected DefaultGovernmentInvoiceMenuNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public DefaultGovernmentInvoiceMenuNotFoundException(ZString subType, ZString country)
		{
			userFriendlyMessage = Res.GetString("ab54d5d7-f447-4727-aa2b-41e3f9fee077", "Default government invoice menu does not exist for sub type {0} in {1}", subType, country);
		}

		public override ZString UserFriendlyMessage
		{
			get
			{
				return userFriendlyMessage;
			}
		}
		readonly ZString userFriendlyMessage;
	}

	[Serializable]
	public class ComplianceSequenceHasNoDocumentMenuDefinedException : ComplianceSequenceRelatedException
	{
		public ComplianceSequenceHasNoDocumentMenuDefinedException()
		{ }

#if NETFRAMEWORK
		protected ComplianceSequenceHasNoDocumentMenuDefinedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceHasNoDocumentMenuDefinedExceptionMessage;
			}
		}
	}

	[Serializable]
	public class EmptyComplianceSubTypeException : ComplianceSequenceRelatedException
	{
		public EmptyComplianceSubTypeException()
		{ }

#if NETFRAMEWORK
		protected EmptyComplianceSubTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				return ComplianceSequenceNumberAllocationErrorMessages.EmptyComplianceSubTypeExceptionMessage;
			}
		}
	}

	[Serializable]
	public class InvoiceDateLessThanPreviousException : ComplianceSequenceRelatedException
	{
		public InvoiceDateLessThanPreviousException()
		{ }

#if NETFRAMEWORK
		protected InvoiceDateLessThanPreviousException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage => ComplianceSequenceNumberAllocationErrorMessages.InvoiceDateLessThanPreviousExceptionMessage;

		public override string Message => ComplianceSequenceNumberAllocationErrorMessages.InvoiceDateLessThanPreviousExceptionMessage;
	}

	[Serializable]
	public class HasNonCMTChargeZeroAmountLineException : ComplianceSequenceRelatedException
	{
		public HasNonCMTChargeZeroAmountLineException()
		{ }

#if NETFRAMEWORK
		protected HasNonCMTChargeZeroAmountLineException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage => ComplianceSequenceNumberAllocationErrorMessages.HasNonCMTChargeZeroAmountLineException;

		public override string Message => ComplianceSequenceNumberAllocationErrorMessages.HasNonCMTChargeZeroAmountLineException;
	}

	[Serializable]
	public class PostDateLessThanPreviousException : ComplianceSequenceRelatedException
	{
		public PostDateLessThanPreviousException()
		{ }

#if NETFRAMEWORK
		protected PostDateLessThanPreviousException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage => ComplianceSequenceNumberAllocationErrorMessages.PostDateLessThanPreviousExceptionMessage;

		public override string Message => ComplianceSequenceNumberAllocationErrorMessages.PostDateLessThanPreviousExceptionMessage;
	}

	[Serializable]
	public class InvoiceDateGreaterThanPostDateException : ComplianceSequenceRelatedException
	{
		public InvoiceDateGreaterThanPostDateException()
		{ }

#if NETFRAMEWORK
		protected InvoiceDateGreaterThanPostDateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage => ComplianceSequenceNumberAllocationErrorMessages.InvoiceDateGreaterThanPostDateExceptionMessage;

		public override string Message => ComplianceSequenceNumberAllocationErrorMessages.InvoiceDateGreaterThanPostDateExceptionMessage;
	}

	[Serializable]
	public class APBulkInvoiceComplianceSequenceRelatedException : ComplianceSequenceRelatedException
	{
		public APBulkInvoiceComplianceSequenceRelatedException()
		{ }

#if NETFRAMEWORK
		protected APBulkInvoiceComplianceSequenceRelatedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage => ComplianceSequenceNumberAllocationErrorMessages.APBulkInvoiceRelatedExceptionMessage;
	}

#endregion
}
