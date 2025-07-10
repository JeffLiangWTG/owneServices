using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(CusInBondHeader.Schema.BH_JobReference)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class CusInBondHeader : BaseCusInBondHeader, IBillTypeProvider, IWorkflowProvider, ISynchroniserReadOnlyMembersProvider, ISupportDataImporting, IBranchProvider
	{
		protected CusInBondHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly new CusInBondHeaderTypeDecider TypeDecider = new CusInBondHeaderTypeDecider();

		#endregion

		public virtual bool ShouldSynchronise
		{
			get { return true; }
		}

		public void SynchroniseWithParentIfNeeded()
		{
			SynchroniseWithParentIfNeededCore();
		}

		protected virtual void SynchroniseWithParentIfNeededCore()
		{ }

		public BusinessObjectSynchroniser Synchroniser
		{
			get { return GetNewSynchroniserCore(); }
		}

		protected virtual BusinessObjectSynchroniser GetNewSynchroniserCore()
		{
			return null;
		}

		protected BusinessObject ParentBusinessObject
		{
			get
			{
				if (parent == null || parent.IsDeleted || parent.PK != BH_ParentID)
				{
					parent = !BH_ParentID.IsEmpty && !BH_ParentTableCode.IsEmpty
						? Factory.Load(BH_ParentTableCode, BH_ParentID)
						: null;
				}
				return parent;
			}
		}
		BusinessObject parent;

		public ForwardingConsol Consol
		{
			get { return GetConsol(); }
		}

		protected virtual ForwardingConsol GetConsol()
		{
			return ParentBusinessObject as ForwardingConsol;
		}

		[ActionFieldFollow(false)]
		[ChildEditable]
		[DocumentFieldExcludeFromMap]
		public ICusInBondBillCollection Bills
		{
			get
			{
				if (bills == null)
				{
					bills = GetNewBillsCollection();
					RegisterEditableChildObject(bills);
				}
				return bills;
			}
		}
		ICusInBondBillCollection bills;

		public Type BillType
		{
			get { return BillTypeCore; }
		}

		protected abstract Type BillTypeCore { get; }

		protected abstract ICusInBondBillCollection GetNewBillsCollection();

		[ChildEditable]
		public CusInBondMoveHeaderCollection MovementHeaders
		{
			get
			{
				if (movementHeaders == null)
				{
					movementHeaders = GetMovementHeaders();
					RegisterEditableChildObject(movementHeaders);
				}
				return movementHeaders;
			}
		}
		CusInBondMoveHeaderCollection movementHeaders;

		protected abstract CusInBondMoveHeaderCollection GetMovementHeaders();

		public CusInBondMoveHeader MovementHeader
		{
			get
			{
				if (movementHeader == null || movementHeader.IsDeleted || movementHeader.BM_BH != PK)
				{
					movementHeader = GetNewMovementHeader();
				}
				return movementHeader;
			}
		}
		CusInBondMoveHeader movementHeader;

		protected virtual CusInBondMoveHeader GetNewMovementHeader()
		{
			var movementHeader = MovementHeaders.Count > 0 ? MovementHeaders.OfType<CusInBondMoveHeader>().OrderBy(x => x.BM_SystemCreateTimeUtc).FirstOrDefault() : null;
			if (movementHeader == null)
			{
				movementHeader = MovementHeaders.AddNew();
				movementHeader.HasChanges = false;
			}
			return movementHeader;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BH_GB = GlbBranch.CurrentBranch.PK;
		}

		public ZBool IsPostDepartureMessageOnly
		{
			get { return BH_PostDepartureOnly; }
		}

		#region Overriden Properties

		public override ZString BH_ApplicationCode
		{
			get { return base.BH_ApplicationCode; }
			set
			{
				base.BH_ApplicationCode = value;
				Bills.MarkAsNeedingValidationIncludingChildren();
			}
		}

		public override ZBool BH_IsActive
		{
			get { return base.BH_IsActive; }
			set
			{
				if (base.BH_IsActive != value)
				{
					base.BH_IsActive = value;
					var cancellableParent = ParentBusinessObject as ICancellable;
					if (cancellableParent != null)
					{
						cancellableParent.IsCancelled = !value;
					}
					UpdateReadOnly();
				}
			}
		}

		#endregion

		#region Override Methods

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (!BH_IsActive)
			{
				UpdateReadOnly();
			}
		}

		void UpdateReadOnly()
		{
			if (!IsDeleted)
			{
				SetReadOnlyIncludingChildren(!BH_IsActive);
			}
		}

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
					var cancellableParent = ParentBusinessObject as ICancellable;
					if (cancellableParent != null)
					{
						var canCancelParent = cancellableParent.CanCancel();
						if (!string.IsNullOrEmpty(canCancelParent))
						{
							return Res.GetString(
								"7E6CC523-FF15-4F1D-8A54-5AE4BD862FC7",
								"This record cannot be deactivated as its parent host record cannot be deactivated due to the following reason.") +
								System.Environment.NewLine + ParentBusinessObject.HumanReadableName + ": " + canCancelParent;
						}
					}

					if (MovementHeaders.OfType<CusInBondMoveHeader>().Any(x => x.IsWaitingForResponse))
					{
						return Res.GetString(
							"935704c3-6274-4df5-bb82-ffc376ae2528",
							"This record cannot be deactivated as there are Movements that are still waiting for a response from Customs.");
					}

					if (MovementHeaders.OfType<CusInBondMoveHeader>().Any(x => x.IsAcceptedByCustoms && !x.IsWithdrawn))
					{
						return Res.GetString(
							"4c00dd3a-6df4-4483-9dbc-0eb02c36e13d",
							"This record cannot be deactivated as there are Movements that have been accepted by Customs.");
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

		public override void Delete()
		{
			Bills.DeleteAll();
			DeleteAllMovementHeaders();
			this.DeleteChildren<CusInBondEvent>(CusInBondEventSchema.BN_BH);
			if (SupportsWorkflow)
			{
				((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		void DeleteAllMovementHeaders()
		{
			var query = new ZQuery(CusInBondMoveHeaderSchema.BM_BH, PK);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			var t = MovementHeaderType ?? typeof(CusInBondMoveHeader);
			var movements = new List<BusinessObject>(Factory.Load(t, query));
			movements.ForEach(x => x.Delete());
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
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

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			var parent = ParentBusinessObject;
			if (SupportsWorkflow && parent == null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);  // Workflow
			}
		}

		#region IWorkflowProvider

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

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
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewCusInBondHeaderProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetNewCusInBondHeaderProcessTaskCollection()
		{
			return new ProcessTaskCollection(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode; }
		}

		#endregion

		#endregion

		#region Send Customs Message Mutex

		ZGlobalMutex SendCustomsMessageMutex
		{
			get { return fSendCustomsMessageMutex ?? (fSendCustomsMessageMutex = new ZGlobalMutex(MutexIDs.SendCustomsMessage, PK.ToString())); }
		}
		ZGlobalMutex fSendCustomsMessageMutex;

		public bool LockSendCustomsMessageMutex() => SendCustomsMessageMutex.Lock();

		public void UnlockSendCustomsMessageMutex()
		{
			if (fSendCustomsMessageMutex != null && fSendCustomsMessageMutex.HasLock)
			{
				fSendCustomsMessageMutex.Unlock();
			}
		}

		public bool IsSendCustomsMessageMutexLocked => SendCustomsMessageMutex.IsLocked;

		public string CannotSendCustomsMessageWhenMutexIsLocked()
		{
			var lockInfo = SendCustomsMessageMutex.GetMutexLockByInfo();
			return Res.GetString("ea6f006a-78ca-4ff5-b4f2-4b1a2d4f0844", "{0} is in the process of sending messages for {1}, please try again later.", lockInfo, HumanReadableName);
		}

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}
		bool fIsImportingData;

		#endregion
	}
}
