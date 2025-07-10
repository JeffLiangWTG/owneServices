using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[CodeProperty(Schema.RCL_ListCode), DescriptionProperty(Schema.RCL_ListName)]
	public class RefComplianceList : AutoRefComplianceList, IWorkflowProvider
	{
		internal static string PreventionMessage => Res.GetString("40D43E66-2760-47FF-BAEE-34A37C63669D", "The Compliance List reference file does not support updating of Active Status.");

		public RefComplianceList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public RefComplianceListSecurityProvider SecurityProvider
		{
			get
			{
				if (securityProvider == null)
				{
					securityProvider = new RefComplianceListSecurityProvider();
				}

				return securityProvider;
			}
		}
		RefComplianceListSecurityProvider securityProvider;

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			string propertyName = property.Name;

			if (propertyName == RefComplianceListSchema.RCL_IsExcluded.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasEditConfigurationSecurity;
			}

			return shouldBeReadOnly || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		public override string CanCancel()
		{
			return PreventionMessage;
		}

		public override string CanReactivate()
		{
			return PreventionMessage;
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

		ProcessTaskCollection workflowItems;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new RefComplianceListProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		public ZString WorkflowType => WorkflowDescriptors.RefComplianceListWorkflowDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
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
	}
}
