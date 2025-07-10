using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobShipmentPreplanningProcessTask : RoutingSupportProcessTask, Integration.Forwarding.IJobShipmentPreplanningProcessTask
	{
		public JobShipmentPreplanningProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.JobShipmentPreplanning; }
		}

		protected override bool MatchesParentForMessageTriggerAction(ProcessTask relatedTrigger)
		{
			bool result = base.MatchesParentForMessageTriggerAction(relatedTrigger);
			if (!result &&
				ProcessTaskNotifications.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback) &&
				relatedTrigger.ProcessTaskNotifications.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback))
			{
				ForwardingConsol relatedConsol = relatedTrigger.Parent as ForwardingConsol;
				ForwardingShipment relatedShipment = relatedTrigger.Parent as ForwardingShipment;

				if (relatedShipment != null && Parent.EF_JS == relatedShipment.PK)
				{
					result = true;
				}
				else if (relatedConsol != null && Parent.Shipment != null && Parent.Shipment.Consols.Contains(relatedConsol))
				{
					result = true;
				}
			}
			return result;
		}

		#region Closing Order AllImportDocumentsReceived Exceptions

		public override ZString P9_Status
		{
			get { return base.P9_Status; }
			set
			{
				if (base.P9_Status != value)
				{
					base.P9_Status = value;
					if (IsException &&
						P9_SE_NKMilestoneEvent == Events.AllImportDocumentsReceived.Code &&
						IsExceptionActioned)
					{
						CloseOrderAllImportDocumentsReceivedExceptions();
					}
				}
			}
		}

		void CloseOrderAllImportDocumentsReceivedExceptions()
		{
			foreach (Order order in Parent.Orders)
			{
				foreach (ProcessTask exception in order.WorkflowItems.Exceptions)
				{
					if (P9_SE_NKMilestoneEvent == Events.AllImportDocumentsReceived.Code &&
						!exception.IsExceptionActioned)
					{
						exception.IsExceptionActioned = true;
					}
				}
			}
		}

		#endregion

		#region Transport Legs

		protected override Type TransportParentType
		{
			get { return typeof(JobShipmentPreplanning); }
		}

		protected override Type TransportType
		{
			get { return typeof(Transport); }
		}

		protected override Transport GetTransportLegToAttach()
		{
			Transport result = null;
			if (P9_SE_NKMilestoneEvent == Events.Departure.Code)
			{
				result = Parent.PreAdviceTransports.FindTransportByLoadPort(Parent.EF_RL_NKPortLoad);
			}
			else if (P9_SE_NKMilestoneEvent == Events.Arrival.Code)
			{
				result = Parent.PreAdviceTransports.FindTransportByDischargePort(Parent.EF_RL_NKPortDisch);
			}
			return result;
		}

		#endregion

		#region Parent

		protected override Type ParentType
		{
			get { return typeof(JobShipmentPreplanning); }
		}

		public new JobShipmentPreplanning Parent
		{
			get { return (JobShipmentPreplanning)base.Parent; }
		}

		#endregion

		#region Exceptions

		protected override void SetExceptionLocation() { }

		#endregion
	}
}
