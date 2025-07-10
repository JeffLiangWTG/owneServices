using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	[Serializable]
	public class SupplierBookingShipmentCreator : LogSubscriber
	{
		public override string Name => "SupplierBookingShipmentCreator";

		public override string[] EventTypes => new string[] { AutoEvents.StatusUpdated.Code };

		public override string[] TableNames => new string[] { JobSupplierBookingSchema.Constants.TableName };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (queuedLogs.Length == 0)
			{
				return;
			}

			var factory = queuedLogs[0].Factory;

			ProcessGeneratingPlanningShipmentEvents(queuedLogs, factory);
			ProcessConvertingLSEBookingEventsAndFinalizingCYBookingEvents(queuedLogs, factory);
		}

		void ProcessConvertingLSEBookingEventsAndFinalizingCYBookingEvents(IQueuedLog[] queuedLogs, BusinessObjectFactory factory)
		{
			var convertService = new OrderManagerConvertService();

			var matchedParentIDs = queuedLogs.Where(queuedLog => queuedLog.MatchReferenceParameter(EventConstants.EventReferenceParameters.Codes.New, SupplierBookingStatus.Shipped)).Select(log => log.SJ_ParentID).Distinct().ToArray();
			var bookingList = factory.Load<JobSupplierBooking>(new ZQuery(JobSupplierBookingSchema.PK, matchedParentIDs).AddToFilter(JobSupplierBookingSchema.JSB_Status, SupplierBookingStatus.Shipped));

			foreach (var booking in bookingList)
			{
				if (booking.JSB_LoadMode == SupplierBookingLoadModeList.Codes.LSE)
				{
					convertService.ConvertLooseCargoSupplierBooking(booking, factory);
				}
				else if (booking.JSB_LoadMode == SupplierBookingLoadModeList.Codes.CY)
				{
					if (booking.ValidateShipmentConsolLink())
					{
						convertService.FinalizingSupplierBookingShipmentCreation(booking);
					}
					else
					{
						booking.Logs.RaisePlannedShipmentValidationException();
						booking.JSB_Status = SupplierBookingStatusList.Codes.PLN;
					}
				}
			}
		}

		static void ProcessGeneratingPlanningShipmentEvents(IQueuedLog[] queuedLogs, BusinessObjectFactory factory)
		{
			var convertService = new OrderManagerConvertService();

			var matchedParentIDs = queuedLogs.Where(queuedLog => StmALog.GetParametersFromReference(queuedLog.SJ_Reference).TryGetValue(EventConstants.EventReferenceParameters.Codes.Type, out var newParameter) && newParameter == "Shipment Planning Complete").Select(log => log.SJ_ParentID).Distinct().ToArray();
			var bookingList = factory.Load<JobSupplierBooking>(new ZQuery(JobSupplierBookingSchema.PK, matchedParentIDs));

			bookingList.ForEach(convertService.ConvertOrderShipmentPlanning);
		}
	}
}
