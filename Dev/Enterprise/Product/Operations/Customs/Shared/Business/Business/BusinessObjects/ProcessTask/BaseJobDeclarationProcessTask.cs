using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BaseJobDeclarationProcessTask<TJobDeclaration> : ProcessTask
		where TJobDeclaration : BaseJobDeclaration, IWorkflowProvider
	{
		public BaseJobDeclarationProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new TJobDeclaration Parent => (TJobDeclaration)base.Parent;

		public override ControllerID ParentControllerID => ControllerIDs.Customs.JobDeclaration;

		public override ZString P9_Status
		{
			get { return base.P9_Status; }
			set
			{
				if (base.P9_Status != value)
				{
					base.P9_Status = value;
					if (IsException &&
						IsExceptionActioned &&
						P9_SE_NKMilestoneEvent == Events.AllImportDocumentsReceived.Code)
					{
						ClosePreadviceAllImportDocumentsReceivedExceptions();
					}
				}
			}
		}

		public override ZDateTime ETA => Parent.JE_DateAtFinalDestination;

		public override ZDateTime ETD => Parent.JE_DateAtOrigin;

		public override ZString LoadOrOriginPort => Parent.JE_RL_NKPortOfLoading;

		public override ZString DischargeOrDestinationPort => Parent.JE_RL_NKPortOfArrival;

		protected override Type ParentType => typeof(TJobDeclaration);

		protected override void OnSetActualDateCore(ZDateTimeOffset value)
		{
			base.OnSetActualDateCore(value);
			if (IsMilestoneOrWorkflowTrigger)
			{
				if (P9_SE_NKMilestoneEvent == Events.PickupCartageAdvised.Code)
				{
					Parent.DocsAndCartage.JP_PickupCartageAdvised = value.ToZDateTime();
				}
				else if (P9_SE_NKMilestoneEvent == Events.DeliveryCartageAdvised.Code)
				{
					Parent.DocsAndCartage.JP_DeliveryCartageAdvised = value.ToZDateTime();
				}
			}
		}

		void ClosePreadviceAllImportDocumentsReceivedExceptions()
		{
			foreach (JobShipmentPreplanning preadvice in Factory.Load<JobShipmentPreplanning>(new ZQuery(JobShipmentPreplanningSchema.EF_JE, Parent.PK)))
			{
				foreach (ProcessTask exception in ((IWorkflowProvider)preadvice).WorkflowItems.Exceptions)
				{
					if (!exception.IsExceptionActioned && P9_SE_NKMilestoneEvent == Events.AllImportDocumentsReceived.Code)
					{
						exception.IsExceptionActioned = true;
					}
				}
			}
		}
	}
}
