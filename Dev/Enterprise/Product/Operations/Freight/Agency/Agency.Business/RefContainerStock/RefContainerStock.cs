using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	[System.Diagnostics.DebuggerDisplay("Ctnr. Stock ({R6_ContainerNum})")]
	[CodeProperty(RefContainerStock.Schema.R6_ContainerNum), DescriptionProperty(RefContainerStock.Schema.Description)]
	[ActionFieldFollow(false)]
	[UniversalDataContext(DataContextType.ContainerStock)]
	public class RefContainerStock : AutoRefContainerStock, IRefContainerStock, IDocManagerSupport, IWorkflowProvider, IJobNumber
	{
		public RefContainerStock(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Schema

		public new class Schema : AutoRefContainerStock.Schema
		{
			public const string Description = "Description";

			public const string R6_RC_ISOType = "R6_RC_ISOType";
			public const string R6_RC_IsHighCube = "R6_RC_IsHighCube";
			public const string R6_RC_HasTynes = "R6_RC_HasTynes";
			public const string R6_RC_HasVents = "R6_RC_HasVents";
		}

		#endregion

		public static RefContainerStock Load(BusinessObjectFactory factory, ZString containerNumber)
		{
			var query = new ZQuery(RefContainerStockSchema.R6_ContainerNum, containerNumber);
			var containers = factory.Load<RefContainerStock>(query);

			if (containers.Length >= 2)
			{
				return containers.FirstOrDefault(r => !r.IsInDatabase);
			}
			return containers.FirstOrDefault();
		}

		#region Related BusinessObjects

		public ContainerMovementCollection Movements
		{
			get
			{
				if (movements == null)
				{
					ICollectionRelationship relationship = new DependentRelationship(this, typeof(ContainerMovement));
					movements = new ContainerMovementCollection(Factory, true, relationship);
				}
				return movements;
			}
		}
		ContainerMovementCollection movements;

		public MovementsFilter Filter
		{
			get
			{
				if (filter == null)
				{
					filter = new MovementsFilter(Factory, Movements.Relationship);
					RegisterEditableChildObject(filter.Movements);
				}
				return filter;
			}
		}
		MovementsFilter filter;

		public ContainerMovement LastMovement
		{
			get
			{
				if (lastMovement == null)
				{
					lastMovement = new CachedProperty<ContainerMovement>(Factory, delegate
					{
						var movementQuery = new ZQuery();
						movementQuery.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.NotEqual, null);
						movementQuery.AddToFilter(JobContainerMoveSchema.E9_R6, PK);
						movementQuery.OrderBy = JobContainerMoveSchema.Constants.E9_MovementDate + " DESC";
						movementQuery.FetchOnlyFromLocalCache = !IsInDatabase;

						var movement = Factory.LoadTop1<ContainerMovement>(movementQuery);

						return movement;
					});
				}

				return lastMovement.Value;
			}
		}
		CachedProperty<ContainerMovement> lastMovement;

		public ContainerMovement CurrentMovement
		{
			get => currentMovement;
			set => currentMovement = value;
		}
		ContainerMovement currentMovement;

		#endregion

		#region Properties

		public ZString Description
		{
			get { return Container == null ? ZString.Empty : Container.RC_DescriptionMultilingual; }
		}

		#region R6_ContainerNum

		[ReadOnlyMember(nameof(IsInDatabase))]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString R6_ContainerNum
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.R6_ContainerNum; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				var oldValue = R6_ContainerNum;
				if (value != oldValue)
				{
					base.R6_ContainerNum = value;

					if (!value.IsEmpty)
					{
						SetDebugLog(value, oldValue);
					}
				}
			}
		}

		#region DebugLog

		void SetDebugLog(ZString value, ZString oldValue)
		{
			DebugLog.AppendLine(FormattableString.Invariant($@"Updating ContainerNum from {oldValue} to {value} for PK: {PK}
Stacktrace:
{System.Environment.StackTrace}
IsInDatabase: {IsInDatabase}")); // Just an issue report
		}

		ZStringBuilder DebugLog => debugLog ?? (debugLog = new ZStringBuilder());
		ZStringBuilder debugLog = new ZStringBuilder();

		#endregion

		#endregion

		[List("Lookups.OwnerTypes")]
		[ActionField(CollectionType = typeof(ContainerOwnershipList))]
		public override ZString R6_OwnerType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.R6_OwnerType; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.R6_OwnerType = value; }
		}

		public override ZGuid R6_RC
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.R6_RC; }
			set
			{
				ZGuid oldValue = R6_RC;
				base.R6_RC = value;

				if (oldValue != value)
				{
					R6_RC_HasTynesInfo.RefreshBinding();
					R6_RC_HasVentsInfo.RefreshBinding();
					R6_RC_IsHighCubeInfo.RefreshBinding();
					R6_RC_ISOTypeInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("RefContainerStock|R6_RC_ISOType", ShortCaption = "ISO", Caption = "ISO Type", FullDescription = "The container's ISO type code.")]
		public ZString R6_RC_ISOType
		{
			get { return Container == null ? ZString.Empty : Container.RC_ISOType; }
		}

		public ZPropertyInfo R6_RC_ISOTypeInfo
		{
			get { return GetZPropertyInfo(Schema.R6_RC_ISOType); }
		}

		[ResourceStringData("RefContainerStock|R6_RC_IsHighCube", Caption = "Is High Cube", FullDescription = "The container is a high cube container.")]
		public ZBool R6_RC_IsHighCube
		{
			get { return Container == null ? ZBool.False : Container.RC_IsHighCube; }
		}

		public ZPropertyInfo R6_RC_IsHighCubeInfo
		{
			get { return GetZPropertyInfo(Schema.R6_RC_IsHighCube); }
		}

		[ResourceStringData("RefContainerStock|R6_RC_HasTynes", Caption = "Has Tynes", FullDescription = "The container has tynes.")]
		public ZBool R6_RC_HasTynes
		{
			get { return Container == null ? ZBool.False : Container.RC_HasTynes; }
		}

		public ZPropertyInfo R6_RC_HasTynesInfo
		{
			get { return GetZPropertyInfo(Schema.R6_RC_HasTynes); }
		}

		[ResourceStringData("RefContainerStock|R6_RC_HasVents", Caption = "Has Vents", FullDescription = "The container has vents.")]
		public ZBool R6_RC_HasVents
		{
			get { return Container == null ? ZBool.False : Container.RC_HasVents; }
		}

		public ZPropertyInfo R6_RC_HasVentsInfo
		{
			get { return GetZPropertyInfo(Schema.R6_RC_HasVents); }
		}

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			Movements.DeleteAll();
			base.Delete();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("eca66c46-536e-4a59-bd27-a7107d946bbf", "Container {0}", R6_ContainerNum); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new RefContainerStockFetchStrategy(this);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return new DocManagerInfo(this, Constants.DocManagerCodes.AgencyContainerManager); }
		}

		#endregion

		#region IWorkflowInformationProvider Members

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

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

		[ChildEditable(true)]
		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new RefContainerStockProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		RefContainerStockProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.ContainerStockManagerWorkflowDescriptorCode; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return R6_ContainerNum; }
		}

		#endregion
	}
}



