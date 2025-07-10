using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer
{
	[Serializable]
	public class ShipmentEventLogSubscriber : LogSubscriber
	{
		public override string Name => "ShipmentEventLogSubscriber";

		public override string[] EventTypes
		{
			get
			{
				return GetEventTypes();
			}
		}

		public override string[] TableNames => new[]
		{
			JobShipmentSchema.Constants.TableName, ViewQuotedBookingSchema.Constants.TableName, JobContainerSchema.Constants.TableName, JobConsolTransportSchema.Constants.TableName
		};

		public override bool HasDynamicProperties => true;

		bool IsTrackingEnabled => FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive;

		bool IsRegistryHasValue
		{
			get
			{
				return FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.ServiceEhubIDs.Count > 0;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Compare text reference")]
		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (!(IsTrackingEnabled && IsRegistryHasValue))
			{
				return;
			}

			var containerLogs = queuedLogs.Where(_ => _.SJ_ParentTableCode == JobContainerSchema.Constants.Prefix && _.SJ_IsEstimate == false).ToList();
			var transportLogs = queuedLogs.Where(_ => _.SJ_ParentTableCode == JobConsolTransportSchema.Constants.Prefix).ToList();
			var shipmentLogs = queuedLogs.Where(_ =>
													(_.SJ_ParentTableCode == JobShipmentSchema.Constants.Prefix || _.SJ_ParentTableCode == ViewQuotedBookingSchema.Constants.Prefix) &&
													_.SJ_IsEstimate == false &&
													!_.SJ_Reference.StartsWith("Propagated:", StringComparison.OrdinalIgnoreCase)).ToList();

			foreach (var shipmentLog in shipmentLogs)
			{
				ShipmentEventLogProcessors.ForEach(processor => processor.ProcessShipmentLogs(shipmentLog, containerLogs));
			}

			foreach (var transportLog in transportLogs)
			{
				ShipmentEventLogProcessors.ForEach(processor => processor.ProcessTransportLogs(transportLog));
			}

			foreach (var containerLog in containerLogs)
			{
				ShipmentEventLogProcessors.ForEach(processor => processor.ProcessContainerLogs(containerLog));
			}
		}

		IEnumerable<ShipmentEventLogProcessor> ShipmentEventLogProcessors
		{
			get { return shipmentEventLogProcessors ?? (shipmentEventLogProcessors = ((IEnumerable)ObjectFactory.Get("ShipmentEventLogProcessors")).Cast<ShipmentEventLogProcessor>()); }
		}

		IEnumerable<ShipmentEventLogProcessor> shipmentEventLogProcessors;

		public string[] GetEventTypes()
		{
			var eventVisibilityOverrides = WebDataRegistry.Instance.EventVisibilityOverride.Value;
			var shipmentCode = eventVisibilityOverrides
				.OfType<EventVisibilityOverride>()
				.FirstOrDefault(evo => evo.Code == "SHP")?
				.EventVisibilityOverrideSettings;

			if (shipmentCode == null || !shipmentCode.Any())
			{
				return Array.Empty<string>();
			}

			var listOfEvents = shipmentCode.
				Cast<EventVisibilityOverrideSetting>().Where(setting => setting.IsSystem && setting.IsActive)
				.Select(setting => setting.EventCode.ToString())
				.ToArray();

			return listOfEvents;
		}
	}
}
