using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.DataTransfer.Freight.Universal;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ShipmentDataObjectReader : BaseShipmentDataObjectReader<ForwardingShipment>
	{
		public ShipmentDataObjectReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ChildShipmentsParent parent, IShipmentDataObjectReader parentReader = null, IUniversalFreightHelper helper = null)
			: this(shipmentDataObject, logger, factory, parent, new ContainerLinkManager<ForwardingConsol>(null), parentReader, helper)
		{
		}

		public ShipmentDataObjectReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ChildShipmentsParent parent, IContainerLinkManager<ForwardingConsol> linkManager, IShipmentDataObjectReader parentReader = null, IUniversalFreightHelper helper = null)
			: base(shipmentDataObject, logger, factory, helper ?? new UniversalForwardingHelper())
		{
			this.parent = parent;
			this.linkManager = Argument.NotNull(linkManager, "linkManager");
			readingHelper = new ShipmentDataObjectReadingHelper(dataObject, logger, factory);
			this.parentReader = parentReader;
		}

		readonly ChildShipmentsParent parent;
		IContainerLinkManager<ForwardingConsol> linkManager;
		readonly ShipmentDataObjectReadingHelper readingHelper;
		readonly IShipmentDataObjectReader parentReader;

		protected override ForwardingShipment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var wayBillNumber = dataObject.WayBillNumber.GetValueOrDefault();
			if (parent != null
				&& dataObject.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House
				&& !wayBillNumber.IsEmpty)
			{
				foreach (var shipment in parent.Shipments)
				{
					if (shipment.JS_HouseBill == wayBillNumber)
					{
						return shipment;
					}
				}
			}

			if (dataObject.AdditionalReferenceCollection != null)
			{
				var found = dataObject.AdditionalReferenceCollection
					.Where(reference => !(reference.ReferenceNumber?.IsEmpty ?? true))
					.FirstOrDefault(reference => (reference.Type?.Code ?? ZString.Empty) == (ZString)WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);

				if (found != null)
				{
					return factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, found.ReferenceNumber.Value));
				}
			}

			return null;
		}

		string EmptyBranchExceptionMessage => Res.GetString("7AE73BC9-FCB3-5DB3-9797-6BC27C1D7FA9", "Unable to import shipment. Job Branch cannot be empty. Please check branch defaulting configuration.");

		protected override void PopulateFromTopLevelObject(ForwardingShipment shipmentBO)
		{
			base.PopulateFromTopLevelObject(shipmentBO);

			if (shipmentBO?.ShipmentJobHeader?.JH_GB.IsEmpty ?? false)
			{
				throw new DataObjectReadFailureException(EmptyBranchExceptionMessage);
			}
		}

		protected override string GetReasonForFailedToGetBusinessObjectFromContextKey(IDataTargetDataObject dataTarget)
		{
			var baseReason = base.GetReasonForFailedToGetBusinessObjectFromContextKey(dataTarget);

			return dataObject.IsCargoReceiptAdviceMessage() ? ("[*" + baseReason + "*]") : baseReason;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(ForwardingShipment targetBO)
		{
			var reason = GetReasonForNotAbleToUpdate(targetBO);

			return reason.IsEmpty
				? base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO)
				: reason;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reject Reasons")]
		public ZString GetReasonForNotAbleToUpdate(ForwardingShipment targetBO)
		{
			if (dataObject.IsVGM())
			{
				if (targetBO == null)
				{
					return Res.GetString("e0476076-3f72-4021-895a-c6b9f06c9022", "XML file contains {0} service code and cannot find a matched shipment.", ServiceCodesList.Codes.VerifiedGrossContainerWeight);
				}

				if (dataObject.ContainerCollection == null || dataObject.ContainerCollection.Count == 0)
				{
					return Res.GetString("98a9a565-af6a-4685-a34f-1bd174875fc5", "XML file does not contain any containers.");
				}

				if (dataObject.ContainerCollection.Any(c => c.ContainerNumber.GetValueOrDefault().IsEmpty))
				{
					return Res.GetString("ea57f978-705a-4f82-a490-8b6fd63df6c6", "The container number of one or multiple specified containers is empty.");
				}

				var reason = GetReasonForNotAllContainersApplicableForVGM(targetBO);
				if (!reason.IsEmpty)
				{
					return reason;
				}
			}

			if (dataObject.HasVGMSection())
			{
				var vgmValidationResult = dataObject.ValidateVGMProperties();
				if(!vgmValidationResult.IsEmpty)
				{
					return vgmValidationResult;
				}
			}

			if (IsNVOCC)
			{
				if (dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress)) == null)
				{
					return Res.GetString("193cf597-7878-4395-9bc3-08364a1edad8", "[*Shipping instruction message received without Booking Party, Booking Party is mandatory to process the Shipping instruction, message rejected.*]");
				}

				var coLoadBookingConfirmationReference = dataObject.CoLoadBookingConfirmationReference.GetValueOrDefault();
				if (coLoadBookingConfirmationReference.IsEmpty)
				{
					return Res.GetString("34e0ea59-1f20-47ae-9c4e-401421a1e8e2", "[*Shipping instruction message received without Booking Number, Booking Number is mandatory to process the Shipping instruction, message rejected.*]");
				}

				var agentsReference = dataObject.AgentsReference.GetValueOrDefault();
				if (agentsReference.IsEmpty)
				{
					return Res.GetString("d215fe3d-64c5-42a0-9ef6-fb7660fb98d4", "[*Shipping instruction message received without Shipper's Reference, Shipper's Reference is mandatory to process the Shipping instruction, message rejected.*]");
				}

				var documentPurposeCode = dataObject.DataContext?.DocumentaryOverride?.Purpose.GetCodeAsUpperCase() ?? ZString.Empty;
				var coLoadMasterBillNumber = dataObject.CoLoadMasterBillNumber.GetValueOrDefault();
				if (documentPurposeCode == MessagePurposes.Codes.Amendment && coLoadMasterBillNumber.IsEmpty)
				{
					return Res.GetString("703865e3-9a73-42e5-b31c-eedb968f6a23",
						"[*Shipping instruction Amendment message received without Bill Number, Bill Number is mandatory to process the Shipping instruction Amendment, message rejected.*]");
				}

				var bookingPartyPK = dataObject.GetBookingParty(logger, factory);
				var bookingPartyName = dataObject.GetBookingPartyName();

				if (bookingPartyPK.IsEmpty)
				{
					return Res.GetString("0c32c33b-6cd6-4635-8bfe-6dfdb39bcbf7",
						"[*Shipping instruction message received from an unknown Booking Party {0}, message rejected.*]",
						bookingPartyName);
				}

				if (targetBO == null)
				{
					return Res.GetString("e7bb610d-f6d4-4b63-8fd0-dd8a2fbd8de2",
						"[*Shipping instruction message with Booking Number {0} cannot find a matching Booking, message rejected.*]",
						coLoadBookingConfirmationReference);
				}
				else
				{
					if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled)
					{
						return Res.GetString("BCB8A7EE-4C47-4D9C-979C-DD13A8CD54F9", "[*Shipping Instruction message cannot be accepted once Booking is Canceled. message rejected.*]");
					}
					if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
					{
						return Res.GetString("D802015F-4394-466F-A014-D58AA56DB8DC", "[*Shipping Instruction message cannot be accepted once Booking Withdrawal/Cancellation request is in progress. message rejected.*]");
					}

					if (targetBO.JS_UniqueConsignRef != coLoadBookingConfirmationReference)
					{
						return Res.GetString("9fe46ea1-7d1d-44a6-869c-b3504b8d7121",
							"[*Shipping instruction message with Booking Number {0} cannot find a matching Booking, message rejected.*]",
							coLoadBookingConfirmationReference);
					}

					if (documentPurposeCode == MessagePurposes.Codes.Original)
					{
						if (targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.Booked
							&& targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.Confirmed
							&& targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.ElectronicShippingInstruction
							&& targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.SIRejected
							&& targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.Amendment)
						{
							return Res.GetString("2d5d66a9-4029-4bfe-ac84-4174eba9e073",
								"[*Shipping instruction cannot be processed, because Booking {0} is not confirmed by carrier, message rejected.*]",
								coLoadBookingConfirmationReference);
						}

						if (!coLoadMasterBillNumber.IsEmpty && targetBO.JS_HouseBill != coLoadMasterBillNumber)
						{
							return Res.GetString("c14e9420-37db-4c79-8c54-bf1cb6f4b3db",
								"[*Shipping instruction Original message with Bill Number {0} cannot find a matching Shipment to update, message rejected.*]",
								coLoadMasterBillNumber);
						}
					}

					if (documentPurposeCode == MessagePurposes.Codes.Amendment)
					{
						if (targetBO.JS_HouseBill != coLoadMasterBillNumber)
						{
							return Res.GetString("ad435042-b9a8-4eee-8237-2e4a4596e10c",
								"[*Shipping instruction Amendment message with Bill Number {0} cannot find a matching Shipment to update, message rejected.*]",
								coLoadMasterBillNumber);
						}

						if (targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.Confirmed
							&& targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.SIRejected)
						{
							return Res.GetString("1d57c970-af14-45fa-97ee-632af07394fb",
								"[*Shipping instruction Amendment message with Bill Number {0} cannot process. Shipping instruction not confirmed, message rejected.*]",
								coLoadMasterBillNumber);
						}
					}

					if (!NVOCCBookingQueryHelper.IsBookingPartyMatched(targetBO, bookingPartyPK, bookingPartyName))
					{
						return Res.GetString("2e8a536a-ee4b-4b31-8465-db3ba30f0659",
								"[*Shipping instruction message received with a wrong Booking Party {0}, message rejected.*]",
								bookingPartyName);
					}
				}
			}
			else if (IsUnconvertedBooking(targetBO) && parent?.Consol == null)
			{
				return Res.GetString("614c4a15-868e-4f32-ae96-85249b00fb5e",
					"[*Matching {0} is in the {1} state, incorrect {2} found.*]",
					nameof(DataContextType.ForwardingShipment),
					nameof(DataContextType.ForwardingBooking),
					"DataTarget");
			}

			return ZString.Empty;
		}

		bool? isNVOCC;
		bool IsNVOCC => (isNVOCC ?? (isNVOCC = dataObject.IsNVOCC())).Value;

		ZString GetReasonForNotAllContainersApplicableForVGM(ForwardingShipment shipment)
		{
			if (dataObject.ContainerCollection != null)
			{
				var consols = shipment.Consols.Cast<ForwardingConsol>().ToArray();

				var dataObjectContainerNumbers = dataObject.ContainerCollection
					.Select(c => c.ContainerNumber.GetValueOrDefault());

				var consolContainers = new List<CommonContainer>();
				foreach (var consol in consols)
				{
					consolContainers.AddRange(consol.Containers.Cast<CommonContainer>());
				}

				var allContainersFound = dataObjectContainerNumbers
					.All(containerNumber => consolContainers.Any(c => c.JC_ContainerNum == containerNumber));
				if (!allContainersFound)
				{
					return Res.GetString("cc327a07-a017-4f22-ad0b-db85070ea318", "One or multiple specified containers cannot be found in the matched shipment's consol.");
				}

				if (consolContainers.Any(c => !VGMHelper.IsContainerModeApplicableForVGM(c.JC_ContainerMode)))
				{
					return Res.GetString("1e9b00fa-b62c-4a30-a2ec-80ad546270f0", "Container Mode should be FCL, GRP or BCN.");
				}
			}

			return ZString.Empty;
		}

		protected override IMatchingBusinessEntityFinder<ForwardingShipment> GetCombinedReferenceMatcher()
		{
			var references = readingHelper.GenerateShipmentReference();
			var shipmentMatcher = GetShipmentMatcher(references);

			if (IsConsolidatedShipment)
			{
				return new ForwardingShipmentOrBookingMatcher(shipmentMatcher, new BookingMatcher(factory.BOFactory, references, logger, helper));
			}

			return shipmentMatcher;
		}

		ForwardingShipmentMatcher GetShipmentMatcher(ShipmentReferences references) => new ForwardingShipmentMatcher(factory.BOFactory, references, logger, helper);

		protected override void PopulateBusinessObject(ForwardingShipment shipmentBO)
		{
			ISupportDataImporting supportDataImporting = shipmentBO;
			supportDataImporting.IsImportingData = true;

			try
			{
				TurnBookingIntoShipment(shipmentBO);

				try
				{
					PopulateBusinessObjectCore(shipmentBO);
				}
				catch (ExportAWBHeaderReplaceMacrosException e)
				{
					throw new DataObjectReadFailureException(e.Message);
				}
				catch (Exception e) when (e.InnerException is ExportAWBHeaderReplaceMacrosException)
				{
					throw new DataObjectReadFailureException(e.InnerException.Message);
				}
			}
			finally
			{
				supportDataImporting.IsImportingData = false;
			}
		}

		protected void PopulateBusinessObjectCore(ForwardingShipment shipmentBO)
		{
			var complianceUniversalDataObjectReader = new ComplianceUniversalDataObjectReader(shipmentBO);
			complianceUniversalDataObjectReader.InitializeComplianceMaterialChangesSnapshotIfNeeded();

			InitializeLinksManager(shipmentBO);

			if (dataObject.IsVGM())
			{
				if (dataObject.ContainerCollection != null && dataObject.ContainerCollection.Count > 0)
				{
					ReadInContainers(linkManager.Consol);
				}

				return;
			}

			if (dataObject.IsCO2eResponse())
			{
				var previousCO2eValue = (TotalCO2e: shipmentBO.GetTotalCO2e(),
										Transports: shipmentBO.Transports ?? Enumerable.Empty<BusinessObject>());
				shipmentBO.GetResponseImporter(shipmentBO.Factory, logger)?.ImportGreenHouseGasEmission(dataObject, shipmentBO, GetBusinessObjectHumanReadableName(shipmentBO), previousCO2eValue);
				return;
			}

			var oldShipmentStatus = shipmentBO.JS_ShipmentStatus;

			readingHelper.LinkManager = linkManager;

			if (parent != null)
			{
				AttachToParentConsol(shipmentBO);
			}

			readingHelper.SetDate = dateDataObject =>
			{
				switch (dateDataObject.Type)
				{
					case DateType.ShippedOnBoard:
						SetValue(shipmentBO, JobShipmentSchema.JS_ShippedOnBoardDate, dateDataObject.Value);
						break;
					case DateType.BillIssued:
						SetValue(shipmentBO, JobShipmentSchema.JS_HouseBillIssueDate, dateDataObject.Value);
						break;
					case DateType.PickupReceiptRequested:
						SetValue(shipmentBO, JobShipmentSchema.JS_ExportReceivingDepotReceiptRequested, dateDataObject.Value);
						break;
					case DateType.DeliveryReceiptRequested:
						SetValue(shipmentBO, JobShipmentSchema.JS_ImportReleaseDepotReceiptRequested, dateDataObject.Value);
						break;
					case DateType.PickupDispatchRequested:
						SetValue(shipmentBO, JobShipmentSchema.JS_ExportReceivingDepotDispatchRequested, dateDataObject.Value);
						break;
					case DateType.DeliveryDispatchRequested:
						SetValue(shipmentBO, JobShipmentSchema.JS_ImportReleaseDepotDispatchRequested, dateDataObject.Value);
						break;
				}
			};

			readingHelper.SetAdditionalAddresses = organisationCollection =>
			{
				var arrivalCFSAddress = organisationCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
				if (arrivalCFSAddress != null)
				{
					readingHelper.SetValue(shipmentBO, JobShipmentSchema.JS_OA_ImportReleaseDepot, null, arrivalCFSAddress);
					organisationCollection.Remove(arrivalCFSAddress);
				}

				var localCartageDeliveryCompanyAddress = organisationCollection.FirstOrDefault(AddressTypes.DeliveryLocalCartage) ?? organisationCollection.FirstOrDefault(nameof(DocAddressType.LocalCartageDeliverToAddress));
				if (localCartageDeliveryCompanyAddress != null)
				{
					readingHelper.SetValue(shipmentBO.DocsAndCartage, JobDocsAndCartageSchema.JP_OA_DeliveryCartageCoAddr, null, localCartageDeliveryCompanyAddress);
					organisationCollection.Remove(localCartageDeliveryCompanyAddress);
				}
			};

			SetValue(shipmentBO, JobShipmentSchema.JS_RH_NKRateCommodity, dataObject.RateCommodity?.Code ?? ZString.Empty);
			SetValue(shipmentBO, JobShipmentSchema.JS_FMCTariffID, dataObject.FMCTariffID);

			SetValue(shipmentBO, JobShipmentSchema.JS_AWBServiceLevel, dataObject.AWBServiceLevel);
			SetValue(shipmentBO, JobShipmentSchema.JS_CartageWaybill, dataObject.CartageWaybillNumber);

			SetValue(shipmentBO, JobShipmentSchema.JS_InterimReceipt, dataObject.InterimReceiptNumber);

			if (dataObject.IsNeutralMaster != null)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_IsNeutralMaster, dataObject.IsNeutralMaster.Value);
			}

			SetValue(shipmentBO, JobShipmentSchema.JS_IsSplitShipment, dataObject.IsSplitShipment);

			SetValue(shipmentBO, JobShipmentSchema.JS_IsHighRisk, dataObject.IsHighRisk);

			SetValue(shipmentBO, JobShipmentSchema.JS_ReleaseType, dataObject.ReleaseType);
			SetValue(shipmentBO, JobShipmentSchema.JS_ShipmentType, dataObject.ShipmentType);
			SetValue(shipmentBO, JobShipmentSchema.JS_ShippedOnBoard, dataObject.ShippedOnBoard);

			SetValue(shipmentBO, JobShipmentSchema.JS_HBLContainerPackModeOverride, dataObject.HBLContainerPackModeOverride);

			SetValue(shipmentBO, JobShipmentSchema.JS_WarehouseLocation, dataObject.WarehouseLocation);

			PopulateOrders(shipmentBO);

			FillCollections(shipmentBO);

			readingHelper.PopulateBusinessObject(shipmentBO);

			ReadInJobDocsAndCartageData(shipmentBO);

			PopulateShipmentStatus(shipmentBO);

			SetValue(shipmentBO, JobShipmentSchema.JS_HBLAWBChargesDisplay, dataObject.HBLAWBChargesDisplay);

			SetValue(shipmentBO, JobShipmentSchema.JS_NoCopyBills, dataObject.NoCopyBills);
			SetValue(shipmentBO, JobShipmentSchema.JS_NoOriginalBills, dataObject.NoOriginalBills);

			SetValue(shipmentBO, JobShipmentSchema.JS_ShipperCODAmount, dataObject.ShipperCODAmount);
			SetValue(shipmentBO, JobShipmentSchema.JS_ShipperCODPayMethod, dataObject.ShipperCODPayMethod);

			SetValue(shipmentBO, JobShipmentSchema.JS_TotalPackageCount, dataObject.TotalNoOfPacks);
			SetValue(shipmentBO, JobShipmentSchema.JS_F3_NKTotalCountPackType, dataObject.TotalNoOfPacksPackageType);

			SetValue(shipmentBO, JobShipmentSchema.JS_ManifestedVolume, dataObject.ManifestedVolume);
			SetValue(shipmentBO, JobShipmentSchema.JS_ManifestedWeight, dataObject.ManifestedWeight);
			SetValue(shipmentBO, JobShipmentSchema.JS_ManifestedChargeable, dataObject.ManifestedChargeable);

			SetValue(shipmentBO, JobShipmentSchema.JS_DocumentedVolume, dataObject.DocumentedVolume);
			SetValue(shipmentBO, JobShipmentSchema.JS_DocumentedWeight, dataObject.DocumentedWeight);
			SetValue(shipmentBO, JobShipmentSchema.JS_DocumentedChargeable, dataObject.DocumentedChargeable);
			SetValue(shipmentBO, JobShipmentSchema.JS_CompanyTariffLevelOverride, dataObject.CompanyTariffLevelOverride);
			SetValue(shipmentBO, JobShipmentSchema.JS_TranshipToOtherCFS, dataObject.TranshipToOtherCFS);
			SetValue(shipmentBO, JobShipmentSchema.JS_CommunityTransitStatus, dataObject.CommunityTransitStatus);
			SetValue(shipmentBO, JobShipmentSchema.JS_LoadingMeters, dataObject.TotalLoadingMeters);
			ImportDeclarationIfNeeded(shipmentBO);
			ImportUSInBondIfNeeded(shipmentBO);
			ImportAWBHeaderIfNeeded(shipmentBO);
			readingHelper.ImportAviationSecurityInspectionTypeIfNeeded(shipmentBO);
			readingHelper.ImportAviationSecurityAdditionalInspectionTypeIfNeeded(shipmentBO);
			ImportDocDataIfSupported(shipmentBO);
			ImportNVOCCDocData(shipmentBO);
			ImportSubHVLShipmentInspectionTypeCodeIfNeeded(shipmentBO);

			SetValue(shipmentBO, JobShipmentSchema.JS_ActualChargeable, shipmentBO.JS_ActualChargeable);
			SetValue(shipmentBO, JobShipmentSchema.JS_DocumentedChargeable, shipmentBO.JS_DocumentedChargeable);
			SetValue(shipmentBO, JobShipmentSchema.JS_ManifestedChargeable, shipmentBO.JS_ManifestedChargeable);

			if (linkManager.Consol == null
				&& shipmentBO.TransportsIncludingRelated.Count == 0)
			{
				SetLoadingAndDischargePorts(shipmentBO);
			}

			complianceUniversalDataObjectReader.SynchronizeComplianceRiskStatusIfNeeded();

			LogSTUEventForShipmentStatusChange(shipmentBO, oldShipmentStatus);
		}

		public void PopulateShipmentStatus(ForwardingShipment shipmentBO)
		{
			if (IsNVOCC)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusList.Codes.ElectronicShippingInstruction);
			}
			else if (!(dataObject.ShipmentStatus?.Code ?? ZString.Empty).IsEmpty)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_ShipmentStatus, dataObject.ShipmentStatus);
			}
		}

		public void LogSTUEventForShipmentStatusChange(ForwardingShipment shipmentBO, ZString oldShipmentStatus)
		{
			if (IsNVOCC)
			{
				var parameters = new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.ElectronicShippingInstruction),
					new KeyValuePair<string, string>(Params.Old, oldShipmentStatus),
					new KeyValuePair<string, string>(Params.Reason, (NoResString)"Electronic Shipping Instruction Received"), // Event Parameter Constant
					new KeyValuePair<string, string>(Params.Type, (NoResString)"Shipment Status") // Event Parameter Constant
				};
				var purpose = dataObject.DataContext?.DocumentaryOverride?.Purpose?.Description ?? ZString.Empty;
				shipmentBO.Logs.CreateOrRecreateEventLog(Events.StatusUpdated,
					EstimateActual.Actual,
					ZDateTimeOffset.Now,
					purpose,
					parameters);
			}

			if (dataObject.IsCargoReceiptAdviceMessage())
			{
				var parameters = new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(Params.MessageType, UniversalShipmentExtension.DocumentNameCargoReceiptAdvice),
				};

				var purpose = (NoResString)"from Carrier"; // Programmatic constant
				shipmentBO.Logs.CreateOrRecreateEventLog(Events.MessageReceived,
					EstimateActual.Actual,
					ZDateTimeOffset.UtcNow,
					purpose,
					parameters);
			}
		}

		#region Attach To Parent Consol if applicable

		void AttachToParentConsol(ForwardingShipment shipmentBO)
		{
			if (parent.Consol != null)
			{
				AttachToConsol(parent.Consol, shipmentBO);
			}
			else if (parent.Consol == null && linkManager.Consol != null)   //sub shipments aren't attached to the parent consol til later
			{
				AttachToConsol(linkManager.Consol, shipmentBO);
			}
		}

		void AttachToConsol(ForwardingConsol consol, ForwardingShipment shipmentBO)
		{
			if (IsUnconvertedBooking(shipmentBO))
			{
				var consolHelper = new BuildConsolHelper();
				consolHelper.AddBookingsToConsol(consol, new[] { shipmentBO.PK }, false);
			}
			else
			{
				consol.Shipments.Add(shipmentBO);
			}

			CheckConsolShipmentsLimitNotExceeded(consol);
			SetConsolContainersGrossWeight(consol);
		}

		bool IsUnconvertedBooking(ForwardingShipment shipmentBO) => shipmentBO != null && shipmentBO.JS_IsBooking && !shipmentBO.JS_IsForwardRegistered;

		void CheckConsolShipmentsLimitNotExceeded(ForwardingConsol consol)
		{
			var notification = new ShipmentsOnConsolLimitHelper(consol).CreateNotification();

			if (notification != null && notification.Type == CargoWise.ComponentModel.NotificationType.Error)
			{
				throw new DataObjectReadFailureException(notification.Message);
			}
		}

		public void TurnBookingIntoShipment(ForwardingShipment shipmentBO)
		{
			if (IsNVOCC && IsUnconvertedBooking(shipmentBO))
			{
				var helper = new BuildConsolHelper();
				helper.TurnBookingIntoShipment(shipmentBO, null, null);
			}
		}

		void SetConsolContainersGrossWeight(ForwardingConsol consol)
		{
			foreach (CommonContainer container in consol.Containers)
			{
				SetValue(container, JobContainerSchema.JC_GrossWeight, container.JC_GrossWeight);
				SetValue(container, JobContainerSchema.JC_GrossVolume, container.JC_GrossVolume);
			}
		}

		#endregion

		void SetLoadingAndDischargePorts(ForwardingShipment shipmentBO)
		{
			if (dataObject.PortOfLoading != null)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_RL_NKLoadPort, dataObject.PortOfLoading.Code);
			}
			if (dataObject.PortOfDischarge != null)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_RL_NKDischargePort, dataObject.PortOfDischarge.Code);
			}
		}

		void ImportUSInBondIfNeeded(ForwardingShipment shipmentBO)
		{
			if (dataObject.SubShipmentCollection != null && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				var inBondData = dataObject.SubShipmentCollection.FirstOrDefault(x => x.GetMatchingDataTarget(UniversalDataBuss.Integration.DataContextType.InBond) != null);
				if (inBondData != null)
				{
					var provider = ObjectFactory.Get<ICustomsInBondDataObjectReaderProvider>();
					var reader = provider.GetReader(inBondData, logger, factory, shipmentBO);
					BusinessObject inBondHeaderBO = null;
					reader.ReadIntoBusinessObject(ref inBondHeaderBO);
				}
			}
		}

		void ImportDeclarationIfNeeded(ForwardingShipment shipmentBO)
		{
			if (dataObject.GetMatchingDataTarget(UniversalDataBuss.Integration.DataContextType.CustomsDeclaration) != null ||
				dataObject.GetMatchingDataTarget(UniversalDataBuss.Integration.DataContextType.CustomsCommercialInvoice) != null)
			{
				var provider = ObjectFactory.Get<ICustomsShipmentDataObjectReaderProvider>();
				var reader = provider.GetReader(dataObject, logger, factory, shipmentBO);
				BusinessObject declarationBO = null;
				reader.ReadIntoBusinessObject(ref declarationBO);
			}
		}

		void ImportAWBHeaderIfNeeded(ForwardingShipment shipmentBO)
		{
			if (dataObject.CarrierDocumentsOverride != null && dataObject.CarrierDocumentsOverride.AWBHeader != null && !(shipmentBO.TemplateRecord?.IsForTemplateSearch ?? false))
			{
				new AWBHeaderDataObjectReader(dataObject.CarrierDocumentsOverride.AWBHeader, logger, factory, shipmentBO).ReadIntoBusinessObject();
			}
		}

		void ImportDocDataIfSupported(ForwardingShipment shipmentBO)
		{
			if (!UniversalShipment.IsDocDataRestricted && dataObject.DocData != null)
			{
				var docNote = DocumentNote.LoadNote(shipmentBO);
				if (docNote == null)
				{
					return;
				}

				if (dataObject.DocData.SystemDefinedDataCollection != null)
				{
					foreach (var systemDefinedData in dataObject.DocData.SystemDefinedDataCollection)
					{
						docNote.SetSystemDefinedFieldValue(systemDefinedData.Name, systemDefinedData.Value);
					}
				}

				if (dataObject.DocData.UserDefinedDataCollection != null)
				{
					foreach (var userDefinedData in dataObject.DocData.UserDefinedDataCollection)
					{
						docNote.SetFieldValue(userDefinedData.Name, userDefinedData.Value);
					}
				}
			}
		}

		#region ImportNVOCCDocData

		void ImportNVOCCDocData(ForwardingShipment shipmentBO)
		{
			if (IsNVOCC)
			{
				var docNote = DocumentNote.LoadNote(shipmentBO);
				if (docNote == null)
				{
					return;
				}

				var originPort = dataObject.PortOfOrigin?.Name ?? ZString.Empty;
				docNote.SetFieldValue((NoResString)"Origin Port - Place Of Receipt", originPort); // Doc Note Field Name

				var destinationPort = dataObject.PortOfDestination?.Name ?? ZString.Empty;
				docNote.SetFieldValue((NoResString)"Destination Port - Place Of Delivery", destinationPort); // Doc Note Field Name

				var portOfLoading = dataObject.PortOfLoading?.Name ?? ZString.Empty;
				docNote.SetFieldValue((NoResString)"Port Of Loading", portOfLoading); // Doc Note Field Name

				var portOfDischarge = dataObject.PortOfDischarge?.Name ?? ZString.Empty;
				docNote.SetFieldValue((NoResString)"Port Of Discharge", portOfDischarge); // Doc Note Field Name

				var placeOfIssue = dataObject.PlaceOfIssue?.Name ?? ZString.Empty;
				docNote.SetFieldValue((NoResString)"Place Of Issue", placeOfIssue); // Doc Note Field Name

				ZDateTime dateOfIssue = dataObject.DateCollection?.FirstOrDefault(d => d.Type.GetValueOrDefault() == DateType.BillIssued)?.Value ?? ZDateTime.Empty;
				docNote.SetFieldValue((NoResString)"Date Of Issue", dateOfIssue.ToShortDateString()); // Doc Note Field Name

				if (dataObject.OrganizationAddressCollection != null)
				{
					UpdateDocumentNoteFieldWithOrganizationAddress(docNote, (NoResString)"Consignor - Shipper", nameof(DocAddressType.ConsignorDocumentaryAddress), true, (NoResString)"Consignor - Shipper Phone"); // Doc Note Field Name
					UpdateDocumentNoteFieldWithOrganizationAddress(docNote, (NoResString)"Consignee - Importer", nameof(DocAddressType.ConsigneeDocumentaryAddress), true, (NoResString)"Consignee - consignee Phone"); // Doc Note Field Name
					UpdateDocumentNoteFieldWithOrganizationAddress(docNote, (NoResString)"Notify Party", nameof(DocAddressType.NotifyParty), true, (NoResString)"Notify - Notify Phone"); // Doc Note Field Name
					UpdateDocumentNoteFieldWithOrganizationAddress(docNote, (NoResString)"Also Notify", nameof(DocAddressType.NotifyParty2), false, string.Empty); // Doc Note Field Name
				}
			}
		}

		void UpdateDocumentNoteFieldWithOrganizationAddress(DocumentNote docNote, string orgFieldName, ZString addressType, bool updatePhoneField, string phoneFieldName)
		{
			var orgAddressDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(addressType);
			if (orgAddressDataObject != null)
			{
				var orgAddress = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory).GetMatched();
				if (orgAddress != null)
				{
					docNote.SetFieldValue(orgFieldName, orgAddress.AddressFullFormatted);
				}

				if (updatePhoneField)
				{
					docNote.SetFieldValue(phoneFieldName, orgAddressDataObject.Phone.GetValueOrDefault());
				}
			}
		}

		#endregion

		void ImportSubHVLShipmentInspectionTypeCodeIfNeeded(ForwardingShipment shipmentBO)
		{
			if (shipmentBO.JS_ShipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValueMaster && dataObject.AviationSecurityInspectionType != null)
			{
				var subHVLShipments = shipmentBO.CoLoadShipments.Where(s => s.JS_ShipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue);
				foreach (ForwardingShipment shipment in subHVLShipments)
				{
					new AviationSecurityInspectionTypeDataObjectReader(dataObject.AviationSecurityInspectionType, logger, factory, shipment, shipment.Logs, shipment.IsInDatabase).ReadIntoBusinessObject();
				}
			}
		}

		void PopulateOrders(ForwardingShipment shipmentBO)
		{
			readingHelper.PopulateAttachedOrders(shipmentBO);

			var linkedWarehouseOrder = readingHelper.LinkWarehouseOrder(shipmentBO);

			var order = dataObject.Order;
			if (order != null)
			{
				SetValue(shipmentBO, JobShipmentSchema.JS_BookingReference, order.ClientReference);

				var docsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);

				if (!order.OrderNumber.GetValueOrDefault().IsEmpty && !docsAndCartage.JP_OrderItemsAsStringInfo.ReadOnly)
				{
					var orderNumber = new OrderNumber { OrderReference = order.OrderNumber };

					if (dataObject.LocalProcessing == null || dataObject.LocalProcessing.OrderNumberCollection == null)
					{
						docsAndCartage.OrderItems.RemoveAndDeleteAll();
					}

					if (!linkedWarehouseOrder)
					{
						docsAndCartage.OrderItems.Add(new OrderNumberDataObjectReader(orderNumber, logger, factory, docsAndCartage).ReadIntoBusinessObject());
					}
				}
			}
		}

		protected override bool ShouldUpdateBO
		{
			get
			{
				if (logger.IsUpdatingConsol)
				{
					return RegistryAllowBOUpdateForConsolShipment();
				}
				else
				{
					return RegistryAllowsBOUpdateForStandaloneShipment();
				}
			}
		}

		bool RegistryAllowsBOUpdateForStandaloneShipment()
		{
			return eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.Value;
		}

		bool RegistryAllowBOUpdateForConsolShipment()
		{
			return eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.Value;
		}

		bool IsConsolidatedShipment
		{
			get
			{
				return (parent != null && parent.Consol != null) || linkManager.Consol != null;
			}
		}

		void ReadInJobDocsAndCartageData(ForwardingShipment shipmentBO)
		{
			if ((dataObject.CustomizedFieldCollection != null && dataObject.CustomizedFieldCollection.Count > 0) || dataObject.LocalProcessing != null)
			{
				if (shipmentBO.IsDeleted)
				{
					JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);
				}
				ReadInCustomFields(shipmentBO);
				if (dataObject.LocalProcessing != null)
				{
					new LocalProcessingDataObjectReader(dataObject, logger, factory).PopulateBusinessObject(shipmentBO.DocsAndCartage);
				}
			}
		}

		void ReadInCustomFields(ForwardingShipment shipmentBO)
		{
			if (dataObject.CustomizedFieldCollection != null && dataObject.CustomizedFieldCollection.Count > 0)
			{
				var jobDocsAndCartage = shipmentBO.DocsAndCartage;
				var reader = new CustomFieldsDataObjectReader<JobDocsAndCartage>(logger, jobDocsAndCartage, new JobDocsAndCartageCustomFieldsDescriptor());
				var usedCustomFields = reader.ReadCustomFields(dataObject.CustomizedFieldCollection);
				PopulateWorkflowCustomFields(shipmentBO, dataObject, usedCustomFields);
			}
		}

		void InitializeLinksManager(ForwardingShipment shipmentBO)
		{
			if (linkManager == null || linkManager.Consol == null)
			{
				if (shipmentBO.Consols.Count == 0 && parent != null && parent.Consol != null)
				{
					linkManager = new ContainerLinkManager<ForwardingConsol>(parent.Consol);
				}
				else
				{
					ForwardingConsol consol = null;

					if (readingHelper.IsTWFunctionalityEnabled(shipmentBO, DataContextType.TransitDispatch, out _))
					{
						consol = GetTWConsolFromLoadList(shipmentBO);
					}

					if (readingHelper.IsTWFunctionalityEnabled(shipmentBO, DataContextType.TransitReceive, out _))
					{
						consol = GetTWConsolFromAdditionalReference(shipmentBO);
					}

					if (consol == null)
					{
						consol = new ConsolFinder<ForwardingConsol>(shipmentBO).GetBestMatchingConsolForShipmentIfExistsOtherwiseAddNew(dataObject, ShouldCreateConsol(shipmentBO));
					}

					linkManager = new ContainerLinkManager<ForwardingConsol>(consol);
				}
			}
		}

		bool ShouldCreateConsol(ForwardingShipment shipmentBO)
		{
			if (dataObject.ContainerCollection == null || dataObject.ContainerCollection.Count == 0 || IsNVOCC)
			{
				return false;
			}

			// Check if all containers are road truck containers, then will not create new consol
			return dataObject.ContainerCollection.Any(c => !IsCurrentContainerInRoadTruckMode(shipmentBO, c));
		}

		ForwardingConsol GetTWConsolFromLoadList(ForwardingShipment shipmentBO)
		{
			var loadlists = dataObject.SubShipmentCollection?.Where(s => s.DataContext?.DataSourceCollection?.Any(ds => ds.Type.GetValueOrDefault() == nameof(DataContextType.TransitDispatchLoadList)) ?? false);
			if (loadlists == null || !loadlists.Any())
			{
				return null;
			}

			var consols = shipmentBO.Consols.Cast<ForwardingConsol>();
			var consolNumberDict = consols.ToDictionary(c => c.JK_UniqueConsignRef);
			var consolMasterBillLookup = consols.ToLookup(c => c.JK_MasterBillNum);

			var loadlist = loadlists.First(); //assumption was made that each TransitDispatch will only contain 1 consol
			var consolNumber = loadlist.AdditionalReferenceCollection?.FirstOrDefault(r => (r.Type?.Code.GetValueOrDefault() ?? ZString.Empty) == WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber)?.ReferenceNumber;

			ForwardingConsol consol = null;
			if (consolNumber.HasValue)
			{
				if (!consolNumberDict.TryGetValue(consolNumber.Value, out consol))
				{
					throw new DataObjectReadFailureException(Res.GetString("8AAD555E-86BF-49F9-9A33-BB8CD39D015C", "Consol number {0} exists in the UXML but unable to find matching consol on shipment", consolNumber.Value));
				}
			}

			if (consol == null)
			{
				var masterBillNumber = loadlist.AdditionalReferenceCollection?.FirstOrDefault(r => (r.Type?.Code.GetValueOrDefault() ?? ZString.Empty) == WarehouseAdditionalReferenceTypes.Codes.MasterBill)?.ReferenceNumber;
				if (masterBillNumber.HasValue)
				{
					var matchingConsols = consolMasterBillLookup[masterBillNumber.Value];
					if (!matchingConsols.Any())
					{
						throw new DataObjectReadFailureException(Res.GetString("A607BA20-B9B8-4AC1-8EE8-21047DC518C8", "Master Bill {0} exists in the UXML but unable to find matching consol on shipment", masterBillNumber.Value));
					}

					if (matchingConsols.Count() > 1)
					{
						throw new DataObjectReadFailureException(Res.GetString("69FF7A79-8D53-4226-AB23-69D28A69858A", "Master Bill {0} exists in the UXML but multiple matching consols found on shipment", masterBillNumber.Value));
					}
					consol = matchingConsols.First();
				}
			}

			return consol;
		}

		ForwardingConsol GetTWConsolFromAdditionalReference(ForwardingShipment shipmentBO)
		{
			var consolReference = dataObject.AdditionalReferenceCollection?.FirstOrDefault(r => (r.Type?.Code ?? ZString.Empty) == WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var masterBillReference = dataObject.AdditionalReferenceCollection?.FirstOrDefault(r => (r.Type?.Code ?? ZString.Empty) == WarehouseAdditionalReferenceTypes.Codes.MasterBill);

			if (consolReference == null && masterBillReference == null)
			{
				return null;
			}

			var consols = shipmentBO.Consols.Cast<ForwardingConsol>();
			var consolNumberDict = consols.ToDictionary(c => c.JK_UniqueConsignRef);
			var consolMasterBillLookup = consols.ToLookup(c => c.JK_MasterBillNum);

			ForwardingConsol consol = null;

			if (consolReference != null)
			{
				var consolNumber = consolReference.ReferenceNumber.GetValueOrDefault();

				if (!consolNumberDict.TryGetValue(consolNumber, out consol))
				{
					throw new DataObjectReadFailureException(Res.GetString("8AAD555E-86BF-49F9-9A33-BB8CD39D015C", "Consol number {0} exists in the UXML but unable to find matching consol on shipment", consolNumber));
				}
			}

			if (consol == null && masterBillReference != null)
			{
				var masterBillNumber = masterBillReference.ReferenceNumber.GetValueOrDefault();

				var matchingConsols = consolMasterBillLookup[masterBillNumber];
				if (!matchingConsols.Any())
				{
					throw new DataObjectReadFailureException(Res.GetString("A607BA20-B9B8-4AC1-8EE8-21047DC518C8", "Master Bill {0} exists in the UXML but unable to find matching consol on shipment", masterBillNumber));
				}

				if (matchingConsols.Count() > 1)
				{
					throw new DataObjectReadFailureException(Res.GetString("69FF7A79-8D53-4226-AB23-69D28A69858A", "Master Bill {0} exists in the UXML but multiple matching consols found on shipment", masterBillNumber));
				}

				consol = matchingConsols.First();
			}

			return consol;
		}

		protected override void SetCustomValue(BusinessObject shipmentBO, ZString name, IZType value)
		{
			shipmentBO.SetUserDefinedValue(name, AddOnColumnDataType.GetCodeFromObject(value), value);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void FillCollections(ForwardingShipment shipmentBO)
		{
			if (dataObject.NoteCollection != null && !(shipmentBO.TemplateRecord?.IsForTemplateSearch ?? false))
			{
				new ForwardingShipmentNotesCollectionReader(dataObject.NoteCollection, logger, factory, shipmentBO).ReadIntoCollection();
			}

			if (dataObject.ContainerCollection != null && dataObject.ContainerCollection.Count > 0 && !IsNVOCC)
			{
				ReadInContainers(linkManager.Consol, shipmentBO);
			}

			FillSubShipmentCollection(shipmentBO);

			if (dataObject.TransportLegCollection != null && !dataObject.IsTransitDataSource())
			{
				foreach (var transportLegDataObject in dataObject.TransportLegCollection)
				{
					var transportParent = new ConsolFinder<ForwardingConsol>(shipmentBO).GetBestMatchingParentForTransport(transportLegDataObject);

					var forwardingShipmentTransportParent = transportParent as ForwardingShipment;
					Func<TransportLeg, Transport> transportLegProvider = null;

					if (forwardingShipmentTransportParent != null)
					{
						transportLegProvider = (dataObj) =>
						{
							var finder = new TransportLegBusinessObjectFinder(dataObj, shipmentBO);
							return finder.Find(forwardingShipmentTransportParent.TransportsIncludingRelated.Cast<Transport>());
						};
					}

					var transportLegReader = new TransportLegDataObjectReader(transportLegDataObject, logger, factory, transportParent, transportLegProvider);

					transportParent.Transports.Add(transportLegReader.ReadIntoBusinessObject());
				}
			}

			if (dataObject.PackingLineCollection == null
				&& (dataObject.DataContext?.DataTargetCollection?.Any(target => target.Type.GetValueOrDefault() == nameof(DataContextType.WarehouseOrder))).GetValueOrDefault()
				&& dataObject.CommercialInfo != null
				&& dataObject.CommercialInfo.CommercialInvoiceCollection != null
				&& dataObject.CommercialInfo.CommercialInvoiceCollection.Count == 1
				&& dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection != null)
			{
				if (!shipmentBO.IsMasterShipmentRepresentingAllChildShipments)
				{
					var commercialInvoiceLineCollectionDataObjectReader = new CommercialInvoiceLineCollectionDataObjectReader(dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.ToArray(), shipmentBO, logger, factory);
					commercialInvoiceLineCollectionDataObjectReader.ReadIntoCollection();
				}
				else
				{
					logger.Log(LogType.Warning, Enterprise.Freight.Forwarding.DataTransfer.Res.GetString("19F06ECC-9498-499D-B0CA-BC44917AF272", "Can't create pack lines from invoice lines as the shipment is Master."));
				}
			}

			FillGatewayInfos(shipmentBO);

			if (dataObject.AdditionalAddressInfoCollection != null)
			{
				foreach (var additionalAddressInfo in dataObject.AdditionalAddressInfoCollection)
				{
					var reader = new JobAddressAdditionalInfoDataObjectReader(additionalAddressInfo, logger, factory, shipmentBO);
					reader.ReadIntoBusinessObject();
				}
			}
		}

		void FillSubShipmentCollection(ForwardingShipment shipmentBO)
		{
			if (dataObject.SubShipmentCollection == null)
			{
				return;
			}

			if (shipmentBO.JS_ShipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue)
			{
				var hlvReadingHelper = ObjectFactory.Get<IHVLShipmentReadingHelper>(nameof(IHVLShipmentReadingHelper), dataObject, shipmentBO, logger, factory);
				hlvReadingHelper.ReadConsignmentsFromSubShipments();

				return;
			}

			foreach (var shipmentDataObject in dataObject.SubShipmentCollection.Where(s => ShouldProcessSubShipment(s)))
			{
				var coloadShipment = new ShipmentDataObjectReader(shipmentDataObject, logger, factory, ChildShipmentsParent.ToChildShipmentsParent(shipmentBO), linkManager, this, helper).ReadIntoBusinessObject();

				if (coloadShipment != null && !shipmentBO.CoLoadShipments.Contains(coloadShipment))
				{
					if (coloadShipment.JS_JS_ColoadMasterShipment.IsValid && coloadShipment.JS_JS_ColoadMasterShipment != shipmentBO.PK)
					{
						var errorMessage = Res.GetString("6916deff-c6d2-42ba-a02d-bb93eebe4017", "Shipment ({0}) is already linked to another job. XML rejected as an invalid link would be created between CLD and STD shipments.", coloadShipment.JobNumber);

						throw new DataObjectReadFailureException(errorMessage);
					}
					else if (coloadShipment.PK == shipmentBO.PK)
					{
						var errorMessage = Res.GetString("a4d54149-4b7c-4780-a3b1-8bd0c98bb67a", "Shipment ({0}) cannot import itself as a sub shipment.", coloadShipment.JobNumber);
						throw new DataObjectReadFailureException(errorMessage);
					}
					else if (WouldCreateCircularDependency(shipmentBO, coloadShipment))
					{
						var errorMessage = Res.GetString("133b42b4-9986-44d7-9b63-c23448fb1751", "Shipment ({0}) cannot be imported as it would create a circular dependency between master and sub shipments.", coloadShipment.JobNumber);
						throw new DataObjectReadFailureException(errorMessage);
					}

					shipmentBO.CoLoadShipments.Add(coloadShipment);
				}
			}
		}

		void FillGatewayInfos(ForwardingShipment shipmentBO)
		{
			if (dataObject.GatewayInfoCollection != null)
			{
				new ShipmentGatewayInfoCollectionReader(dataObject.GatewayInfoCollection.ToArray(), shipmentBO, logger, factory).ReadIntoCollection();
			}
		}

		bool ShouldProcessSubShipment(UniversalShipment subShipmentDataObject)
		{
			if (subShipmentDataObject.GetMatchingDataSource(DataContextType.CustomsDeclaration) != null
				|| subShipmentDataObject.GetMatchingDataSource(DataContextType.InBond) != null
				|| subShipmentDataObject.GetMatchingDataSource(DataContextType.HVLVConsignment) != null)
			{
				return false;
			}

			if (subShipmentDataObject.DataContext?.DataTargetCollection != null && subShipmentDataObject.DataContext.DataTargetCollection.Any())
			{
				return subShipmentDataObject.GetMatchingDataTarget(DataContextType.ForwardingShipment) != null;
			}
			else if (subShipmentDataObject.DataContext?.DataSourceCollection != null && subShipmentDataObject.DataContext.DataSourceCollection.Any())
			{
				return subShipmentDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment) != null;
			}

			return true;
		}

		#region Circular Dependency Check

		bool WouldCreateCircularDependency(CommonShipment masterShipment, CommonShipment shipmentToAdd)
		{
			if (masterShipment.PK == shipmentToAdd.PK)
			{
				return true;
			}

			while (masterShipment.CoLoadMasterShipment != null)
			{
				masterShipment = masterShipment.CoLoadMasterShipment;
				if (masterShipment.PK == shipmentToAdd.PK)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		void ReadInContainers(ForwardingConsol consol, ForwardingShipment shipmentBO = null)
		{
			if (consol == null)
			{
				return;
			}

			foreach (var containerDataObject in dataObject.ContainerCollection)
			{
				if (dataObject.IsVGM())
				{
					containerDataObject.UpdateContainerGrossWeightVerificationDateTime();
				}

				if (IsCurrentContainerInRoadTruckMode(shipmentBO, containerDataObject))
				{
					continue;
				}

				var containerBizObjCreator = new Func<Container, ForwardingContainer>(data => consol.Containers.AddNew());

				if (dataObject?.DataContext?.DataSourceCollection?.Any(x => x.Type.GetValueOrDefault() == nameof(DataContextType.TransitDispatch) || x.Type.GetValueOrDefault() == nameof(DataContextType.TransitReceive)) ?? false)
				{
					containerBizObjCreator = new Func<Container, ForwardingContainer>(data =>
					{
						var oldAutomaticallyUpdatePackLineContainers = consol.AutomaticallyUpdatePackLineContainers;
						consol.AutomaticallyUpdatePackLineContainers = false;

						using (new DisposableAction(() => consol.AutomaticallyUpdatePackLineContainers = oldAutomaticallyUpdatePackLineContainers))
						{
							return consol.Containers.AddNew();
						}
					});
				}

				var container = new ContainerWithPackLinesDataObjectReader<ForwardingContainer, ForwardingConsol>(containerDataObject, logger, factory, linkManager, FindContainerByNumber, data => containerBizObjCreator(data)).ReadIntoBusinessObject();
				ProcessContainerMode(consol, container);
			}
		}

		bool IsCurrentContainerInRoadTruckMode(ForwardingShipment shipmentBO, Container containerDataObject)
		{
			var containerCode = (containerDataObject.ContainerType?.Code).GetValueOrDefault();

			if (!containerCode.IsEmpty && shipmentBO != null && readingHelper.IsTWFunctionalityEnabled(shipmentBO) && FindContainerByNumber(containerDataObject) == null)
			{
				var refContainer = new RefContainer.Loader(factory.BOFactory).LoadFromCode(containerCode);
				if (refContainer?.IsRoadTruckContainer == true)
				{
					return true;
				}
			}

			return false;
		}

		ForwardingContainer FindContainerByNumber(Container containerDO)
		{
			var containerNumber = containerDO.ContainerNumber.GetValueOrDefault();

			return (!containerNumber.IsEmpty && linkManager.Consol != null)
				? (ForwardingContainer)linkManager.Consol.Containers.FindAnyByContainerNumber(containerNumber)
				: null;
		}

		void ProcessContainerMode(ForwardingConsol consol, ForwardingContainer container)
		{
			if (!container.JC_ContainerMode.IsEmpty)
			{
				return;
			}

			if (dataObject.DataContext?.DataSourceCollection?.Any(source => (source.Type ?? ZString.Empty) == nameof(DataContextType.TransitDispatch) || (source.Type ?? ZString.Empty) == nameof(DataContextType.TransitReceive)) ?? false)
			{
				if (container.JC_ContainerMode.IsEmpty && ((dataObject.TransportMode?.Code ?? ZString.Empty) == Core.Constants.TransportModes.Air || (consol?.JK_TransportMode ?? ZString.Empty) == Core.Constants.TransportModes.Air))
				{
					container.JC_ContainerMode = Core.Constants.ContainerModes.ULD;
				}
				else if (container.JC_ContainerMode.IsEmpty && !consol.JK_ConsolMode.IsEmpty && container.JC_ContainerMode_List.ContainsCode(consol.JK_ConsolMode))
				{
					container.JC_ContainerMode = consol.JK_ConsolMode;
				}
				else
				{
					container.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
				}
			}
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.ForwardingShipment; }
		}

		public override IShipmentDataObjectReader ParentReader
		{
			get { return parentReader; }
		}

		protected override IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelismCore()
		{
			var baseValues = base.ReadKeysForParallelismCore();
			var references = readingHelper.GenerateShipmentReference();
			var matcherValues = GetShipmentMatcher(references).GetMatchingShipmentKeys(references);

			if (baseValues != null)
			{
				return baseValues.Concat(matcherValues);
			}
			else
			{
				return matcherValues;
			}
		}

		protected override bool ModuleHasReferenceAndPartyIDMatchingEnabled => base.ModuleHasReferenceAndPartyIDMatchingEnabled && !IsNVOCC;
	}
}
