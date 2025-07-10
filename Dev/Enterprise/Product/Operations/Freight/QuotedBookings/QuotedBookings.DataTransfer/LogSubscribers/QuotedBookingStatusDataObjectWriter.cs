using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Event = Enterprise.ZArchitecture.Business.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	public class QuotedBookingStatusDataObjectWriter : ForwardingBookingDataObjectWriter
	{
		public QuotedBookingStatusDataObjectWriter(IDataWritingManager manager, QuotedBooking bookingBO)
			: base(manager, bookingBO)
		{
		}

		public Event Event { get; set; }
		public string ReasonForRejectionNoteText { get; set; }
		public string DataContextDocumentName { get; set; }
		public bool ShouldPopulateTransportLegCollection { get; set; }

		protected override void PopulateDataObject(ForwardingShipment shipmentBO, UniversalShipment shipmentDataObject)
		{
			base.PopulateDataObject(shipmentBO, shipmentDataObject);

			var purposeList = new CodeDescriptionPairList();
			purposeList.AddPair(Event.Code, Event.Description);
			shipmentDataObject.DataContext.SetDocumentaryOverride(DataContextDocumentName, Event.Code, purposeList, ZBool.False, 1, 1);

			shipmentDataObject.CoLoadMasterBillNumber = shipmentBO.JS_HouseBill;

			PopulateNoteCollection(shipmentDataObject);

			if (ShouldPopulateTransportLegCollection)
			{
				PopulateTransportLegCollection(shipmentBO, shipmentDataObject);
			}
		}

		void PopulateNoteCollection(UniversalShipment shipmentDataObject)
		{
			if (!string.IsNullOrEmpty(ReasonForRejectionNoteText))
			{
				var note = new Note
				{
					Description = (NoResString)"Reason for Rejection",   // Hard coded document name.
					NoteText = ReasonForRejectionNoteText,
					IsCustomDescription = false
				};

				if (shipmentDataObject.NoteCollection == null)
				{
					shipmentDataObject.SetNoteCollection(() => new DataObjectList<Note> { note });
				}
				else
				{
					shipmentDataObject.NoteCollection.Add(note);
				}
			}
		}

		void PopulateTransportLegCollection(ForwardingShipment shipmentBO, UniversalShipment shipmentDataObject)
		{
			if (shipmentBO.Sailing != null)
			{
				shipmentDataObject.SetTransportLegCollection(() =>
				{
					var transportLegDataObject = new TransportLeg
					{
						LegOrder = 1,
						TransportMode = new TransportModeConverter().ToEnumValue(shipmentBO.JS_TransportMode),
						LegType = LegType.Main,
						VesselName = shipmentDataObject.VesselName,
						VesselLloydsIMO = shipmentDataObject.LloydsIMO,
						VoyageFlightNo = shipmentDataObject.VoyageFlightNo,
						PortOfLoading = shipmentDataObject.PortOfLoading,
						PortOfDischarge = shipmentDataObject.PortOfDischarge,
					};

					var origin = shipmentBO.Sailing.Origin;
					if (origin != null)
					{
						transportLegDataObject.EstimatedDeparture = origin.JA_E_DEP;
						transportLegDataObject.ActualDeparture = origin.JA_A_DEP;
						transportLegDataObject.FCLCutOff = origin.JA_CutOff;
						transportLegDataObject.DocumentCutOff = origin.JA_DocumentaryCutoff;
						transportLegDataObject.VGMCutOff = origin.JA_VGMCutOff;
					}

					var destination = shipmentBO.Sailing.Destination;
					if (destination != null)
					{
						transportLegDataObject.EstimatedArrival = destination.JB_E_ARV;
						transportLegDataObject.ActualArrival = destination.JB_A_ARV;
					}

					transportLegDataObject.LCLCutOff = shipmentBO.Sailing.JX_DepotCutOff;
					transportLegDataObject.LCLReceivalCommences = shipmentBO.Sailing.JX_DepotReceivalCommences;
					return new DataObjectList<TransportLeg> { transportLegDataObject };
				});
			}
		}
	}
}
