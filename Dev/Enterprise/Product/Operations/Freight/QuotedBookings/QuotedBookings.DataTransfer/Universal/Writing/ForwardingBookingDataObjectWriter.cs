using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	public class ForwardingBookingDataObjectWriter : BaseShipmentDataObjectWriter
	{
		public ForwardingBookingDataObjectWriter(IDataWritingManager manager, QuotedBooking bookingBO)
			: base(manager)
		{
			this.bookingBO = bookingBO;
		}

		protected QuotedBooking bookingBO { get; private set; }

		protected override void PopulateDataObject(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			base.PopulateDataObject(shipmentBO, shipmentData);

			PopulateShipmentData(shipmentData);

			var docsBO = shipmentBO.DocsAndCartage;
			if (docsBO != null)
			{
				shipmentData.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy)
				{
					InsuranceRequired = docsBO.JP_InsuranceRequired,
					FCLPickupEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(docsBO.JP_FCLPickupEquipmentNeeded, docsBO.Lookups.PickupEquipmentNeededList),
					FCLDeliveryEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(docsBO.JP_FCLDeliveryEquipmentNeeded, docsBO.Lookups.DeliveryEquipmentNeededList),
					EstimatedDelivery = docsBO.JP_EstimatedDelivery,
					DeliveryRequiredBy = docsBO.JP_DeliveryRequiredBy,
					EstimatedPickup = docsBO.JP_EstimatedPickup,
					PickupRequiredBy = docsBO.JP_PickupRequiredBy,
				};
				shipmentData.LocalProcessing.SetOrderNumberCollection(() => ProcessCollection(docsBO.OrderItems, new OrderNumberDataObjectWriter(writeManager), CollectionContent.Complete));
				shipmentData.LocalProcessing.SetAdditionalServiceCollection(() => ProcessCollection(docsBO.Services, new AdditionalServiceDataObjectWriter(writeManager), CollectionContent.Complete));
			}

			if (bookingBO.ObjectState != QuotedBookingState.BookingOnly)
			{
				shipmentData.SetLocalClientAddress(writeManager, bookingBO);
			}

			shipmentData.SetContainerCollection(() => ProcessCollection(bookingBO.QuotedBookingContainers, new ContainerDataObjectWriter(writeManager), CollectionContent.Complete, true));
			shipmentData.SetRelatedShipmentCollection(() => ProcessCollection(shipmentBO.AttachedOrders, new OrderDataObjectWriter(writeManager, OrderLineLinkManager)));

			var notes = bookingBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			shipmentData.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));

			shipmentData.DateCollection?.Add(Date.New(DateType.ClientRequestedETA, ZBool.True, shipmentBO.JS_ClientRequestedETA));

			shipmentData.AviationSecurityInspectionType = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_InspectionTypeCode, shipmentBO.Lookups.InspectionTypes);
			shipmentData.ReleaseType = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_ReleaseType, shipmentBO.Lookups.JS_ReleaseType_List);
			shipmentData.HBLAWBChargesDisplay = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_HBLAWBChargesDisplay, shipmentBO.Lookups.JS_HBLAWBChargesDisplay_List);
			shipmentData.ShippedOnBoard = ListHelper.GetWithDescription<CodeDescriptionPair>(shipmentBO.JS_ShippedOnBoard, shipmentBO.Lookups.JS_ShippedOnBoard_List);
			shipmentData.CarrierContractNumber = shipmentBO.JS_CarrierContractNumber;
			shipmentData.SetCarrier(writeManager, bookingBO);
			shipmentData.SetCreditor(writeManager, bookingBO);

			shipmentData.HBLContainerPackModeOverride = shipmentBO.JS_HBLContainerPackModeOverride;
			shipmentData.RateCommodity = ListHelper.GetWithDescription<Commodity>(bookingBO.Commodity, bookingBO.CommodityCodeList);
			shipmentData.FMCTariffID = bookingBO.FMCTariffID;

			PopulateCO2eFields(shipmentBO, shipmentData);
		}

		void PopulateCO2eFields(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			shipmentData.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = bookingBO.GetTotalCO2e(),
				CO2eUnit = ListHelper.GetWithDescription<UnitOfWeight>(Constants.Weight.Kilograms, bookingBO.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)),
				CO2eDescriptiveStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.GetCO2eStatus(), new CO2eStatusList())
					.AdditionalSetup(cdp => cdp.Description = CO2eHelper.GetCO2eStatusShortDescription(cdp.Code)),
			};
		}

		void PopulateShipmentData(UniversalShipment shipmentData)
		{
			shipmentData.PortOfLoading = ListHelper.GetWithName(bookingBO.LoadPort, bookingBO.LoadPortLocations);
			shipmentData.PortOfDischarge = ListHelper.GetWithName(bookingBO.DischargePort, bookingBO.DischargePortLocations);
			shipmentData.AWBServiceLevel = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.ScheduleChooser.AWBServiceLevel, bookingBO.ScheduleChooser.NeutralAirWaybillServiceLevelList);
			shipmentData.CarrierServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(bookingBO.CarrierServiceLevel, bookingBO.CarrierServiceLevels);

			shipmentData.AgentsReference = bookingBO.Booking?.JS_BookingReference ?? ZString.Empty;
			shipmentData.CoLoadBookingConfirmationReference = bookingBO.Booking?.JS_UniqueConsignRef ?? ZString.Empty;
			shipmentData.CompanyTariffLevelOverride = bookingBO.Booking?.JS_CompanyTariffLevelOverride ?? ZByte.Zero;
			var sailing = bookingBO.ScheduleChooser.Sailing;
			if (sailing != null)
			{
				shipmentData.VoyageFlightNo = sailing.JX_JV_VoyageFlight;
				shipmentData.VesselName = sailing.JX_JV_NKVessel;
				shipmentData.LloydsIMO = sailing.Vessel.GetLloydsIMO();
			}
		}

		protected override JobCosting GenerateJobCostingData(IJobHeaderParentCore sourceJobCostingParent)
		{
			if (bookingBO.ObjectState != QuotedBookingState.BookingOnly)
			{
				return base.GenerateJobCostingData(bookingBO.Quote);
			}
			return base.GenerateJobCostingData(sourceJobCostingParent);
		}

		protected override IDataContextManager GetDataContextManager(BusinessObject sourceBO)
		{
			return bookingBO.GetUniversalDataContextManager();
		}

		protected override ForwardingShipment GetTypedBusinessObject(BusinessObject sourceBO)
		{
			if (this.bookingBO == null)
			{
				this.bookingBO = ((QuotedBooking)sourceBO);
			}

			return this.bookingBO.Booking;
		}

		protected override void PopulateWorkflowRelatedProperties(ForwardingShipment sourceBO, UniversalShipment dataObject)
		{
			var booking = sourceBO.Factory.Load<QuotedBooking>(sourceBO.PK);
			if (!writeManager.IsPublishingInternally)
			{
				ObjectFactory.Get<IUniversalMilestoneWriter>().PopulateMilestones(booking, dataObject, writeManager.ShouldPopulateInternalMilestones);
			}

			ObjectFactory.Get<IUniversalExceptionWriter>().PopulateExceptions(booking, dataObject);
			ObjectFactory.Get<IUniversalTaskSetWriter>().PopulateTaskSets(writeManager.WriterStrategy, booking, dataObject);
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(ForwardingShipment shipmentBO)
		{
			return bookingBO.GetUserDefinedValues();
		}

		protected override IEnumerable<IPropertyValue> GetAllCustomPropertiesWithDefaultValue(ForwardingShipment shipmentBO) => GetAllCustomPropertiesWithDefaultValue(bookingBO);

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.ForwardingBooking;
		}

		protected override DocAddressType ExportReceivingDepotAddressType
		{
			get
			{
				var organisationType = QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(bookingBO.Mode);

				if (!organisationType.Equals(QuotedBookingHelper.ReceiverOrganization.PackDepot))
				{
					return DocAddressType.DepartureCTOAddress;
				}

				return DocAddressType.DepartureCFSAddress;
			}
		}
	}
}
