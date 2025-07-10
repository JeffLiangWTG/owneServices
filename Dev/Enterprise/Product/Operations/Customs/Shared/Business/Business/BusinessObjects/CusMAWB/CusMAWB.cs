using System;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ICountryCodeProvider = Enterprise.Integration.Customs.Shared.ICountryCodeProvider;
using ICusMAWB = Enterprise.Integration.Customs.Shared.ICusMAWB;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[UniversalDataContext(DataContextType.AirManifest)]
	[UserDefinedValues]
	public class CusMAWB :
		AutoCusMAWB,
		ICusMAWB,
		ICountryCodeProvider,
		IDocumentSupportable,
		IDocManagerSupport,
		IEDocsProvider,
		IWorkflowProvider,
		ICustomFieldParent,
		IUniversalXMLNoteParent,
		IBranchProvider
	{
		public new class Schema : AutoCusMAWB.Schema
		{
			public const string CustomsCargoStatusFilter = "CustomsCargoStatusFilter";
			public const string CustomsMessageStatusFilter = "CustomsMessageStatusFilter";
			public const string InvalidBillsOnlyFilter = "InvalidBillsOnlyFilter";
		}

		public CusMAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new CusMAWBTypeDecider();

		public static TriLockMutex CreateMutexForConsol(ZGuid consolPK, string countryCode)
		{
			Argument.NotNull(countryCode, nameof(countryCode));
			return new TriLockMutex(MutexIDs.CusMAWBJobBeingCreatedForConsol, consolPK.ToString() + countryCode);
		}

		#region OrgHeaders

		public BusinessObject EffectiveResponsiblePartyOrgHeader
		{
			get
			{
				return GetEffectiveResponsibleParty();
			}
		}

		public ZString ReasonEffectiveResponsiblePartyIsUnavailable
		{
			get
			{
				return GetReasonEffectiveResponsiblePartyIsUnavailable();
			}
		}

		public ZString ReasonCFSIsUnavailable
		{
			get
			{
				return GetReasonCFSIsUnavailable();
			}
		}

		public BusinessObject DeConsolidatorOrgHeader
		{
			get
			{
				return GetDeConsolidator();
			}
		}

		#endregion // OrgHeaders

		#region ICountryCodeProvider
		public virtual ZString CountryCode => Branch?.Company?.GC_RN_NKCountryCode ?? ZString.Empty;
		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public Loader(BusinessObjectFactory factory, CusMAWB parent)
				: base(factory)
			{
				this.parent = parent;
			}
			readonly CusMAWB parent;

			public ZString[] ApplicationCodes
			{
				get
				{
					return GetApplicationCodes();
				}
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusMAWB);
			}

			protected virtual ZString[] GetApplicationCodes()
			{
				return Array.Empty<ZString>();
			}

			public CusMAWB[] ActiveMAWBsWithThisMatchingMAWBNo()
			{
				return FindMatchingActiveMAWBsWithParent(parent.CM_MAWB, false, "", false, false);
			}

			public CusMAWB[] FindMatchingActiveMAWBsWithParent(ZString mAWBNo, ZString coLoadMaster)
			{
				return FindMatchingActiveMAWBsWithParent(mAWBNo, true, coLoadMaster, true, false);
			}

			public CusMAWB[] FindMatchingActiveMAWBsWithParent(ZString mAWBNo, bool excludeParent, ZString coLoadMaster, bool alwaysCheckCoload, bool excludeOld)
			{
				ZQuery findFilter = new ZQuery(CusMAWBSchema.CM_MAWB, mAWBNo);
				findFilter.AddToFilter(CusMAWBSchema.CM_ApplicationCode, parent.CM_ApplicationCode);
				findFilter.AddToFilter(CusMAWBSchema.CM_IsActive, true);
				if (excludeParent)
				{
					findFilter.AddToFilter(CusMAWBSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);
				}

				if (!coLoadMaster.IsEmpty || alwaysCheckCoload)
				{
					findFilter.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, coLoadMaster);
				}

				if (excludeOld)
				{
					findFilter.AddToFilter(DateQuery);
				}

				return Factory.Load<CusMAWB>(findFilter);
			}

			public CusMAWB[] FindMatchingMAWBs(ZString mAWBNo)
			{
				return FindMatchingMAWBs(mAWBNo, ZGuid.Empty, false);
			}

			public CusMAWB[] FindMatchingMAWBs(ZGuid consolPK, bool reloadExistingRows)
			{
				return FindMatchingMAWBs("", consolPK, reloadExistingRows);
			}

			public CusMAWB[] FindMatchingMAWBs(ZString mAWBNo, ZGuid consolPK, bool reloadExistingRows, bool excludeOld = false)
			{
				return Factory.Load<CusMAWB>(FindMatchingMAWBsFilter(mAWBNo, consolPK, reloadExistingRows, excludeOld));
			}

			public CusMAWB[] FindMatchingMAWBs(ZString mAWBNo, ZString companyCode, ZQuery additionalFilter)
			{
				CusMAWB[] result = null;
				var mainFilter = FindMatchingMAWBsFilter(mAWBNo,
														 consolPK: ZGuid.Empty,
														 reloadExistingRows: false,
														 excludeOld: true);
				var mawbQuery = new ZQuery(mainFilter, JoinCondition.And, additionalFilter);
				var companies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, companyCode));
				var branches = companies.Length == 1 ? companies[0].Branches.GetPKs() : null;
				if (!branches.IsNullOrEmpty())
				{
					var mawbQueryWithBranch = new ZQuery(mawbQuery, JoinCondition.And, new ZQuery(CusMAWBSchema.CM_GB, branches));
					result = Factory.Load<CusMAWB>(mawbQueryWithBranch);
				}
				if (result.IsNullOrEmpty())
				{
					result = Factory.Load<CusMAWB>(mawbQuery);
				}

				return result;
			}

			public ZQuery FindMatchingMAWBsFilter(ZString mAWBNo, ZGuid consolPK, bool reloadExistingRows, bool excludeOld)
			{
				ZQuery result;
				if (consolPK.IsEmpty)
				{
					result = new ZQuery(CusMAWBSchema.CM_MAWB, mAWBNo)
					{
						ReLoadExistingRows = reloadExistingRows,
					};
					if (ApplicationCodes.Length > 0)
					{
						result.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodes);
					}
					if (excludeOld)
					{
						result.AddToFilter(DateQuery);
					}
				}
				else
				{
					result = ForwardingConsol.GetMAWBsFilter(consolPK, reloadExistingRows, ApplicationCodes);
				}
				return result;
			}

			public static ZQuery DateQuery
			{
				get
				{
					var result = new ZQuery(CusMAWBSchema.CM_ArrivalDate, ZDateTime.Empty);
					result.AddToFilter(JoinCondition.Or, CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
					return result;
				}
			}

			public CusMAWB FindFirstMatchingMAWB(ZString mAWBNo)
			{
				return FindFirstMatchingMAWB(mAWBNo, ZGuid.Empty, false, false, true);
			}

			public CusMAWB FindFirstMatchingMAWB(ZGuid consolPK)
			{
				return FindFirstMatchingMAWB("", consolPK, false, false, false);
			}

			public CusMAWB FindFirstMatchingForwardedMAWB(ZString mAWBNo)
			{
				return FindFirstMatchingMAWB(mAWBNo, ZGuid.Empty, true, false, true);
			}

			public CusMAWB FindFirstMatchingCTOMAWB(ZString mAWBNo)
			{
				return FindFirstMatchingMAWB(mAWBNo, ZGuid.Empty, true, true, true);
			}

			public CusMAWB FindFirstMatchingMAWB(ZString mAWBNo, ZGuid consolPK, bool checkIsCTO, bool isCTOMAWB, bool excludeOld)
			{
				CusMAWB result;
				if (consolPK.IsEmpty)
				{
					var findFilter = new ZQuery(CusMAWBSchema.CM_MAWB, mAWBNo);
					if (ApplicationCodes.Length > 0)
					{
						findFilter.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodes);
					}
					if (checkIsCTO)
					{
						findFilter.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, isCTOMAWB);
					}
					if (excludeOld)
					{
						findFilter.AddToFilter(DateQuery);
					}
					result = Factory.LoadTop1<CusMAWB>(findFilter);
				}
				else
				{
					result = ForwardingConsol.GetFirstMatchingMAWB<ICusMAWB>(
						Factory, consolPK, checkIsCTO ? isCTOMAWB : new bool?(), excludeOld, ApplicationCodes) as CusMAWB;
				}
				return result;
			}

			public CusMAWB[] FindMatchingCTOMAWBs(ZString flightNumber, ZDateTime arrivalDate)
			{
				ZQuery findFilter = new ZQuery();
				if (ApplicationCodes.Length > 0)
				{
					findFilter.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodes);
				}

				findFilter.AddToFilter(CusMAWBSchema.CM_FlightNo, flightNumber);
				findFilter.AddToFilter(CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.EqualToDatePartOnly, arrivalDate);
				findFilter.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, ZBool.True);
				return Factory.Load<CusMAWB>(findFilter);
			}

			public CusMAWB[] FindMatchingForwarderMAWBs(ZString mAWB, ZString flightNumber, ZDateTime arrivalDate)
			{
				ZQuery findFilter = new ZQuery();
				if (ApplicationCodes.Length > 0)
				{
					findFilter.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodes);
				}

				findFilter.AddToFilter(CusMAWBSchema.CM_MAWB, mAWB);
				findFilter.AddToFilter(CusMAWBSchema.CM_FlightNo, flightNumber);
				findFilter.AddToFilter(CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.EqualToDatePartOnly, arrivalDate);
				findFilter.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, ZBool.False);
				return Factory.Load<CusMAWB>(findFilter);
			}

			public ZDBOnlyQuery GetMAWBQuery(ZString flightNumber, ZDateTime arrivalDate)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusMAWB));
				if (ApplicationCodes.Length > 0)
				{
					result.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodes);
				}

				result.AddToFilter(CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.EqualToDatePartOnly, arrivalDate);
				result.AddToFilter(CusMAWBSchema.CM_FlightNo, flightNumber);
				return result;
			}
		}

		public Loader CusMAWBLoader
		{
			get
			{
				if (cusMAWBLoader == null)
				{
					cusMAWBLoader = new Loader(Factory, this);
				}
				return cusMAWBLoader;
			}
		}
		Loader cusMAWBLoader;

		#endregion

		#region ChildBills Collection

		public void RegisterHouseBillsAsEditableChildren()
		{
			if (ChildBillsIsLoaded)
			{
				if (!IsRegisteredEditableChildObject(ChildBills))
				{
					RegisterEditableChildObject(ChildBills);
				}
			}
		}

		public void UnregisterHouseBillsAsEditableChildren()
		{
			if (ChildBillsIsLoaded)
			{
				if (IsRegisteredEditableChildObject(ChildBills))
				{
					UnRegisterEditableChildObject(ChildBills);
				}
			}
		}

		[ChildEditable(true)]
		public CusHAWBDependentCollection ChildBills
		{
			get
			{
				if (fChildBills == null)
				{
					fChildBills = GetNewChildBillsCollection();
					LoadNewChildBillsCollection(fChildBills);
				}
				return fChildBills;
			}
		}
		CusHAWBDependentCollection fChildBills;

		protected virtual void LoadNewChildBillsCollection(CusHAWBDependentCollection childBills)
		{
			childBills.Load();
			RegisterEditableChildObject(childBills);
		}

		protected virtual CusHAWBDependentCollection GetNewChildBillsCollection()
		{
			return new CusHAWBDependentCollection(this);
		}

		public bool ChildBillsIsLoaded
		{
			get { return fChildBills != null; }
		}
		#endregion

		#region FilteredChildBills Collection

		public CusHAWBFilteredCollection FilteredChildBills
		{
			get
			{
				if (filteredChildBills == null)
				{
					filteredChildBills = GetNewFilteredChildBillsCollection();
					filteredChildBills.Rebuild();
				}
				return filteredChildBills;
			}
		}
		CusHAWBFilteredCollection filteredChildBills;

		[List(nameof(Lookups) + "." + nameof(CusMAWBLookups.CustomsCargoStatusList))]
		[BusinessObjectTestExclude]
		[MaxLength(CusHAWB.Schema.CS_CustomsStatusMaxLength)]
		public ZString CustomsCargoStatusFilter
		{
			get
			{
				return customsCargoStatusFilter;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (SetNonPersistentPropertyValue(CustomsCargoStatusFilterInfo, ref customsCargoStatusFilter, value))
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateCustomsCargoStatusFilter();
						}
						CustomsCargoStatusFilterInfo.RefreshBinding();
					}
				}
			}
		}
		ZString customsCargoStatusFilter;

		public ZPropertyInfo CustomsCargoStatusFilterInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.CustomsCargoStatusFilter);
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusMAWBLookups.CustomsMessageStatusList))]
		[BusinessObjectTestExclude]
		[MaxLength(CusHAWB.Schema.CS_MsgStatusMaxLength)]
		public ZString CustomsMessageStatusFilter
		{
			get
			{
				return customsMessageStatusFilter;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (SetNonPersistentPropertyValue(CustomsMessageStatusFilterInfo, ref customsMessageStatusFilter, value))
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateCustomsMessageStatusFilter();
						}
						CustomsMessageStatusFilterInfo.RefreshBinding();
					}
				}
			}
		}
		ZString customsMessageStatusFilter;

		public ZPropertyInfo CustomsMessageStatusFilterInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.CustomsMessageStatusFilter);
			}
		}

		[BusinessObjectTestExclude]
		public ZBool InvalidBillsOnlyFilter
		{
			get
			{
				return invalidBillsOnlyFilter;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (SetNonPersistentPropertyValue(InvalidBillsOnlyFilterInfo, ref invalidBillsOnlyFilter, value))
					{
						InvalidBillsOnlyFilterInfo.RefreshBinding();
					}
				}
			}
		}
		ZBool invalidBillsOnlyFilter;

		public ZPropertyInfo InvalidBillsOnlyFilterInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.InvalidBillsOnlyFilter);
			}
		}

		#region Implementation

		protected virtual CusHAWBFilteredCollection GetNewFilteredChildBillsCollection()
		{
			return new CusHAWBFilteredCollection(this);
		}

		#endregion // Implementation

		#endregion // FilteredChildBills Collection

		public override void Delete()
		{
			base.Delete();
			ChildBills.RemoveAndDeleteAll();
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll(); // Workflow
		}

		#region Implementation

		internal protected virtual void AddDeferredScheduledMessagesEvent(ZString triggerAction)
		{
		}

		protected virtual OrgHeader GetEffectiveResponsibleParty()
		{
			return ResponsibleParty;
		}

		protected virtual ZString GetReasonEffectiveResponsiblePartyIsUnavailable()
		{
			var result = ZString.Empty;
			if (ResponsibleParty == null)
			{
				result = Res.GetString("a92b37a9-ba83-4176-8bab-e35652bb1749", "Responsible party is not specified");
			}
			return result;
		}

		protected ZString GetReasonCFSIsUnavailable()
		{
			var result = ZString.Empty;
			if (UnpackDepotAddress == null)
			{
				result = Res.GetString("2afef2fa-862e-433d-af8d-e7ae14ed8a5f", "CFS is not specified");
			}
			return result;
		}

		protected virtual OrgHeader GetDeConsolidator()
		{
			return null;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CM_GB = GlbBranch.CurrentBranch.PK;
		}

		public override ZBool CM_IsActive
		{
			get { return base.CM_IsActive; }
			set
			{
				if (base.CM_IsActive != value)
				{
					base.CM_IsActive = value;
					if (Consol != null)
					{
						Consol.IsCancelled = !value;
					}
				}
			}
		}

		protected ForwardingConsol fConsol;
		public ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null || fConsol.IsDeleted)
				{
					fConsol = Factory.Load<ForwardingConsol>(CM_JK);
				}
				return fConsol;
			}
		}

		#endregion

		#region IDocumentSupportable
		public DocumentSupporter DocumentSupporter
		{
			get { return GetNewDocumentSupporter(); }
		}

		protected virtual DocumentSupporter GetNewDocumentSupporter()
		{
			return new CusMAWBDocumentSupporter(this);
		}
		#endregion

		#region IDocManagerSupport Members
		DocManagerInfo fDocManagerInfo;
		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return fDocManagerInfo ?? (fDocManagerInfo = GetNewDocManagerInfo()); }
		}

		protected virtual DocManagerInfo GetNewDocManagerInfo()
		{
			return new DocManagerInfo(this, Enterprise.Core.Constants.DocManagerCodes.AirCargoMaster);
		}

		#endregion

		#region IEDocsProvider Members
		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}
		#endregion

		#region ICancellable Members

		public override string CanCancel()
		{
			var result = base.CanCancel();
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}

			if (!checkingCanCancelRelatedObjects)
			{
				checkingCanCancelRelatedObjects = true;
				try
				{
					var consol = Consol;
					if (consol != null)
					{
						var canCancelConsol = consol.CanCancel();
						if (!string.IsNullOrEmpty(canCancelConsol))
						{
							return Res.GetString(
								"7E6CC523-FF15-4F1D-8A54-5AE4BD862FC7",
								"This record cannot be deactivated as its parent host record cannot be deactivated due to the following reason.") +
								System.Environment.NewLine + consol.HumanReadableName + ": " + canCancelConsol;
						}
					}
				}
				finally
				{
					checkingCanCancelRelatedObjects = false;
				}
			}

			return null;
		}
		bool checkingCanCancelRelatedObjects;

		#endregion

		#region Workflow

		public bool SupportsWorkflow
		{
			get { return SupportsWorkflowCore; }
		}

		protected virtual bool SupportsWorkflowCore
		{
			get { return false; }
		}

		public override void OnSaving()
		{
			PopulateCM_MessageReferenceIfRequired();
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				CM_MessageReference = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		public virtual void PopulateCM_MessageReferenceIfRequired()
		{
			PopulateNumberPropertyIfRequired<ZString>(CM_MessageReferenceInfo, factory => Environment.Env.NumberFountains.CusMAWBMessageReferenceNumberFountain.GetNextFormatted(factory));
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			var consol = Consol;

			if (consol != null && consol.IsCancelled != IsCancelled)
			{
				IsCancelled = consol.IsCancelled;
			}

			if (SupportsWorkflow && Consol == null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);  // Workflow
			}
		}

		#region IWorkflowProvider

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return workflowInformationProvider ?? (workflowInformationProvider = new CusMAWBWorkflowInformationProvider(this));
		}
		IWorkflowInformationProvider workflowInformationProvider;

		[ChildEditable]
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
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewCusMAWBProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetNewCusMAWBProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return GetTemplateSelectionCriteriaCore();
		}

		protected virtual IColumnValueRanker GetTemplateSelectionCriteriaCore()
		{
			return new ColumnValueRanker();
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return JobInvoicingConsumerTypes.CusMAWB.Code; }
		}

		#endregion

		#region ICustomFieldParent Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, this, properties);
			}

			return customBusinessObject;
		}
		CustomBusinessObject customBusinessObject;

		protected void ResetCustomBusinessObject() => ((ICustomFieldParent)(this)).ResetCustomBusinessObject();

		void ICustomFieldParent.ResetCustomBusinessObject()
		{
			customBusinessObject = null;
			OnResetCustomBusinessObject?.Invoke();
		}

		public OnResetCustomBusinessObjectDelegate OnResetCustomBusinessObject { get; set; }

		#endregion

		#endregion

		public virtual void DefaultFromConsol()
		{
			throw new NotImplementedException("Must implement in subclass");
		}
	}
}
