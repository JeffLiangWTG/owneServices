using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer
{
	public class ShipmentEventDataObjectWriter : EventDataObjectWriter
	{
		public ShipmentEventDataObjectWriter(IDataWritingManager writeManager, CommonShipment shipment, IDictionary<string, string> contextMappings)
			: base(writeManager)
		{
			Parent = shipment;
			CarrierC1CCode = contextMappings[nameof(Event.ContextTypes.CarrierC1CCode)];
			CarriersBookingReference = contextMappings[nameof(Event.ContextTypes.CarriersBookingReference)] ?? shipment?.JS_UniqueConsignRef;
			MBOLNumber = contextMappings[nameof(Event.ContextTypes.MBOLNumber)] ?? shipment?.JS_HouseBill;
			ContainerNumber = contextMappings[nameof(Event.ContextTypes.ContainerNumber)];
			ContainerISOCode = contextMappings[nameof(Event.ContextTypes.ContainerISOCode)];
			VesselName = contextMappings[nameof(Event.ContextTypes.VesselName)];
			LloydsNumber = contextMappings[nameof(Event.ContextTypes.LloydsNumber)];
			VoyageNumber = contextMappings[nameof(Event.ContextTypes.VoyageNumber)];
			LegOriginUNLOCO = contextMappings[nameof(Event.ContextTypes.LegOriginUNLOCO)];
			LegDestinationUNLOCO = contextMappings[nameof(Event.ContextTypes.LegDestinationUNLOCO)];
			MBOLOriginUNLOCO = contextMappings[nameof(Event.ContextTypes.MBOLOriginUNLOCO)] ?? shipment?.JS_RL_NKOrigin;
			MBOLDestinationUNLOCO = contextMappings[nameof(Event.ContextTypes.MBOLDestinationUNLOCO)] ?? shipment?.JS_RL_NKDestination;
			EventSource = contextMappings[ShipmentEventContextType.EventSource];
		}

		public ZString SubscriptionReference { get; set; }
		public ZString CarrierC1CCode { get; set; }
		public ZString CarriersBookingReference { get; set; }
		public ZString MBOLNumber { get; set; }
		public ZString ContainerNumber { get; set; }
		public ZString ContainerISOCode { get; set; }
		public ZString VesselName { get; set; }
		public ZString LloydsNumber { get; set; }
		public ZString VoyageNumber { get; set; }
		public ZString LegOriginUNLOCO { get; set; }
		public ZString LegDestinationUNLOCO { get; set; }
		public ZString MBOLOriginUNLOCO { get; set; }
		public ZString MBOLDestinationUNLOCO { get; set; }
		public ZString EventSource { get; set; }

		protected CommonShipment Parent { get; private set; }

		protected override void PopulateDataObject(BaseStmALog logBO, Event logData)
		{
			logData.EventTime = logBO.EventTimeOffset;
			logData.EventType = logBO.SL_SE_NKEvent;
			logData.IsEstimate = logBO.SL_IsEstimate;

			PopulateEventReference(logBO, logData);
			PopulateContextCollection(logBO, logData);
		}

		void PopulateEventReference(BaseStmALog logBO, Event logData)
		{
			var eventParameters = new EventParameters();

			foreach (var pair in logBO.Parameters)
			{
				var property = EventParameters.GetPropertyByCode(pair.Key);
				if (property != null)
				{
					var parameterValue = StmALog.ParseParameterValue(pair, Nullable.GetUnderlyingType(property.PropertyType));
					property.SetValue(eventParameters, parameterValue, null);
				}
			}

			logData.EventParameters = eventParameters;
		}

		void PopulateContextCollection(BaseStmALog logBO, Event logData)
		{
			var sbrReference = StmALog.GetParametersFromReference(SubscriptionReference);

			logData.ContextCollection = new List<Context>();

			var reference = GetParameterValue(sbrReference, "ITN");
			if (!reference.IsNullOrEmpty())
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = ShipmentEventContextType.Reference },
					Value = reference
				});
			}

			CarrierC1CCode = GetParameterValue(sbrReference, "ORG");
			if (!CarrierC1CCode.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.CarrierC1CCode) },
					Value = CarrierC1CCode
				});
			}

			if (!CarriersBookingReference.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.CarriersBookingReference) },
					Value = CarriersBookingReference
				});
			}

			if (!MBOLNumber.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.MBOLNumber) },
					Value = MBOLNumber
				});
			}

			if (!ContainerNumber.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.ContainerNumber) },
					Value = ContainerNumber
				});
			}

			if (!ContainerISOCode.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.ContainerISOCode) },
					Value = ContainerISOCode
				});
			}

			if (!VesselName.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.VesselName) },
					Value = VesselName
				});
			}

			if (!LloydsNumber.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.LloydsNumber) },
					Value = LloydsNumber
				});
			}

			if (!VoyageNumber.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.VoyageNumber) },
					Value = VoyageNumber
				});
			}

			if (!LegOriginUNLOCO.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.LegOriginUNLOCO) },
					Value = LegOriginUNLOCO
				});
			}

			if (!LegDestinationUNLOCO.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.LegDestinationUNLOCO) },
					Value = LegDestinationUNLOCO
				});
			}

			if (!MBOLOriginUNLOCO.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.MBOLOriginUNLOCO) },
					Value = MBOLOriginUNLOCO
				});
			}

			if (!MBOLDestinationUNLOCO.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = nameof(Event.ContextTypes.MBOLDestinationUNLOCO) },
					Value = MBOLDestinationUNLOCO
				});
			}

			if (!EventSource.IsEmpty)
			{
				logData.ContextCollection.Add(new Context
				{
					Type = new ContextType { Type = ShipmentEventContextType.EventSource },
					Value = EventSource
				});
			}
		}

		string GetParameterValue(ObservableDictionary<string, string> logParameters, string key)
		{
			var result = string.Empty;
			logParameters?.TryGetValue(key, out result);

			return result;
		}
	}
}
