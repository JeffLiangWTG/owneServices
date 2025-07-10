using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business
{
	public class BookingToTransportJobProcessor : IProcessor
	{
		public BookingToTransportJobProcessor(DtbBooking booking, ProcessTaskNotification action)
		{
			this.booking = booking;
			this.action = action;
		}

		readonly DtbBooking booking;
		readonly ProcessTaskNotification action;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			if (booking != null && !((ICancellable)booking).IsCancelled)
			{
				try
				{
					string targetModule;
					switch (action.PQ_TriggerParty)
					{
						case MessageRecipientPartyTypeList.Codes.TransportJobRegistry:
							var containerModes = BookingToTransportJobCommonCreator.TargetModuleDataObjectWriter.GetBookingContainerModeForTargetModule(booking);
							var targetModules = containerModes.Select(m => GetRegistryTargetModule(m));
							targetModule = targetModules.Distinct().Count() == 1 ? targetModules.First().ToString() : AutoCreatorTargetModules.Codes.PortTransport;
							break;

						default:
							return;
					}

					CreateQueueRecord(GlbBranch.CurrentBranch.GB_Code, targetModule);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notifications.AddError(ex.Message);
					throw;
				}
			}
			else if (booking != null && ((ICancellable)booking).IsCancelled)
			{
				var message = Res.GetString("8a5c99ba-8d7f-462d-a6ff-3e0ffecfa42d", "{0} is deactivated and cannot create new Transport Jobs.", booking.HumanReadableName);
				notifications.AddWarning(message);
			}
		}

		void CreateQueueRecord(string branchCode, string targetModule)
		{
			var queueRecord = booking.Factory.New<DtbBookingQueue>();
			queueRecord.KMQ_ParentID = booking.PK;
			queueRecord.KMQ_ParentTableCode = TableCode;
			queueRecord.KMQ_GB_NKBranch = branchCode;
			queueRecord.KMQ_Direction = "";
			queueRecord.KMQ_CombineContainers = false;
			queueRecord.KMQ_TargetModule = targetModule;
			queueRecord.Iteration = 0;
		}

		string TableCode
		{
			get
			{
				return booking?.TablePrefix;
			}
		}

		ZString GetRegistryTargetModule(ZString containerMode)
		{
			return TransportRegistry.Instance.ServiceTaskCreatorOption.Value.GetTargetModule(containerMode);
		}
	}
}
