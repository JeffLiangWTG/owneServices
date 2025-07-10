using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	class UniversalCMMProcessingAdapter : CMMProcessingAdapter
	{
		public UniversalCMMProcessingAdapter(BusinessObjectFactory factory, UniversalEvent dataObject)
		{
			Factory = Argument.NotNull(factory, "factory");
			eventDataObject = Argument.NotNull(dataObject, "dataObject");
		}

		protected override void LoadCore()
		{
			var addressType = AddressHelper.DefaultShipperAddressType(MessageType);
			senderAddress = AddressHelper.FindOrganisationAddressByEHubCode(Factory, addressType, MessageSenderCode);
		}

		public override EDIMessage Message
		{
			get { return null; }
		}

		public override void AttachMessage(ContainerMovement movement, string status)
		{
			var message = Factory.New<ContainerManagementEDIMessage>();
			message.MessageNumberStrategy = new ReceivingContainerManagementNumberStrategy(Factory, MessageSenderCode);
			message.EM_MessageText = MessageText;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageType = ContainerManagementEDIMessage.ContainerManagementMessageType;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.ContainerMovements;

			if (!string.IsNullOrWhiteSpace(MessageSenderCode))
			{
				message.EM_MessageOwner = MessageSenderCode.Length > EDIMessageSchema.EM_MessageOwner.MaxLength ?
					MessageSenderCode.Substring(0, EDIMessageSchema.EM_MessageOwner.MaxLength) :
					MessageSenderCode;
			}

			message.EM_Status = status;
			movement.Messages.Add(message);
		}

		UniversalEvent EventDataObject
		{
			get { return eventDataObject; }
		}
		readonly UniversalEvent eventDataObject;

		IXmlEventValueObject EventValueObject
		{
			get { return EventDataObject; }
		}

		public override string LloydsNumber
		{
			get { return EventValueObject.Context.LloydsNumber.GetValueOrDefault(); }
		}

		public override string VoyageNumber
		{
			get { return EventValueObject.Context.VoyageNumber.GetValueOrDefault(); }
		}

		public override string TransportMode
		{
			get { return EventDataObject?.EventParameters?.TransportMode.GetValueOrDefault(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event message only")]
		public override string MessageText
		{
			get
			{
				var builder = new StringBuilder();

				if (!EventValueObject.EventType.IsEmpty)
				{
					builder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"Event Type: {0}'", EventValueObject.EventType));
				}

				if (!EventValueObject.EventReference.IsEmpty)
				{
					builder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"Event Reference: {0}'", EventValueObject.EventReference));
				}

				if (!EventValueObject.EventTime.IsEmpty)
				{
					builder.Append(string.Format(CultureInfo.InvariantCulture, "Event Time: {0}'", EventValueObject.EventTime.ToZDateTime().ToStandardDateTimeString()));
				}

				if (!EventValueObject.CreatedTime.IsEmpty)
				{
					builder.Append(string.Format(CultureInfo.InvariantCulture, "Created Time: {0}'", EventValueObject.CreatedTime.ToZDateTime().ToStandardDateTimeString()));
				}

				var messageItems = MessageSourceItemRetriever.GetSourceItems(Factory, EventValueObject);
				foreach (KeyDataPair messageItem in messageItems)
				{
					builder.Append(string.Format(CultureInfo.InvariantCulture, "{0} - {1}'", messageItem.Key, messageItem.Data));
				}
				return builder.ToString();
			}
		}

		public override CMMMessageType MessageType
		{
			get
			{
				if (EventDataObject.EventType.HasValue)
				{
					switch (EventDataObject.EventType.ToString())
					{
						case Events.FreightLoadedCode:
							return CMMMessageType.Load;
						case Events.FreightUnloadedCode:
							return CMMMessageType.Discharge;
						case Events.GateInCode:
							return CMMMessageType.GateIn;
						case Events.GateOutCode:
							return CMMMessageType.GateOut;
					}
				}
				return CMMMessageType.Unknown;
			}
		}

		public override string MessageSenderCode
		{
			get { return EventValueObject.Context.DepotCode; }
		}

		public override CMMOrganisationType MessageSenderCodeType
		{
			get { return CMMOrganisationType.EHubOrganisationCode; }
		}

		public override OrgAddress SenderAddress
		{
			get { return senderAddress; }
		}
		OrgAddress senderAddress;

		public override List<CMMMessageContainer> Containers
		{
			get
			{
				if (containers == null && IsLoaded)
				{
					containers = GetContainers();
				}
				return containers ?? new List<CMMMessageContainer>();
			}
		}
		List<CMMMessageContainer> containers;

		List<CMMMessageContainer> GetContainers()
		{
			List<CMMMessageContainer> result = new List<CMMMessageContainer>();
			var eventContext = EventValueObject.Context;
			var containerNumbers = eventContext.ContainerNumbers != null && eventContext.ContainerNumbers.Any()
										? eventContext.ContainerNumbers
										: new List<ZString> { "" }; // existing functionality: still create container if no Container #
			containerNumbers.ForEach(containerNumber => CreateContainerForEvent(EventValueObject, containerNumber, result));
			return result;
		}

		static void CreateContainerForEvent(IXmlEventValueObject eventValueObject, string containerNumber, List<CMMMessageContainer> containers)
		{
			var eventContext = eventValueObject.Context;
			var container = new CMMMessageContainer
			{
				BillOfLading = eventContext.MBOLNumber.GetValueOrDefault(),
				BookingReference = eventContext.CarriersBookingReference,
				ContainerNumber = containerNumber,
				IsEmpty = eventContext.IsEmptyContainer,
				ISOType = eventContext.ContainerISOCode,
				OwnerType = eventContext.ContainerOwnershipType,
				GoodsDeclarationNumber = eventContext.GoodsDeclarationNumber,
				ContainerLeaseNumber = eventContext.ContainerLeaseNumber
			};

			CMMEquipmentSupplier supplierValue;
			if (CMMEquipmentSupplier.TryParse(eventContext.ContainerOwnershipType, out supplierValue))
			{
				container.CmmEquipmentSupplier = supplierValue;
			}
			else
			{
				container.CmmEquipmentSupplier = CMMEquipmentSupplier.Unknown;
			}

			DateTime parsedDateTime;
			container.PositioningDateTime = DateTime.TryParse(eventContext.PositioningDateTime, out parsedDateTime) ? parsedDateTime : eventValueObject.EventTime.ToDateTime();
			decimal parsedWeight;
			container.GrossWeightKG = Decimal.TryParse(eventContext.ContainerGrossWeight, out parsedWeight) ? parsedWeight : null;

			// SealNumbers
			var sealNumbers = new List<string>();
			if (eventContext.ContainerSealNo != string.Empty)
			{
				sealNumbers.Add(eventContext.ContainerSealNo);
			}

			if (eventContext.ContainerSealNo2 != string.Empty)
			{
				sealNumbers.Add(eventContext.ContainerSealNo2);
			}

			if (eventContext.ContainerSealNo3 != string.Empty)
			{
				sealNumbers.Add(eventContext.ContainerSealNo3);
			}

			container.SealNumbers = sealNumbers;
			if (container.HasValue())
			{
				containers.Add(container);
			}
		}
	}
}




