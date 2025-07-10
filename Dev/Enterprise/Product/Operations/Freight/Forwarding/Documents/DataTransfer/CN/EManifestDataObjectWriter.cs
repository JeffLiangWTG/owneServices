using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using EZC = Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CN
{
	sealed class EManifestDataObjectWriter : DataObjectWriter<EManifest, UniversalShipment>
	{
		public EManifestDataObjectWriter(IDataWritingManager writeManager, MessageType messageType, DocumentVisualizer.Core.IDocument document = null)
			: base(writeManager)
		{
			this.document = document;
			this.messageType = messageType;
		}

		readonly DocumentVisualizer.Core.IDocument document;
		readonly MessageType messageType;

		protected override UniversalShipment PopulateDataObject(EManifest eManifest)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = eManifest.CreateUXmlDataContext();

			shipment.BookingConfirmationReference = eManifest.CarrierBookingReference;
			shipment.NoOriginalBills = (byte?)eManifest.NumberOfOriginals;
			shipment.NoCopyBills = (byte?)eManifest.NumberOfCopies;
			shipment.WayBillNumber = eManifest.BillOfLadingNumber;
			shipment.ShipmentType = new CodeDescriptionPair
			{
				Code = eManifest.ShipmentType.Code,
				Description = eManifest.ShipmentType.Description
			};

			shipment.DeliveryMode = CreateDeliveryMode(eManifest);

			shipment.ContainerMode = new ContainerMode
			{
				Code = eManifest.ContainerMode.Code,
				Description = eManifest.ContainerMode.Description
			};

			shipment.ReleaseType = new CodeDescriptionPair
			{
				Code = eManifest.ReleaseType.Code,
				Description = eManifest.ReleaseType.Description
			};

			shipment.VesselName = eManifest.Transports.Main?.Vessel.Name;
			shipment.LloydsIMO = eManifest.Transports.Main?.Vessel.LloydsIMO;
			shipment.VoyageFlightNo = eManifest.Transports.Main?.VoyageFlightNumber;

			shipment.PortOfLoading = new UNLOCO
			{
				Code = eManifest.PortOfLoad.Code,
				Name = eManifest.PortOfLoad.Name
			};

			shipment.PortOfDischarge = new UNLOCO
			{
				Code = eManifest.PortOfDischarge.Code,
				Name = eManifest.PortOfDischarge.Name
			};

			shipment.PlaceOfIssue = new UNLOCO
			{
				Code = eManifest.PlaceOfIssue.Code,
				Name = eManifest.PlaceOfIssue.Name
			};

			shipment.PlaceOfReceipt = new UNLOCO
			{
				Code = eManifest.PlaceOfReceipt.Code,
				Name = eManifest.PlaceOfReceipt.Name
			};

			shipment.PlaceOfDelivery = new UNLOCO
			{
				Code = eManifest.PlaceOfDelivery.Code,
				Name = eManifest.PlaceOfDelivery.Name
			};

			shipment.PortOfDestination = new UNLOCO
			{
				Code = eManifest.PortOfDestination.Code,
				Name = eManifest.PortOfDestination.Name
			};

			shipment.SetTransportLegCollection(() => CreateTransportLegs(eManifest));

			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(eManifest).ToList();
				return addresses.Any() ? addresses : null;
			});

			shipment.SetDateCollection(() =>
			{
				var dates = CreateDates(eManifest).ToList();
				return dates.Any() ? dates : null;
			});

			shipment.SetNoteCollection(() =>
			{
				var notes = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>(CreateNotes(eManifest));
				return notes.Any() ? notes : null;
			});

			shipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferenceNumbers = new DataObjectList<AdditionalReference>(eManifest.ToUXmlAdditionalReferences());
				return additionalReferenceNumbers.Any() ? additionalReferenceNumbers : null;
			});

			shipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfos(eManifest).ToList();
				return addInfos.Any() ? addInfos : null;
			});

			shipment.SetPaymentHandlingInstructionCollection(() =>
			{
				var paymentInstructions = CreatePaymentInstructions(eManifest).ToList();
				return paymentInstructions.Any() ? paymentInstructions : null;
			});

			var billOfLadingClauses = CreateBillOfLadingClauses(eManifest).ToList();
			shipment.SetBillOfLadingClauseCollection(() => billOfLadingClauses.Any()
				? billOfLadingClauses
				: null);

			PopulateGoodsDetails(shipment, eManifest);

			if (eManifest.IsRequiredSendAttachment)
			{
				PopulateAttachedDocuments(shipment);
			}

			return shipment;
		}

		IEnumerable<AddInfo> CreateAddInfos(EManifest eManifest)
		{
			if (!eManifest.OperationalPort.IsEmpty())
			{
				foreach (var addInfo in eManifest.OperationalPort.ToUXmlAddInfos(nameof(eManifest.OperationalPort)))
				{
					yield return addInfo;
				}
			}

			if (!eManifest.FreightPayableAt.IsEmpty())
			{
				foreach (var addInfo in eManifest.FreightPayableAt.ToUXmlAddInfos(nameof(eManifest.FreightPayableAt)))
				{
					yield return addInfo;
				}
			}

			yield return new AddInfo { Key = DocDataConstants.AddinfoTypes.FormVersion, Value = "1.0.0" }; // Programatic version number
			yield return new AddInfo() { Key = "MasterSONumber", Value = eManifest.UseBkgRefAsMasterSO ? "CBR" : "MBL" };
		}

		#region AttachedDocuments

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Attributes")]
		void PopulateAttachedDocuments(UniversalShipment uxmlShipment)
		{
			var fileAttributes = new DataObjectWriterHelper.FileAttributes()
			{
				Name = "eManifest (CN)",
				Description = "eManifest (CN)",
				Code = "EMN",
				IsPublished = false
			};

			DataObjectWriterHelper.AppendPopulatePDFAttachedDocument(uxmlShipment, document, fileAttributes);
		}

		#endregion

		#region Goods Details

		#region SuppressResourceStringsCheckRegion

		static class ActionPurpuseCodes
		{
			public const string AsPerPayload = "APP";
			public const string Amendment = "AMD";
		}

		static class ActionPurpuseDescriptions
		{
			public const string AsPerPayload = "As Per Payload";
			public const string Amendment = "Amendment";
		}

		const string bookingDataSouceType = "Booking";

		#endregion

		void PopulateGoodsDetails(UniversalShipment shipment, IEManifest eManifest)
		{
			var seed = 0;

			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			var uxmlPackingLines = new List<UniversalDataBuss.DataObjects.Universal.PackingLine>();

			var uxmlPackingLinesByReferenceNumber = new Dictionary<ZString, List<UniversalDataBuss.DataObjects.Universal.PackingLine>>();

			foreach (var container in eManifest.Containers ?? Enumerable.Empty<IContainer>())
			{
				var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
				uxmlContainer.Link = ++seed;
				uxmlContainers.Add(uxmlContainer);

				foreach (var packingLine in container.PackingLines)
				{
					var uxmlPackingLine = packingLine.ToUXmlPackingLine(writeManager.WriterStrategy);
					uxmlPackingLine.ContainerLink = uxmlContainer.Link;
					uxmlPackingLines.Add(uxmlPackingLine);

					if (!uxmlPackingLinesByReferenceNumber.TryGetValue(packingLine.ExportReferenceNumber, out List<UniversalDataBuss.DataObjects.Universal.PackingLine> bookingPackingLines))
					{
						bookingPackingLines = new List<UniversalDataBuss.DataObjects.Universal.PackingLine>();
						uxmlPackingLinesByReferenceNumber.Add(packingLine.ExportReferenceNumber, bookingPackingLines);
					}

					bookingPackingLines.Add(uxmlPackingLine);
				}
			}

			shipment.SetContainerCollection(() => uxmlContainers);

			shipment.SetSubShipmentCollection(() =>
			{
				var uxmlBookings = new DataObjectList<UniversalShipment>();

				foreach (var booking in eManifest.Bookings ?? Enumerable.Empty<Booking>())
				{
					if (!booking.Send)
					{
						continue;
					}

					var uxmlBooking = CreateBookingSubShipment(booking);
					uxmlBookings.Add(uxmlBooking);

					if (uxmlPackingLinesByReferenceNumber.TryGetValue(booking.BookingNumber, out List<UniversalDataBuss.DataObjects.Universal.PackingLine> bookingPackingLines))
					{
						uxmlBooking.SetPackingLineCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>(bookingPackingLines));
					}
				}

				return uxmlBookings;
			});
		}

		UniversalShipment CreateBookingSubShipment(Booking booking)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = bookingDataSouceType,
					Key = booking.BookingNumber
				},
				DocumentaryOverride = new DocumentaryOverride
				{
					Purpose = CreatePurpose(booking)
				}
			};

			shipment.BookingConfirmationReference = booking.BookingNumber;

			return shipment;
		}

		CodeDescriptionPair CreatePurpose(Booking booking)
		{
			if (messageType == MessageType.Withdrawal)
			{
				return new CodeDescriptionPair
				{
					Code = MessagePurposes.Codes.Withdrawal,
					Description = MessagePurposes.Descriptions.Withdrawal
				};
			}
			else if (booking.MessageStatus.AllowSendOriginal)
			{
				return new CodeDescriptionPair
				{
					Code = MessagePurposes.Codes.Original,
					Description = MessagePurposes.Descriptions.Original
				};
			}
			else if (booking.MessageStatus.AllowSendAmendment)
			{
				return new CodeDescriptionPair
				{
					Code = MessagePurposes.Codes.Amendment,
					Description = MessagePurposes.Descriptions.Amendment
				};
			}
			return null;
		}

		#endregion

		#region Delivery Mode

		CodeDescriptionPair CreateDeliveryMode(IEManifest eManifest)
		{
			var code = string.Format(CultureInfo.InvariantCulture, "{0}T{1}", eManifest.IsDoorPickup ? "D" : "P", eManifest.IsDoorDelivery ? "D" : "P"); // hardcoded constant
			var description = string.Format(CultureInfo.InvariantCulture, (EZC.NoResString)"{0} To {1}", eManifest.IsDoorPickup ? (EZC.NoResString)"Door" : (EZC.NoResString)"Peer", eManifest.IsDoorDelivery ? (EZC.NoResString)"Door" : (EZC.NoResString)"Peer"); // hardcoded constant

			return new CodeDescriptionPair
			{
				Code = code,
				Description = description
			};
		}

		#endregion

		#region Transpot Legs

		DataObjectList<TransportLeg> CreateTransportLegs(IEManifest eManifest)
		{
			var list = new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete };
			list.AddRange(eManifest
				.Transports
				.Select(t => t.ToUXmlTransportLeg(writeManager.WriterStrategy)));

			return list;
		}

		#endregion

		#region Addresses

		IEnumerable<OrganizationAddress> CreateAddresses(IEManifest eManifest)
		{
			if (!eManifest.SendingAgent.IsEmpty())
			{
				yield return eManifest.SendingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress),
					writeManager.WriterStrategy,
					eManifest.SendingAgentTaxInfo != null && !eManifest.SendingAgentTaxInfo.Number.IsEmpty
					? new[] { eManifest.SendingAgentTaxInfo.ToUXmlRegistrationNumber() }
					: Enumerable.Empty<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			}

			if (!eManifest.Carrier.IsEmpty())
			{
				yield return eManifest.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy);
			}

			if (!eManifest.ReceivingAgent.IsEmpty())
			{
				yield return eManifest.ReceivingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress),
					writeManager.WriterStrategy,
					eManifest.ReceivingAgentTaxInfo != null && !eManifest.ReceivingAgentTaxInfo.Number.IsEmpty
					? new[] { eManifest.ReceivingAgentTaxInfo.ToUXmlRegistrationNumber() }
					: Enumerable.Empty<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			}

			if (!eManifest.CarrierHandlingAgent.IsEmpty())
			{
				yield return eManifest.CarrierHandlingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.CarrierHandlingAgent), writeManager.WriterStrategy);
			}

			if (!eManifest.CarrierBookingAgent.IsEmpty())
			{
				yield return eManifest.CarrierBookingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.CarrierBookingAgent), writeManager.WriterStrategy);
			}

			if (!eManifest.NotifyParty.IsEmpty())
			{
				yield return eManifest.NotifyParty.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty),
					writeManager.WriterStrategy,
					eManifest.NotifyPartyTaxInfo != null && !eManifest.NotifyPartyTaxInfo.Number.IsEmpty
					? new[] { eManifest.NotifyPartyTaxInfo.ToUXmlRegistrationNumber() }
					: Enumerable.Empty<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			}

			if (!eManifest.NotifyParty2.IsEmpty())
			{
				yield return eManifest.NotifyParty2.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty2),
					writeManager.WriterStrategy, eManifest.NotifyParty2TaxInfo != null && !eManifest.NotifyParty2TaxInfo.Number.IsEmpty
					? new[] { eManifest.NotifyParty2TaxInfo.ToUXmlRegistrationNumber() }
					: Enumerable.Empty<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			}

			if (!eManifest.Forwarder.IsEmpty())
			{
				yield return eManifest.Forwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!eManifest.PickupFrom.IsEmpty())
			{
				yield return eManifest.PickupFrom.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorPickupDeliveryAddress), writeManager.WriterStrategy);
			}

			if (!eManifest.DeliverTo.IsEmpty())
			{
				yield return eManifest.DeliverTo.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneePickupDeliveryAddress), writeManager.WriterStrategy);
			}

			if (!eManifest.CurrentUser.IsEmpty())
			{
				yield return eManifest.CurrentUser.ToUXmlOrganizationAddress(nameof(eManifest.CurrentUser), writeManager.WriterStrategy);
			}
		}

		#endregion

		#region Dates

		IEnumerable<Date> CreateDates(IEManifest eManifest)
		{
			if (!eManifest.RequestedDateOfIssue.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.BillRequiredBy,
					Value = eManifest.RequestedDateOfIssue
				};
			}
		}

		#endregion

		#region Notes

		IEnumerable<UniversalDataBuss.DataObjects.Universal.Note> CreateNotes(IEManifest eManifest)
		{
			if (!string.IsNullOrWhiteSpace(eManifest.SpecialInstructions))
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = PredefinedNoteTypes.Instance.SpecialInstructions.MultilingualDescription.GetUnresolvedString(),
					NoteText = eManifest.SpecialInstructions
				};
			}

			if (!eManifest.FreightPayableAt.IsEmpty())
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = (EZC.NoResString)"Freight Payable At",  // note type constant for xml only
					NoteText = eManifest.FreightPayableAt.Code
				};
			}

			if (!eManifest.OtherCharges?.Remarks.IsEmpty ?? false)
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = (EZC.NoResString)"Payment Instruction Remark",  // note type constant for xml only
					NoteText = eManifest.OtherCharges.Remarks
				};
			}

			yield return new UniversalDataBuss.DataObjects.Universal.Note
			{
				Description = "ChargesFreighted",
				NoteText = eManifest.IsChargesFreighted ? "Y" : "N"
			};
		}

		#endregion

		#region Payment Instructions

		IEnumerable<PaymentHandlingInstruction> CreatePaymentInstructions(IEManifest eManifest)
		{
			var paymentMethod = GetFreightPaymentMethod(eManifest);

			if (paymentMethod == null)
			{
				yield break;
			}

			yield return new PaymentHandlingInstruction
			{
				Category = new CodeDescriptionPair
				{
					Code = DocDataConstants.ChargeCategoryCodes.Freight,
					Description = DocDataConstants.ChargeCategoryDescriptions.Freight
				},
				PaymentMethod = paymentMethod
			};
		}

		CodeDescriptionPair GetFreightPaymentMethod(IEManifest eManifest)
		{
			if (eManifest.OtherCharges == null)
			{
				return null;
			}

			if (eManifest.OtherCharges.IsPrepaid)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.ChargeCodes.Prepaid,
					Description = DocDataConstants.ChargeDescriptions.Prepaid
				};
			}

			if (eManifest.OtherCharges.IsCollect)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.ChargeCodes.Collect,
					Description = DocDataConstants.ChargeDescriptions.Collect
				};
			}

			if (eManifest.OtherCharges.IsFree)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.ChargeCodes.Free,
					Description = DocDataConstants.ChargeDescriptions.Free
				};
			}

			if (eManifest.OtherCharges.IsPayableElsewhere)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.ChargeCodes.PayableElsewhere,
					Description = DocDataConstants.ChargeDescriptions.PayableElsewhere
				};
			}

			if (eManifest.OtherCharges.IsFirstLinePrepaidLineSecondCollect)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.ChargeCodes.FirstLinePrepaidSecondLineCollect,
					Description = DocDataConstants.ChargeDescriptions.FirstLinePrepaidSecondLineCollect
				};
			}

			return null;
		}

		#endregion

		#region Bill Of Lading Clauses

		IEnumerable<BillOfLadingClause> CreateBillOfLadingClauses(IEManifest eManifest)
		{
			yield return new BillOfLadingClause
			{
				Type = new CodeDescriptionPair
				{
					Code = eManifest.IsFreightPrepaid
						? BillOfLadingTypeCodes.Prepaid
						: BillOfLadingTypeCodes.Collect,

					Description = eManifest.IsFreightPrepaid
						? BillOfLadingTypeDescriptions.Prepaid
						: BillOfLadingTypeDescriptions.Collect
				}
			};
		}

		#endregion
	}
}
