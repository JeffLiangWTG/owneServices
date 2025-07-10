using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer
{
	class PortMessagingConsolDataObjectWriter : ConsolDataObjectWriter
	{
		public PortMessagingConsolDataObjectWriter(IDataWritingManager manager, PortMessagingManager.MessageType messageType, string purpose)
			: base(manager)
		{
			Argument.NotNullOrEmpty(purpose, "purpose");

			this.Purpose = purpose;
			this.messageType = messageType;
		}

		public PortMessagingConsolDataObjectWriter(IDataWritingManager manager, IContainerLinkManager<ForwardingConsol> linkManager, DataWriterOptions dataWriterOptions, PortMessagingManager.MessageType messageType, string purpose)
			: base(manager, linkManager, dataWriterOptions)
		{
			Argument.NotNullOrEmpty(purpose, "purpose");

			this.Purpose = purpose;
			this.messageType = messageType;
		}

		readonly string Purpose;
		readonly PortMessagingManager.MessageType messageType;

		protected override void PopulateDataObject(ForwardingConsol consolBO, UniversalShipment shipmentData)
		{
			base.PopulateDataObject(consolBO, shipmentData);

			shipmentData.PortMessaging = new UniversalDataBuss.DataObjects.Universal.PortMessaging();

			var portMessaging = new ConsolPortMessagingData(consolBO);

			shipmentData.PortMessaging.MessagePurpose = Purpose;
			shipmentData.PortMessaging.Date = portMessaging.Date;
			shipmentData.PortMessaging.Berth = portMessaging.Berth;
			shipmentData.PortMessaging.OperatorName = portMessaging.Operator;
			shipmentData.PortMessaging.Telephone = portMessaging.OperatorPhone;
			shipmentData.PortMessaging.Fax = portMessaging.OperatorFax;
			shipmentData.PortMessaging.Email = portMessaging.OperatorEmail;
			shipmentData.PortMessaging.ShippingLine = portMessaging.ShippingLine;

			shipmentData.PortMessaging.SenderAgentCode = portMessaging.SenderCode;
			shipmentData.PortMessaging.SenderAgentName = portMessaging.SenderName;
			shipmentData.PortMessaging.SendingAgentQuayAccount = portMessaging.AccountNo;
			shipmentData.PortMessaging.VesselName = portMessaging.VesselName;
			shipmentData.PortMessaging.BillNo = portMessaging.BillNo;
			shipmentData.PortMessaging.Departure = portMessaging.Departure;
			shipmentData.PortMessaging.VoyageNo = portMessaging.VoyageNo;
			shipmentData.PortMessaging.Destination = portMessaging.Destination;

			var notes = consolBO.Notes.FindByDescription(PredefinedNoteTypes.Instance.PortMessageRemarks.Description);
			if (notes.Any())
			{
				shipmentData.PortMessaging.Remarks = notes.First().ST_NoteDataAsText;
			}
			shipmentData.PortMessaging.ShipperCode = portMessaging.ShipperCode;
			shipmentData.PortMessaging.ShipperName = portMessaging.ShipperName;
			shipmentData.PortMessaging.ShipperQuayAccount = portMessaging.ShipperPortAccount;

			if (!string.IsNullOrEmpty(CancellationNote))
			{
				if (shipmentData.NoteCollection != null || shipmentData.SetNoteCollection(() => new DataObjectList<Note>()))
				{
					var note = new Note();
					note.Description = CancellationReasonNoteType;
					note.NoteText = CancellationNote;
					note.Visibility = new CodeDescriptionPair { Code = nameof(StmNoteVisibility.PRV), Description = StmNoteDescription.Prv };

					shipmentData.NoteCollection.Add(note);
				}
			}
		}

		protected override DataObjectList<UniversalShipment> PopulateSubShipmentCollection(ForwardingConsol consolBO)
		{
			var data = ProcessCollection(consolBO.TopLevelShipments, new PortMessagingShipmentDataObjectForConsolWriter(writeManager, false, false, messageType, Purpose));
			return data != null ? new DataObjectList<UniversalShipment>(data) : null;
		}

		const string CancellationReasonNoteType = "DAKOSYPortMessageCancellationType";
		const string CancellationBecauseOfErrors = "CancellationBecauseOfErrors";
		const string CancellationOnExit = "CancellationOnExit";
		const string ForwardingCancellation = "ForwardingCancellation";

		string CancellationNote
		{
			get
			{
				if (cancellationNote == null)
				{
					switch (messageType)
					{
						case PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors:
							cancellationNote = CancellationBecauseOfErrors;
							break;
						case PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit:
							cancellationNote = CancellationOnExit;
							break;
						case PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation:
							cancellationNote = ForwardingCancellation;
							break;
						default:
							cancellationNote = string.Empty;
							break;
					}
				}
				return cancellationNote;
			}
		}

		string cancellationNote;
	}
}
