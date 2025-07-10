using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ContainerAutomationConsolEventDataObjectWriter : EventDataObjectWriter
	{
		public ContainerAutomationConsolEventDataObjectWriter(IDataWritingManager writeManager, ITransportParent parent)
			: base(writeManager)
		{
			Parent = parent;
		}

		public ZString ContainerMode { get; internal set; }

		public bool AcceptsOnlyProvidedContainers { get; internal set; }

		public ZString CoLoadBookingReference { get; internal set; }

		public ZString CoLoadWithCode { get; internal set; }

		public ZString CoLoadWithC1CCode { get; internal set; }

		public ZString CoLoadWithName { get; internal set; }

		public ZString CoLoadBillNumber { get; internal set; }

		public ZString TransportMode { get; internal set; }

		protected ITransportParent Parent { get; private set; }

		protected override void PopulateDataObject(BaseStmALog logBO, Event logData)
		{
			base.PopulateDataObject(logBO, logData);

			var orderedTransports = new TransportOrderHelper(Parent.Transports.OfType<Transport>().ToArray());
			var lastSeaLeg = orderedTransports.LastLegMatching(leg => leg.IsSea);
			foreach (var transport in orderedTransports)
			{
				if (lastSeaLeg != null && transport.PK == lastSeaLeg.PK)
				{
					logData.ContextCollection.Add(GetContext(lastSeaLeg, Parent));
				}
				else
				{
					logData.ContextCollection.Add(GetContext(transport));
				}
			}

			if (Parent is IContainerParent containerParent)
			{
				var shippingLine = containerParent.ShippingLine;
				if (shippingLine != null)
				{
					var context = new Context
					{
						Type = new ContextType { Type = ContainerAutomationEventContextType.Carrier },
						Value = shippingLine.OH_FullName,
					};

					logData.ContextCollection.Add(context);
				}
			}

			logData.ContextCollection.Add(new Context
			{
				Type = new ContextType { Type = ContainerAutomationEventContextType.AcceptsOnlyProvidedContainers },
				Value = AcceptsOnlyProvidedContainers.Serialize()
			});

			logData.ContextCollection.Add(new Context
			{
				Type = new ContextType { Type = ContainerAutomationEventContextType.ContainerMode },
				Value = ContainerMode
			});

			logData.ContextCollection.Add(new Context
			{
				Type = new ContextType { Type = ContainerAutomationEventContextType.TransportMode },
				Value = TransportMode
			});

			var hasCoLoadCbr = !string.IsNullOrWhiteSpace(CoLoadBookingReference);
			var hasCoLoadMbn = !string.IsNullOrWhiteSpace(CoLoadBillNumber);
			var hasCoLoadWithCode = !string.IsNullOrWhiteSpace(CoLoadWithCode);
			var hasCoLoadWithName = !string.IsNullOrWhiteSpace(CoLoadWithName);
			var hasCoLoadWithC1CCode = !string.IsNullOrWhiteSpace(CoLoadWithC1CCode);

			// CoLoad info is only put into UniversalEvent if both CoLoadWith and one of CoLoadMBN/CoLoadCBR are present
			if ((hasCoLoadCbr || hasCoLoadMbn) && (hasCoLoadWithCode || hasCoLoadWithName || hasCoLoadWithC1CCode))
			{
				if (hasCoLoadCbr)
				{
					logData.ContextCollection.Add(new Context
					{
						Type = new ContextType { Type = ContainerAutomationEventContextType.CoLoadBookingReference },
						Value = CoLoadBookingReference
					});
				}

				if (hasCoLoadMbn)
				{
					logData.ContextCollection.Add(new Context
					{
						Type = new ContextType { Type = ContainerAutomationEventContextType.CoLoadBillNumber },
						Value = CoLoadBillNumber
					});
				}

				if (hasCoLoadWithCode)
				{
					logData.ContextCollection.Add(new Context
					{
						Type = new ContextType { Type = ContainerAutomationEventContextType.CoLoadWithCode },
						Value = CoLoadWithCode
					});
				}

				if (hasCoLoadWithC1CCode)
				{
					logData.ContextCollection.Add(new Context
					{
						Type = new ContextType { Type = ContainerAutomationEventContextType.CoLoadWithC1CCode },
						Value = CoLoadWithC1CCode
					});
				}

				if (hasCoLoadWithName)
				{
					logData.ContextCollection.Add(new Context
					{
						Type = new ContextType { Type = ContainerAutomationEventContextType.CoLoadWithName },
						Value = CoLoadWithName
					});
				}
			}
		}

		static Context GetContext(Transport transport, ITransportParent parent = null)
		{
			var context = new Context
			{
				Type = new ContextType { Type = ContainerAutomationEventContextType.TransportLeg },
				Value = transport.JW_LegOrder.ToString(),
				SubContextCollection = new List<Context>()
			};
			var etd = transport.JW_ETD;
			var eta = transport.JW_ETA;

			context.SubContextCollection.Add(GetContext(Event.ContextTypes.TransportMode, transport.JW_TransportMode));

			if (!eta.IsEmpty)
			{
				context.SubContextCollection.Add(GetContext(Event.ContextTypes.EstimatedTimeOfArrival, eta.ToISO8601String()));
			}

			if (!etd.IsEmpty)
			{
				context.SubContextCollection.Add(GetContext(Event.ContextTypes.EstimatedTimeOfDeparture, etd.ToISO8601String()));
			}

			var portOfLoadingUnloco = transport.JW_RL_NKLoadPort;
			var portOfDischargeUnloco = transport.JW_RL_NKDiscPort;

			if (!string.IsNullOrWhiteSpace(portOfLoadingUnloco))
			{
				context.SubContextCollection.Add(GetContext(Event.ContextTypes.LegOriginUNLOCO, portOfLoadingUnloco));
			}

			if (!string.IsNullOrWhiteSpace(portOfDischargeUnloco))
			{
				context.SubContextCollection.Add(GetContext(Event.ContextTypes.LegDestinationUNLOCO, portOfDischargeUnloco));
			}

			if (transport.TransportMode == Core.Constants.TransportModes.Sea)
			{
				var vesselImo = transport.Vessel?.RV_LloydsNumber ?? string.Empty;
				if (!vesselImo.IsEmpty)
				{
					context.SubContextCollection.Add(GetContext(Event.ContextTypes.LloydsNumber, vesselImo));
				}

				var vesselName = transport.JW_Vessel;
				if (!vesselName.IsEmpty)
				{
					context.SubContextCollection.Add(GetContext(Event.ContextTypes.VesselName, vesselName));
				}

				var voyageNumber = transport.JW_VoyageFlight;
				if (!voyageNumber.IsEmpty)
				{
					context.SubContextCollection.Add(GetContext(Event.ContextTypes.VoyageNumber, voyageNumber));
				}

				if (parent != null && parent is IContainerParent)
				{
					var arrivalCTOAddress = (parent as IContainerParent).ArrivalCTOAddress;

					var legDestinationTerminalCode = arrivalCTOAddress?.Header?.OrgRefFacilities?
						.Where(c => c.OFC_OA_PremisesAddress == arrivalCTOAddress.PK)?
						.FirstOrDefault()?.Facility?.RFT_Code;

					if (!legDestinationTerminalCode.HasValue)
					{
						legDestinationTerminalCode = arrivalCTOAddress?.Header?.OrgRefFacilities?
							.Select(c => c.Facility)?
							.Where(c => c.RFT_FacilityType == Core.Constants.FacilityType.Code.Terminal &&
										 c.RFT_RL_NKLocationCode == transport.JW_RL_NKDiscPort)?
							.FirstOrDefault()?.RFT_Code;
					}

					if (legDestinationTerminalCode.HasValue)
					{
						context.SubContextCollection.Add(GetContext(Event.ContextTypes.LegDestinationTerminalCode, legDestinationTerminalCode.Value));
					}
				}
			}

			return context;
		}

		static Context GetContext(Event.ContextTypes contextType, ZString contextValue)
		{
			return new Context
			{
				Type = new ContextType { Type = contextType.ToString() },
				Value = contextValue
			};
		}
	}
}
