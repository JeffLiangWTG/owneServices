using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageDeliveryContextLine : AutoEDIMessageDeliveryContextLine, IEDIMessageDeliveryContextResult, IWorkflowTypeProvider, IRootTypeProvider
	{
		public EDIMessageDeliveryContextLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public EDIMessageDeliveryContextSelector Parent => Factory.Load<EDIMessageDeliveryContextSelector>(ECL_ECS_MessageDeliveryContextSelector);

		[RelatedBusinessObject("Parent")]
		public override ZGuid ECL_ECS_MessageDeliveryContextSelector { get => base.ECL_ECS_MessageDeliveryContextSelector; set => base.ECL_ECS_MessageDeliveryContextSelector = value; }

		#region IEDIMessageDeliveryContextSelectorResult

		public ZString ContextType => ECL_ContextType;

		public ZString Description => ECL_Description;

		public ZString ContextValue { get; set; }

		#endregion

		#region IWorkflowTypeProvider

		bool IWorkflowTypeProvider.IsTemplate => false;

		ZString IWorkflowTypeProvider.WorkflowProcessType => Parent.ECS_ProcessType;

		#endregion

		#region IRootTypeProvider

		Type[] IRootTypeProvider.RootTypes => this.GetRootTypes();

		BusinessObject[] IRootTypeProvider.Roots => Array.Empty<BusinessObject>();

		#endregion
	}
}
