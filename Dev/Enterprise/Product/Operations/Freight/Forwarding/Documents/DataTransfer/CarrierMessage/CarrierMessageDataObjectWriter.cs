using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using EZC = Enterprise.ZArchitecture.Core;
using PackingLine = Enterprise.Freight.Forwarding.Documents.DocDataObjects.PackingLine;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	sealed class CarrierMessageDataObjectWriter : DataObjectWriter<CarrierMessageData, UniversalShipment>
	{
		public CarrierMessageDataObjectWriter(IDataWritingManager writeManager, string dataContext, IDocument document = null)
			: base(writeManager)
		{
			this.dataContext = dataContext;
			this.document = document;
		}

		readonly string dataContext;
		readonly IDocument document;

		protected override UniversalShipment PopulateDataObject(CarrierMessageData carrierMessageData)
		{
			var uxmlShipment = new UniversalShipment(writeManager.WriterStrategy);

			uxmlShipment.DataContext = carrierMessageData.CreateUXmlDataContext();
			uxmlShipment.BookingConfirmationReference = carrierMessageData.BookingReference;

			uxmlShipment.WayBillNumber = carrierMessageData.MasterBillNumber;
			uxmlShipment.CoLoadBookingConfirmationReference = carrierMessageData.CoLoadBookingReference;
			uxmlShipment.CoLoadMasterBillNumber = carrierMessageData.CoLoadMasterBillNumber;

			uxmlShipment.DeliveryMode = Extensions.CreateUXmlDeliveryMode(carrierMessageData.IsDoorPickup, carrierMessageData.IsDoorDelivery);
			uxmlShipment.GoodsValue = carrierMessageData.GoodsValue?.Amount;
			uxmlShipment.GoodsValueCurrency = carrierMessageData.GoodsValue?.Currency?.ToUXmlCurrency();
			uxmlShipment.TransportMode = carrierMessageData.TransportMode?.ToUXmlCodeDescriptionPair();
			uxmlShipment.ContainerMode = carrierMessageData.ContainerMode?.ToUXmlContainerMode();
			uxmlShipment.ShipmentType = carrierMessageData.AgentType?.ToUXmlCodeDescriptionPair();
			uxmlShipment.IsHazardous = carrierMessageData.IsHazardous;

			if (dataContext != DataContext.BookingRequest)
			{
				uxmlShipment.ReleaseType = carrierMessageData.ReleaseType?.ToUXmlCodeDescriptionPair();

				var releaseTypeCode = uxmlShipment.ReleaseType?.Code ?? ZString.Empty;

				if (releaseTypeCode == ShippingInstructionReleaseTypes.Codes.BOLOriginal)
				{
					uxmlShipment.NoOriginalBills = (byte?)carrierMessageData.NumberOfOriginals;
				}

				if (releaseTypeCode != ShippingInstructionReleaseTypes.Codes.HouseBill)
				{
					uxmlShipment.NoCopyBills = (byte?)carrierMessageData.NumberOfCopies;
				}
			}

			uxmlShipment.PaymentMethod = GetOptionalChargePaymentMethod(carrierMessageData.OptionalChargeBasicFreight);
			uxmlShipment.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			var transportBookingPickupDeliveryInfos = carrierMessageData.Containers?.Where(c => c.TransportBookingPickupDeliveryInfos != null)?.SelectMany(c => c.TransportBookingPickupDeliveryInfos)?.Distinct();
			var transportBookingPickupInfos = transportBookingPickupDeliveryInfos?.Where(info => info.Type == "PickupFrom")?.ToList();
			var transportBookingDeliveryInfos = transportBookingPickupDeliveryInfos?.Where(info => info.Type == "DeliveryTo")?.ToList();
			var hasTransportBookingPickupInfos = transportBookingPickupInfos != null && transportBookingPickupInfos.Any();
			var hasTransportBookingDeliveryInfos = transportBookingDeliveryInfos != null && transportBookingDeliveryInfos.Any();

			shouldPopulateCTN = dataContext == DataContext.ShippingInstruction && carrierMessageData.IsCanadaExport;

			PopulateVesselAndVoyage(carrierMessageData, uxmlShipment);
			PopulatePorts(carrierMessageData, uxmlShipment);
			PopulateAddresses(carrierMessageData, uxmlShipment, hasTransportBookingPickupInfos, hasTransportBookingDeliveryInfos);
			PopulateAdditionalReferences(carrierMessageData, uxmlShipment);
			PopulateTransportLegs(carrierMessageData, uxmlShipment);
			PopulateNotes(carrierMessageData, uxmlShipment);
			PopulateAddInfos(carrierMessageData, uxmlShipment);
			PopulateDates(carrierMessageData, uxmlShipment);
			PopulatePaymentInstructions(carrierMessageData, uxmlShipment);
			PopulateContainers(carrierMessageData, uxmlShipment);
			PopulateShipments(carrierMessageData, uxmlShipment);
			PopulateBillOfLadingClauses(carrierMessageData, uxmlShipment);
			PopulatePreallocatedUNDGCollection(carrierMessageData, uxmlShipment);

			if (carrierMessageData.IsRequiredSendAttachment)
			{
				PopulateAttachedDocuments(uxmlShipment);
			}

			if (dataContext == DataContext.BookingRequest)
			{
				uxmlShipment.IsOutOfGauge = carrierMessageData.IsOutOfGauge;

				PopulateInstructionCollections(carrierMessageData, transportBookingPickupInfos, transportBookingDeliveryInfos, hasTransportBookingPickupInfos, hasTransportBookingDeliveryInfos, uxmlShipment);
			}

			return uxmlShipment;
		}

		bool ShouldPopulateCTN => shouldPopulateCTN;
		bool shouldPopulateCTN;

		#region AttachedDocuments

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Attributes")]
		void PopulateAttachedDocuments(UniversalShipment uxmlShipment)
		{
			var isShippingInstruction = dataContext == DataContext.ShippingInstruction;
			var fileAttributes = new DataObjectWriterHelper.FileAttributes()
			{
				Name = isShippingInstruction ? "Shipping Instruction" : "Booking Request",
				Description = isShippingInstruction ? "Shipping Instruction" : "Booking Request",
				Code = isShippingInstruction ? "SHI" : "BKG",
				IsPublished = false
			};

			DataObjectWriterHelper.AppendPopulatePDFAttachedDocument(uxmlShipment, document, fileAttributes);
		}

		#endregion

		#region Vessel And Voyage

		void PopulateVesselAndVoyage(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			var vessel = carrierMessageData.Transports?.Main?.Vessel;
			uxmlShipment.VesselName = vessel?.Name;
			uxmlShipment.LloydsIMO = vessel?.LloydsIMO;
			uxmlShipment.VoyageFlightNo = carrierMessageData?.Transports?.Main?.VoyageFlightNumber;
		}

		#endregion

		#region Transports

		void PopulateTransportLegs(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			if (carrierMessageData.Transports?.Any() ?? false)
			{
				uxmlShipment.SetTransportLegCollection(() =>
				{
					var result = new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete };

					result.AddRange(carrierMessageData.Transports.Select(t => t.ToUXmlTransportLeg(writeManager.WriterStrategy)));
					return result;
				});
			}
		}

		#endregion

		#region Ports

		void PopulatePorts(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			uxmlShipment.PortOfLoading = carrierMessageData.PortOfLoading.ToUXmlUnloco();
			uxmlShipment.PortOfDischarge = carrierMessageData.PortOfDischarge.ToUXmlUnloco();
			uxmlShipment.PortOfOrigin = carrierMessageData.Origin.ToUXmlUnloco();
			uxmlShipment.PortOfDestination = carrierMessageData.Destination.ToUXmlUnloco();
			uxmlShipment.PlaceOfIssue = carrierMessageData.PlaceOfIssue.ToUXmlUnloco();
			uxmlShipment.PlaceOfReceipt = carrierMessageData.PlaceOfReceipt.ToUXmlUnloco();
			uxmlShipment.PlaceOfDelivery = carrierMessageData.PlaceOfDelivery.ToUXmlUnloco();
			uxmlShipment.CarrierBookingOffice = carrierMessageData.CarrierBookingOffice.ToUXmlUnloco();
		}

		#endregion

		#region Addresses

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void PopulateAddresses(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment, bool hasTransportBookingPickupInfos, bool hasTransportBookingDeliveryInfos)
		{
			var isForShippingInstruction = carrierMessageData.DocumentName == DataContext.ShippingInstruction;
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!carrierMessageData.Shipper.IsEmpty())
				{
					var consignorDocumentaryAddress = carrierMessageData.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy, carrierMessageData.ShipperTaxInfo?.Select(t => t.ToUXmlRegistrationNumber(isForShippingInstruction)).Where(t => !t.Value.GetValueOrDefault().IsEmpty) ?? Enumerable.Empty<RegistrationNumber>(), isUpperCase: true);
					addresses.Add(consignorDocumentaryAddress);
				}

				if (!carrierMessageData.Carrier.IsEmpty())
				{
					addresses.Add(carrierMessageData.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.Creditor.IsEmpty())
				{
					addresses.Add(carrierMessageData.Creditor.ToUXmlOrganizationAddress(nameof(DocAddressType.Creditor), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.Recipient.IsEmpty() && (carrierMessageData.IsCoload || carrierMessageData.IsGatewayCoload))
				{
					addresses.Add(carrierMessageData.Recipient.ToUXmlOrganizationAddress(nameof(DocAddressType.CoLoadWith), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.Consignee.IsEmpty())
				{
					var consigneeDocumentaryAddress = carrierMessageData.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy, carrierMessageData.ConsigneeTaxInfo?.Select(t => t.ToUXmlRegistrationNumber(isForShippingInstruction)).Where(t => !t.Value.GetValueOrDefault().IsEmpty) ?? Enumerable.Empty<RegistrationNumber>(), isUpperCase: true);
					addresses.Add(consigneeDocumentaryAddress);
				}

				if (!carrierMessageData.NotifyParty.IsEmpty())
				{
					var notifyPartyAddress = carrierMessageData.NotifyParty.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty), writeManager.WriterStrategy, carrierMessageData.NotifyPartyTaxInfo?.Select(t => t.ToUXmlRegistrationNumber(isForShippingInstruction)).Where(t => !t.Value.GetValueOrDefault().IsEmpty) ?? Enumerable.Empty<RegistrationNumber>(), isUpperCase: true);
					addresses.Add(notifyPartyAddress);
				}

				if (!carrierMessageData.NotifyParty2.IsEmpty())
				{
					addresses.Add(carrierMessageData.NotifyParty2.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty2), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.NotifyParty3.IsEmpty())
				{
					addresses.Add(carrierMessageData.NotifyParty3.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty3), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.Forwarder.IsEmpty())
				{
					addresses.Add(carrierMessageData.Forwarder.ToUXmlOrganizationAddress(nameof(CarrierMessageAddressType.Forwarder), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.SendingForwarder.IsEmpty())
				{
					addresses.Add(carrierMessageData.SendingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.Buyer.IsEmpty())
				{
					addresses.Add(carrierMessageData.Buyer.ToUXmlOrganizationAddress(nameof(DocAddressType.BuyerDocumentaryAddress), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.FreightPayer.IsEmpty())
				{
					addresses.Add(carrierMessageData.FreightPayer.ToUXmlOrganizationAddress(nameof(CarrierMessageAddressType.FreightPayer), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!hasTransportBookingPickupInfos && carrierMessageData.IsDoorPickup && !carrierMessageData.PickupFrom.IsEmpty())
				{
					addresses.Add(carrierMessageData.PickupFrom.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorPickupDeliveryAddress), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!hasTransportBookingDeliveryInfos && carrierMessageData.IsDoorDelivery && !carrierMessageData.DeliverTo.IsEmpty())
				{
					addresses.Add(carrierMessageData.DeliverTo.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneePickupDeliveryAddress), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.CurrentUser.IsEmpty())
				{
					addresses.Add(carrierMessageData.CurrentUser.ToUXmlOrganizationAddress(nameof(carrierMessageData.CurrentUser), writeManager.WriterStrategy, isUpperCase: true));
				}

				if (!carrierMessageData.CustomsBroker.IsEmpty())
				{
					addresses.Add(carrierMessageData.CustomsBroker.ToUXmlOrganizationAddress(nameof(carrierMessageData.CustomsBroker), writeManager.WriterStrategy, isUpperCase: true));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		#endregion

		#region Additional References

		void PopulateAdditionalReferences(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			var additionalReferenceNumbers = new DataObjectList<AdditionalReference>();

			AddAdditionalReference(additionalReferenceNumbers, carrierMessageData.MasterBillNumber, DocDataConstants.AdditionalReferences.Codes.BillOfLading, DocDataConstants.AdditionalReferences.Descriptions.BillOfLading);
			AddAdditionalReference(additionalReferenceNumbers, carrierMessageData.ShipperReference, DocDataConstants.AdditionalReferences.Codes.ShipperReference, DocDataConstants.AdditionalReferences.Descriptions.ShipperReference);
			AddAdditionalReference(additionalReferenceNumbers, carrierMessageData.SourceID, DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference, DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference);
			if (carrierMessageData.HIRReference != null)
			{
				AddAdditionalReference(additionalReferenceNumbers, carrierMessageData.HIRReference.Value, carrierMessageData.HIRReference.Type.Code, carrierMessageData.HIRReference.Type.Description);
			}

			AddSingleMatchedAdditionalReferenceFromNumbers(carrierMessageData.Numbers, additionalReferenceNumbers, DocDataConstants.AdditionalReferences.Codes.CarrierQuoteNumber, DocDataConstants.AdditionalReferences.Codes.CarrierQuoteNumber, DocDataConstants.AdditionalReferences.Descriptions.CarrierQuoteNumber);

			AddAdditionalReference(additionalReferenceNumbers, carrierMessageData.CarrierContractNumbersFormatted, DocDataConstants.AdditionalReferences.Codes.CarrierContractNumber, DocDataConstants.AdditionalReferences.Descriptions.CarrierContractNumber);

			AddSingleMatchedAdditionalReferenceFromNumbers(carrierMessageData.Numbers, additionalReferenceNumbers, DocDataConstants.AdditionalReferences.Codes.ContractNamedAccount, DocDataConstants.AdditionalReferences.Codes.ContractNamedAccount, DocDataConstants.AdditionalReferences.Descriptions.ContractNamedAccount);
			AddSingleMatchedAdditionalReferenceFromNumbers(carrierMessageData.Numbers, additionalReferenceNumbers, CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.LetterOfCreditNumber, DocDataConstants.AdditionalReferences.Codes.LetterOfCreditNumber, DocDataConstants.AdditionalReferences.Descriptions.LetterOfCreditNumber);

			AddMultipleMatchedAdditionalReferenceFromNumbers(carrierMessageData.Numbers, additionalReferenceNumbers, CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, DocDataConstants.AdditionalReferences.Codes.CarrierBookingReference, DocDataConstants.AdditionalReferences.Descriptions.CarrierBookingReference);
			AddMultipleMatchedAdditionalReferenceFromNumbers(carrierMessageData.Numbers, additionalReferenceNumbers, ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber, DocDataConstants.AdditionalReferences.Codes.ShippingOrderNumber, DocDataConstants.AdditionalReferences.Descriptions.ShippingOrderNumber);

			foreach (var acidNumber in carrierMessageData.AcidNumber.Split("\r\n"))
			{
				AddAdditionalReference(additionalReferenceNumbers, acidNumber, DocDataConstants.AdditionalReferences.Codes.AcidNumber, DocDataConstants.AdditionalReferences.Descriptions.AcidNumber);
			}

			foreach (var rucNumber in carrierMessageData.RUCNumber.Split(","))
			{
				AddAdditionalReference(additionalReferenceNumbers, rucNumber, BrazilAdditionalReferenceNumberTypes.Codes.RUC, BrazilAdditionalReferenceNumberTypes.Descriptions.RUC);
			}

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

		void AddSingleMatchedAdditionalReferenceFromNumbers(IEnumerable<DocumentVisualizer.DocDataObjects.IReferenceNumber> referenceNumbers, DataObjectList<AdditionalReference> additionalReferences, ZString referenceNumberType, ZString additionalReferenceTypeCode, ZString additionalReferenceTypeDescription)
		{
			var referenceNumber = referenceNumbers?.FirstOrDefault(x => x.Type != null && x.Type.Code == referenceNumberType)?.Value ?? ZString.Empty;
			AddAdditionalReference(additionalReferences, referenceNumber, additionalReferenceTypeCode, additionalReferenceTypeDescription);
		}

		void AddMultipleMatchedAdditionalReferenceFromNumbers(IEnumerable<DocumentVisualizer.DocDataObjects.IReferenceNumber> referenceNumbers, DataObjectList<AdditionalReference> additionalReferences, string referenceNumberType, string additionalReferenceTypeCode, string additionalReferenceTypeDescription)
		{
			var matchedReferenceNumbers = referenceNumbers?.Where(x => x.Type != null && x.Type.Code == referenceNumberType).Where(x => !x.Value.IsEmpty).Select(x => x.Value) ?? Enumerable.Empty<ZString>();
			foreach (var referenceNumber in matchedReferenceNumbers)
			{
				AddAdditionalReference(additionalReferences, referenceNumber, additionalReferenceTypeCode, additionalReferenceTypeDescription);
			}
		}

		#endregion

		#region Notes

		void PopulateNotes(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetNoteCollection(() =>
			{
				var notes = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>();

				AddNote(notes, carrierMessageData.GoodsHandlingInstructions, PredefinedNoteTypes.Instance.HandlingInstructions.MultilingualDescription.GetUnresolvedString());
				AddNote(notes, carrierMessageData.USCanadaManifestSelfFilerID, DocDataConstants.NoteTypes.USCanadaManifestSelfFilerID, carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported);
				AddNote(notes, carrierMessageData.BRWoodenPackageProcessType?.Code ?? ZString.Empty, DocDataConstants.NoteTypes.WoodenPackageProcessType);
				AddNote(notes, carrierMessageData.OtherBillClauses, DocDataConstants.NoteTypes.OtherBillClauses);
				AddNote(notes, carrierMessageData.IssueFreightedBillOfLading.ToString(), DocDataConstants.NoteTypes.ChargesFreighted);
				if (dataContext != DataContext.BookingRequest)
				{
					AddNote(notes, carrierMessageData.ForwardingInstructions, PredefinedNoteTypes.Instance.ForwardingInstructionNotes.MultilingualDescription.GetUnresolvedString());
				}
				AddNote(notes, carrierMessageData.SpecialInstructions, PredefinedNoteTypes.Instance.SpecialInstructions.MultilingualDescription.GetUnresolvedString());

				return notes.Any() ? notes : null;
			});
		}

		void AddNote(DataObjectList<UniversalDataBuss.DataObjects.Universal.Note> notes, ZString noteText, ZString noteDescription, bool createNoteIfEmpty = false)
		{
			if (!noteText.IsEmpty || createNoteIfEmpty)
			{
				notes.Add(new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = noteDescription,
					NoteText = noteText
				});
			}
		}

		#endregion

		#region AddInfos

		void PopulateAddInfos(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();
				if (!carrierMessageData.CarrierBookingOffice.IsEmpty())
				{
					addInfos.AddRange(carrierMessageData.CarrierBookingOffice.ToUXmlAddInfos(DocDataConstants.AddinfoTypes.CarrierBookingOffice));
				}

				addInfos.Add(new AddInfo { Key = DocDataConstants.AddinfoTypes.FormVersion, Value = FormVersionHelper.GetOCMFormVersion() });

				if (carrierMessageData.IsCoload || carrierMessageData.IsNVO)
				{
					addInfos.Add(new AddInfo { Key = DocDataConstants.AddinfoTypes.IsCoLoad, Value = (EZC.NoResString)"true" }); // true string
				}

				if (carrierMessageData.ElectronicBillOfLadingProviderMandatory)
				{
					addInfos.Add(
						new AddInfo
						{
							Key = DocDataConstants.ShippingLineMessagingRequirement.AddInfoKey.BillOfLadingProvider,
							Value = carrierMessageData.EBLProvider.Codes is EZC.CodeDescriptionPairList eBLProviderList && eBLProviderList.ContainsCode(carrierMessageData.EBLProvider.Code)
									&& carrierMessageData.EBLProvider.Code != EBLProviderConstants.Codes.NotListed
								? carrierMessageData.EBLProvider.Code
								: ZString.Empty
						}
					);
				}

				if (!carrierMessageData.OperationalPort.IsEmpty())
				{
					addInfos.AddRange(carrierMessageData.OperationalPort.ToUXmlAddInfos(DocDataConstants.AddinfoTypes.OperationalPort));
				}
				if (!carrierMessageData.FreightPayableAt.IsEmpty())
				{
					addInfos.AddRange(carrierMessageData.FreightPayableAt.ToUXmlAddInfos(DocDataConstants.AddinfoTypes.FreightPayableAt));
				}

				if (FreightDataRegistry.Instance.EnablePackageGrouping.Value && carrierMessageData.PackageGrouping != null)
				{
					addInfos.Add(
						new AddInfo
						{
							Key = DocDataConstants.AddinfoTypes.GroupingMethod,
							Value = carrierMessageData.PackageGrouping.Code
						}
					);
				}

				if (dataContext == DataContext.ShippingInstruction)
				{
					if (!carrierMessageData.ICS2DeclarantEORINumber.IsEmpty)
					{
						addInfos.Add(
							new AddInfo
							{
								Key = DocDataConstants.ICS2DeclarantEORI.AddInfoKey.ICS2DeclarantEORI,
								Value = carrierMessageData.ICS2DeclarantEORINumber.SubstringSafe(0, 35)
							}
						);
					}

					var ics2FilingTypeValue = carrierMessageData.IsShowICS2
						? !carrierMessageData.ICS2DeclarantEORINumber.IsEmpty ? DocDataConstants.ICS2DeclarantEORI.AddInfoValue.Declarant : DocDataConstants.ICS2DeclarantEORI.AddInfoValue.Carrier
						: string.Empty;

					if (!string.IsNullOrEmpty(ics2FilingTypeValue))
					{
						addInfos.Add(
							new AddInfo
							{
								Key = DocDataConstants.ICS2DeclarantEORI.AddInfoKey.ICS2FilingType,
								Value = ics2FilingTypeValue
							}
						);
					}
				}

				return addInfos.Any() ? addInfos : null;
			});
		}

		#endregion

		#region Dates

		void PopulateDates(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetDateCollection(() =>
			{
				var dates = new List<Date>();

				AddDate(dates, carrierMessageData.DateOfIssue, DateType.BillIssued);
				AddDate(dates, carrierMessageData.PortOfFirstArrivalDate, DateType.FirstArrivalInCountry);
				AddDate(dates, carrierMessageData.FirstForeignArrivalDate, DateType.FirstForeignArrival);
				AddDate(dates, carrierMessageData.LastForeignDepartureDate, DateType.LastForeignDeparture);
				AddDate(dates, carrierMessageData.EarliestDepartureDate, DateType.EarliestDeparture);
				AddDate(dates, carrierMessageData.LatestDeliveryDate, DateType.LatestDelivery);
				if ((carrierMessageData.IsCoload || carrierMessageData.IsNVO) && carrierMessageData.ContainerMode.Code == Core.Constants.ContainerModes.LCL && carrierMessageData.IsDoorPickup)
				{
					AddDate(dates, carrierMessageData.EstCargoPickupDateTime, DateType.Pickup, true);
				}

				return dates.Any() ? dates : null;
			});
		}

		void AddDate(List<Date> dates, ZDateTime dateTimeToBeAdded, DateType dateType, ZBool? isEstimate = null)
		{
			if (!dateTimeToBeAdded.IsEmpty)
			{
				dates.Add(new Date
				{
					Type = dateType,
					Value = dateTimeToBeAdded,
					IsEstimate = isEstimate
				});
			}
		}

		#endregion

		#region Payment Instructions

		void PopulatePaymentInstructions(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetPaymentHandlingInstructionCollection(() =>
			{
				var paymentInstructions = new List<PaymentHandlingInstruction>();

				AddPaymentInstruction(paymentInstructions, carrierMessageData.OptionalChargeBasicFreight, DocDataConstants.Charges.Categories.Codes.Freight, DocDataConstants.Charges.Categories.Descriptions.Freight);
				AddPaymentInstruction(paymentInstructions, carrierMessageData.OptionalChargeDestinationHaulage, DocDataConstants.Charges.Categories.Codes.DestinationHaulage, DocDataConstants.Charges.Categories.Descriptions.DestinationHaulage);
				AddPaymentInstruction(paymentInstructions, carrierMessageData.OptionalChargeDestinationPort, DocDataConstants.Charges.Categories.Codes.DestinationPort, DocDataConstants.Charges.Categories.Descriptions.DestinationPort);
				AddPaymentInstruction(paymentInstructions, carrierMessageData.OptionalChargeOriginHaulage, DocDataConstants.Charges.Categories.Codes.OriginHaulage, DocDataConstants.Charges.Categories.Descriptions.OriginHaulage);
				AddPaymentInstruction(paymentInstructions, carrierMessageData.OptionalChargeOriginPort, DocDataConstants.Charges.Categories.Codes.OriginPort, DocDataConstants.Charges.Categories.Descriptions.OriginPort);

				return paymentInstructions.Any() ? paymentInstructions : null;
			});
		}

		void AddPaymentInstruction(List<PaymentHandlingInstruction> paymentInstructions, IOptionalCharge optionalCharge, ZString categoryCode, ZString categoryDescription)
		{
			var optionalChargePaymentMethod = GetOptionalChargePaymentMethod(optionalCharge);
			if (optionalChargePaymentMethod != null)
			{
				paymentInstructions.Add(new PaymentHandlingInstruction
				{
					Category = new CodeDescriptionPair
					{
						Code = categoryCode,
						Description = categoryDescription
					},
					PaymentMethod = optionalChargePaymentMethod
				});
			}
		}

		CodeDescriptionPair GetOptionalChargePaymentMethod(IOptionalCharge optionalCharge)
		{
			if (optionalCharge == null)
			{
				return null;
			}

			if (optionalCharge.IsPrepaid)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.Prepaid,
					Description = DocDataConstants.Charges.Descriptions.Prepaid
				};
			}

			if (optionalCharge.IsCollect)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.Collect,
					Description = DocDataConstants.Charges.Descriptions.Collect
				};
			}

			if (optionalCharge.IsPayableElsewhere)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.PayableElsewhere,
					Description = DocDataConstants.Charges.Descriptions.PayableElsewhere
				};
			}

			return null;
		}

		#endregion

		#region Bill Of Lading Clauses

		void PopulateBillOfLadingClauses(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetBillOfLadingClauseCollection(() =>
			{
				var billOfLadingClauses = new List<BillOfLadingClause>();

				AddBillOfLadingClause(billOfLadingClauses,
						carrierMessageData.IsFreightPrepaid || carrierMessageData.IsFreightCollect,
						carrierMessageData.IsFreightPrepaid ? DocDataConstants.BillOfLadingTypes.Codes.Prepaid : DocDataConstants.BillOfLadingTypes.Codes.Collect,
						carrierMessageData.IsFreightPrepaid ? DocDataConstants.BillOfLadingTypes.Descriptions.Prepaid : DocDataConstants.BillOfLadingTypes.Descriptions.Collect);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsFreightAsAgreed && dataContext != DataContext.BookingRequest, DocDataConstants.BillOfLadingTypes.Codes.AsAgreed, DocDataConstants.BillOfLadingTypes.Descriptions.AsAgreed);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsReceivedForShipment, DocDataConstants.BillOfLadingTypes.Codes.ReceivedForShipment, DocDataConstants.BillOfLadingTypes.Descriptions.ReceivedForShipment);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsLadenOnBoard, DocDataConstants.BillOfLadingTypes.Codes.LadenOnBoard, DocDataConstants.BillOfLadingTypes.Descriptions.LadenOnBoard);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsOnBoardRail, DocDataConstants.BillOfLadingTypes.Codes.OnBoardRail, DocDataConstants.BillOfLadingTypes.Descriptions.OnBoardRail);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsLadenOnBoardVessel, DocDataConstants.BillOfLadingTypes.Codes.LadenOnBoardVessel, DocDataConstants.BillOfLadingTypes.Descriptions.LadenOnBoardVessel);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsOnBoardVessel, DocDataConstants.BillOfLadingTypes.Codes.OnBoardVessel, DocDataConstants.BillOfLadingTypes.Descriptions.OnBoardVessel);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsLadenOnBoardNamedVessel, DocDataConstants.BillOfLadingTypes.Codes.LadenOnBoardNamedVessel, DocDataConstants.BillOfLadingTypes.Descriptions.LadenOnBoardNamedVessel);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsShipperLoadAndCount, DocDataConstants.BillOfLadingTypes.Codes.ShipperLoadAndCount, DocDataConstants.BillOfLadingTypes.Descriptions.ShipperLoadAndCount);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsShipperLoadStowageAndCount, DocDataConstants.BillOfLadingTypes.Codes.ShipperLoadStowageAndCount, DocDataConstants.BillOfLadingTypes.Descriptions.ShipperLoadStowageAndCount);
				AddBillOfLadingClause(billOfLadingClauses, carrierMessageData.IsNoShipperExportDeclarationRequired, DocDataConstants.BillOfLadingTypes.Codes.NoShipperExportDeclarationRequired, DocDataConstants.BillOfLadingTypes.Descriptions.NoShipperExportDeclarationRequired);

				return billOfLadingClauses.Any() ? billOfLadingClauses : null;
			});
		}

		void AddBillOfLadingClause(List<BillOfLadingClause> billOfLadingClauses, bool shouldAdd, string clauseCode, string clauseDescription)
		{
			if (shouldAdd)
			{
				billOfLadingClauses.Add(new BillOfLadingClause
				{
					Type = new CodeDescriptionPair
					{
						Code = clauseCode,
						Description = clauseDescription
					}
				});
			}
		}

		#endregion

		#region Good Details

		void PopulateContainers(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			var containerLink = 0;
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			PackingLineContainerLinkMap = new Dictionary<ZGuid, int>();
			ContainerLinkMap = new Dictionary<ZString, int>();

			if (carrierMessageData.Containers != null)
			{
				foreach (var container in carrierMessageData.Containers)
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					uxmlContainer.Link = ++containerLink;

					if (carrierMessageData.PackageGrouping != null
						&& (carrierMessageData.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByShipment || carrierMessageData.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByPackLine))
					{
						if (!ContainerLinkMap.ContainsKey(container.Identifier.ToString()))
						{
							ContainerLinkMap.Add(container.Identifier.ToString(), containerLink);
						}
					}
					else
					{
						foreach (var packingLine in container.PackingLines)
						{
							PackingLineContainerLinkMap.Add((ZGuid)packingLine.Identifier, containerLink);
						}
					}
					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetContainerCollection(() => uxmlContainers);
		}

		void PopulateShipments(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			var uxmlSubShipments = carrierMessageData.Shipments?.OrderBy(s => s.ShipmentID)
				.Select(x => x.ToUXmlShipment(PackingLineContainerLinkMap, writeManager.WriterStrategy, true, true, true, true, () => GetPackingLineCollection(carrierMessageData, x), GetHouseBillPaymentTypeForShipment(carrierMessageData, x), true)).ToList()
				?? new List<UniversalShipment>();
			uxmlShipment.SetSubShipmentCollection(() => uxmlSubShipments.Any() ? new DataObjectList<UniversalShipment>(uxmlSubShipments) : null);
		}

		Dictionary<ZGuid, int> PackingLineContainerLinkMap;
		Dictionary<ZString, int> ContainerLinkMap;

		DataObjectList<UniversalPackingLine> GetPackingLineCollection(CarrierMessageData carrierMessageData, DocumentVisualizer.DocDataObjects.IShipment shipment)
		{
			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value)
			{
				var parentPackingLineUXmls = new DataObjectList<UniversalPackingLine>();

				if (carrierMessageData.PackageGrouping != null
					&& (carrierMessageData.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByShipment || carrierMessageData.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByPackLine))
				{
					foreach (var parentPackingLineDO in shipment.PackingLines)
					{
						var parentITNNumbers = string.Empty;
						var parentDUENumbers = string.Empty;
						var parentUCRNumbers = string.Empty;
						var parentCTKNumbers = string.Empty;
						var parentCTNNumbers = string.Empty;

						if (dataContext == DataContext.ShippingInstruction)
						{
							parentITNNumbers = parentPackingLineDO.GroupITNNumber.IsEmpty ? parentPackingLineDO.GroupPOFNumber : parentPackingLineDO.GroupITNNumber;
							parentDUENumbers = parentPackingLineDO.GroupDUENumber;
							parentUCRNumbers = parentPackingLineDO.GroupUCRNumber;
							parentCTKNumbers = parentPackingLineDO.GroupCTKNumber;
							parentCTNNumbers = ShouldPopulateCTN ? parentPackingLineDO.GroupCTNNumber : string.Empty;
						}
						else
						{
							parentITNNumbers = string.Join(", ", parentPackingLineDO.ShipmentID.Split(", ")
								.SelectMany(shipmentID => new ZString(shipment.GetMatchedShipmentForPackLine(shipmentID)?.GetDistinctNumbers(Customs.Common.US.CusEntryNumberTypeList.Codes.ITN) ?? ZString.Empty).Split(", ") ?? Array.Empty<ZString>()).Distinct());
							parentDUENumbers = string.Join(", ", parentPackingLineDO.ShipmentID.Split(", ")
								.SelectMany(shipmentID => new ZString(shipment.GetMatchedShipmentForPackLine(shipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Brazil.DUE) ?? ZString.Empty).Split(", ") ?? Array.Empty<ZString>()).Distinct());
							parentUCRNumbers = string.Join(", ", parentPackingLineDO.ShipmentID.Split(", ")
								.SelectMany(shipmentID => new ZString(shipment.GetMatchedShipmentForPackLine(shipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Standard.UniqueConsignementReference) ?? ZString.Empty).Split(", ") ?? Array.Empty<ZString>()).Distinct());
						}

						var parentPackingLineUXml = parentPackingLineDO.ToUXmlPackingLine(writeManager.WriterStrategy, true, null, itnNumbers: parentITNNumbers, dueNumbers: parentDUENumbers, ucrNumbers: parentUCRNumbers, ctkNumbers: parentCTKNumbers, splitHarmonisedCode: true,
							ctnNumbers: parentCTNNumbers);

						parentPackingLineUXml.SetPackingLineCollection(() =>
						{
							return new List<UniversalPackingLine>(parentPackingLineDO.PackingLines
								.OrderBy(x => x.ContainerNumber)
								.Select(x =>
								{
									var childITNNumbers = x.ShipmentID.Split(", ")
										.SelectMany(shipmentID => new ZString(shipment.GetMatchedShipmentForPackLine(shipmentID)?.GetDistinctNumbers(Customs.Common.US.CusEntryNumberTypeList.Codes.ITN) ?? ZString.Empty).Split(", ") ?? Array.Empty<ZString>()).Distinct();
									var childDUENumbers = x.ShipmentID.Split(", ")
										.SelectMany(shipmentID => new ZString(shipment.GetMatchedShipmentForPackLine(shipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Brazil.DUE) ?? ZString.Empty).Split(", ") ?? Array.Empty<ZString>()).Distinct();
									var childUCRNumbers = x.ShipmentID.Split(", ")
										.SelectMany(shipmentID => new ZString(shipment.GetMatchedShipmentForPackLine(shipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Standard.UniqueConsignementReference) ?? ZString.Empty).Split(", ") ?? Array.Empty<ZString>()).Distinct();
									var childCTKNumbers = dataContext == DataContext.ShippingInstruction ? x.ShipmentID.Split(", ")
										.SelectMany(shipmentID => new ZString(shipment.GetMatchedShipmentForPackLine(shipmentID)?.GetDistinctNumbers(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote) ?? ZString.Empty).Split(", ") ?? Array.Empty<ZString>()).Distinct() : Array.Empty<ZString>();
									var childCTNNumbers = ShouldPopulateCTN ? x.ShipmentID.Split(", ")
										.SelectMany(shipmentID => new ZString(shipment.GetMatchedShipmentForPackLine(shipmentID)?.GetDistinctNumbers(CanadaAdditionalReferenceNumberTypes.Codes.CTN) ?? ZString.Empty).Split(", ") ?? Array.Empty<ZString>()).Distinct()
										: Array.Empty<ZString>();

									var childPackingLineUXml = x.ToUXmlPackingLine(writeManager.WriterStrategy, true, null, itnNumbers: string.Join(", ", childITNNumbers), dueNumbers: string.Join(", ", childDUENumbers), ucrNumbers: string.Join(", ", childUCRNumbers),
										ctkNumbers: string.Join(", ", childCTKNumbers), splitHarmonisedCode: true, ctnNumbers: string.Join(", ", childCTNNumbers));

									var containerPK = ((PackingLine)x).Identifier.ToString().Substring(0, 36);
									if (ContainerLinkMap.TryGetValue(containerPK, out var containerLink))
									{
										childPackingLineUXml.ContainerLink = containerLink;
									}

									childPackingLineUXml.MarksAndNos = parentPackingLineUXml.MarksAndNos;
									childPackingLineUXml.GoodsDescription = parentPackingLineUXml.GoodsDescription;
									childPackingLineUXml.DetailedDescription = parentPackingLineUXml.DetailedDescription;

									return childPackingLineUXml;
								}));
						});

						PopulateCUSCodes(parentPackingLineUXml, parentPackingLineDO as PackingLine);

						parentPackingLineUXmls.Add(parentPackingLineUXml);
					}
				}
				else
				{
					var packingLineDOs = ((DocDataObjects.Shipment)shipment).AllPackingLinesIncludeCoLoad
						.OrderBy(p => p.ContainerNumber)
						.ThenBy(p => p.PackingLineID);

					foreach (var packingLineDO in packingLineDOs)
					{
						var packingLineUXml = packingLineDO.ToUXmlPackingLine(writeManager.WriterStrategy, true, PackingLineContainerLinkMap
							, itnNumbers: shipment.GetMatchedShipmentForPackLine(packingLineDO.ShipmentID)?.GetDistinctNumbers(Customs.Common.US.CusEntryNumberTypeList.Codes.ITN)
							, dueNumbers: shipment.GetMatchedShipmentForPackLine(packingLineDO.ShipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Brazil.DUE)
							, ucrNumbers: shipment.GetMatchedShipmentForPackLine(packingLineDO.ShipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Standard.UniqueConsignementReference)
							, ctkNumbers: dataContext == DataContext.ShippingInstruction ? shipment.GetMatchedShipmentForPackLine(packingLineDO.ShipmentID)?.GetDistinctNumbers(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote) : string.Empty
							, splitHarmonisedCode: true
							, ctnNumbers: ShouldPopulateCTN ? shipment.GetMatchedShipmentForPackLine(packingLineDO.ShipmentID)?.GetDistinctNumbers(CanadaAdditionalReferenceNumberTypes.Codes.CTN) : string.Empty);

						packingLineUXml.SetPackingLineCollection(() => new List<UniversalPackingLine> { (UniversalPackingLine)packingLineUXml.Clone() });

						PopulateCUSCodes(packingLineUXml, packingLineDO);

						parentPackingLineUXmls.Add(packingLineUXml);
					}
				}

				return parentPackingLineUXmls;
			}
			else
			{
				var packingLineDOs = ((DocDataObjects.Shipment)shipment).AllPackingLinesIncludeCoLoad
					.OrderBy(p => p.ContainerNumber)
					.ThenBy(p => p.PackingLineID);

				return new DataObjectList<UniversalPackingLine>(packingLineDOs
					.Select(x =>
					{
						var universalPackingLine = x.ToUXmlPackingLine
						(
							writeManager.WriterStrategy,
							true,
							PackingLineContainerLinkMap,
							itnNumbers: shipment.GetMatchedShipmentForPackLine(x.ShipmentID)?.GetDistinctNumbers(Customs.Common.US.CusEntryNumberTypeList.Codes.ITN),
							dueNumbers: shipment.GetMatchedShipmentForPackLine(x.ShipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Brazil.DUE),
							ucrNumbers: shipment.GetMatchedShipmentForPackLine(x.ShipmentID)?.GetDistinctNumbers(CusEntryNumberTypes.Standard.UniqueConsignementReference),
							ctkNumbers: dataContext == DataContext.ShippingInstruction ? shipment.GetMatchedShipmentForPackLine(x.ShipmentID)?.GetDistinctNumbers(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote) : string.Empty,
							ctnNumbers: ShouldPopulateCTN ? shipment.GetMatchedShipmentForPackLine(x.ShipmentID)?.GetDistinctNumbers(CanadaAdditionalReferenceNumberTypes.Codes.CTN) : string.Empty
						);

						PopulateCUSCodes(universalPackingLine, x);

						return universalPackingLine;
					}));
			}
		}

		void PopulateCUSCodes(UniversalPackingLine universalPackingLine, PackingLine packingLineDO)
		{
			if (universalPackingLine != null && packingLineDO != null)
			{
				var classificationCollection = universalPackingLine.ClassificationCollection ?? new DataObjectList<Classification>();

				foreach (var cusCode in new[] { packingLineDO.CUSCode1, packingLineDO.CUSCode2, packingLineDO.CUSCode3,
					packingLineDO.CUSCode4, packingLineDO.CUSCode5, packingLineDO.CUSCode6,
					packingLineDO.CUSCode7,packingLineDO.CUSCode8, packingLineDO.CUSCode9
				}.Where(x => !x.IsEmpty).Distinct())
				{
					classificationCollection.Add(new Classification()
					{
						Code = cusCode,
						Type = new CodeDescriptionPair
						{
							Code = Freight.Business.FreightConstants.Classification.Codes.ECICS,
							Description = Freight.Business.FreightConstants.Classification.Description.ECICS
						}
					});
				}

				universalPackingLine.SetClassificationCollection(() => classificationCollection);
			}
		}

		CodeDescriptionPair GetHouseBillPaymentTypeForShipment(CarrierMessageData carrierMessageData, DocDataObjects.Shipment shipment)
		{
			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value && carrierMessageData.DocumentName == DataContext.ShippingInstruction && shipment?.PackingLines != null)
			{
				var packingLine = shipment.Shipments?.FirstOrDefault()?.PackingLines?.FirstOrDefault() ?? shipment.PackingLines.FirstOrDefault();

				if (packingLine != null && !packingLine.HBLPaymentType.IsEmpty)
				{
					return new CodeDescriptionPair
					{
						Code = packingLine.HBLPaymentType,
						Description = packingLine.HBLPaymentTypeList.GetDescriptionFromCode(packingLine.HBLPaymentType)
					};
				}
			}
			return null;
		}

		#endregion

		#region PopulateInstructionCollections

		void PopulateInstructionCollections(CarrierMessageData carrierMessageData, IEnumerable<TransportBookingPickupDeliveryInfo> transportBookingPickupInfos, IEnumerable<TransportBookingPickupDeliveryInfo> transportBookingDeliveryInfos, bool hasTransportBookingPickupInfos, bool hasTransportBookingDeliveryInfos, UniversalShipment uxmlShipment)
		{
			var sequence = 0;
			var result = new DataObjectList<Instruction>();

			if (carrierMessageData.IsDoorPickup && hasTransportBookingPickupInfos)
			{
				AddInstructions(result, transportBookingPickupInfos, uxmlShipment, sequence, true);
			}
			if (carrierMessageData.IsDoorDelivery && hasTransportBookingDeliveryInfos)
			{
				AddInstructions(result, transportBookingDeliveryInfos, uxmlShipment, sequence, false);
			}
			uxmlShipment.SetInstructionCollection(() => result.Any() ? result : null);
		}

		void AddInstructions(DataObjectList<Instruction> list, IEnumerable<TransportBookingPickupDeliveryInfo> infos, UniversalShipment uxmlShipment, int sequence, bool isDoorPickup)
		{
			var instructionType = new CodeDescriptionPair()
			{
				Code = isDoorPickup ? InstructionTypes.Codes.PickUp : InstructionTypes.Codes.Delivery,
				Description = isDoorPickup ? InstructionTypes.Descriptions.PickUp : InstructionTypes.Descriptions.Delivery
			};

			foreach (var info in infos)
			{
				var containerLinks = uxmlShipment.ContainerCollection?.Where(container =>
					info.Packages?.Any(package =>
						package.KP_PackageID.IsEmpty ? package.PackageIDWithFallback == container.ContainerCount + Core.Constants.ContainerModes.Containerised + container.ContainerType.Code
							: package.PackageIDWithFallback == container.ContainerNumber.Value) ?? false
					)?.Select(c => c.Link);
				var instruction = new Instruction(writeManager.WriterStrategy);
				instruction.Sequence = ++sequence;
				instruction.Type = instructionType;
				instruction.Address = info.Address.ToUXmlOrganizationAddress(isDoorPickup ? nameof(DocAddressType.ConsignorPickupDeliveryAddress) : nameof(DocAddressType.ConsigneePickupDeliveryAddress), writeManager.WriterStrategy, isUpperCase: true);
				PopulateInstructionContainerLink(instruction, info, containerLinks);

				list.Add(instruction);
			}
		}

		void PopulateInstructionContainerLink(Instruction instructionDataObject, TransportBookingPickupDeliveryInfo info, IEnumerable<ZInt?> containerLinks)
		{
			instructionDataObject.SetInstructionContainerLinkCollection(() =>
			{
				var result = new List<InstructionContainerLink>();

				if (containerLinks != null)
				{
					foreach (var link in containerLinks)
					{
						var instructionContainerLinkDataObject = new InstructionContainerLink();
						instructionContainerLinkDataObject.ContainerLink = link;
						instructionContainerLinkDataObject.ConfirmationCollection = new List<Confirmation>();

						var confirmation = new Confirmation()
						{
							EstimatedDate = info.AddressETD
						};
						instructionContainerLinkDataObject.ConfirmationCollection.Add(confirmation);

						result.Add(instructionContainerLinkDataObject);
					}
				}
				return result.Any() ? result : null;
			});
		}

		#endregion

		#region PopulatePreallocatedUNDGCollection

		void PopulatePreallocatedUNDGCollection(CarrierMessageData carrierMessageData, UniversalShipment uxmlShipment)
		{
			if (carrierMessageData.DocumentName == DataContext.BookingRequest && carrierMessageData.PreallocatedUNDGCollection != null)
			{
				var uxmlPreallocatedUNDGCollection = new DataObjectList<UNDG>();

				foreach (var dgRestriction in carrierMessageData.PreallocatedUNDGCollection)
				{
					var uxmlUNDG = dgRestriction.ToUXmlUNDG(writeManager.WriterStrategy);
					if (uxmlUNDG != null)
					{
						uxmlPreallocatedUNDGCollection.Add(uxmlUNDG);
					}
				}

				uxmlShipment.SetPreallocatedUNDGCollection(() => uxmlPreallocatedUNDGCollection);
			}
		}

		#endregion
	}
}
