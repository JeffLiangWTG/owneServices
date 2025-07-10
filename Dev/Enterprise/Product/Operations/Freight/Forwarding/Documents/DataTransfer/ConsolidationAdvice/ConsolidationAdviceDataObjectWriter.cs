using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	sealed class ConsolidationAdviceDataObjectWriter : DataObjectWriter<ConsolidationAdvice, UniversalShipment>
	{
		public ConsolidationAdviceDataObjectWriter(IDataWritingManager writeManager, DocumentVisualizer.Core.IDocument document = null)
			: base(writeManager)
		{
			this.document = document;
		}
		readonly DocumentVisualizer.Core.IDocument document;

		protected override UniversalShipment PopulateDataObject(ConsolidationAdvice consolidationAdvice)
		{
			var uxmlShipment = new UniversalShipment(writeManager.WriterStrategy);

			uxmlShipment.DataContext = consolidationAdvice.CreateUXmlDataContext().AddUserBranchAndDepartment();

			uxmlShipment.CoLoadBookingConfirmationReference = consolidationAdvice.BookingReference;
			uxmlShipment.CoLoadMasterBillNumber = consolidationAdvice.MasterBillNumber;
			uxmlShipment.TransportMode = consolidationAdvice.TransportMode?.ToUXmlCodeDescriptionPair();
			uxmlShipment.ContainerMode = consolidationAdvice.ContainerMode?.ToUXmlContainerMode();
			uxmlShipment.ShipmentType = consolidationAdvice.ShipmentType?.ToUXmlCodeDescriptionPair();
			uxmlShipment.VesselName = consolidationAdvice.VesselName;
			uxmlShipment.LloydsIMO = consolidationAdvice?.LloydsIMO;
			uxmlShipment.VoyageFlightNo = consolidationAdvice.VoyageNumber;
			uxmlShipment.PortOfLoading = consolidationAdvice.PortOfLoading.ToUXmlUnloco();
			uxmlShipment.PortOfDischarge = consolidationAdvice.PortOfDischarge.ToUXmlUnloco();
			uxmlShipment.PortOfOrigin = consolidationAdvice.Origin.ToUXmlUnloco();
			uxmlShipment.PortOfDestination = consolidationAdvice.Destination.ToUXmlUnloco();

			PopulateContainers(consolidationAdvice, uxmlShipment);
			PopulateAdditionalReferences(consolidationAdvice, uxmlShipment);
			PopulateDates(consolidationAdvice, uxmlShipment);
			PopulateNotes(consolidationAdvice, uxmlShipment);
			PopulateAddresses(consolidationAdvice, uxmlShipment);
			PopulateSubShipments(consolidationAdvice, uxmlShipment);
			PopulateTransportLegs(consolidationAdvice, uxmlShipment);
			PopulateAttachedDocuments(uxmlShipment);

			return uxmlShipment;
		}

		#region AttachedDocuments

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Attributes")]
		void PopulateAttachedDocuments(UniversalShipment uxmlShipment)
		{
			var fileAttributes = new DataObjectWriterHelper.FileAttributes()
			{
				Name = "Consolidation Advice",
				Description = "Consolidation Advice",
				Code = "COA",
				IsPublished = false
			};

			DataObjectWriterHelper.AppendPopulatePDFAttachedDocument(uxmlShipment, document, fileAttributes);
		}

		#endregion

		#region Transports

		void PopulateTransportLegs(ConsolidationAdvice consolidationAdvice, UniversalShipment uxmlShipment)
		{
			if (consolidationAdvice.Transports?.Any() ?? false)
			{
				uxmlShipment.SetTransportLegCollection(() =>
				{
					var result = new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete };

					result.AddRange(consolidationAdvice.Transports.Select(t => t.ToUXmlTransportLeg(writeManager.WriterStrategy)));
					return result;
				});
			}
		}

		#endregion

		#region Addresses

		void PopulateAddresses(ConsolidationAdvice consolidationAdvice, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!consolidationAdvice.BookingParty.IsEmpty())
				{
					addresses.Add(consolidationAdvice.BookingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy));
				}

				if (!consolidationAdvice.ReceivingForwarder.IsEmpty())
				{
					addresses.Add(consolidationAdvice.ReceivingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy));
				}

				if (!consolidationAdvice.SendingForwarder.IsEmpty())
				{
					addresses.Add(consolidationAdvice.SendingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy));
				}

				if (!consolidationAdvice.CurrentUser.IsEmpty())
				{
					addresses.Add(consolidationAdvice.CurrentUser.ToUXmlOrganizationAddress(nameof(consolidationAdvice.CurrentUser), writeManager.WriterStrategy));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		void PopulateSubAddresses(SubShipment subShipment, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!subShipment.BookingParty.IsEmpty())
				{
					addresses.Add(subShipment.BookingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy));
				}

				return addresses.Any() ? addresses : null;
			});
		}
		#endregion

		#region Additional References

		void PopulateAdditionalReferences(ConsolidationAdvice consolidationAdvice, UniversalShipment uxmlShipment)
		{
			var additionalReferenceNumbers = new DataObjectList<AdditionalReference>() { Content = CollectionContent.Partial };
			AddAdditionalReference(additionalReferenceNumbers, consolidationAdvice.ContractNumber, DocDataConstants.AdditionalReferences.Codes.CarrierContractNumber, DocDataConstants.AdditionalReferences.Descriptions.CarrierContractNumber);
			uxmlShipment.SetAdditionalReferenceCollection(() => additionalReferenceNumbers.Any() ? additionalReferenceNumbers : null);
		}

		void PopulateSubAdditionalReferences(SubShipment subShipment, UniversalShipment uxmlShipment)
		{
			var additionalReferenceNumbers = new DataObjectList<AdditionalReference>() { Content = CollectionContent.Partial };
			AddAdditionalReference(additionalReferenceNumbers, subShipment.MessageRef, CustomsReferenceNumberType.eHubInterchangeReference.HIR, ShipmentNonCustomsAdditionalReferenceCodesCodeList.Descriptions.HIR);
			uxmlShipment.SetAdditionalReferenceCollection(() => additionalReferenceNumbers.Any() ? additionalReferenceNumbers : null);
		}

		void AddAdditionalReference(DataObjectList<AdditionalReference> additionalReferences, ZString additionalReferenceNumber, ZString additionalReferenceTypeCode, ZString additionalReferenceTypeDescription)
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

		#endregion

		#region Notes

		void PopulateNotes(ConsolidationAdvice consolidationAdvice, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetNoteCollection(() =>
			{
				var notes = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>() { Content = CollectionContent.Partial };
				AddNote(notes, consolidationAdvice.BookingConfirmationNotes, (NoResString)"Booking Confirmation Notes", false);   // programmatic constant
				return notes.Any() ? notes : null;
			});
		}

		void AddNote(DataObjectList<UniversalDataBuss.DataObjects.Universal.Note> notes, ZString noteText, ZString noteDescription, ZBool? isCustomDescription = null)
		{
			if (!noteText.IsEmpty)
			{
				notes.Add(new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = noteDescription,
					IsCustomDescription = isCustomDescription,
					NoteText = noteText
				});
			}
		}

		#endregion

		#region Dates

		void PopulateDates(ConsolidationAdvice consolidationAdvice, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetDateCollection(() =>
			{
				var dates = new List<Date>();
				AddDate(dates, consolidationAdvice.EstimatedTimeArrival, DateType.Arrival);
				AddDate(dates, consolidationAdvice.EstimatedTimeDeparture, DateType.Departure);

				return dates.Any() ? dates : null;
			});
		}

		void AddDate(List<Date> dates, ZDateTime dateTimeToBeAdded, DateType dateType)
		{
			if (!dateTimeToBeAdded.IsEmpty)
			{
				dates.Add(new Date
				{
					Type = dateType,
					Value = dateTimeToBeAdded,
					IsEstimate = true
				});
			}
		}

		#endregion

		#region SubShipments

		void PopulateSubShipments(ConsolidationAdvice consolidationAdvice, UniversalShipment uxmlShipment)
		{
			var subShipmentsList = new DataObjectList<UniversalShipment>();

			foreach (var subShipment in consolidationAdvice.SubShipments)
			{
				var uxmlSubShipment = new UniversalShipment(writeManager.WriterStrategy);
				uxmlSubShipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
				{
					DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
					{
						Type = DocDataConstants.DataSources.ForwardingShipment,
						Key = subShipment.ShipmentNumber
					}
				};
				uxmlSubShipment.AgentsReference = subShipment.ShippersRef;
				uxmlSubShipment.CoLoadBookingConfirmationReference = subShipment.ShipmentNumber;
				uxmlSubShipment.PortOfOrigin = subShipment.Origin.ToUXmlUnloco();
				uxmlSubShipment.PortOfDestination = subShipment.Destination.ToUXmlUnloco();
				PopulateSubAdditionalReferences(subShipment, uxmlSubShipment);
				PopulateSubAddresses(subShipment, uxmlSubShipment);
				subShipmentsList.Add(uxmlSubShipment);
			}

			uxmlShipment.SetSubShipmentCollection(() => subShipmentsList.Any() ? subShipmentsList : null);
		}

		#endregion

		#region Container

		void PopulateContainers(ConsolidationAdvice consolidationAdvice, UniversalShipment uxmlShipment)
		{
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();

			if (consolidationAdvice.Containers != null)
			{
				foreach (var container in consolidationAdvice.Containers)
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);

					if (container.PackingLines != null)
					{
						uxmlContainer.SetPackingLineCollection(() => container.PackingLines.Select(packingLine => packingLine.ToUXmlPackingLine(writeManager.WriterStrategy)).ToList());
					}

					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetContainerCollection(() => uxmlContainers);
		}

		#endregion
	}
}
