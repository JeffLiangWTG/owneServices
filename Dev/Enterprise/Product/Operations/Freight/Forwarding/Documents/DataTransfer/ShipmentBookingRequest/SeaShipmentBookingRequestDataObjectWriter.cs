using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalDataObjects = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	sealed class SeaShipmentBookingRequestDataObjectWriter : DataObjectWriter<SeaShipmentBookingRequest, UniversalDataObjects.Shipment>
	{
		public SeaShipmentBookingRequestDataObjectWriter(IDataWritingManager writeManager, DocumentVisualizer.Core.IDocument document = null)
			: base(writeManager)
		{
			this.document = document;
		}

		readonly DocumentVisualizer.Core.IDocument document;

		protected override UniversalDataObjects.Shipment PopulateDataObject(SeaShipmentBookingRequest seaShipmentBookingRequest)
		{
			var uxmlShipment = new UniversalDataObjects.Shipment(writeManager.WriterStrategy);

			uxmlShipment.DataContext = seaShipmentBookingRequest.CreateUXmlDataContext();

			var dataContext = (UniversalDataObjects._2012_11.DataContext)uxmlShipment.DataContext;
			dataContext.DocumentaryOverride = new DocumentaryOverride
			{
				DocumentName = "BookingRequest"
			};

			uxmlShipment.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.CoLoadMaster, Description = Core.Constants.ShipmentTypeDescriptions.CoLoadMaster };
			uxmlShipment.CoLoadBookingConfirmationReference = seaShipmentBookingRequest.BookingReference;
			uxmlShipment.ContainerMode = seaShipmentBookingRequest.ContainerMode?.ToUXmlContainerMode();
			uxmlShipment.DeliveryMode = Extensions.CreateUXmlDeliveryMode(seaShipmentBookingRequest.IsDoorPickup, seaShipmentBookingRequest.IsDoorDelivery);
			uxmlShipment.TotalNoOfPacks = seaShipmentBookingRequest.TotalPacks;
			uxmlShipment.TotalVolume = seaShipmentBookingRequest.TotalCargoVolume?.Value;
			uxmlShipment.TotalVolumeUnit = seaShipmentBookingRequest.TotalCargoVolume?.Unit.ToUXmlUnitOfVolume();
			uxmlShipment.TotalWeight = seaShipmentBookingRequest.TotalCargoWeight?.Value;
			uxmlShipment.TotalWeightUnit = seaShipmentBookingRequest.TotalCargoWeight?.Unit.ToUXmlUnitOfWeight();
			uxmlShipment.ShipmentIncoTerm = new IncoTerm
			{
				Code = seaShipmentBookingRequest.PaymentTerms.IsPrepaid ? Core.Constants.IncoTerms.CostAndFreight : Core.Constants.IncoTerms.FreeOnBoard,
				Description = seaShipmentBookingRequest.PaymentTerms.IsPrepaid ? Core.Constants.IncoTerms.Descriptions.CostAndFreight : Core.Constants.IncoTerms.Descriptions.FreeOnBoard
			};
			uxmlShipment.AdditionalTerms = seaShipmentBookingRequest.AdditionalTerms;
			uxmlShipment.TransportMode = seaShipmentBookingRequest.TransportMode?.ToUXmlCodeDescriptionPair();
			uxmlShipment.ReleaseType = seaShipmentBookingRequest.ReleaseType?.ToUXmlCodeDescriptionPair();
			uxmlShipment.VesselName = seaShipmentBookingRequest.VesselName;
			uxmlShipment.LloydsIMO = seaShipmentBookingRequest.LloydsIMO;
			uxmlShipment.VoyageFlightNo = seaShipmentBookingRequest.VoyageNumber;

			PopulateAdditionalReferences(seaShipmentBookingRequest, uxmlShipment);
			PopulatePorts(seaShipmentBookingRequest, uxmlShipment);
			PopulateAddresses(seaShipmentBookingRequest, uxmlShipment);
			PopulateDates(seaShipmentBookingRequest, uxmlShipment);
			PopulateGoodsAndEquipmentDetails(seaShipmentBookingRequest, uxmlShipment);
			PopulateNotes(seaShipmentBookingRequest, uxmlShipment);
			PopulateAddInfos(seaShipmentBookingRequest, uxmlShipment);

			if (seaShipmentBookingRequest.IsRequiredSendAttachment)
			{
				PopulateAttachedDocuments(uxmlShipment);
			}

			PopulateTransportLegs(seaShipmentBookingRequest, uxmlShipment);

			return uxmlShipment;
		}

		#region AttachedDocuments

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Attributes")]
		void PopulateAttachedDocuments(UniversalDataObjects.Shipment uxmlShipment)
		{
			var fileAttributes = new DataObjectWriterHelper.FileAttributes()
			{
				Name = "Booking Request",
				Description = "Booking Request",
				Code = "BKG",
				IsPublished = false
			};

			DataObjectWriterHelper.AppendPopulatePDFAttachedDocument(uxmlShipment, document, fileAttributes);
		}

		#endregion

		#region Ports

		void PopulatePorts(SeaShipmentBookingRequest seaShipmentBookingRequest, UniversalDataObjects.Shipment uxmlShipment)
		{
			uxmlShipment.PortOfLoading = seaShipmentBookingRequest.PortOfLoading.ToUXmlUnloco();
			uxmlShipment.PortOfDischarge = seaShipmentBookingRequest.PortOfDischarge.ToUXmlUnloco();
			uxmlShipment.PortOfOrigin = seaShipmentBookingRequest.Origin.ToUXmlUnloco();
			uxmlShipment.PortOfDestination = seaShipmentBookingRequest.Destination.ToUXmlUnloco();
			uxmlShipment.PlaceOfReceipt = seaShipmentBookingRequest.PlaceOfReceipt.ToUXmlUnloco();
			uxmlShipment.PlaceOfDelivery = seaShipmentBookingRequest.PlaceOfDelivery.ToUXmlUnloco();
		}

		#endregion

		#region Addresses

		void PopulateAddresses(SeaShipmentBookingRequest seaShipmentBookingRequest, UniversalDataObjects.Shipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!seaShipmentBookingRequest.CurrentUser.IsEmpty())
				{
					var currentUserAddress =
						seaShipmentBookingRequest
							.CurrentUser
							.ToUXmlOrganizationAddress(
								"CurrentUser",
								writeManager.WriterStrategy,
								isUpperCase: true
							);
					addresses.Add(currentUserAddress);
				}

				if (!seaShipmentBookingRequest.Shipper.IsEmpty())
				{
					var forwarder =
						seaShipmentBookingRequest
							.Shipper
							.ToUXmlOrganizationAddress(
								nameof(DocAddressType.Forwarder),
								writeManager.WriterStrategy,
								isUpperCase: true
							);
					var consignorDocumentaryAddress =
						seaShipmentBookingRequest
							.Shipper
							.ToUXmlOrganizationAddress(
								nameof(DocAddressType.ConsignorDocumentaryAddress),
								writeManager.WriterStrategy,
								isUpperCase: true
							);

					addresses.Add(forwarder);
					addresses.Add(consignorDocumentaryAddress);
				}

				if (!seaShipmentBookingRequest.Consignee.IsEmpty())
				{
					var consigneeDocumentaryAddress =
						seaShipmentBookingRequest
							.Consignee
							.ToUXmlOrganizationAddress(
								nameof(DocAddressType.ConsigneeDocumentaryAddress),
								writeManager.WriterStrategy,
								isUpperCase: true
							);

					addresses.Add(consigneeDocumentaryAddress);
				}

				if (!seaShipmentBookingRequest.Recipient.IsEmpty())
				{
					addresses.Add(
						seaShipmentBookingRequest
							.Recipient
							.ToUXmlOrganizationAddress(
								nameof(DocAddressType.CoLoadWith),
								writeManager.WriterStrategy,
								isUpperCase: true
							)
						);
				}

				if (seaShipmentBookingRequest.IsDoorPickup && !seaShipmentBookingRequest.PickupFrom.IsEmpty())
				{
					var pickupFromDocumentaryAddress =
						seaShipmentBookingRequest
							.PickupFrom
							.ToUXmlOrganizationAddress(
								nameof(DocAddressType.ConsignorPickupDeliveryAddress),
								writeManager.WriterStrategy,
								isUpperCase: true
							);
					addresses.Add(pickupFromDocumentaryAddress);
				}

				if (seaShipmentBookingRequest.IsDoorDelivery && !seaShipmentBookingRequest.DeliverTo.IsEmpty())
				{
					var notifyPartyAddress =
						seaShipmentBookingRequest
							.DeliverTo
							.ToUXmlOrganizationAddress(
								nameof(DocAddressType.ConsigneePickupDeliveryAddress),
								writeManager.WriterStrategy,
								isUpperCase: true
							);
					addresses.Add(notifyPartyAddress);
				}

				return addresses.Any() ? addresses : null;
			});
		}

		#endregion

		#region Dates

		void PopulateDates(SeaShipmentBookingRequest seaShipmentBookingRequest, UniversalDataObjects.Shipment uxmlShipment)
		{
			uxmlShipment.SetDateCollection(() =>
			{
				var dates = new List<Date>();
				dates.Add(seaShipmentBookingRequest.EarliestDepartureDate, DateType.EarliestDeparture);
				dates.Add(seaShipmentBookingRequest.LatestDeliveryDate, DateType.LatestDelivery);
				dates.Add(seaShipmentBookingRequest.EstCargoPickupDateTime, DateType.Pickup, true);
				dates.Add(seaShipmentBookingRequest.ETD, DateType.Departure, true);
				dates.Add(seaShipmentBookingRequest.ETA, DateType.Arrival, true);
				return dates.Any() ? dates : null;
			});
		}

		#endregion

		#region GoodsAndEquipmentDetails

		void PopulateGoodsAndEquipmentDetails(SeaShipmentBookingRequest seaShipmentBookingRequest, UniversalDataObjects.Shipment uxmlShipment)
		{
			if (seaShipmentBookingRequest.GoodsAndEquipmentDetails?.Any() ?? false)
			{
				uxmlShipment.SetPackingLineCollection(() =>
				{
					var result = new DataObjectList<UniversalDataObjects.PackingLine>();

					var packingLines = seaShipmentBookingRequest
						.GoodsAndEquipmentDetails
						.Select(goodsDetail => goodsDetail.ToUXmlPackingLine(writeManager.WriterStrategy));

					result.AddRange(packingLines);

					return result;
				});
			}
		}

		#endregion

		#region Additional References

		void PopulateAdditionalReferences(SeaShipmentBookingRequest seaShipmentBookingRequest, UniversalDataObjects.Shipment uxmlShipment)
		{
			var additionalReferenceNumbers = new DataObjectList<AdditionalReference>();

			AddAdditionalReference(
				additionalReferenceNumbers,
				seaShipmentBookingRequest.ShipperReference,
				DocDataConstants.AdditionalReferences.Codes.ShipperReference,
				DocDataConstants.AdditionalReferences.Descriptions.ShipperReference);

			AddAdditionalReference(
				additionalReferenceNumbers,
				seaShipmentBookingRequest.SourceID,
				DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
				DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference);

			AddSingleMatchedAdditionalReferenceFromNumbers(
				seaShipmentBookingRequest.Numbers,
				additionalReferenceNumbers,
				DocDataConstants.AdditionalReferences.Codes.CarrierContractNumber,
				DocDataConstants.AdditionalReferences.Codes.CarrierContractNumber,
				DocDataConstants.AdditionalReferences.Descriptions.CarrierContractNumber);

			AddMultipleMatchedAdditionalReferenceFromNumbers(
				seaShipmentBookingRequest.Numbers,
				additionalReferenceNumbers,
				CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG,
				DocDataConstants.AdditionalReferences.Codes.CarrierBookingReference,
				DocDataConstants.AdditionalReferences.Descriptions.CarrierBookingReference);

			uxmlShipment.SetAdditionalReferenceCollection(() => additionalReferenceNumbers.Any() ? additionalReferenceNumbers : null);
		}

		void AddAdditionalReference(
				DataObjectList<AdditionalReference> additionalReferences,
				ZString additionalReferenceNumber,
				ZString additionalReferenceTypeCode,
				ZString additionalReferenceTypeDescription)
		{
			if (!additionalReferenceNumber.IsEmpty)
			{
				var additionalReference = new AdditionalReference
				{
					Type = new EntryType
					{
						Code = additionalReferenceTypeCode,
						Description = additionalReferenceTypeDescription
					},
					ReferenceNumber = additionalReferenceNumber
				};
				additionalReferences.Add(additionalReference);
			}
		}

		void AddSingleMatchedAdditionalReferenceFromNumbers(
				IEnumerable<IReferenceNumber> referenceNumbers,
				DataObjectList<AdditionalReference> additionalReferences,
				ZString referenceNumberType,
				ZString additionalReferenceTypeCode,
				ZString additionalReferenceTypeDescription)
		{
			bool IsRefNumberType(IReferenceNumber referenceNumber) => referenceNumber.Type != null && referenceNumber.Type.Code == referenceNumberType;

			var matchedReferenceNumber = referenceNumbers?.FirstOrDefault(IsRefNumberType)?.Value ?? ZString.Empty;
			AddAdditionalReference(additionalReferences, matchedReferenceNumber, additionalReferenceTypeCode, additionalReferenceTypeDescription);
		}

		void AddMultipleMatchedAdditionalReferenceFromNumbers(
				IEnumerable<IReferenceNumber> referenceNumbers,
				DataObjectList<AdditionalReference> additionalReferences,
				ZString referenceNumberType,
				ZString additionalReferenceTypeCode,
				ZString additionalReferenceTypeDescription)
		{
			foreach (var referenceNumber in referenceNumbers)
			{
				if (referenceNumber.Type != null
					&& referenceNumber.Type.Code == referenceNumberType
					&& !referenceNumber.Value.IsEmpty)
				{
					AddAdditionalReference(additionalReferences, referenceNumber.Value, additionalReferenceTypeCode, additionalReferenceTypeDescription);
				}
			}
		}

		#endregion

		#region Notes

		void PopulateNotes(SeaShipmentBookingRequest seaShipmentBookingRequest, UniversalDataObjects.Shipment uxmlShipment)
		{
			uxmlShipment.SetNoteCollection(() =>
			{
				var notes = new DataObjectList<UniversalDataObjects.Note>();

				AddNote(notes, seaShipmentBookingRequest.GoodsHandlingInstructions, PredefinedNoteTypes.Instance.HandlingInstructions.MultilingualDescription.GetUnresolvedString());
				return notes.Any() ? notes : null;
			});
		}

		void AddNote(DataObjectList<UniversalDataObjects.Note> notes, ZString noteText, ZString noteDescription)
		{
			if (!noteText.IsEmpty)
			{
				notes.Add(new UniversalDataObjects.Note
				{
					Description = noteDescription,
					NoteText = noteText
				});
			}
		}

		#endregion

		#region AddInfos

		void PopulateAddInfos(SeaShipmentBookingRequest carrierMessageData, UniversalDataObjects.Shipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();
				if ((!carrierMessageData.CarrierBookingOffice?.IsEmpty()) ?? false)
				{
					addInfos.AddRange(carrierMessageData.CarrierBookingOffice.ToUXmlAddInfos(DocDataConstants.AddinfoTypes.CarrierBookingOffice));
				}

				addInfos.Add(new AddInfo { Key = DocDataConstants.AddinfoTypes.FormVersion, Value = FormVersionHelper.GetOCMFormVersion() });

				if ((!carrierMessageData.OperationalPort?.IsEmpty()) ?? false)
				{
					addInfos.AddRange(carrierMessageData.OperationalPort.ToUXmlAddInfos(DocDataConstants.AddinfoTypes.OperationalPort));
				}

				if ((!carrierMessageData.FreightPayableAt?.IsEmpty()) ?? false)
				{
					addInfos.AddRange(carrierMessageData.FreightPayableAt.ToUXmlAddInfos(DocDataConstants.AddinfoTypes.FreightPayableAt));
				}

				return addInfos.Any() ? addInfos : null;
			});
		}

		#endregion

		#region MainTransportLeg

		void PopulateTransportLegs(SeaShipmentBookingRequest seaShipmentBookingRequest, UniversalDataObjects.Shipment uxmlShipment)
		{
			uxmlShipment.SetTransportLegCollection(() =>
			{
				var list = new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete };
				var mainTransportLeg = new TransportLeg();
				mainTransportLeg.VesselName = seaShipmentBookingRequest.VesselName;
				mainTransportLeg.VesselLloydsIMO = seaShipmentBookingRequest.LloydsIMO;
				mainTransportLeg.VoyageFlightNo = seaShipmentBookingRequest.VoyageNumber;
				mainTransportLeg.EstimatedDeparture = seaShipmentBookingRequest.ETD;
				mainTransportLeg.EstimatedArrival = seaShipmentBookingRequest.ETA;
				mainTransportLeg.PortOfLoading = seaShipmentBookingRequest.PortOfLoading.ToUXmlUnloco();
				mainTransportLeg.PortOfDischarge = seaShipmentBookingRequest.PortOfDischarge.ToUXmlUnloco();
				mainTransportLeg.TransportMode = Extensions.ToUXmlTransportMode(seaShipmentBookingRequest.LegTransportMode);
				mainTransportLeg.LegOrder = seaShipmentBookingRequest.LegOrder;
				mainTransportLeg.LegType = new LegTypeConverter().ToEnumValue(seaShipmentBookingRequest.LegType);

				list.Add(mainTransportLeg);
				return list;
			});
		}
		#endregion
	}
}
