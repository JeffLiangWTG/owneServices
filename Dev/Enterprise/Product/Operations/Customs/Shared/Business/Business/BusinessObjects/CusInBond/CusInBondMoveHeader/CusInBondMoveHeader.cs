using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class CusInBondMoveHeader : BaseCusInBondMoveHeader, ISynchroniserReadOnlyMembersProvider, IBranchProvider, IWorkflowProvider
	{
		protected CusInBondMoveHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly new TypeDecider TypeDecider = new CusInBondMoveHeaderTypeDecider();

		#endregion

		public new CusInBondHeader Header => (CusInBondHeader)base.Header;

		protected override void MajorMarkAsNeedingValidationCore()
		{
			CusInBondHeader parent = Header;
			if (parent != null)
			{
				parent.Bills.MarkAsNeedingValidationIncludingChildren();
			}

			base.MajorMarkAsNeedingValidationCore();
		}

		[ActionFieldFollow(false)]
		[ChildEditable]
		public ICusInBondMoveDetailCollection MovementDetails
		{
			get
			{
				if (movementDetails == null)
				{
					movementDetails = CreateMovementDetails();
					RegisterEditableChildObject(movementDetails);
				}
				return movementDetails;
			}
		}
		ICusInBondMoveDetailCollection movementDetails;

		protected abstract ICusInBondMoveDetailCollection CreateMovementDetails();

		#region Override Methods

		public override void Delete()
		{
			if (MovementDetailType != null)
			{
				MovementDetails.DeleteAll();
			}
			if (SupportsWorkflow)
			{
				((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			}
			base.Delete();
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

		public virtual bool IsWaitingForResponse
		{
			get { return false; }
		}

		public virtual bool IsAcceptedByCustoms
		{
			get { return false; }
		}

		public virtual bool IsWithdrawn
		{
			get { return false; }
		}

		GlbBranch IBranchProvider.Branch => HeaderBranch;

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
			var parent = Header;
			if (SupportsWorkflow && HasChanges && parent == null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);  // Workflow
			}
		}

		#region IWorkflowProvider

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewCusInBondMoveHeaderProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetNewCusInBondMoveHeaderProcessTaskCollection()
		{
			return new ProcessTaskCollection(this);
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
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

		public ZString WorkflowType
		{
			get
			{
				var wfType = ZString.Empty;
				return wfType;
			}
		}
		#endregion
		#endregion
	}
}
