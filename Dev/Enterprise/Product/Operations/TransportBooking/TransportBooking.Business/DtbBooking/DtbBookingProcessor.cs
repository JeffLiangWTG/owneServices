using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingProcessor : IProcessor
	{
		public DtbBookingProcessor(IDtbBookingParent parent, ProcessTaskNotification action)
		{
			bookingParent = parent;
			this.action = action;
		}

		readonly IDtbBookingParent bookingParent;
		readonly ProcessTaskNotification action;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			if (bookingParent != null && !((ICancellable)bookingParent).IsCancelled)
			{
				try
				{
					var combineContainers = action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking;
					DtbBookingDirection direction;
					switch (action.PQ_TriggerParty)
					{
						case MessageRecipientPartyTypeList.Codes.PickupCartage:
							direction = DtbBookingDirection.PIC;
							break;

						case MessageRecipientPartyTypeList.Codes.DeliveryCartage:
							direction = DtbBookingDirection.DLV;
							break;

						default:
							return;
					}

					CreateQueueRecord(GlbBranch.CurrentBranch.GB_Code, direction, combineContainers);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notifications.AddError(ex.Message);
					throw;
				}
			}
			else if (bookingParent != null && ((ICancellable)bookingParent).IsCancelled)
			{
				var message = Res.GetString("3dc48fc8-f279-46f1-affe-495628e1a8b9", "{0} is deactivated and cannot create new Transport Bookings.", bookingParent.HumanReadableName);
				notifications.AddWarning(message);
			}
		}

		void CreateQueueRecord(string branchCode, DtbBookingDirection direction, bool combineContainers)
		{
			var queueRecord = bookingParent.Factory.New<DtbBookingQueue>();
			queueRecord.KMQ_ParentID = bookingParent.PK;
			queueRecord.KMQ_ParentTableCode = TableCode;
			queueRecord.KMQ_GB_NKBranch = branchCode;
			queueRecord.KMQ_Direction = direction.ToString();
			queueRecord.KMQ_CombineContainers = combineContainers;
			queueRecord.Iteration = 0;
		}

		string TableCode
		{
			get
			{
				return bookingParent?.TablePrefix;
			}
		}
	}
}
