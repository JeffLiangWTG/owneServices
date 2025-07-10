using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderDataObjectWriter : TopLevelDataObjectWriter<Order, UniversalShipment>
	{
		public OrderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		public OrderDataObjectWriter(IDataWritingManager manager, IOrderLineLinkManager orderLineLinkManager)
			: this(manager)
		{
			this.orderLineLinkManager = orderLineLinkManager;
		}

		readonly IOrderLineLinkManager orderLineLinkManager;

		#region Context

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.OrderManagerOrder;
		}

		#endregion

		#region PopulateDataObject

		protected override void PopulateDataObject(Order orderBO, UniversalShipment orderDataObject)
		{
			PopulateData(orderBO, orderDataObject, writeManager.WriterStrategy);
			PopulateRelatedEntities(orderBO, orderDataObject);
		}

		#region PopulateData

		static void PopulateData(Order orderBO, UniversalShipment orderDataObject, IDataObjectWriterStrategy strategy)
		{
			orderDataObject.AdditionalTerms = orderBO.JD_AdditionalTerms;
			orderDataObject.BookingConfirmationReference = orderBO.JD_BookingConfRef;
			orderDataObject.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(orderBO.JD_ContainerMode, orderBO.JD_ContainerMode_List);
			orderDataObject.CommercialInfo = new CommercialInfo
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
				{
					new CommercialInvoiceHeader
					{
						InvoiceNumber = orderBO.JD_InvoiceNumber,
						InvoiceDate = orderBO.JD_InvoiceDate
					}
				}
			};
			orderDataObject.CountryOfSupply = Country.New(orderBO.CountryOfSupply);
			orderDataObject.FirstBuyerContact = orderBO.JD_FirstBuyerContact;
			orderDataObject.FreightRate = orderBO.JD_EstimatedExchangeRate;
			orderDataObject.FreightRateCurrency = Currency.New(orderBO.OrderCurrency);
			orderDataObject.GoodsDescription = orderBO.JD_OrderGoodsDescription;

			orderDataObject.Order = new UniversalDataBuss.DataObjects.Universal.Order(strategy);
			orderDataObject.Order.ClientReference = orderBO.JD_BookingConfRef;
			orderDataObject.Order.OrderNumber = orderBO.JD_OrderNumber;
			orderDataObject.Order.OrderNumberSplit = orderBO.JD_OrderNumberSplit;
			orderDataObject.Order.Status = ListHelper.GetWithDescription<CodeDescriptionPair>(orderBO.JD_OrderStatus, orderBO.JD_OrderStatus_List);
			orderDataObject.Order.IsReleased = orderBO.JD_IsReleased;
			orderDataObject.OuterPacks = orderBO.JD_Packs;
			orderDataObject.OuterPacksPackageType = ListHelper.GetWithDescription<PackageType>(orderBO.JD_F3_NKPackType, orderBO.JD_F3_NKPackType_List);
			orderDataObject.PortOfDestination = UNLOCO.New(orderBO.GoodsDeliveredTo);
			orderDataObject.PortOfDischarge = UNLOCO.New(orderBO.PortOfDischarge);
			orderDataObject.PortOfLoading = UNLOCO.New(orderBO.PortOfLoading);
			orderDataObject.PortOfOrigin = UNLOCO.New(orderBO.GoodsAvailableAt);
			orderDataObject.SecondBuyerContact = orderBO.JD_SecondBuyerContact;
			orderDataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(orderBO.JD_RS_NKServiceLevel_NI, orderBO.JD_RS_List);
			orderDataObject.ShipmentIncoTerm = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.IncoTerm>(orderBO.JD_IncoTerm, orderBO.JD_IncoTerm_List);
			orderDataObject.TotalVolume = orderBO.JD_ActualVolume;
			orderDataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(orderBO.JD_UnitOfVolume, orderBO.JD_UnitOfVolume_List);
			orderDataObject.TotalWeight = orderBO.JD_ActualWeight;
			orderDataObject.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(orderBO.JD_UnitOfWeight, orderBO.JD_UnitOfWeight_List);
			orderDataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(orderBO.JD_TransportMode, orderBO.JD_TransportMode_List);
			orderDataObject.WayBillNumber = orderBO.JD_Waybill;
			orderDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());

			orderDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>()
			{
				new AdditionalBill(strategy)
				{
					BillNumber = orderBO.JD_MasterWaybill,
					BillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList()),
				}
			});

			CustomLabelsCustomizedFieldDataObjectWriter.Write(JobOrderHeaderSchema.Instance, orderBO, orderDataObject, new Order.CustomLabelsProvider(orderBO, false));
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(Order orderBO)
		{
			return orderBO.GetUserDefinedValues();
		}

		#endregion

		#region PopulateRelatedEntities

		void PopulateRelatedEntities(Order orderBO, UniversalShipment orderDataObject)
		{
			PopulateNotes(orderBO, orderDataObject);
			PopulateDates(orderBO, orderDataObject);

			if (orderBO.PlanningVoyageState == PlanningVoyageState.OneVoyage)
			{
				PopulateLeg(orderBO, orderDataObject, new ZByte(1), orderBO.JD_RV_NKDepartureVessel, orderBO.JD_DepartureVoyage, orderBO.JD_Milestone_E_DEP, orderBO.JD_Milestone_E_ARV);
			}
			else
			{
				PopulateLeg(orderBO, orderDataObject, new ZByte(1), orderBO.JD_RV_NKDepartureVessel, orderBO.JD_DepartureVoyage, orderBO.JD_Milestone_E_DEP, orderBO.JD_E_ARV_1stIntermediate);
				if (orderBO.PlanningVoyageState == PlanningVoyageState.TwoVoyage)
				{
					PopulateLeg(orderBO, orderDataObject, new ZByte(2), orderBO.JD_RV_NKArrivalVessel, orderBO.JD_ArrivalVoyage, orderBO.JD_E_DEP_3, orderBO.JD_Milestone_E_ARV);
				}
				else
				{
					PopulateLeg(orderBO, orderDataObject, new ZByte(2), orderBO.JD_RV_NKIntermediateVessel, orderBO.JD_IntermediateVoyage, orderBO.JD_E_DEP_2, orderBO.JD_E_ARV_2ndIntermediate);
					PopulateLeg(orderBO, orderDataObject, new ZByte(3), orderBO.JD_RV_NKArrivalVessel, orderBO.JD_ArrivalVoyage, orderBO.JD_E_DEP_3, orderBO.JD_Milestone_E_ARV);
				}
			}

			PopulateOrganisations(orderBO, orderDataObject);
			PopulateContainers(orderBO, orderDataObject);
			PopulateOrderLines(orderBO, orderDataObject);
		}

		void PopulateNotes(Order orderBO, UniversalShipment orderDataObject)
		{
			var notes = orderBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			orderDataObject.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		void PopulateDates(Order orderBO, UniversalShipment orderDataObject)
		{
			orderDataObject.SetDateCollection(() => new List<Date>());
			if (orderDataObject.DateCollection != null)
			{
				orderDataObject.DateCollection.Add(DateType.BookingConfirmed, false, orderBO.JD_BookingConfDate);
				orderDataObject.DateCollection.Add(DateType.DepartureVesselCutoffDate, false, orderBO.JD_DepartureVesselCutoffDate);
				orderDataObject.DateCollection.Add(DateType.ExWorksRequiredBy, false, orderBO.JD_ExWorksRequiredBy);
				orderDataObject.DateCollection.Add(DateType.FollowUp, false, orderBO.JD_FollowUpDate);
				orderDataObject.DateCollection.Add(DateType.OrderDate, false, orderBO.JD_OrderDate);
				orderDataObject.DateCollection.Add(DateType.ShipmentWindowStart, false, orderBO.JD_ShipmentWindowStart);
				orderDataObject.DateCollection.Add(DateType.ShipmentWindowEnd, false, orderBO.JD_ShipmentWindowEnd);
			}
			orderDataObject.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy);
			orderDataObject.LocalProcessing.DeliveryRequiredBy = orderBO.JD_DeliveryRequiredBy;
		}

		void PopulateLeg(Order orderBO, UniversalShipment orderDataObject, ZByte legOrder, ZString vessel, ZString voyage, ZDateTime? estimatedDeparture, ZDateTime? estimatedArrival)
		{
			if (!vessel.IsEmpty || !voyage.IsEmpty)
			{
				if (orderDataObject.TransportLegCollection == null)
				{
					orderDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete });
				}
				if (orderDataObject.TransportLegCollection != null)
				{
					var transportLeg = new TransportLeg
					{
						LegOrder = legOrder,
						VesselName = vessel,
						VoyageFlightNo = voyage,
						EstimatedDeparture = estimatedDeparture,
						EstimatedArrival = estimatedArrival,
						TransportMode = new TransportModeConverter().ToEnumValue(orderBO.JD_TransportMode)
					};
					orderDataObject.TransportLegCollection.Add(transportLeg);
				}
			}
		}

		void PopulateOrganisations(Order orderBO, UniversalShipment orderDataObject)
		{
			orderDataObject.SetOrganizationAddressCollection(() => ProcessCollection(orderBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));

			orderDataObject.AddOrgAddress(writeManager, orderBO.SupplierAddress, DocAddressType.ConsignorDocumentaryAddress);

			orderDataObject.AddOrgAddress(writeManager, orderBO.Carrier, DocAddressType.Carrier);
			orderDataObject.AddOrgAddress(writeManager, orderBO.SendingAgent, DocAddressType.SendingForwarderAddress);
			orderDataObject.AddOrgAddress(writeManager, orderBO.ReceivingAgent, DocAddressType.ReceivingForwarderAddress);

			var goodsDeliveredToAddress = orderDataObject.AddOrgAddress(writeManager, orderBO.GoodsDeliveredToAddress);
			if (goodsDeliveredToAddress != null)
			{
				goodsDeliveredToAddress.AddressType = nameof(DocAddressType.ConsigneePickupDeliveryAddress);
			}

			var goodsAvailableAtAddress = orderDataObject.AddOrgAddress(writeManager, orderBO.GoodsAvailableAtAddress);
			if (goodsAvailableAtAddress != null)
			{
				goodsAvailableAtAddress.AddressType = nameof(DocAddressType.ConsignorPickupDeliveryAddress);
			}

			if (writeManager.Schema == UniversalXmlSchema.Version_2011_11 && orderBO.ControllingCustomerDocAddress != null)
			{
				var address = orderDataObject.AddOrgAddress(writeManager, orderBO.ControllingCustomerDocAddress);

				if (address != null)
				{
					address.AddressType = LegacyUniversalAddressTypes.LegacyOrderControllingPartyAddressType;
				}
			}

			if (!orderBO.ConsigneeDocumentaryAddress.IsEmpty)
			{
				orderDataObject.AddOrgAddress(writeManager, orderBO.BuyerAddress, DocAddressType.BuyerDocumentaryAddress);
			}
			else
			{
				orderDataObject.AddOrgAddress(writeManager, orderBO.BuyerAddress, DocAddressType.ConsigneeDocumentaryAddress);
			}
		}

		void PopulateContainers(Order orderBO, UniversalShipment orderDataObject)
		{
			orderDataObject.SetContainerCollection(() => ProcessCollection(orderBO.PlannedContainers, new OrderContainerDataObjectWriter<OrderContainer>(writeManager), CollectionContent.Complete));
		}

		void PopulateOrderLines(Order orderBO, UniversalShipment orderDataObject)
		{
			orderDataObject.Order.SetOrderLineCollection(() => ProcessCollection(orderBO.OrderLines, new OrderLineDataObjectWriter(writeManager, orderLineLinkManager, new LineRelatedDataWriterHelper(orderBO)), CollectionContent.Complete));
		}

		#endregion

		#endregion
	}
}

