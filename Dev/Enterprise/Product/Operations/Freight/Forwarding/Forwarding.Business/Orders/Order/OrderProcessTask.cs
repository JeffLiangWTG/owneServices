using System;
using System.Data;
using System.Linq;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderProcessTasks : ProcessTask, Integration.Forwarding.IOrderProcessTasks
	{
		public OrderProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ZArchitecture.Modules.ControllerID ParentControllerID
		{
			get
			{
				return Enterprise.ZArchitecture.Modules.ControllerIDs.Orders;
			}
		}

		protected override Type ParentType
		{
			get { return typeof(Order); }
		}

		public new Order Parent
		{
			get { return (Order)base.Parent; }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
		OrderTrackingDatesMap CreateOrderTrackingDatesMap()
		{
			OrderTrackingDatesMap result = null;

			if (Parent != null)
			{
				if (Parent.IsShipmentAttached)
				{
					result = new ForwardingShipmentOrderTrackingDatesMap(Parent.Shipment);
				}
				else if (Parent.IsDeclarationAttached)
				{
					result = new DeclarationOrderTrackingDatesMap(Parent.Declaration);
				}
			}

			return result;
		}

		public void UpdateActualDateFromAttachedShipmentOrDeclaration(bool synchronizeWhenEmpty)
		{
			if (IsMilestoneOrWorkflowTrigger)
			{
				var orderTrackingDatesMap = CreateOrderTrackingDatesMap();

				if (orderTrackingDatesMap != null)
				{
					switch (P9_SE_NKMilestoneEvent)
					{
						case Events.CustomsClearedCode:
						case Events.CustomsCommencedCode:
						case Events.ExportCustomsClearedCode:
						case Events.ExportCustomsCommencedCode:
							UpdateCustomsActualDate(orderTrackingDatesMap, synchronizeWhenEmpty);
							break;
						default:
							var dateToUpdate = GetDateToUpdate(P9_SE_NKMilestoneEvent, orderTrackingDatesMap);

							if (dateToUpdate != null && dateToUpdate.Value != P9_ActualDate.ToZDateTime())
							{
								Update(P9_ActualDateInfo, synchronizeWhenEmpty, dateToUpdate.Value);
							}
							break;
					}
				}
			}
		}

		ZDateTime? GetDateToUpdate(string milestoneEvent, OrderTrackingDatesMap orderTrackingDatesMap)
		{
			switch (milestoneEvent)
			{
				case Events.DepartureCode:
					return orderTrackingDatesMap.DepartureActualDate;

				case Events.ArrivalCode:
					return orderTrackingDatesMap.ArrivalActualDate;

				case Events.CargoAvailableCode:
					return orderTrackingDatesMap.CargoAvailableActualDate;

				case Events.DeliveryCartageAdvisedCode:
					return orderTrackingDatesMap.DeliveryCartageAdvisedActualDate;

				case Events.DeliveryCartageCompleteFinalisedCode:
					return orderTrackingDatesMap.DeliveryCartageCompleteFinalizedActualDate;
			}

			return null;
		}

		public void UpdateScheduledDateFromAttachedShipmentOrDeclaration(bool synchronizeWhenEmpty)
		{
			if (IsMilestoneOrWorkflowTrigger)
			{
				var orderTrackingDatesMap = CreateOrderTrackingDatesMap();

				if (orderTrackingDatesMap != null)
				{
					switch (P9_SE_NKMilestoneEvent)
					{
						case Events.DepartureCode:
							Update(P9_ScheduledDateInfo, synchronizeWhenEmpty, orderTrackingDatesMap.DepartureScheduledDate);
							break;

						case Events.ArrivalCode:
							Update(P9_ScheduledDateInfo, synchronizeWhenEmpty, orderTrackingDatesMap.ArrivalScheduledDate);
							break;

						case Events.DeliveryCartageCompleteFinalisedCode:
							Update(P9_ScheduledDateInfo, synchronizeWhenEmpty, orderTrackingDatesMap.DeliveryCartageCompleteFinalizedScheduledDate);
							break;
					}
				}
			}
		}

		void Update(ZPropertyInfo propertyInfo, bool synchronizeWhenEmpty, ZDateTime dateTime)
		{
			if (synchronizeWhenEmpty || !dateTime.IsEmpty)
			{
				propertyInfo.Value = dateTime;
			}
		}

		void UpdateCustomsActualDate(OrderTrackingDatesMap orderTrackingDatesMap, bool synchronizeWhenEmpty)
		{
			var result = orderTrackingDatesMap.FindLatestEventLogTime(P9_SE_NKMilestoneEvent);

			if (result.IsEmpty)
			{
				var tasks = Parent.WorkflowItems.Cast<OrderProcessTasks>().Where(x => x.IsMilestoneOrWorkflowTrigger);
				bool customsCode = tasks.All(x => x.P9_SE_NKMilestoneEvent != Events.ExportCustomsClearedCode && x.P9_SE_NKMilestoneEvent != Events.ExportCustomsCommencedCode);

				if (P9_SE_NKMilestoneEvent == Events.CustomsClearedCode && customsCode)
				{
					result = orderTrackingDatesMap.FindLatestEventLogTime(Events.ExportCustomsClearedCode);
				}
				else if (P9_SE_NKMilestoneEvent == Events.CustomsCommencedCode && customsCode)
				{
					result = orderTrackingDatesMap.FindLatestEventLogTime(Events.ExportCustomsCommencedCode);
				}
			}

			if (synchronizeWhenEmpty || !result.IsEmpty)
			{
				if (P9_ActualDateForBinding != result)
				{
					new ActualDateWorkAround().SetActualDateAndIKnowIShouldNotBeCallingThis(this, result);
				}
			}
		}
	}
}
