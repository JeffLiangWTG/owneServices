using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer
{
	[Serializable]
	public sealed class ShipmentStatusUpdatedLogSubscriber : LogSubscriber
	{
		public override string Name => "ShipmentStatusUpdatedLogSubscriber";

		public override string FriendlyName => (NoResString)"Shipment Status Updated Log Subscriber";

		public override string[] EventTypes => new[] { AutoEvents.StatusUpdated.Code };

		public override string[] TableNames => new[] { JobShipmentSchema.Constants.TableName, ViewQuotedBookingSchema.Constants.TableName };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (var log in queuedLogs)
			{
				ShipmentStatusUpdatedLogProcessors.ForEach(processor => processor.Process(DefaultLogger, log));
			}
		}

		IEnumerable<ShipmentStatusUpdatedLogProcessor> ShipmentStatusUpdatedLogProcessors
		{
			get { return shipmentStatusUpdatedLogProcessors ?? (shipmentStatusUpdatedLogProcessors = ((IEnumerable)ObjectFactory.Get("ShipmentStatusUpdatedLogProcessors")).Cast<ShipmentStatusUpdatedLogProcessor>()); }
		}
		IEnumerable<ShipmentStatusUpdatedLogProcessor> shipmentStatusUpdatedLogProcessors;
	}
}
