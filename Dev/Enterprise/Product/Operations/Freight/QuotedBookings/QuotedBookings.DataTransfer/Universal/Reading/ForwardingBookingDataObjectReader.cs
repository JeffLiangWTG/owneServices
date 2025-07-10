using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using LegType = Enterprise.UniversalDataBuss.DataObjects.Universal.LegType;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	public class ForwardingBookingDataObjectReader : ShipmentDataObjectReader<QuotedBooking>
	{
		public ForwardingBookingDataObjectReader(UniversalShipment bookingDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IUniversalFreightHelper helper = null)
			: base(bookingDataObject, logger, factory)
		{
			readingHelper = new ShipmentDataObjectReadingHelper(bookingDataObject, logger, factory);
			readingHelper.LinkManager = new ContainerLinkManager<ForwardingConsol>(null);

			this.helper = helper ?? new UniversalForwardingHelper();
		}

		readonly ShipmentDataObjectReadingHelper readingHelper;
		readonly IUniversalFreightHelper helper;

		public override DataContextType DataContextType => DataContextType.ForwardingBooking;

		protected override QuotedBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override IMatchingBusinessEntityFinder<QuotedBooking> GetCombinedReferenceMatcher()
		{
			return new QuotedBookingMatcher(factory.BOFactory, readingHelper.GenerateShipmentReference(), logger, helper);
		}

		protected override QuotedBooking GetNewBusinessObject()
		{
			return QuotedBooking.New(QuoteBookingType.QuickBooking, factory.BOFactory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "This method should not be split.")]
		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(QuotedBooking targetBO)
		{
			if (IsNVOCC)
			{
				if (dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress)) == null)
				{
					return Res.GetString("6cc3a751-bfe2-4d24-acca-fad2ca43d113", "[*Booking party is missing.*]");
				}

				var bookingPartyPK = dataObject.GetBookingParty(logger, factory);
				var bookingPartyName = dataObject.GetBookingPartyName();

				if (bookingPartyPK.IsEmpty)
				{
					return Res.GetString("24855c69-2023-4398-a925-e24a1d91be04",
						"[*Booking Request message is received from an unknown Booking Party {0}. Booking rejected.*]",
						bookingPartyName);
				}

				var coLoadBookingConfirmationReference = dataObject.CoLoadBookingConfirmationReference.GetValueOrDefault();
				var agentsReference = dataObject.AgentsReference.GetValueOrDefault();

				var documentPurposeCode = dataObject.DataContext?.DocumentaryOverride?.Purpose.GetCodeAsUpperCase() ?? ZString.Empty;
				if (documentPurposeCode == MessagePurposes.Codes.Amendment && coLoadBookingConfirmationReference.IsEmpty)
				{
					return Res.GetString("ae625e23-ab7f-47fe-8e88-b2d1bd7f4355", "[*Mandatory Booking Number is missing in Booking Request Amendment message, amendment rejected.*]");
				}

				if (targetBO == null && agentsReference.IsEmpty)
				{
					return Res.GetString("ed03f469-bd52-4612-97a2-795b37bd6e21", "[*Shipper reference is missing.*]");
				}
				else if (targetBO == null)
				{
					if (documentPurposeCode == MessagePurposes.Codes.Withdrawal)
					{
						return Res.GetString("81b8cb37-72cc-433b-9c77-c8074759c974", "[*No booking matched for Booking Request Withdrawal message. Booking Request Withdrawal rejected.*]");
					}

					if (documentPurposeCode == MessagePurposes.Codes.Amendment)
					{
						return Res.GetString("8371ef0d-a204-4234-afd4-7f6cf6dbb035", "[*No booking matched for Booking Request Amendment message. Booking Request Amendment rejected.*]");
					}
				}

				if (targetBO?.Booking != null)
				{
					if (documentPurposeCode == MessagePurposes.Codes.Original)
					{
						if (targetBO.Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.ElectronicBooking
							&& targetBO.Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.Booked
							&& targetBO.Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.BookingRejected)
						{
							return Res.GetString("c68b8471-9c6b-4f2b-8669-d7fff504b903", "[*Forwarder Booking has already been processed.*]");
						}

						if (targetBO.Booking.JS_IsForwardRegistered)
						{
							return Res.GetString("4780ce86-84e8-4c3f-b46d-10e257f38279", "[*Booking Request original message cannot be processed once Booking is converted to Shipment. Booking rejected.*]");
						}

						if (targetBO.Booking.JS_BookingReference != agentsReference)
						{
							return Res.GetString("a27e70e0-d0e0-4705-b3f4-dc7be6508aee",
								"[*Booking Request message cannot update Booking {0} because Shipper Reference in the message {1} is different to one in the Booking. Booking rejected.*]",
								coLoadBookingConfirmationReference,
								agentsReference);
						}

						if (targetBO.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.Booked)
						{
							return Res.GetString("b630c316-880c-426f-846a-0b03b2132ca9", "[*Booking Request original cannot be processed once Booking is confirmed (Booking Status BKD). Booking rejected.*]");
						}

						if (!coLoadBookingConfirmationReference.IsEmpty
							&& targetBO.Booking.JS_UniqueConsignRef != coLoadBookingConfirmationReference)
						{
							return Res.GetString("41289e87-39bc-4738-b580-396ac401a683",
								"[*Booking message with Booking Number {0} cannot find a matching Booking to update. Booking rejected.*]",
								coLoadBookingConfirmationReference);
						}
					}

					if (documentPurposeCode == MessagePurposes.Codes.Withdrawal)
					{
						if (targetBO.Booking.JS_IsForwardRegistered)
						{
							return Res.GetString("b81a3f25-c11d-4b19-8080-d21312a6b746", "[*Booking Request Withdrawal message cannot be processed once Booking is converted to Shipment. Booking Request Withdrawal rejected.*]");
						}

						if (targetBO.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingRejected)
						{
							return Res.GetString("fd2c0a2c-bca1-436a-8de7-891222705342", "[*Booking Request Withdrawal cannot be processed once Booking is rejected. Booking Request Withdrawal rejected.*]");
						}

						if (targetBO.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.ElectronicShippingInstruction)
						{
							return Res.GetString("1cbf7cc0-aef4-4288-8cb8-b0c99265cf6a", "[*Booking Request Withdrawal cannot be processed once Shipping Instruction is received. Booking Request Withdrawal rejected.*]");
						}

						if (targetBO.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.Confirmed)
						{
							return Res.GetString("977c74a7-85fc-407e-b7f2-5ff4a7e4bfe5", "[*Booking Request Withdrawal cannot be processed once Shipping Instruction is accepted. Booking Request Withdrawal rejected.*]");
						}

						if (targetBO.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.SIRejected)
						{
							return Res.GetString("db95a94b-6ff4-42bb-a24f-841f1627ebdd", "[*Booking Request Withdrawal cannot be processed once Shipping Instruction is rejected. Booking Request Withdrawal rejected.*]");
						}
					}

					if (documentPurposeCode == MessagePurposes.Codes.Amendment)
					{
						if (targetBO.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled)
						{
							return Res.GetString("6A9680D3-9186-4444-9517-0F8881B91FC8", "[*Booking Request amendment message cannot be accepted once Booking is Canceled. Amendment rejected.*]");
						}
						if (targetBO.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
						{
							return Res.GetString("6022A004-3354-4E66-8C61-A6DB140286D6", "[*Booking Request amendment message cannot be accepted once Booking Withdrawal/Cancellation request is in progress. Amendment rejected.*]");
						}

						if (targetBO.Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.ElectronicBooking
							&& targetBO.Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.Booked
							&& targetBO.Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.BookingRejected
							&& targetBO.Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.Amendment)
						{
							return Res.GetString("46d9414f-dc8b-4edb-a8c3-8fac3c0c9229", "[*Booking Request amendment message cannot be accepted once Shipping Instruction is processed. Amendment rejected.*]");
						}

						if (targetBO.Booking.JS_IsForwardRegistered
							&& targetBO.Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.Booked
							&& targetBO.Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.Amendment)
						{
							return Res.GetString("6118b2a7-fc64-4c27-9413-8055c43a7471", "[*Booking Amendment message cannot be processed once Booking is converted to Shipment. Amendment rejected.*]");
						}

						if (targetBO.Booking.JS_BookingReference != agentsReference
							|| targetBO.Booking.JS_UniqueConsignRef != coLoadBookingConfirmationReference)
						{
							return Res.GetString("15b37274-a69c-42ce-be4f-fb03cae155dc",
								"[*Booking Amendment message with Booking Number {0} cannot find a matching Booking to update, amendment rejected.*]",
								coLoadBookingConfirmationReference);
						}
					}

					if (documentPurposeCode == MessagePurposes.Codes.Withdrawal)
					{
						if (targetBO.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled)
						{
							return Res.GetString("9605D88F-D1F3-4FE3-8FF8-CD15190AA815", "[*Booking Request Withdrawal/Cancellation message cannot be accepted once Booking is already Canceled. Withdrawal/Cancellation rejected.*]");
						}
						if (targetBO.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
						{
							return Res.GetString("D7343745-49D4-4B07-9A3D-367EDE8F7BD5", "[*Booking Request Withdrawal/Cancellation message cannot be accepted once Booking Withdrawal/Cancellation request is already in progress. Withdrawal/Cancellation rejected.*]");
						}
					}

					if (targetBO.Booking.JS_UniqueConsignRef == coLoadBookingConfirmationReference
						&& !NVOCCBookingQueryHelper.IsBookingPartyMatched(targetBO.Booking, bookingPartyPK, bookingPartyName))
					{
						return Res.GetString("180947d8-d008-40aa-b275-29c38af3da8f",
							"[*Booking Request message is received with a wrong Booking Party {0}. Booking rejected.*]",
							bookingPartyName);
					}
				}
			}
			else if (targetBO?.Booking is ForwardingShipment booking && booking.JS_IsForwardRegistered)
			{
				return Res.GetString("e7f12e0b-ff14-4d94-8a6e-b22f723c6267",
					"[*XML Targeted a converted Booking. Target Type should be {0} for updating converted Bookings.*]",
					nameof(DataContextType.ForwardingShipment));
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		protected override void PopulateBusinessObject(QuotedBooking bookingBO)
		{
			var shipmentBO = bookingBO.Booking;
			var supportDataImportingShipment = (ISupportDataImporting)shipmentBO;

			supportDataImportingShipment.IsImportingData = true;

			try
			{
				var complianceUniversalDataObjectReader = new ComplianceUniversalDataObjectReader(bookingBO);
				complianceUniversalDataObjectReader.InitializeComplianceMaterialChangesSnapshotIfNeeded();

				if (dataObject.IsCO2eResponse())
				{
					var previousCO2eValue = (TotalCO2e: bookingBO.GetTotalCO2e(),
											Transports: bookingBO.Booking.Transports ?? Enumerable.Empty<BusinessObject>());
					bookingBO.GetResponseImporter(bookingBO.Factory, logger).ImportGreenHouseGasEmission(dataObject, bookingBO, GetBusinessObjectHumanReadableName(bookingBO), previousCO2eValue);
					return;
				}

				var oldShipmentStatus = shipmentBO.JS_ShipmentStatus;
				var purposeCode = dataObject.DataContext?.DocumentaryOverride?.Purpose.GetCodeAsUpperCase() ?? ZString.Empty;

				var newShipmentStatus = string.Empty;
				var newShipmentStatusReason = string.Empty;
				if (shipmentBO.JS_IsForwardRegistered && purposeCode == MessagePurposes.Codes.Amendment)
				{
					newShipmentStatus = ShipmentStatusList.Codes.Amendment;
					newShipmentStatusReason = (NoResString)"Booking Request Amendment Received"; // Event Parameter Constant
				}
				else if (purposeCode == MessagePurposes.Codes.Withdrawal)
				{
					newShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
					newShipmentStatusReason = (NoResString)"Electronic Booking Cancellation Request Received"; // Event Parameter Constant
				}
				else
				{
					newShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
					newShipmentStatusReason = (NoResString)"Electronic Booking Request Received"; // Event Parameter Constant
				}

				readingHelper.SetDate = dateDataObject =>
				{
					if (dateDataObject.Type == DateType.ClientRequestedETA)
					{
						SetValue(shipmentBO, JobShipmentSchema.JS_ClientRequestedETA, dateDataObject.Value);
					}
				};

				SetValue(shipmentBO, JobShipmentSchema.JS_ReleaseType, dataObject.ReleaseType);

				readingHelper.PopulateOrders(shipmentBO);
				readingHelper.PopulateBusinessObject(shipmentBO, IsNVOCC && !IsNewBO);

				if (IsNVOCC)
				{
					SetValue(shipmentBO, JobShipmentSchema.JS_BookingReference, dataObject.AgentsReference);
					SetValue(shipmentBO, JobShipmentSchema.JS_ShipmentStatus, newShipmentStatus);
				}

				var localProcessing = dataObject.LocalProcessing;
				if (localProcessing != null)
				{
					SetupDocsAndCartage(shipmentBO, localProcessing);
				}

				SetValue(shipmentBO, JobShipmentSchema.JS_AWBServiceLevel, dataObject.AWBServiceLevel);
				SetValue(shipmentBO, JobShipmentSchema.JS_PL_NKCarrierServiceLevel, dataObject.CarrierServiceLevel);
				SetupDischargeLoading(bookingBO);
				PopulateWorkflowCustomFields(bookingBO, dataObject);
				SetupCollections(bookingBO, shipmentBO);
				SetCommodity(bookingBO);

				SetValue(shipmentBO, JobShipmentSchema.JS_ActualChargeable, dataObject.ActualChargeable);
				SetValue(shipmentBO, JobShipmentSchema.JS_DocumentedChargeable, dataObject.DocumentedChargeable);
				SetValue(shipmentBO, JobShipmentSchema.JS_ManifestedChargeable, dataObject.ManifestedChargeable);

				SetValue(shipmentBO, JobShipmentSchema.JS_HBLAWBChargesDisplay, dataObject.HBLAWBChargesDisplay);
				SetValue(shipmentBO, JobShipmentSchema.JS_ShippedOnBoard, dataObject.ShippedOnBoard);

				SetValue(shipmentBO, JobShipmentSchema.JS_HBLContainerPackModeOverride, dataObject.HBLContainerPackModeOverride);

				SetValue(shipmentBO, JobShipmentSchema.JS_ActualChargeable, shipmentBO.JS_ActualChargeable);
				SetValue(shipmentBO, JobShipmentSchema.JS_DocumentedChargeable, shipmentBO.JS_DocumentedChargeable);
				SetValue(shipmentBO, JobShipmentSchema.JS_ManifestedChargeable, shipmentBO.JS_ManifestedChargeable);
				SetValue(shipmentBO, JobShipmentSchema.JS_CompanyTariffLevelOverride, dataObject.CompanyTariffLevelOverride);
				PopulateCarrierContractNumber(shipmentBO);
				PopulateInspectionType(shipmentBO);

				if (IsNVOCC && oldShipmentStatus != shipmentBO.JS_ShipmentStatus)
				{
					var parameters = new List<KeyValuePair<string, string>>
					{
						new KeyValuePair<string, string>(Params.New, newShipmentStatus),
						new KeyValuePair<string, string>(Params.Reason, newShipmentStatusReason),
						new KeyValuePair<string, string>(Params.Type, (NoResString)"Shipment Status") // Event Parameter Constant
					};

					if (shipmentBO.IsInDatabase)
					{
						parameters.Add(new KeyValuePair<string, string>(Params.Old, oldShipmentStatus));
					}

					var purposeDescription = dataObject.DataContext?.DocumentaryOverride?.Purpose?.Description ?? ZString.Empty;
					shipmentBO.Logs.CreateOrRecreateEventLog(Events.StatusUpdated,
						EstimateActual.Actual,
						ZDateTimeOffset.Now,
						purposeDescription,
						parameters.ToArray());
				}

				complianceUniversalDataObjectReader.SynchronizeComplianceRiskStatusIfNeeded();
			}
			finally
			{
				supportDataImportingShipment.IsImportingData = false;
			}
		}

		void SetCommodity(QuotedBooking bookingBO)
		{
			bookingBO.Commodity = dataObject.RateCommodity?.Code ?? ZString.Empty;
			bookingBO.FMCTariffID = dataObject.FMCTariffID ?? ZString.Empty;
		}

		bool? isNVOCC;
		bool IsNVOCC => (isNVOCC ?? (isNVOCC = dataObject.DataContext?.RecipientRoleCollection?.Any(c => c.Code == RecipientRoleType.NVO && c.ServiceCode == ServiceCodeType.BRQ) ?? false)).Value;

		void PopulateCarrierContractNumber(ForwardingShipment shipmentBO)
		{
			if (dataObject.CarrierContractNumber.HasValue)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_CarrierContractNumber, dataObject.CarrierContractNumber);
			}
			else
			{
				var carrierContractNumberReference = dataObject
					.AdditionalReferenceCollection?
					.FirstOrDefault(number => (string)number.Type?.Code == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON);

				if (carrierContractNumberReference != null)
				{
					SetValue(shipmentBO, JobShipmentSchema.JS_CarrierContractNumber, carrierContractNumberReference.ReferenceNumber);
				}
			}
		}

		void PopulateInspectionType(ForwardingShipment shipmentBO)
		{
			if (dataObject.AviationSecurityInspectionType is null)
			{
				return;
			}
			var code = (ZString)dataObject.AviationSecurityInspectionType.Code;
			if (string.IsNullOrEmpty(code) || code == BaseJobShipmentLookups.InspectionType_Approved)
			{
				return;
			}
			shipmentBO.JS_InspectionTypeCode = code;
		}

		protected override IModuleMatcher<IShipmentDataObjectReader, QuotedBooking> GetReferenceAndPartyIDMatcher(UniversalObjectFactory factory)
		{
			var matcher = new ForwardingBookingModuleMatcher(factory);

			matcher.AddPossibleMatchReferenceAndJobDocAddress(ReferenceElementName.BookingConfirmationReference, JobShipmentSchema.JS_BookingReference, MatchableOrganizationType.ConsignorDocumentaryAddress, DocAddressType.ConsignorDocumentaryAddress, new Score() { FullMatch = 90, ReferenceOnlyMatch = 10, Conflict = 0 }, OrganisationTypes.Consignor);
			matcher.AddPossibleMatchReferenceAndJobDocAddress(ReferenceElementName.BookingConfirmationReference, JobShipmentSchema.JS_BookingReference, MatchableOrganizationType.ConsigneeDocumentaryAddress, DocAddressType.ConsigneeDocumentaryAddress, new Score() { FullMatch = 90, ReferenceOnlyMatch = 10, Conflict = 0 }, OrganisationTypes.Consignee);

			matcher.AddPossibleMatchReferenceAndJobDocAddress(ReferenceElementName.InterimReceiptNumber, JobShipmentSchema.JS_InterimReceipt, MatchableOrganizationType.ConsignorDocumentaryAddress, DocAddressType.ConsignorDocumentaryAddress, new Score() { FullMatch = 90, ReferenceOnlyMatch = 10, Conflict = 0 }, OrganisationTypes.Consignor);

			matcher.AddPossibleMatchReferenceAndOrgHeaderPKs(ReferenceElementName.WayBillNumber, JobShipmentSchema.JS_HouseBill, MatchableOrganizationType.SendingForwarderAddress, new ForwarderFinder(factory).GetSendingForwarderPKs, new Score() { FullMatch = 180, ReferenceOnlyMatch = 30, Conflict = -180 });

			return matcher;
		}

		protected override IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelismCore()
		{
			var references = readingHelper.GenerateShipmentReference();
			return new QuotedBookingMatcher(this.factory.BOFactory, references, logger, helper).GetMatchingShipmentKeys(references);
		}

		#region Implementation

		void SetupContainerCollection(QuotedBooking bookingBO)
		{
			if (dataObject.ContainerCollection.Count == 0 && dataObject.ContainerCollection.Content != CollectionContent.Partial)
			{
				bookingBO.QuotedBookingContainers.RemoveAndDeleteAll();
				return;
			}

			var containersInsertedOrUpdated = new HashSet<ZGuid>();
			ForwardingContainer findContainer(Container containerDO)
			{
				var containerNumber = containerDO.ContainerNumber.GetValueOrDefault();
				var containerType = containerDO.ContainerType.GetCodeAsUpperCase();

				foreach (ForwardingContainer container in bookingBO.QuotedBookingContainers)
				{
					if (!containersInsertedOrUpdated.Contains(container.PK))
					{
						if (!containerNumber.IsEmpty && container.JC_ContainerNum == containerNumber)
						{
							return container;
						}

						if (container.RefContainer != null
							&& container.RefContainer.RC_Code == containerType)
						{
							return container;
						}
					}
				}

				return null;
			}

			foreach (var containerDataObject in dataObject.ContainerCollection)
			{
				var containerBO = new ContainerDataObjectReader<ForwardingContainer>(containerDataObject, logger, factory, findContainer).ReadIntoBusinessObject();

				bookingBO.QuotedBookingContainers.Add(containerBO);
				containersInsertedOrUpdated.Add(containerBO.PK);
			}

			if (dataObject.ContainerCollection.Content != CollectionContent.Partial)
			{
				RemoveUnnecessaryContainers(bookingBO.QuotedBookingContainers, containersInsertedOrUpdated);
			}
		}

		void SetupDocsAndCartage(ForwardingShipment shipmentBO, LocalProcessing localProcessing)
		{
			var docsBO = shipmentBO.DocsAndCartage as JobDocsAndCartage ?? JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);

			SetValue(docsBO, JobDocsAndCartageSchema.JP_EstimatedPickup, localProcessing.EstimatedPickup);
			SetValue(docsBO, JobDocsAndCartageSchema.JP_PickupRequiredBy, localProcessing.PickupRequiredBy);
			SetValue(docsBO, JobDocsAndCartageSchema.JP_EstimatedDelivery, localProcessing.EstimatedDelivery);
			SetValue(docsBO, JobDocsAndCartageSchema.JP_DeliveryRequiredBy, localProcessing.DeliveryRequiredBy);
			SetValue(docsBO, JobDocsAndCartageSchema.JP_InsuranceRequired, localProcessing.InsuranceRequired);
			SetValue(docsBO, JobDocsAndCartageSchema.JP_FCLPickupEquipmentNeeded, localProcessing.FCLPickupEquipmentNeeded);
			SetValue(docsBO, JobDocsAndCartageSchema.JP_FCLDeliveryEquipmentNeeded, localProcessing.FCLDeliveryEquipmentNeeded);

			if (localProcessing.OrderNumberCollection != null)
			{
				var currentList = docsBO.OrderItems;
				var incomingList = localProcessing.OrderNumberCollection;

				foreach (var incomingOrder in incomingList)
				{
					docsBO.OrderItems.Add(new OrderNumberDataObjectReader(incomingOrder, logger, factory, docsBO).ReadIntoBusinessObject());
				}

				for (int index = currentList.Count - 1; index >= 0; index--)
				{
					var orderItem = currentList[index];
					if (!incomingList.Any(incomingOrder => incomingOrder.OrderReference.GetValueOrDefault() == orderItem.JT_OrderReference))
					{
						currentList.RemoveAndDelete(orderItem);
					}
				}
			}

			if (localProcessing.AdditionalServiceCollection != null)
			{
				var additionalservicesCollectionReader = new AdditionalServiceDataObjectCollectionReader(localProcessing.AdditionalServiceCollection, logger, factory, docsBO);
				additionalservicesCollectionReader.ReadIntoCollection();
			}
		}

		void SetupCollections(QuotedBooking bookingBO, ForwardingShipment shipmentBO)
		{
			if (dataObject.NoteCollection != null)
			{
				if (bookingBO.CustomNoteTypesDelegate == null)
				{
					bookingBO.CustomNoteTypesDelegate = new GetValueDelegate<NoteTypeCollection>(delegate
					{ return CustomNotesProvider.Instance.CustomNoteTypesForModuleAndAllCountries(ModuleIDs.QuotedBookings.Name); });
				}

				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, shipmentBO, masterBizoNoteTypes: bookingBO.NoteTypes).ReadIntoCollection();
			}

			if (dataObject.ContainerCollection != null)
			{
				SetupContainerCollection(bookingBO);
			}

			if (dataObject.TransportLegCollection != null)
			{
				SetupCompatibleTransportLeg(bookingBO);
			}
		}

		void SetupCompatibleTransportLeg(QuotedBooking bookingBO)
		{
			var transportLeg = GetCompatibleLegToLink(bookingBO);

			if (transportLeg != null)
			{
				new ForwardingBookingTransportLegDataObjectReader(transportLeg, logger, factory, bookingBO, dataObject).ReadIntoBusinessObject();
			}
		}

		TransportLeg GetCompatibleLegToLink(QuotedBooking bookingBO)
		{
			if (dataObject.TransportLegCollection.Count == 0)
			{
				return null;
			}

			var transportModeConverter = new TransportModeConverter();
			var legsMatchedByMode = dataObject.TransportLegCollection
				.Where(t => transportModeConverter.FromEnumValue(t.TransportMode) == bookingBO.TransportMode)
				.ToArray();

			if (legsMatchedByMode.Length == 1)
			{
				return legsMatchedByMode.First();
			}

			if (legsMatchedByMode.Length > 1)
			{
				if (legsMatchedByMode.FirstOrDefault(l => l.LegType == LegType.Main) is TransportLeg leg)
				{
					logger.Log(LogType.Information, "There are multiple compatible legs. The Main leg has been selected to try link to QuotedBooking.");
					return leg;
				}

				logger.Log(LogType.Information, "There are multiple compatible legs. The first compatible leg has been selected to try link to QuotedBooking.");
				return legsMatchedByMode.First();
			}

			logger.Log(LogType.Information, "No leg is compatible to link to QuotedBooking.");
			return null;
		}

		static void RemoveUnnecessaryContainers(QuotedBookingContainerDependentCollection containers, HashSet<ZGuid> containersInsertedOrUpdated)
		{
			foreach (var container in containers.ToArray())
			{
				if (!containersInsertedOrUpdated.Contains(container.PK))
				{
					containers.RemoveAndDelete(container);
				}
			}
		}

		void SetupDischargeLoading(QuotedBooking bookingBO)
		{
			var portOfLoading = dataObject.PortOfLoading.GetUNLOCOAsUpperCase(factory.BOFactory);
			var portOfDischarge = dataObject.PortOfDischarge.GetUNLOCOAsUpperCase(factory.BOFactory);

			if (!portOfLoading.IsEmpty || !portOfDischarge.IsEmpty)
			{
				bookingBO.DischargePort = portOfDischarge;
				bookingBO.LoadPort = portOfLoading;
			}
		}

		protected override bool ModuleHasReferenceAndPartyIDMatchingEnabled => base.ModuleHasReferenceAndPartyIDMatchingEnabled && !IsNVOCC;

		class ForwarderFinder
		{
			public ForwarderFinder(UniversalObjectFactory factory)
			{
				this.factory = factory;
			}
			readonly UniversalObjectFactory factory;

			internal ZGuid[] GetSendingForwarderPKs(ForwardingShipment matchingBO)
			{
				var companyQuery = new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True);
				companyQuery.AddToFilter(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, ZGuid.Empty);

				var companies = factory.Load<GlbCompany>(companyQuery);
				return companies.Select(o => o.GC_OH_OrgProxy).ToArray();
			}
		}

		#endregion
	}
}
