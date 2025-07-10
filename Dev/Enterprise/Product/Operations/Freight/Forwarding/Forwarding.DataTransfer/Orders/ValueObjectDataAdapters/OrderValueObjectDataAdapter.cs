using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Business.ProductMatching;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderXmlDataImporter : Enterprise.DataTransfer.Business.XmlDataImporter
	{
		public OrderXmlDataImporter(OrderValueObjectDataAdapter<Order, Xsd.Order> adapter)
			: base(adapter)
		{
		}

		protected override XmlValueObjectSerializer GetSerializer()
		{
			return new OrderXMLValueObjectSerializer();
		}
	}

	public class OrderValueObjectDataAdapter : OrderValueObjectDataAdapter<Order, Xsd.Order>
	{
		public OrderValueObjectDataAdapter()
		{
		}

		public OrderValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}
	}

	public class OrderValueObjectDataAdapter<TBusinessObject, TValueObject> : ValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : Order
		where TValueObject : Xsd.Order
	{
		public OrderValueObjectDataAdapter()
			: this(EventsWithSourceType.Empty)
		{
		}

		public OrderValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			this.TriggeredByEvents = triggeredByEvents;
		}
		protected readonly EventsWithSourceType TriggeredByEvents;

		public override string RootCollectionElementName
		{
			get { return (NoResString)"Orders"; }
		}

		public override string RootElementName
		{
			get { return (NoResString)"Order"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleOrderSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.OrdersSchema; }
		}

		protected override bool IsValueObjectCancellation(IValueObject value, IValueObjectImportContext context)
		{
			bool result = false;

			Xsd.Order orderValue = value as Xsd.Order;
			if (orderValue != null)
			{
				result = (!orderValue.OrderLines.IsSpecified || orderValue.OrderLines.Count == 0) &&
					orderValue.OrderDetail.IsSpecified &&
					orderValue.OrderDetail.OrderStatusSpecified &&
					orderValue.OrderDetail.OrderStatus == Constants.OrderStatus.Cancelled;
			}

			return result;
		}

		protected override TBusinessObject CancelBusinessObject(TValueObject xsdOrder, IValueObjectImportContext context)
		{
			TBusinessObject result = null;
			TBusinessObject[] matchedOrders = FindBusinessObjects(xsdOrder, context);
			if (matchedOrders != null)
			{
				foreach (TBusinessObject order in matchedOrders)
				{
					if (MarkOrderAsCancelled(order, context))
					{
						result = order;
					}
				}
			}
			return result;
		}

		protected virtual bool MarkOrderAsCancelled(TBusinessObject order, INotifications notify)
		{
			bool result = false;
			if (order.IsShipmentAttached)
			{
				notify.Notify(new WarningNotification(Res.GetString("3103fea5-48fa-4670-bb26-f36ef667c3f7", "{0} is linked to a shipment and cannot be canceled.", order.HumanReadableName)));
			}
			else if (order.IsDeclarationAttached)
			{
				notify.Notify(new WarningNotification(Res.GetString("3467ceab-f94e-45f5-a863-0f1be2323591", "{0} is linked to a declaration and cannot be canceled.", order.HumanReadableName)));
			}
			else
			{
				order.JD_IsCancelled = true;
				result = true;
			}
			return result;
		}

		protected TBusinessObject[] FindBusinessObjects(Xsd.Order xsdOrder, IValueObjectImportContext context)
		{
			TBusinessObject[] result = null;

			if (xsdOrder != null && xsdOrder.OrderIdentifier.IsSpecified && xsdOrder.OrderDetail.IsSpecified)
			{
				var filter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, xsdOrder.OrderIdentifier.OrderNumber);
				filter.AddToFilter(JobOrderHeaderSchema.JD_OrderStatus, SQLComparisonOperator.NotEqual, Constants.OrderStatus.Cancelled);

				var buyer = context.FindOrganisation(xsdOrder.OrderDetail.Buyer, null, OrganisationTypes.None);
				filter.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyer == null ? ZGuid.Empty : buyer.Addresses.Select(x => x.PK));
				result = (TBusinessObject[])context.Factory.Load(typeof(TBusinessObject), filter);
			}

			return result;
		}

		protected override TBusinessObject FindBusinessObject(TValueObject orderValue, IValueObjectImportContext context)
		{
			TBusinessObject result = null;

			if (orderValue.OrderIdentifier.IsSpecified && orderValue.OrderDetail.IsSpecified)
			{
				var filter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, orderValue.OrderIdentifier.OrderNumber);

				if (orderValue.OrderIdentifier.OrderNumberSplitSpecified)
				{
					//if split number is specified, then find the order with that split number.
					filter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, orderValue.OrderIdentifier.OrderNumberSplit);
				}
				else
				{
					//if split number is not specified update the order with highest split number.
					filter.OrderBy = JobOrderHeaderSchema.JD_OrderNumberSplit.Name + " DESC ";
				}

				var buyer = context.FindOrganisation(orderValue.OrderDetail.Buyer, null, OrganisationTypes.None);
				filter.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyer == null ? ZGuid.Empty : buyer.Addresses.Select(x => x.PK));
				filter.IgnoreActiveFilter = true;
				result = context.Factory.LoadTop1<TBusinessObject>(filter);
			}

			return result;
		}

		protected override bool ShouldUpdateExistingObject(TBusinessObject orderBizObj, INotifications notifications)
		{
			bool result = false;
			ZString warning = ZString.Empty;
			if (orderBizObj != null)
			{
				if (ShouldNotUpdateAShipmentRelatedOrder(orderBizObj))
				{
					warning = Res.GetString("aa2bf206-a9e8-4652-9e29-6a95c65797a8", "{0} is linked to a shipment and cannot be updated.", orderBizObj.HumanReadableName);
				}
				else if (orderBizObj.IsDeclarationAttached)
				{
					warning = Res.GetString("1428898d-e29c-4d49-ac54-be38b54e9bb5", "{0} is linked to a declaration and cannot be updated.", orderBizObj.HumanReadableName);
				}
				else
				{
					result = true;
				}
			}
			NotifyWarningIfRequired(warning, notifications);

			return result;
		}

		protected virtual bool ShouldNotUpdateAShipmentRelatedOrder(TBusinessObject orderBizObj)
		{
			return orderBizObj.IsShipmentAttached && orderBizObj.Shipment.JS_IsForwardRegistered;
		}

		protected virtual void NotifyWarningIfRequired(ZString warning, INotifications notifications)
		{
			if (!warning.IsEmpty)
			{
				notifications.Notify(new WarningNotification(warning));
			}
		}

		protected string GetCustomDateErrorMessage(string fieldName, string value)
		{
			return Res.GetString("60AF327B-2537-402D-BC51-DCAB907628E9", "{0} field is invalid. {1} is not in range [01-Jan-1900 - 06-Jun-2079].", fieldName, value);
		}

		#region ImportFromValueObject

		protected override void ImportFromValueObjectCore(TBusinessObject orderBizObj, TValueObject orderValue, IValueObjectImportContext context)
		{
			if (!orderValue.OrderDetail.ReferenceNumber.Value.IsEmpty)
			{
				LinkOrderToShipmentOrDeclaration(orderBizObj, orderValue.OrderDetail.ReferenceNumber, context);
			}
			ImportOrderDetails(orderBizObj, orderValue, context);
		}

		protected virtual void ImportOrderDetails(TBusinessObject orderBizObj, TValueObject orderValue, IValueObjectImportContext context)
		{
			INotifications notify = context;

			if (orderValue.OrderDetail.IsSpecified)
			{
				string errorContext = Res.GetString("3163f497-0b42-44a8-be40-261686b76d2f", "Order {0}", orderBizObj.JD_OrderNumber);
				StmALogValueObjectDataAdapter.New(orderBizObj, errorContext).FromXmlCollectionValueObject(orderValue.Events, context);
				ImportOrderBuyerSupplier(orderBizObj, orderValue.OrderDetail, context, errorContext);
				ImportOrderNumberAndSplit(orderValue, orderBizObj, context);
				ImportOtherOrderDetails(orderBizObj, orderValue.OrderDetail, context, errorContext);
				ImportOrderShipmentPlanning(orderBizObj, orderValue.OrderDetail.ShipmentPlanning, context, errorContext);
				ImportOrderMilestones(orderBizObj, orderValue.OrderDetail.Milestones, notify);
				ImportOrderExchangeRate(orderBizObj, orderValue.OrderDetail, context);
				UnmatchedDeliverPoint = new ZStringBuilder();
				ZDecimal totalValue = ImportOrderLines(orderBizObj, orderValue.OrderLines, context, errorContext);
				if (!UnmatchedDeliverPoint.IsEmpty)
				{
					StmNote note = GetOrCreateNote(orderBizObj);
					note.ST_NoteDataAsText = UnmatchedDeliverPoint.ToStringWithNewLineBetweenAppends();
				}
				if (orderValue.OrderDetail.OrderTotal.IsSpecified && totalValue != orderValue.OrderDetail.OrderTotal.Value)
				{
					notify.Notify(new WarningNotification(Res.GetString("2eb75279-8f85-4e0c-a390-cbfb1d1bdd62", "Given total value is not consistent with the total value on the order lines")));
				}
				new NoteValueObjectDataAdapter().ImportNotesAndAttachToBusinessObjectNotes(orderBizObj.Notes, orderValue.Notes, context);
				ImportCustomOrderDetails(orderBizObj, orderValue.OrderDetail, context);
			}

			AddImportEvent(orderBizObj);
		}

		protected void ImportCustomOrderDetails(Order orderBizObj, Xsd.OrderOrderDetail orderDetailValue, IValueObjectImportContext context)
		{
			if (orderDetailValue.Custom.IsSpecified)
			{
				Xsd.OrderOrderDetailCustom custom = orderDetailValue.Custom;

				if (custom.Date1.IsValid)
				{
					if (custom.Date1.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderBizObj.JD_CustomDate1Info, custom.Date1.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage("Custom Date1", custom.Date1.ToDateTime().ToString()));
					}
				}

				if (custom.Date2.IsValid)
				{
					if (custom.Date2.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderBizObj.JD_CustomDate2Info, custom.Date2.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage("Custom Date2", custom.Date2.ToDateTime().ToString()));
					}
				}

				if (custom.Decimal1Specified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomDecimal1Info, custom.Decimal1, JobOrderHeaderSchema.JD_CustomDecimal1);
				}
				if (custom.Decimal2Specified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomDecimal2Info, custom.Decimal2, JobOrderHeaderSchema.JD_CustomDecimal2);
				}
				if (custom.Decimal3Specified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomDecimal3Info, custom.Decimal3, JobOrderHeaderSchema.JD_CustomDecimal3);
				}
				if (custom.Decimal4Specified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomDecimal4Info, custom.Decimal4, JobOrderHeaderSchema.JD_CustomDecimal4);
				}
				if (custom.Decimal5Specified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomDecimal5Info, custom.Decimal5, JobOrderHeaderSchema.JD_CustomDecimal5);
				}

				if (custom.Flag1Specified)
				{
					orderBizObj.JD_CustomFlag1 = custom.Flag1;
				}
				if (custom.Flag2Specified)
				{
					orderBizObj.JD_CustomFlag2 = custom.Flag2;
				}
				if (custom.Flag3Specified)
				{
					orderBizObj.JD_CustomFlag3 = custom.Flag3;
				}
				if (custom.Flag4Specified)
				{
					orderBizObj.JD_CustomFlag4 = custom.Flag4;
				}
				if (custom.Flag5Specified)
				{
					orderBizObj.JD_CustomFlag5 = custom.Flag5;
				}

				if (custom.Text1.IsValid)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomAttrib1Info, custom.Text1, custom.Text1Specified, Res.GetString("df154988-147f-4b38-ac1f-5259a8dc7ff3", "Custom Attribute 1"));
				}
				if (custom.Text2.IsValid)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomAttrib2Info, custom.Text2, custom.Text2Specified, Res.GetString("47693ac1-6a1f-4398-ab1a-3072d604d064", "Custom Attribute 2"));
				}
				if (custom.Text3.IsValid)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomAttrib3Info, custom.Text3, custom.Text3Specified, Res.GetString("2d38b62a-b612-4088-9246-b6e07d97bf27", "Custom Attribute 3"));
				}
				if (custom.Text4.IsValid)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomAttrib4Info, custom.Text4, custom.Text4Specified, Res.GetString("7459f9fa-c32f-40a8-ad51-04181d8b4eae", "Custom Attribute 4"));
				}
				if (custom.Text5.IsValid)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_CustomAttrib5Info, custom.Text5, custom.Text5Specified, Res.GetString("10eaf23d-1ad2-4473-a6e9-49aa1f23b293", "Custom Attribute 5"));
				}

				if (custom.Contact1.IsValid)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_FirstBuyerContactInfo, custom.Contact1, custom.Contact1Specified, Res.GetString("810b4ffe-7db4-4ac9-85b3-352957ad1e21", "Custom Contact 1"));
				}

				if (custom.Contact2.IsValid)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_SecondBuyerContactInfo, custom.Contact2, custom.Contact2Specified, Res.GetString("2feb0046-6ef1-4d69-b176-b287f1895625", "Custom Contact 2"));
				}
			}
		}

		void ImportOrderBuyerSupplier(Order orderBizObj, Xsd.OrderOrderDetail orderDetailValue, IValueObjectImportContext context, string errorContext)
		{
			orderBizObj.BuyerPK = context.FindOrCreateTempOrganisationPK(orderDetailValue.Buyer, orderBizObj, OrganisationTypes.Consignee);
			if (!SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value || orderDetailValue.Supplier.IsSpecified)
			{
				orderBizObj.SupplierPK = context.FindOrCreateTempOrganisationPK(orderDetailValue.Supplier, orderBizObj, OrganisationTypes.Consignor);
			}

			if (!orderBizObj.BuyerPK.IsValid)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("62beb5fa-c82a-4e16-9973-8d4a97e7e9d8", "Buyer field in {0}", orderBizObj.HumanReadableName)));
				context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("726ea705-e698-49ac-be2a-ad18eae8d018", "Insufficient data to create Buyer organization. Please set Maintain -> System -> Registry -> Organizations -> Use Default Organization for Matching ON so that it will match to the UNMATCHED organization")));
			}
			else if (!orderBizObj.SupplierPK.IsValid)
			{
				context.Notify(new WarningNotification(Res.GetString("a92c5dd3-e58c-407d-80ae-8202b36db347", "No supplier specified in {0}", orderBizObj.HumanReadableName)));
			}

			if (orderDetailValue.TransportModeSpecified)
			{
				context.SetPropertyInfoValue(orderBizObj.JD_TransportModeInfo, TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(orderDetailValue.TransportMode, errorContext, context), orderDetailValue.TransportModeSpecified);
			}
		}

		protected virtual void ImportOrderNumberAndSplit(Xsd.Order orderValue, Order orderBizObj, IValueObjectImportContext context)
		{
			if (orderValue.OrderIdentifier.IsSpecified)
			{
				context.SetPropertyInfoValue(orderBizObj.JD_OrderNumberInfo, orderValue.OrderIdentifier.OrderNumber, orderValue.OrderIdentifier.OrderNumberSpecified);
				if (orderValue.OrderIdentifier.OrderNumberSplit > 0)
				{
					orderBizObj.JD_OrderNumberSplit = (byte)orderValue.OrderIdentifier.OrderNumberSplit;
				}
			}
		}

		void ImportOrderShipmentPlanning(Order orderBizObj, Xsd.OrderOrderDetailShipmentPlanning orderShipmentPlanningValue, IValueObjectImportContext context, string errorContext)
		{
			if (orderShipmentPlanningValue.IsSpecified)
			{
				context.SetPropertyInfoValue(orderBizObj.JD_WaybillInfo, orderShipmentPlanningValue.HouseBill, orderShipmentPlanningValue.HouseBillSpecified);

				if (orderShipmentPlanningValue.GoodsOrigin.IsSpecified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_RL_NKGoodsAvailableAtInfo, orderShipmentPlanningValue.GoodsOrigin.Value, ForeignKeyType.PortNK);
				}

				if (orderShipmentPlanningValue.GoodsDestination.IsSpecified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_RL_NKGoodsDeliveredToInfo, orderShipmentPlanningValue.GoodsDestination.Value, ForeignKeyType.PortNK);
				}

				if (orderShipmentPlanningValue.DischargePort.IsSpecified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_RL_NKPortOfDischargeInfo, orderShipmentPlanningValue.DischargePort.Value, ForeignKeyType.PortNK);
				}

				if (orderShipmentPlanningValue.LoadPort.IsSpecified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_RL_NKPortOfLoadingInfo, orderShipmentPlanningValue.LoadPort.Value, ForeignKeyType.PortNK);
				}

				if (orderShipmentPlanningValue.Packs.IsSpecified)
				{
					try
					{
						orderBizObj.JD_Packs = new ZInt(Convert.ToInt32(orderShipmentPlanningValue.Packs.Value));
					}
					catch
					{
						string text = (string.IsNullOrEmpty(errorContext) ? "" : ("; " + errorContext));
						context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("7E3D008A-8793-43DD-88E7-2EE98A2C3D27", "Unsupported number of packs '{0}'{1}", orderShipmentPlanningValue.Packs.Value.ToString(), text)));
					}
					context.SetPropertyInfoValue(orderBizObj.JD_F3_NKPackTypeInfo, orderShipmentPlanningValue.Packs.DimensionType);
				}

				if (orderShipmentPlanningValue.Weight.IsSpecified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_ActualWeightInfo, orderShipmentPlanningValue.Weight.Value, JobOrderHeaderSchema.JD_ActualWeight);
					context.SetPropertyInfoValue(orderBizObj.JD_UnitOfWeightInfo, orderShipmentPlanningValue.Weight.DimensionType);
				}

				if (orderShipmentPlanningValue.Volume.IsSpecified)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_ActualVolumeInfo, orderShipmentPlanningValue.Volume.Value, JobOrderHeaderSchema.JD_ActualVolume);
					context.SetPropertyInfoValue(orderBizObj.JD_UnitOfVolumeInfo, orderShipmentPlanningValue.Volume.DimensionType);
				}

				var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationSubType = Res.GetString("030648cd-f1e5-4446-83c1-f68eb082e25b", "Receiving Forwarder") };

				ZGuid receivingAgentPK = context.FindOrCreateTempOrganisationPK(orderShipmentPlanningValue.ReceivingAgent, orderBizObj, OrganisationTypes.Forwarder, unmatchOrgRecordCriteria);
				if (!receivingAgentPK.IsEmpty)
				{
					orderBizObj.JD_OH_ReceivingAgent = receivingAgentPK;
				}

				unmatchOrgRecordCriteria.OrganisationSubType = Res.GetString("b9cf8dff-8bd3-464d-baeb-9f0c3bbbbf82", "Sending Forwarder");
				ZGuid sendingAgentPK = context.FindOrCreateTempOrganisationPK(orderShipmentPlanningValue.SendingAgent, orderBizObj, OrganisationTypes.Forwarder, unmatchOrgRecordCriteria);
				if (!sendingAgentPK.IsEmpty)
				{
					orderBizObj.JD_OH_SendingAgent = sendingAgentPK;
				}

				if (!orderShipmentPlanningValue.DepartureVessel.IsEmpty)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_RV_NKDepartureVesselInfo, orderShipmentPlanningValue.DepartureVessel, ForeignKeyType.None);
				}

				if (!orderShipmentPlanningValue.DepartureVoyageFlight.IsEmpty)
				{
					context.SetPropertyInfoValue(orderBizObj.JD_DepartureVoyageInfo, orderShipmentPlanningValue.DepartureVoyageFlight, ForeignKeyType.None);
				}

				if (orderShipmentPlanningValue.GoodsAvailAtSpecified && !orderShipmentPlanningValue.GoodsAvailAt.IsEmpty)
				{
					var query = new ZQuery(OrgAddressSchema.OA_Code, orderShipmentPlanningValue.GoodsAvailAt);
					var goodsAvalAtAddress = orderBizObj.Factory.LoadTop1<OrgAddress>(query);
					if (goodsAvalAtAddress != null)
					{
						orderBizObj.GoodsAvailableAtAddress.E2_OA_Address = goodsAvalAtAddress.PK;
					}
				}

				if (orderShipmentPlanningValue.GoodsDelivToSpecified && !orderShipmentPlanningValue.GoodsDelivTo.IsEmpty)
				{
					var query = new ZQuery(OrgAddressSchema.OA_Code, orderShipmentPlanningValue.GoodsDelivTo);
					var goodsDelivToAddress = orderBizObj.Factory.LoadTop1<OrgAddress>(query);
					if (goodsDelivToAddress != null)
					{
						orderBizObj.GoodsDeliveredToAddress.E2_OA_Address = goodsDelivToAddress.PK;
					}
				}

				ImportOrderPlannedContainers(orderBizObj, orderShipmentPlanningValue.PlannedContainers, context);
			}
		}

		void ImportOrderPlannedContainers(Order orderBizObj, Xsd.PlannedContainerCollection plannedContainersValue, IValueObjectImportContext context)
		{
			foreach (Xsd.PlannedContainer plannedContainer in plannedContainersValue)
			{
				ZQuery containerNumberFilter = new ZQuery(JobOrderContainerSchema.J1_ContainerNumber, plannedContainer.Number);
				OrderContainer[] locatedContainers = (OrderContainer[])orderBizObj.PlannedContainers.Find(containerNumberFilter);
				OrderContainer locatedContainer;
				if (locatedContainers.Length > 0)
				{
					locatedContainer = locatedContainers[0];
				}
				else
				{
					locatedContainer = orderBizObj.PlannedContainers.AddNew();
					context.SetPropertyInfoValue(locatedContainer.J1_ContainerNumberInfo, plannedContainer.Number, ForeignKeyType.None);
				}

				locatedContainer.J1_ContainerCount = plannedContainer.Quantity;
				new ContainerValueObjectHelper(context).ImportContainerType(locatedContainer.J1_RCInfo, plannedContainer.Type);
			}
		}

		void ImportOrderMilestones(Order orderBizObj, Xsd.OrderOrderDetailMilestones orderMilestonesValue, INotifications notify)
		{
			if (orderMilestonesValue.IsSpecified)
			{
				XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.Arrival, orderBizObj, Events.Arrival);
				XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.CartageAdvised, orderBizObj, Events.DeliveryCartageAdvised);
				XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.CustomsCommenced, orderBizObj, Events.CustomsCommenced);
				XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.CustomsFinalised, orderBizObj, Events.CustomsCleared);
				XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.Delivery, orderBizObj, Events.DeliveryCartageCompleteFinalised);
				XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.Departure, orderBizObj, Events.Departure);
				XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.ExFactory, orderBizObj, Events.ExWorks);
				XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.OriginReceival, orderBizObj, Events.GateIn);
				XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.Unpacked, orderBizObj, Events.CargoAvailable);

				if (orderMilestonesValue.UserDate.IsSpecified)
				{
					if (orderMilestonesValue.UserDate.Count > 2)
					{
						notify.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("0b9cd2e1-9e1d-4766-84f9-d7e87223f521", "Only 2 custom dates are supported, but {0} were entered for {1}", orderMilestonesValue.UserDate.Count, orderBizObj.HumanReadableName)));
					}

					if (orderMilestonesValue.UserDate.Count >= 1)
					{
						XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.UserDate[0], orderBizObj.JD_EstimateUserDate1Info, orderBizObj.JD_ActualUserDate1Info);
					}

					if (orderMilestonesValue.UserDate.Count >= 2)
					{
						XsdOrderMilestoneDateHelper.ToEstimatedActualDates(orderMilestonesValue.UserDate[1], orderBizObj.JD_EstimateUserDate2Info, orderBizObj.JD_ActualUserDate2Info);
					}
				}
			}
		}

		void ImportOrderExchangeRate(Order orderBizObj, Xsd.OrderOrderDetail orderDetailValue, IValueObjectImportContext context)
		{
			if (orderDetailValue.ExchangeRateSpecified)
			{
				if (orderDetailValue.ExchRateBasis == Xsd.OrderOrderDetailExchRateBasis.L)
				{
					if (orderDetailValue.ExchangeRate != 0M)
					{
						context.SetPropertyInfoValue(orderBizObj.JD_EstimatedExchangeRateInfo, 1 / orderDetailValue.ExchangeRate, JobOrderHeaderSchema.JD_EstimatedExchangeRate);
					}
					else
					{
						context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("3f773f7f-985d-436b-b29c-9999ff1e5efd", "Exchange Rate cannot be 0 where the Basis is 'L'")));
					}
				}
				else
				{
					context.SetPropertyInfoValue(orderBizObj.JD_EstimatedExchangeRateInfo, orderDetailValue.ExchangeRate, JobOrderHeaderSchema.JD_EstimatedExchangeRate);
				}
			}
		}

		void ImportOtherOrderDetails(Order orderBizObj, Xsd.OrderOrderDetail orderDetailValue, IValueObjectImportContext context, string errorContext)
		{
			context.SetPropertyInfoValue(orderBizObj.JD_BookingConfRefInfo, orderDetailValue.ConfirmNumber, orderDetailValue.ConfirmNumberSpecified);

			if (orderDetailValue.ConfirmDate.IsValid)
			{
				if (orderDetailValue.ConfirmDate.IsValidSmallDateTime)
				{
					orderBizObj.JD_BookingConfDate = orderDetailValue.ConfirmDate;
				}
				else
				{
					throw new XmlException(GetCustomDateErrorMessage(nameof(orderDetailValue.ConfirmDate), orderDetailValue.ConfirmDate.ToDateTime().ToString()));
				}
			}

			context.SetPropertyInfoValue(orderBizObj.JD_TransportModeInfo, orderDetailValue.TransportMode.ToString(), orderDetailValue.TransportModeSpecified);
			context.SetPropertyInfoValue(orderBizObj.JD_ContainerModeInfo, orderDetailValue.ContainerMode.ToString(), orderDetailValue.ContainerModeSpecified);
			context.SetPropertyInfoValue(orderBizObj.JD_OrderGoodsDescriptionInfo, orderDetailValue.Description, orderDetailValue.DescriptionSpecified);
			if (!SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value || !orderDetailValue.Incoterm.IsEmpty)
			{
				context.SetPropertyInfoValue(orderBizObj.JD_IncoTermInfo, orderDetailValue.Incoterm, ForeignKeyType.IncoTermNK);
			}
			context.SetPropertyInfoValue(orderBizObj.JD_AdditionalTermsInfo, orderDetailValue.AdditionalTerms, orderDetailValue.AdditionalTermsSpecified);
			context.SetPropertyInfoValue(orderBizObj.JD_InvoiceNumberInfo, orderDetailValue.InvoiceNumber, orderDetailValue.InvoiceNumberSpecified);

			if (orderDetailValue.InvoiceDate.IsValid && !orderDetailValue.InvoiceDate.IsEmpty)
			{
				if (orderDetailValue.InvoiceDate.IsValidSmallDateTime)
				{
					orderBizObj.JD_InvoiceDate = orderDetailValue.InvoiceDate;
				}
				else
				{
					throw new XmlException(GetCustomDateErrorMessage(nameof(orderDetailValue.InvoiceDate), orderDetailValue.InvoiceDate.ToDateTime().ToString()));
				}
			}

			if (orderDetailValue.CountryOfOriginSpecified)
			{
				context.SetPropertyInfoValue(orderBizObj.JD_RN_NKCountryOfSupplyInfo, orderDetailValue.CountryOfOrigin, orderDetailValue.CountryOfOriginSpecified);
			}

			if (orderDetailValue.OrderDateTime.IsValid)
			{
				if (orderDetailValue.OrderDateTime.IsValidSmallDateTime)
				{
					orderBizObj.JD_OrderDate = orderDetailValue.OrderDateTime;
				}
				else
				{
					throw new XmlException(GetCustomDateErrorMessage(nameof(orderDetailValue.OrderDateTime), orderDetailValue.OrderDateTime.ToDateTime().ToString()));
				}
			}

			if (orderDetailValue.OrderStatusSpecified)
			{
				context.SetPropertyInfoValue(orderBizObj.JD_OrderStatusInfo, GetValidOrderOrOrderLineStatusCode(orderBizObj.JD_OrderStatus_List, orderDetailValue.OrderStatus, context), orderDetailValue.OrderStatusSpecified);
			}

			if (orderDetailValue.OrderTotal.IsSpecified)
			{
				context.SetPropertyInfoValue(orderBizObj.JD_RX_NKOrderCurrencyInfo, orderDetailValue.OrderTotal.CurrencyCode, ForeignKeyType.CurrencyNK);
			}

			if (orderDetailValue.ContainerModeSpecified)
			{
				context.SetPropertyInfoValue(orderBizObj.JD_ContainerModeInfo, OrderContainerModeToXmlCodeMappings.Instance.GetEnterpriseCode(orderDetailValue.ContainerMode, errorContext, context), orderDetailValue.ContainerModeSpecified);
			}
			else if (orderBizObj.JD_ContainerMode_List.Count > 0)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(orderBizObj.JD_ContainerModeInfo, orderBizObj.JD_ContainerMode_List[0].Code);
			}

			if (orderDetailValue.ExWorksRequiredBy.IsValid)
			{
				if (orderDetailValue.ExWorksRequiredBy.IsValidSmallDateTime)
				{
					orderBizObj.JD_ExWorksRequiredBy = orderDetailValue.ExWorksRequiredBy;
				}
				else
				{
					throw new XmlException(GetCustomDateErrorMessage(nameof(orderDetailValue.ExWorksRequiredBy), orderDetailValue.ExWorksRequiredBy.ToDateTime().ToString()));
				}
			}

			if (orderDetailValue.DeliveryRequiredBy.IsValid)
			{
				if (orderDetailValue.DeliveryRequiredBy.IsValidSmallDateTime)
				{
					orderBizObj.JD_DeliveryRequiredBy = orderDetailValue.DeliveryRequiredBy;
				}
				else
				{
					throw new XmlException(GetCustomDateErrorMessage(nameof(orderDetailValue.DeliveryRequiredBy), orderDetailValue.DeliveryRequiredBy.ToDateTime().ToString()));
				}
			}

			ImportControllingPartAddress(orderBizObj, orderDetailValue, context);
		}

		void ImportControllingPartAddress(Order bizObj, Xsd.OrderOrderDetail value, IValueObjectImportContext context)
		{
			DocAddressValueObjectHelper addressHelper = new DocAddressValueObjectHelper(string.Empty);
			addressHelper.ImportFromValueObjectCollection(value.DocAddresses.DocAddress, bizObj.DocAddresses, context);
		}

		protected ZDecimal ImportOrderLines(Order orderBizObj, Xsd.OrderOrderLineCollection orderLinesValue, IValueObjectImportContext context, string errorContext)
		{
			ZDecimal totalValue = 0;
			if (orderLinesValue.IsSpecified)
			{
				foreach (Xsd.OrderOrderLine orderLineValue in orderLinesValue)
				{
					totalValue += ImportOrderLine(orderBizObj, orderLineValue, context, errorContext);
				}
			}
			RemoveMissingOrderLines(orderBizObj.OrderLines, orderLinesValue, context);
			return totalValue;
		}

		protected virtual ZDecimal ImportOrderLine(Order orderBizObj, Xsd.OrderOrderLine orderLineValue, IValueObjectImportContext context, string errorContext)
		{
			ZDecimal totalValue = 0;
			OrderLine orderLine = FindExistingOrderLine(orderBizObj.OrderLines, orderLineValue, context)
				?? orderBizObj.OrderLines.AddNew();

			((ISupportDataImporting)orderLine).IsImportingData = true;
			try
			{
				if (orderLineValue.OrderLineNo > 0)
				{
					orderLine.JO_LineNo = orderLineValue.OrderLineNo;
				}
				else
				{
					context.Notify(new WarningNotification(Res.GetString("9606d019-520b-4d67-a6c9-4326bb66329d", "Line Number is not greater than or equal to 1 in {0}. It will take the next valid Line Number.", orderLine.HumanReadableName)));
				}

				if (orderLineValue.OrderLineSplitNoSpecified)
				{
					orderLine.JO_LineSplitNumber = (short)orderLineValue.OrderLineSplitNo;
				}

				if (orderLineValue.OrderSubLineNoSpecified)
				{
					orderLine.JO_SubLineNo = orderLineValue.OrderSubLineNo == 0 ? (ZInt)1 : orderLineValue.OrderSubLineNo;
				}

				if (orderLineValue.OrderLineDetail.IsSpecified)
				{
					context.SetPropertyInfoValue(orderLine.JO_DescriptionInfo, orderLineValue.OrderLineDetail.Description, orderLineValue.OrderLineDetail.DescriptionSpecified);
					if (orderLineValue.OrderLineDetail.DropDate.IsValid)
					{
						if (orderLineValue.OrderLineDetail.DropDate.IsValidSmallDateTime)
						{
							context.SetPropertyInfoValue(orderLine.JO_LineDropDateInfo, orderLineValue.OrderLineDetail.DropDate.ToDateTime());
						}
						else
						{
							throw new XmlException(GetCustomDateErrorMessage(nameof(orderLineValue.OrderLineDetail.DropDate), orderLineValue.OrderLineDetail.DropDate.ToDateTime().ToString()));
						}
					}

					if (orderLineValue.OrderLineDetail.InnerPacks.IsSpecified)
					{
						context.SetPropertyInfoValue(orderLine.JO_InnerPacksInfo, orderLineValue.OrderLineDetail.InnerPacks.Value, JobOrderLineSchema.JO_InnerPacks);
						ImportPackageType(orderLine.JO_InnerPacksUQInfo, orderLineValue.OrderLineDetail.InnerPacks.DimensionType, context, orderBizObj);
					}

					if (orderLineValue.OrderLineDetail.OuterPacks.IsSpecified)
					{
						context.SetPropertyInfoValue(orderLine.JO_OuterPacksInfo, orderLineValue.OrderLineDetail.OuterPacks.Value, JobOrderLineSchema.JO_OuterPacks);
						ImportPackageType(orderLine.JO_OuterPacksUQInfo, orderLineValue.OrderLineDetail.OuterPacks.DimensionType, context, orderBizObj);
					}

					context.SetPropertyInfoValue(orderLine.JO_ItemPriceInfo, orderLineValue.OrderLineDetail.ItemPrice.Value, JobOrderLineSchema.JO_ItemPrice);
					context.SetPropertyInfoValue(orderLine.JO_LinePriceInfo, orderLineValue.OrderLineDetail.LinePrice.Value, JobOrderLineSchema.JO_LinePrice);

					if (orderLineValue.OrderLineDetail.VolumeTypeSpecified && orderLineValue.OrderLineDetail.VolumeType.Length <= 2)
					{
						context.SetPropertyInfoValue(orderLine.JO_UnitOfVolumeInfo, orderLineValue.OrderLineDetail.VolumeType);
					}

					if (orderLineValue.OrderLineDetail.VolumeSpecified)
					{
						context.SetPropertyInfoValue(orderLine.JO_ActualVolumeInfo, orderLineValue.OrderLineDetail.Volume, JobOrderLineSchema.JO_ActualVolume);
					}

					if (orderLineValue.OrderLineDetail.WeightTypeSpecified && orderLineValue.OrderLineDetail.WeightType.Length <= 2)
					{
						context.SetPropertyInfoValue(orderLine.JO_UnitOfWeightInfo, orderLineValue.OrderLineDetail.WeightType);
					}

					if (orderLineValue.OrderLineDetail.WeightSpecified)
					{
						context.SetPropertyInfoValue(orderLine.JO_ActualWeightInfo, orderLineValue.OrderLineDetail.Weight, JobOrderLineSchema.JO_ActualWeight);
					}

					context.SetPropertyInfoValue(orderLine.JO_PartnoInfo, orderLineValue.OrderLineDetail.Product, orderLineValue.OrderLineDetail.ProductSpecified);

					if (orderLineValue.OrderLineDetail.QtyOrdered.IsSpecified)
					{
						if (orderLineValue.OrderLineDetail.LinePrice.IsSpecified)
						{
							totalValue += orderLineValue.OrderLineDetail.LinePrice.Value;
						}

						context.SetPropertyInfoValue(orderLine.JO_QuantityInfo, orderLineValue.OrderLineDetail.QtyOrdered.Value, JobOrderLineSchema.JO_Quantity);

						if (!orderLineValue.OrderLineDetail.QtyOrdered.DimensionType.IsEmpty)
						{
							ImportPackageType(orderLine.JO_F3_NKPackTypeInfo, orderLineValue.OrderLineDetail.QtyOrdered.DimensionType, context, orderBizObj);
						}
					}

					SetOrderItemLineQuantityValues(orderLine, context);

					if (orderLineValue.OrderLineDetail.QtyInvoiced.IsSpecified)
					{
						context.SetPropertyInfoValue(orderLine.JO_QtyInvoicedInfo, orderLineValue.OrderLineDetail.QtyInvoiced.Value, JobOrderLineSchema.JO_QtyInvoiced);
					}

					if (orderLineValue.OrderLineDetail.QtyReceived.IsSpecified)
					{
						context.SetPropertyInfoValue(orderLine.JO_QtyReceivedInfo, orderLineValue.OrderLineDetail.QtyReceived.Value, JobOrderLineSchema.JO_QtyReceived);
					}

					if (orderLineValue.OrderLineDetail.LineStatusSpecified)
					{
						context.SetPropertyInfoValue(orderLine.JO_LineStatusInfo, GetValidOrderOrOrderLineStatusCode(orderLine.JO_LineStatus_List, orderLineValue.OrderLineDetail.LineStatus, context), orderLineValue.OrderLineDetail.LineStatusSpecified);
					}

					context.SetPropertyInfoValue(orderLine.JO_PartAttrib1Info, orderLineValue.OrderLineDetail.PartAttrib1, orderLineValue.OrderLineDetail.PartAttrib1Specified);
					context.SetPropertyInfoValue(orderLine.JO_PartAttrib2Info, orderLineValue.OrderLineDetail.PartAttrib2, orderLineValue.OrderLineDetail.PartAttrib2Specified);
					context.SetPropertyInfoValue(orderLine.JO_PartAttrib3Info, orderLineValue.OrderLineDetail.PartAttrib3, orderLineValue.OrderLineDetail.PartAttrib3Specified);
					context.SetPropertyInfoValue(orderLine.JO_ContainerNumberInfo, orderLineValue.OrderLineDetail.ContainerNumber, orderLineValue.OrderLineDetail.ContainerNumberSpecified);
					context.SetPropertyInfoValue(orderLine.JO_CommercialInvoiceNoInfo, orderLineValue.OrderLineDetail.CommercialInvoiceNo, orderLineValue.OrderLineDetail.CommercialInvoiceNoSpecified);
					context.SetPropertyInfoValue(orderLine.JO_RN_NKCountryOfOriginInfo, orderLineValue.OrderLineDetail.CountryOfOrigin, orderLineValue.OrderLineDetail.CountryOfOriginSpecified);
					context.SetPropertyInfoValue(orderLine.JO_SpecialInstructionsInfo, orderLineValue.OrderLineDetail.SpecialInstructions, orderLineValue.OrderLineDetail.SpecialInstructionsSpecified);
					context.SetPropertyInfoValue(orderLine.JO_AdditionalInformationInfo, orderLineValue.OrderLineDetail.AdditionalInformation, orderLineValue.OrderLineDetail.AdditionalInformationSpecified);
					if (orderLineValue.OrderLineDetail.ContainerPackingOrder > 0)
					{
						orderLine.JO_ContainerPackingOrder = orderLineValue.OrderLineDetail.ContainerPackingOrder;
					}

					context.SetPropertyInfoValue(orderLine.JO_INCOInfo, orderLineValue.OrderLineDetail.Incoterm, orderLineValue.OrderLineDetail.IncotermSpecified);
					context.SetPropertyInfoValue(orderLine.JO_AdditionalTermsInfo, orderLineValue.OrderLineDetail.AdditionalTerms, orderLineValue.OrderLineDetail.AdditionalTermsSpecified);

					context.SetPropertyInfoValue(orderLine.JO_ConfirmationNumInfo, orderLineValue.OrderLineDetail.ConfirmNumber, orderLineValue.OrderLineDetail.ConfirmNumberSpecified);

					if (orderLineValue.OrderLineDetail.ConfirmDate.IsValid)
					{
						if (orderLineValue.OrderLineDetail.ConfirmDate.IsValidSmallDateTime)
						{
							context.SetPropertyInfoValue(orderLine.JO_ConfirmationDateInfo, orderLineValue.OrderLineDetail.ConfirmDate.ToDateTime());
						}
						else
						{
							throw new XmlException(GetCustomDateErrorMessage(nameof(orderLineValue.OrderLineDetail.ConfirmDate), orderLineValue.OrderLineDetail.ConfirmDate.ToDateTime().ToString()));
						}
					}

					if (orderLineValue.OrderLineDetail.ExWorksRequiredBy.IsValid)
					{
						if (orderLineValue.OrderLineDetail.ExWorksRequiredBy.IsValidSmallDateTime)
						{
							context.SetPropertyInfoValue(orderLine.JO_ExWorksDateInfo, orderLineValue.OrderLineDetail.ExWorksRequiredBy.ToDateTime());
						}
						else
						{
							throw new XmlException(GetCustomDateErrorMessage(nameof(orderLineValue.OrderLineDetail.ExWorksRequiredBy), orderLineValue.OrderLineDetail.ExWorksRequiredBy.ToDateTime().ToString()));
						}
					}

					ImportUNDGSubstanceDetails(orderLineValue.OrderLineDetail, orderLine, context);
					ImportOrderLineDeliveries(orderLineValue.OrderLineDeliveries, orderLine, context, errorContext);
					ImportCustomOrderLineDetails(orderLineValue, orderLine, context);
				}

				MatchProduct(orderLine, context);
			}
			finally
			{
				((ISupportDataImporting)orderLine).IsImportingData = false;
			}

			return totalValue;
		}

		void MatchProduct(OrderLine line, IValueObjectImportContext context)
		{
			var part = new Part
			{
				PartNum = line.JO_Partno,
				Description = line.JO_Description,
				Buyer = line.Order != null ? line.Order.Buyer : null,
				Supplier = line.Order != null ? line.Order.Supplier : null,
			};

			new ProductMatcher(context.Factory).Match(part, out var match, out var reasonForNotMatched);
			if (match == null && reasonForNotMatched == ProductMatcher.ReasonsForNotMatched.DuplicateProduct)
			{
				context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("8ab3bdda-4790-4ee0-990b-556b75591e54", "Could not add Product {0} as it would result in a duplicate.", part.PartNum)));
			}
		}

		void ImportPackageType(ZPropertyInfo propertyInfo, ZString value, IValueObjectImportContext context, Order order)
		{
			string packageContextMsg = Res.GetString("64d39b0b-1cf7-49be-8d4b-a0ba06212445", "The Package type ({0}) of {1}", value, order.HumanReadableName);
			CommonDefinedResourceStrings.SetPackageTypeProperty(propertyInfo, value, packageContextMsg, context, true);
		}

		void ImportUNDGSubstanceDetails(Xsd.OrderOrderLineOrderLineDetail orderLineDetail, OrderLine orderLine, IValueObjectImportContext context)
		{
			string errorContext = Res.GetString("95cdec79-3861-4f01-a386-4ca3887c6cbb", "Order {0} Line {1}", orderLine.Order.JD_OrderNumber, orderLine.JO_LineNo);
			orderLineDetail.DangerousGoods.ImportToUNDGDataItems(() => orderLine.UNDGs, errorContext, context);

			foreach (Xsd.HazardousGoods hazGoods in orderLineDetail.DangerousGoods)
			{
				var substances = UNDGSubstanceLoader.LoadSubstances(context.Factory, hazGoods.UNDGCode.SubstringSafe(0, 4), hazGoods.UNDGCode.SubstringSafe(4, 2), hazGoods.Standard).ToArray();
				if (substances == null || substances.Length == 0 || substances.Length > 1 || (substances.Length > 0 && substances[0].DG_PSN == UNDGDataItemLookups.Constants.UnknownSubstance.ToString()))
				{
					AddInvalidUNDGWorkflowException(orderLine, hazGoods.UNDGCode);
				}
			}

			// Legacy
			if (!orderLineDetail.DGSubstanceCode.IsEmpty)
			{
				UNDGDataItem dgItem = orderLine.UNDGs.TryGetOrCreate(orderLineDetail.DGSubstanceCode, orderLineDetail.Standard);
				if (dgItem != null)
				{
					dgItem.DI_DGFlashPoint = orderLineDetail.DGFlashPoint;
				}
				if (dgItem == null || dgItem.Substance == null || dgItem.Substance.DG_PSN == UNDGDataItemLookups.Constants.UnknownSubstance.ToString())
				{
					AddInvalidUNDGWorkflowException(orderLine, orderLineDetail.DGSubstanceCode);
				}
			}
		}

		void AddInvalidUNDGWorkflowException(OrderLine orderLine, string invalidDGCode)
		{
			ProcessTask exception = orderLine.Order.WorkflowItems.Exceptions.AddNew();
			string uNDGExceptionMessage = Res.GetString("1d9a62ae-15de-4aa2-9dcd-b2ac9a8bd542", "Invalid or multiple UNDG for Line {0} - '{1}'", orderLine.JO_LineNo, invalidDGCode);

			string description;
			if (uNDGExceptionMessage.Length > ProcessTasksSchema.P9_Description.MaxLength)
			{
				description = Res.GetString("2b570056-ea25-4866-b2da-16cb79b04af8", "Invalid or multiple UNDG for Line {0}", orderLine.JO_LineNo);
			}
			else
			{
				description = Res.GetString("3a8900b7-042b-4857-9423-f6ee3a92e7d6", "Invalid or multiple UNDG for Line {0} - '{1}'", orderLine.JO_LineNo, invalidDGCode);
			}
			exception.P9_Description = description.Length > ProcessTasksSchema.P9_Description.MaxLength ? description.Substring(0, ProcessTasksSchema.P9_Description.MaxLength) : description;

			exception.P9_Notes = ZBlob.FromAscii(uNDGExceptionMessage);
			exception.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionDataConversionIssue;
		}

		protected virtual void SetOrderItemLineQuantityValues(OrderLine orderLine, IValueObjectImportContext context)
		{
			if (orderLine.JO_LinePrice != 0 && orderLine.JO_Quantity != 0)
			{
				if (orderLine.JO_ItemPrice.IsEmpty)
				{
					context.SetPropertyInfoValue(orderLine.JO_ItemPriceInfo, orderLine.JO_LinePrice / orderLine.JO_Quantity, JobOrderLineSchema.JO_ItemPrice);
				}
			}

			if (orderLine.JO_LinePrice != 0 && orderLine.JO_ItemPrice != 0)
			{
				if (orderLine.JO_Quantity.IsEmpty)
				{
					context.SetPropertyInfoValue(orderLine.JO_QuantityInfo, orderLine.JO_LinePrice / orderLine.JO_ItemPrice, JobOrderLineSchema.JO_Quantity);
				}
			}

			if (orderLine.JO_ItemPrice != 0 && orderLine.JO_Quantity != 0)
			{
				if (orderLine.JO_LinePrice.IsEmpty)
				{
					context.SetPropertyInfoValue(orderLine.JO_LinePriceInfo, orderLine.JO_Quantity * orderLine.JO_ItemPrice, JobOrderLineSchema.JO_LinePrice);
				}
			}
		}

		protected virtual OrderLine FindExistingOrderLine(OrderLineCollection existingLines, Xsd.OrderOrderLine xsdOrderLine, INotifications notify)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobOrderLineSchema.JO_LineNo, xsdOrderLine.OrderLineNo);
			query.AddToFilter(JobOrderLineSchema.JO_SubLineNo, xsdOrderLine.OrderSubLineNo);
			IList<OrderLine> orderLines = new List<OrderLine>(existingLines.Find(query));
			if (orderLines.Count == 0 && xsdOrderLine.OrderSubLineNo == 0)
			{
				query = new ZQuery();
				query.AddToFilter(JobOrderLineSchema.JO_LineNo, xsdOrderLine.OrderLineNo);
				query.AddToFilter(JobOrderLineSchema.JO_SubLineNo, 1);
				orderLines = new List<OrderLine>(existingLines.Find(query));
			}
			return orderLines.Count == 1 ? orderLines[0] : null;
		}

		protected virtual void RemoveMissingOrderLines(OrderLineCollection existingLines, Xsd.OrderOrderLineCollection xsdOrderLines, INotifications notify)
		{
		}

		void ImportCustomOrderLineDetails(Xsd.OrderOrderLine orderLineValue, OrderLine orderLine, IValueObjectImportContext context)
		{
			if (orderLineValue.OrderLineDetail.Custom.IsSpecified)
			{
				Xsd.OrderOrderLineOrderLineDetailCustom custom = orderLineValue.OrderLineDetail.Custom;
				if (custom.Date1.IsValid)
				{
					if (custom.Date1.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLine.JO_CustomDate1Info, custom.Date1.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date1), custom.Date1.ToDateTime().ToString()));
					}
				}
				if (custom.Date2.IsValid)
				{
					if (custom.Date2.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLine.JO_CustomDate2Info, custom.Date2.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date2), custom.Date2.ToDateTime().ToString()));
					}
				}
				if (custom.Date3.IsValid)
				{
					if (custom.Date3.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLine.JO_CustomDate3Info, custom.Date3.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date3), custom.Date3.ToDateTime().ToString()));
					}
				}
				if (custom.Date4.IsValid)
				{
					if (custom.Date4.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLine.JO_CustomDate4Info, custom.Date4.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date4), custom.Date4.ToDateTime().ToString()));
					}
				}
				if (custom.Date5.IsValid)
				{
					if (custom.Date5.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLine.JO_CustomDate5Info, custom.Date5.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date5), custom.Date5.ToDateTime().ToString()));
					}
				}

				if (custom.Decimal1Specified)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomDecimal1Info, custom.Decimal1, JobOrderLineSchema.JO_CustomDecimal1);
				}
				if (custom.Decimal2Specified)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomDecimal2Info, custom.Decimal2, JobOrderLineSchema.JO_CustomDecimal2);
				}
				if (custom.Decimal3Specified)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomDecimal3Info, custom.Decimal3, JobOrderLineSchema.JO_CustomDecimal3);
				}
				if (custom.Decimal4Specified)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomDecimal4Info, custom.Decimal4, JobOrderLineSchema.JO_CustomDecimal4);
				}
				if (custom.Decimal5Specified)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomDecimal5Info, custom.Decimal5, JobOrderLineSchema.JO_CustomDecimal5);
				}

				if (custom.Flag1Specified)
				{
					orderLine.JO_CustomFlag1 = custom.Flag1;
				}
				if (custom.Flag2Specified)
				{
					orderLine.JO_CustomFlag2 = custom.Flag2;
				}
				if (custom.Flag3Specified)
				{
					orderLine.JO_CustomFlag3 = custom.Flag3;
				}
				if (custom.Flag4Specified)
				{
					orderLine.JO_CustomFlag4 = custom.Flag4;
				}
				if (custom.Flag5Specified)
				{
					orderLine.JO_CustomFlag5 = custom.Flag5;
				}

				if (custom.Text1.IsValid)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomAttrib1Info, custom.Text1, custom.Text1Specified, Res.GetString("9a098413-80fb-4384-a62f-c84cfc6c2c24", "Line Custom Attribute 1"));
				}
				if (custom.Text2.IsValid)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomAttrib2Info, custom.Text2, custom.Text2Specified, Res.GetString("2fcb8408-a5a2-4366-ad50-a953c46463d1", "Line Custom Attribute 2"));
				}
				if (custom.Text3.IsValid)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomAttrib3Info, custom.Text3, custom.Text3Specified, Res.GetString("2f21b950-c9b5-4db7-a26e-5a6c766a926f", "Line Custom Attribute 3"));
				}
				if (custom.Text4.IsValid)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomAttrib4Info, custom.Text4, custom.Text4Specified, Res.GetString("ed4a124c-8a83-450d-960d-2f535502cb52", "Line Custom Attribute 4"));
				}
				if (custom.Text5.IsValid)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomAttrib5Info, custom.Text5, custom.Text5Specified, Res.GetString("6ed1627b-4b15-4a57-a66c-0dd759abb788", "Line Custom Attribute 5"));
				}
				if (custom.Text6.IsValid)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomAttrib6Info, custom.Text6, custom.Text6Specified, Res.GetString("6dd739a4-f82e-4d55-bae3-82a45a42aae6", "Line Custom Attribute 6"));
				}

				if (custom.CustomText1.IsValid)
				{
					context.SetPropertyInfoValue(orderLine.JO_CustomTextBlob1Info, custom.CustomText1, custom.CustomText1Specified, Res.GetString("35b422c2-7775-4cf7-ac58-b2cbc342e85a", "Line Custom Text Blob 1"));
				}
			}
		}

		void ImportOrderLineDeliveries(Xsd.OrderOrderLineOrderLineDeliveryCollection orderLineDeliveriesValue, OrderLine orderLine, IValueObjectImportContext context, string errorContext)
		{
			if (orderLineDeliveriesValue.IsSpecified)
			{
				List<OrderLineDelivery> updatedDeliveries = new List<OrderLineDelivery>();

				foreach (Xsd.OrderOrderLineOrderLineDelivery deliveryValue in orderLineDeliveriesValue)
				{
					if (deliveryValue.DeliveryDetails.IsSpecified)
					{
						OrderLineDelivery delivery = FindExistingOrderLineDelivery(orderLine, orderLine.Deliveries, deliveryValue, context)
							?? orderLine.Deliveries.AddNew();
						updatedDeliveries.Add(delivery);

						string deliveryPortName = deliveryValue.DeliveryDetails.DelPort.IsSpecified ? deliveryValue.DeliveryDetails.DelPort.Value : null;
						context.SetPropertyInfoValue(delivery.J4_RL_NKDestinationPortInfo, deliveryPortName, ForeignKeyType.PortNK);
						context.SetPropertyInfoValue(delivery.J4_AllocatedInfo, deliveryValue.DeliveryDetails.QtyAllocated, JobOrderLineDeliverySchema.J4_Allocated);
						ImportOrderLineDeliveryCustomDetails(deliveryValue, delivery, context);
						ImportDeliverPoint(deliveryValue, delivery, orderLine, context);
						ImportOrderLineDeliveryContainers(deliveryValue, delivery, context, errorContext);
					}
				}

				RemoveNotIncludedDeliveries(updatedDeliveries.ToArray(), orderLine.Deliveries);
			}
		}

		void ImportOrderLineDeliveryCustomDetails(Xsd.OrderOrderLineOrderLineDelivery orderLineDeliveryValue, OrderLineDelivery orderLineDelivery, IValueObjectImportContext context)
		{
			if (orderLineDeliveryValue.DeliveryDetails.Custom.IsSpecified)
			{
				Xsd.OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom custom = orderLineDeliveryValue.DeliveryDetails.Custom;

				if (custom.Decimal1Specified)
				{
					context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDecimal1Info, custom.Decimal1, JobOrderLineDeliverySchema.J4_CustomDecimal1);
				}
				if (custom.Decimal2Specified)
				{
					context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDecimal2Info, custom.Decimal2, JobOrderLineDeliverySchema.J4_CustomDecimal2);
				}
				if (custom.Decimal3Specified)
				{
					context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDecimal3Info, custom.Decimal3, JobOrderLineDeliverySchema.J4_CustomDecimal3);
				}
				if (custom.Decimal4Specified)
				{
					context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDecimal4Info, custom.Decimal4, JobOrderLineDeliverySchema.J4_CustomDecimal4);
				}
				if (custom.Decimal5Specified)
				{
					context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDecimal5Info, custom.Decimal5, JobOrderLineDeliverySchema.J4_CustomDecimal5);
				}

				context.SetPropertyInfoValue(orderLineDelivery.J4_CustomAttribute1Info, custom.Text1, custom.Text1Specified, Res.GetString("6ddec07c-5678-41e6-9cb6-262ec5bd279d", "Delivery Line Custom Attribute 1"));
				context.SetPropertyInfoValue(orderLineDelivery.J4_CustomAttribute2Info, custom.Text2, custom.Text2Specified, Res.GetString("0f3db150-1bb6-44db-841b-8d52c9c30667", "Delivery Line Custom Attribute 2"));
				context.SetPropertyInfoValue(orderLineDelivery.J4_CustomAttribute3Info, custom.Text3, custom.Text3Specified, Res.GetString("7701f7e9-a1d2-419b-8940-2e1a8845617a", "Delivery Line Custom Attribute 3"));
				context.SetPropertyInfoValue(orderLineDelivery.J4_CustomAttribute4Info, custom.Text4, custom.Text4Specified, Res.GetString("66b1aae7-b29b-4f37-9989-0464204f9ac8", "Delivery Line Custom Attribute 4"));
				context.SetPropertyInfoValue(orderLineDelivery.J4_CustomAttribute5Info, custom.Text5, custom.Text5Specified, Res.GetString("2e193454-a43a-4a35-893d-6df25c1a1819", "Delivery Line Custom Attribute 5"));

				if (custom.Flag1Specified)
				{
					orderLineDelivery.J4_CustomFlag1 = custom.Flag1;
				}
				if (custom.Flag2Specified)
				{
					orderLineDelivery.J4_CustomFlag2 = custom.Flag2;
				}
				if (custom.Flag3Specified)
				{
					orderLineDelivery.J4_CustomFlag3 = custom.Flag3;
				}
				if (custom.Flag4Specified)
				{
					orderLineDelivery.J4_CustomFlag4 = custom.Flag4;
				}
				if (custom.Flag5Specified)
				{
					orderLineDelivery.J4_CustomFlag5 = custom.Flag5;
				}

				if (custom.Date1.IsValid)
				{
					if (custom.Date1.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDate1Info, custom.Date1.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date1), custom.Date1.ToDateTime().ToString()));
					}
				}
				if (custom.Date2.IsValid)
				{
					if (custom.Date2.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDate2Info, custom.Date2.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date2), custom.Date2.ToDateTime().ToString()));
					}
				}
				if (custom.Date3.IsValid)
				{
					if (custom.Date3.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDate3Info, custom.Date3.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date3), custom.Date3.ToDateTime().ToString()));
					}
				}
				if (custom.Date4.IsValid)
				{
					if (custom.Date4.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDate4Info, custom.Date4.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date4), custom.Date4.ToDateTime().ToString()));
					}
				}
				if (custom.Date5.IsValid)
				{
					if (custom.Date5.IsValidSmallDateTime)
					{
						context.SetPropertyInfoValue(orderLineDelivery.J4_CustomDate5Info, custom.Date5.ToDateTime());
					}
					else
					{
						throw new XmlException(GetCustomDateErrorMessage(nameof(custom.Date5), custom.Date5.ToDateTime().ToString()));
					}
				}
			}
		}

		protected virtual void RemoveNotIncludedDeliveries(IList<OrderLineDelivery> updatedDeliveries, OrderLineDeliveryCollection existingDeliveries)
		{
			for (int i = existingDeliveries.Count - 1; i >= 0; i--)
			{
				if (!updatedDeliveries.Contains(existingDeliveries[i]))
				{
					existingDeliveries[i].Delete();
				}
			}
		}

		protected virtual OrderLineDelivery FindExistingOrderLineDelivery(OrderLine orderLine, OrderLineDeliveryCollection existingDeliveries, Xsd.OrderOrderLineOrderLineDelivery xsdOrderLineDelivery, IValueObjectImportContext context)
		{
			Xsd.OrgAddress addressValue;
			ZQuery query = new ZQuery();
			query.AddToFilter(JobOrderLineDeliverySchema.J4_RL_NKDestinationPort, xsdOrderLineDelivery.DeliveryDetails.DelPort.Value);
			ZString deliveryPointCode = xsdOrderLineDelivery.DeliveryDetails.AddressFreeText;
			if (deliveryPointCode.IsEmpty)
			{
				deliveryPointCode = GetDeliverPointAddress(xsdOrderLineDelivery, orderLine, context, out addressValue);
			}

			query.AddToFilter(JobOrderLineDeliverySchema.J4_OA_NKDeliveryPoint, deliveryPointCode);
			IList<OrderLineDelivery> orderLineDeliveries = new List<OrderLineDelivery>(existingDeliveries.Find(query));
			return orderLineDeliveries.Count == 1 ? orderLineDeliveries[0] : null;
		}

		void ImportDeliverPoint(Xsd.OrderOrderLineOrderLineDelivery deliveryValue, OrderLineDelivery delivery, OrderLine orderLine, IValueObjectImportContext context)
		{
			if (deliveryValue.DeliveryDetails.AddressFreeText.IsEmpty)
			{
				Xsd.OrgAddress orgAddressValue;
				string matchingAddressCode = GetDeliverPointAddress(deliveryValue, orderLine, context, out orgAddressValue);

				if (orgAddressValue != null && orderLine.Order.Buyer != null)
				{
					delivery.J4_OA_NKDeliveryPoint = matchingAddressCode;
					if (string.IsNullOrEmpty(matchingAddressCode))
					{
						string noteText = Res.GetString("dd9b22c5-7b28-4058-bb23-0f214fb035f2", "Order Line {0}:\r\nAddress Code: {1}\r\nAddress: {2}, {3}, {4}, {5}, {6}", orderLine.JO_LineNo, orgAddressValue.AddressCode, orgAddressValue.AddressLine1, orgAddressValue.AddressLine2, orgAddressValue.CityOrSuburb, orgAddressValue.StateOrProvince, orgAddressValue.PostCode) + System.Environment.NewLine;
						UnmatchedDeliverPoint.Append(noteText);

						AddOrderToEmailList(orgAddressValue, deliveryValue.DeliveryDetails.Address.Organisation.OrganisationDetails.Name, orderLine, context);
					}
				}
			}
			else
			{
				delivery.J4_OA_NKDeliveryPoint = deliveryValue.DeliveryDetails.AddressFreeText.SubstringSafe(0, delivery.J4_OA_NKDeliveryPointInfo.MaxLength);
			}
		}

		string GetDeliverPointAddress(Xsd.OrderOrderLineOrderLineDelivery deliveryValue, OrderLine orderLine, IValueObjectImportContext context, out Xsd.OrgAddress orgAddressValue)
		{
			ZString matchingAddressCode = "";
			orgAddressValue = null;

			Xsd.AddressReference addressValue = deliveryValue.DeliveryDetails.Address;
			foreach (Xsd.OrgAddress current in addressValue.Organisation.OrganisationDetails.Addresses)
			{
				if (current.Sequence == addressValue.AddressSequenceRef)
				{
					orgAddressValue = current;
					break;
				}
			}

			OrgHeader orderBuyer = orderLine.Order.Buyer;

			if (orgAddressValue != null && orderBuyer != null)
			{
				if (orderBuyer != null && !orgAddressValue.AddressCode.IsEmpty)
				{
					ZQuery filter = new ZQuery(OrgAddressSchema.OA_Code, orgAddressValue.AddressCode);
					OrgAddress[] matchedAddresses = (OrgAddress[])orderBuyer.Addresses.Find(filter);
					matchingAddressCode = (matchedAddresses.Length > 0) ? matchedAddresses[0].OA_Code : ZString.Empty;
				}

				if (matchingAddressCode.IsEmpty)
				{
					AddressValueObjectHelper addressHelper = new AddressValueObjectHelper(Res.GetString("f5eb9edf-fc4a-4936-a255-1950f1ba5684", "Delivery Point on Order #/Product #{0}/{1}", orderLine.Order.JD_OrderNumber, orderLine.JO_Partno));
					matchingAddressCode = addressHelper.FromAddressReferenceGetAddressCode(deliveryValue.DeliveryDetails.Address, orderBuyer, context);
				}
			}

			return matchingAddressCode;
		}

		protected ZStringBuilder UnmatchedDeliverPoint;

		protected StmNote GetOrCreateNote(Order order)
		{
			StmNote result = null;
			StmNote[] notes = order.Notes.FindByDescription(UnmatchedOrderLineDeliverPointNote);
			if (notes.Length == 0)
			{
				result = order.Notes.AddNew();
				result.ST_Description = UnmatchedOrderLineDeliverPointNote;
				result.ST_IsCustomDescription = true;
			}
			else
			{
				result = notes[0];
			}
			return result;
		}

		internal static string UnmatchedOrderLineDeliverPointNote
		{
			get { return Res.GetString("30b99935-e99e-4df9-99de-ed63521267ec", "Unmatched Order Line Deliver Point Address"); }
		}

		protected virtual void AddOrderToEmailList(Xsd.OrgAddress orgAddressValue, string organisationName, OrderLine orderLine, IValueObjectImportContext context)
		{
			ZString addressDetails = Res.GetString("1b57b334-cede-4ee6-8d6c-b3355efa4ce4", "Organization Name: {0}, Address Code: {1}, Address Line 1: {2}, Address Line 2: {3}, City/Suburb: {4}, State/Province: {5}, Postcode: {6}",
						organisationName,
						orgAddressValue.AddressCode,
						orgAddressValue.AddressLine1,
						orgAddressValue.AddressLine2,
						orgAddressValue.CityOrSuburb,
						orgAddressValue.StateOrProvince,
						orgAddressValue.PostCode);

			ZString deliveryNotFoundMessage = Res.GetString("225e7013-6f9a-4a9a-8a01-c04d4f0acf3e", "Order: {0} ordered by {1} (Code = '{2}'), {3}",
						orderLine.Order.JD_OrderNumber,
												orderLine.Order.Buyer.OH_FullNameTruncated,
						orderLine.Order.Buyer.OH_Code,
						addressDetails);

			AddDeliveryNotFoundMessage(context, deliveryNotFoundMessage);
		}

		protected virtual void AddDeliveryNotFoundMessage(IValueObjectImportContext context, ZString deliveryNotFoundMessage)
		{
			GetDeliverPointNotFoundService(context.Factory).DeliverPointsNotFoundList.Add(deliveryNotFoundMessage);
		}

		protected virtual bool ShouldDeleteAllExistingOrderLineDeliveryContainers
		{
			get { return true; }
		}

		void ImportOrderLineDeliveryContainers(Xsd.OrderOrderLineOrderLineDelivery deliveryValue, OrderLineDelivery delivery, IValueObjectImportContext context, string errorContext)
		{
			if (deliveryValue.DeliveryContainers.IsSpecified)
			{
				if (ShouldDeleteAllExistingOrderLineDeliveryContainers)
				{
					delivery.Containers.DeleteAll();
				}

				foreach (Xsd.OrderOrderLineOrderLineDeliveryDeliveryContainer containerValue in deliveryValue.DeliveryContainers)
				{
					var container = delivery.Containers.AddNew();
					if (containerValue.Container.IsSpecified)
					{
						context.SetPropertyInfoValue(container.J5_ContainerNumInfo, containerValue.Container.ContainerNumber, containerValue.Container.ContainerNumberSpecified);
						new ContainerValueObjectHelper(context).ImportContainerType(container.J5_RC_NKContainerTypeInfo, containerValue.Container.ContainerType);

						if (containerValue.Container.PackingMode != ContainerModeToXmlCodeMappings.Instance.GetExternalCode(delivery.OrderLine.Order.JD_ContainerMode, errorContext, context))
						{
							context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("a6f4378c-ecda-4768-a61d-a685b22b4033", "You cannot have a packing mode on the container that differs from that of the order ({0})", delivery.OrderLine.Order.HumanReadableName)));
						}

						context.SetPropertyInfoValue(container.J5_ContainerSealInfo, containerValue.Container.Seal, containerValue.Container.SealSpecified);
					}

					if (containerValue.ETA.IsValid)
					{
						if (containerValue.ETA.IsValidSmallDateTime)
						{
							container.J5_ETA = containerValue.ETA;
						}
						else
						{
							throw new XmlException(GetCustomDateErrorMessage(nameof(containerValue.ETA), containerValue.ETA.ToDateTime().ToString()));
						}
					}

					if (containerValue.ETD.IsValid)
					{
						if (containerValue.ETD.IsValidSmallDateTime)
						{
							container.J5_ETD = containerValue.ETD;
						}
						else
						{
							throw new XmlException(GetCustomDateErrorMessage(nameof(containerValue.ETD), containerValue.ETD.ToDateTime().ToString()));
						}
					}

					context.SetPropertyInfoValue(container.J5_RL_NKLoadPortInfo, containerValue.LoadPort.IsSpecified ? containerValue.LoadPort.Value : null, ForeignKeyType.PortNK);
					context.SetPropertyInfoValue(container.J5_MasterBillInfo, containerValue.MasterBillNo, containerValue.MasterBillNoSpecified);
					container.J5_PackCount = (short)containerValue.PackCount;
					context.SetPropertyInfoValue(container.J5_F3_NKPackTypeInfo, PkgUnitXmlCodeMappings.Instance.GetEnterpriseCode(containerValue.PackType, errorContext, context), containerValue.PackTypeSpecified);
					context.SetPropertyInfoValue(container.J5_RV_NKArrivalVesselInfo, containerValue.Vessel, containerValue.VesselSpecified);
					context.SetPropertyInfoValue(container.J5_QuantityInvoicedInfo, containerValue.QtyInvoiced, JobOrderLineDeliverContainerSchema.J5_QuantityInvoiced);
					context.SetPropertyInfoValue(container.J5_QuantityInStoreInfo, containerValue.QtyDelivered, JobOrderLineDeliverContainerSchema.J5_QuantityInStore);

					if (containerValue.Volume.IsSpecified)
					{
						context.SetPropertyInfoValue(container.J5_VolumeInfo, containerValue.Volume.Value, JobOrderLineDeliverContainerSchema.J5_Volume);
						container.J5_VolumeUQ = VolumeUQXmlCodeMappings.Instance.GetEnterpriseCode(containerValue.Volume.DimensionType, errorContext, context);
					}

					if (containerValue.Weight.IsSpecified)
					{
						context.SetPropertyInfoValue(container.J5_WeightInfo, containerValue.Weight.Value, JobOrderLineDeliverContainerSchema.J5_Weight);
						container.J5_WeightUQ = WeightUQXmlCodeMappings.Instance.GetEnterpriseCode(containerValue.Weight.DimensionType, errorContext, context);
					}

					context.SetPropertyInfoValue(container.J5_VoyageInfo, containerValue.Voyage, containerValue.VoyageSpecified);

					if (!containerValue.Container.IsSpecified || !containerValue.Container.ContainerType.IsSpecified)
					{
						container.J5_RC_NKContainerType = ZString.Empty;
					}
				}
			}
		}

		bool LinkOrderToShipmentOrDeclaration(Order orderBizObj, Xsd.OrderOrderDetailReferenceNumber orderDetailReferenceNo, IValueObjectImportContext context)
		{
			BusinessObjectFactory factory = orderBizObj.Factory;
			ZQuery filter;
			ZString referenceNumber = orderDetailReferenceNo.Value;
			BusinessObject bizObjToLinkTo = null;
			if (orderDetailReferenceNo.Type == Xsd.OrderOrderDetailReferenceNumberType.Shipment)
			{
				filter = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, referenceNumber);
				bizObjToLinkTo = factory.LoadTop1<ForwardingShipment>(filter);

				if (bizObjToLinkTo != null)
				{
					orderBizObj.JD_JS = bizObjToLinkTo.PK;

					var helper = new OrdersOnShipmentLimitHelper((ForwardingShipment)bizObjToLinkTo);
					ShipmentOrdersDataAdapterHelper.CheckOrderLimitNotExceeded(context, helper);
				}
			}
			else
			{
				filter = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, referenceNumber);
				bizObjToLinkTo = factory.LoadTop1(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(), filter);

				if (bizObjToLinkTo != null)
				{
					orderBizObj.JD_JE = bizObjToLinkTo.PK;

					var notification = new OrdersOnDeclarationLimitHelper((Enterprise.Integration.Customs.IBaseJobDeclaration)bizObjToLinkTo).CreateNotification();

					if (notification != null && notification.Type == CargoWise.ComponentModel.NotificationType.Error)
					{
						context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, notification.Message));
					}
				}
			}

			if (bizObjToLinkTo == null)
			{
				ZString warningMessage = Res.GetString("371db6f5-ca79-4a37-8da5-8ab8665441be", "No {0} exists with {0} Number: {1}", orderDetailReferenceNo.Type.ToString(), referenceNumber);
				context.Notify(new WarningNotification(warningMessage));
			}

			return bizObjToLinkTo != null;
		}

		#region class DeliverPointNotFoundService

		protected DeliverPointNotFoundService GetDeliverPointNotFoundService(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("OrderValueObjectDataAdapter.DeliverPointNotFoundService", () =>
			{
				var result = factory.ServiceContainer.GetService<DeliverPointNotFoundService>();
				if (result == null)
				{
					result = new DeliverPointNotFoundService(factory);
					factory.ServiceContainer.AddService(result);
				}

				return result;
			});
		}

		protected class DeliverPointNotFoundService : IService
		{
			public DeliverPointNotFoundService(BusinessObjectFactory factory)
			{
				Factory = factory;
				Factory.Saved += new BusinessObjectFactory.SavedEventHandler(factory_Saved);
			}

			void SendEmail()
			{
				EmailDef email = new EmailDef();
				ZStringBuilder emailBody = new ZStringBuilder();

				foreach (ZString current in DeliverPointsNotFoundList)
				{
					emailBody.Append(current);
				}
				email.Body = DeliverPointAddressErrorMessage + System.Environment.NewLine + emailBody.ToStringWithNewLineBetweenAppends();
				email.Subject = Res.GetString("ad9867d3-f798-4bf9-bb97-b6229fc99b32", "Order Import from XML - Delivery Points not found");
				Env.OutgoingMailManager.CreateAndSave(email, NotificationDataRegistry.Instance.OrderImportNotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(NotificationDataRegistry.Instance.OrderImportNotificationGroup));
			}

			void factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					if (DeliverPointsNotFoundList.Count > 0)
					{
						SendEmail();
					}
					Factory.ServiceContainer.RemoveService<DeliverPointNotFoundService>();
				}
			}

			public List<ZString> DeliverPointsNotFoundList = new List<ZString>();
			readonly BusinessObjectFactory Factory;
		}

		#endregion

		internal static string DeliverPointAddressErrorMessage
		{
			get { return Res.GetString("f2519668-f4dd-4c0a-8cc6-21e7bd42b991", "No matching Deliver Point found on 'Ordered by' organization. Deliver point could therefore not be imported on the following order(s):"); }
		}

		#endregion

		#region ExportToValueObject

		protected override void ExportToValueObjectCore(TBusinessObject orderBizObj, TValueObject xsdOrder, IValueObjectExportContext context)
		{
			string errorContext = Res.GetString("f1b42fa1-5bd1-4e07-8280-09b9c9d85b16", "Order {0}", orderBizObj.JD_OrderNumber);
			xsdOrder.Events = StmALogValueObjectDataAdapter.New(orderBizObj, errorContext, TriggeredByEvents).ToXmlCollectionValueObject(context);
			xsdOrder.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(orderBizObj.Notes, context);

			xsdOrder.OrderIdentifier = ExportOrderNumberAndSplit(orderBizObj);
			ZDecimal totalValue = ExportOrderLines(orderBizObj, xsdOrder, context, errorContext);
			xsdOrder.OrderDetail = ExportOrderDetails(orderBizObj, totalValue, context, errorContext);
			xsdOrder.OrderDetail.ShipmentPlanning = ExportOrderShipmentPlanning(orderBizObj, context);
			xsdOrder.OrderDetail.Milestones = ExportOrderMilestones(orderBizObj);
			xsdOrder.OrderDetail.ReferenceNumber.Value = ZString.Empty;

			ExportCustomOrderDetails(xsdOrder.OrderDetail.Custom, orderBizObj);

			AddExportEvent(xsdOrder, orderBizObj, context);
		}

		protected virtual void ExportCustomOrderDetails(Xsd.OrderOrderDetailCustom custom, Order orderBizObj)
		{
			if (orderBizObj.JD_CustomDate1.IsValid)
			{
				custom.Date1 = orderBizObj.JD_CustomDate1;
			}
			if (orderBizObj.JD_CustomDate2.IsValid)
			{
				custom.Date2 = orderBizObj.JD_CustomDate2;
			}

			CustomLabelInfoList list = new Order.CustomLabelsProvider(orderBizObj.Buyer, false).GetCustomFields(orderBizObj.Buyer, orderBizObj.Factory);

			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomDecimal1.Name).IsEnabled)
			{
				custom.Decimal1 = orderBizObj.JD_CustomDecimal1;
			}
			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomDecimal2.Name).IsEnabled)
			{
				custom.Decimal2 = orderBizObj.JD_CustomDecimal2;
			}
			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomDecimal3.Name).IsEnabled)
			{
				custom.Decimal3 = orderBizObj.JD_CustomDecimal3;
			}
			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomDecimal4.Name).IsEnabled)
			{
				custom.Decimal4 = orderBizObj.JD_CustomDecimal4;
			}
			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomDecimal5.Name).IsEnabled)
			{
				custom.Decimal5 = orderBizObj.JD_CustomDecimal5;
			}

			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomFlag1.Name).IsEnabled)
			{
				custom.Flag1 = orderBizObj.JD_CustomFlag1;
				custom.Flag1Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomFlag2.Name).IsEnabled)
			{
				custom.Flag2 = orderBizObj.JD_CustomFlag2;
				custom.Flag2Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomFlag3.Name).IsEnabled)
			{
				custom.Flag3 = orderBizObj.JD_CustomFlag3;
				custom.Flag3Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomFlag4.Name).IsEnabled)
			{
				custom.Flag4 = orderBizObj.JD_CustomFlag4;
				custom.Flag4Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderHeaderSchema.JD_CustomFlag5.Name).IsEnabled)
			{
				custom.Flag5 = orderBizObj.JD_CustomFlag5;
				custom.Flag5Specified = true;
			}

			custom.Text1 = orderBizObj.JD_CustomAttrib1.IsEmpty ? null : orderBizObj.JD_CustomAttrib1;
			custom.Text2 = orderBizObj.JD_CustomAttrib2.IsEmpty ? null : orderBizObj.JD_CustomAttrib2;
			custom.Text3 = orderBizObj.JD_CustomAttrib3.IsEmpty ? null : orderBizObj.JD_CustomAttrib3;
			custom.Text4 = orderBizObj.JD_CustomAttrib4.IsEmpty ? null : orderBizObj.JD_CustomAttrib4;
			custom.Text5 = orderBizObj.JD_CustomAttrib5.IsEmpty ? null : orderBizObj.JD_CustomAttrib5;

			custom.Contact1 = orderBizObj.JD_FirstBuyerContact.IsEmpty ? null : orderBizObj.JD_FirstBuyerContact;
			custom.Contact2 = orderBizObj.JD_SecondBuyerContact.IsEmpty ? null : orderBizObj.JD_SecondBuyerContact;
		}

		Xsd.OrderOrderIdentifier ExportOrderNumberAndSplit(Order orderBizObj)
		{
			Xsd.OrderOrderIdentifier result = new Xsd.OrderOrderIdentifier();

			if (!orderBizObj.JD_OrderNumber.IsEmpty)
			{
				result.OrderNumber = orderBizObj.JD_OrderNumber;
			}

			result.OrderNumberSplit = orderBizObj.JD_OrderNumberSplit;

			return result;
		}

		Xsd.OrderOrderDetail ExportOrderDetails(Order orderBizObj, ZDecimal totalValue, IValueObjectExportContext context, string errorContext)
		{
			Xsd.OrderOrderDetail result = new Xsd.OrderOrderDetail();

			result.Buyer = (Xsd.Organisation)GetOrganisationValueObjectDataAdapter().ExportToValueObject(orderBizObj.Buyer, context);
			result.Supplier = (Xsd.Organisation)GetOrganisationValueObjectDataAdapter().ExportToValueObject(orderBizObj.Supplier, context);
			result.AdditionalTerms = orderBizObj.JD_AdditionalTerms;

			if (!orderBizObj.JD_BookingConfRef.IsEmpty)
			{
				result.ConfirmNumber = orderBizObj.JD_BookingConfRef;
			}

			if (orderBizObj.JD_BookingConfDate.IsValid)
			{
				result.ConfirmDate = orderBizObj.JD_BookingConfDate;
			}

			if (orderBizObj.JD_InvoiceDate.IsValid)
			{
				result.InvoiceDate = orderBizObj.JD_InvoiceDate;
			}

			if (!orderBizObj.JD_ContainerMode.IsEmpty)
			{
				result.ContainerMode = OrderContainerModeToXmlCodeMappings.Instance.GetExternalCode(orderBizObj.JD_ContainerMode, errorContext, context);
			}

			if (!orderBizObj.JD_OrderGoodsDescription.IsEmpty)
			{
				result.Description = orderBizObj.JD_OrderGoodsDescription;
			}

			if (orderBizObj.JD_EstimatedExchangeRate > 0)
			{
				result.ExchangeRate = orderBizObj.JD_EstimatedExchangeRate;
			}

			result.ExchRateBasis = Xsd.OrderOrderDetailExchRateBasis.F;

			if (!orderBizObj.JD_IncoTerm.IsEmpty)
			{
				result.Incoterm = orderBizObj.JD_IncoTerm;
			}

			if (!orderBizObj.JD_InvoiceNumber.IsEmpty)
			{
				result.InvoiceNumber = orderBizObj.JD_InvoiceNumber;
			}

			if (orderBizObj.JD_OrderDate.IsValid)
			{
				result.OrderDateTime = orderBizObj.JD_OrderDate.ToDateTime();
			}

			if (!orderBizObj.JD_OrderStatus.IsEmpty)
			{
				result.OrderStatus = GetValidOrderOrOrderLineStatusCode(orderBizObj.JD_OrderStatus_List, orderBizObj.JD_OrderStatus, context);
			}

			result.OrderTotal = Xsd.FinancialValue.FromAmountAndCurrency(totalValue, orderBizObj.OrderCurrency);

			if (!orderBizObj.JD_TransportMode.IsEmpty)
			{
				result.TransportMode = OrderTransportModeToXmlCodeMappings.Instance.GetExternalCode(orderBizObj.JD_TransportMode, errorContext, context);
			}

			if (!orderBizObj.JD_RN_NKCountryOfSupply.IsEmpty)
			{
				result.CountryOfOrigin = orderBizObj.JD_RN_NKCountryOfSupply;
			}

			if (orderBizObj.JD_ExWorksRequiredBy.IsValid)
			{
				result.ExWorksRequiredBy = orderBizObj.JD_ExWorksRequiredBy;
			}

			if (orderBizObj.JD_DeliveryRequiredBy.IsValid)
			{
				result.DeliveryRequiredBy = orderBizObj.JD_DeliveryRequiredBy;
			}

			return result;
		}

		protected virtual IValueObjectDataAdapter GetOrganisationValueObjectDataAdapter()
		{
			return new OrganisationValueObjectDataAdapter();
		}

		Xsd.OrderOrderDetailShipmentPlanning ExportOrderShipmentPlanning(Order orderBizObj, IValueObjectExportContext context)
		{
			Xsd.OrderOrderDetailShipmentPlanning result = new Xsd.OrderOrderDetailShipmentPlanning();

			result.DischargePort = Xsd.UNLOCO.FromPort(orderBizObj.PortOfDischarge);
			result.LoadPort = Xsd.UNLOCO.FromPort(orderBizObj.PortOfLoading);
			result.GoodsDestination = Xsd.UNLOCO.FromPort(orderBizObj.GoodsDeliveredTo);
			result.GoodsOrigin = Xsd.UNLOCO.FromPort(orderBizObj.GoodsAvailableAt);
			if (!orderBizObj.JD_Waybill.IsEmpty)
			{
				result.HouseBill = orderBizObj.JD_Waybill;
			}

			result.ReceivingAgent = new OrganisationValueObjectDataAdapter().ExportToValueObject(orderBizObj.ReceivingAgent, context);
			result.SendingAgent = new OrganisationValueObjectDataAdapter().ExportToValueObject(orderBizObj.SendingAgent, context);
			result.Packs = Xsd.DimensionValue.FromAmountAndUnit(orderBizObj.JD_Packs, orderBizObj.JD_F3_NKPackType);
			result.Volume = Xsd.DimensionValue.FromAmountAndUnit(orderBizObj.JD_ActualVolume, orderBizObj.JD_UnitOfVolume);
			result.Weight = Xsd.DimensionValue.FromAmountAndUnit(orderBizObj.JD_ActualWeight, orderBizObj.JD_UnitOfWeight);
			result.DepartureVessel = orderBizObj.JD_RV_NKDepartureVessel;
			result.DepartureVoyageFlight = orderBizObj.JD_DepartureVoyage;

			result.GoodsAvailAt = orderBizObj.GoodsAvailableAtAddress.Address?.OA_Code ?? ZString.Empty;
			result.GoodsDelivTo = orderBizObj.GoodsDeliveredToAddress.Address?.OA_Code ?? ZString.Empty;

			ExportOrderPlannedContainers(orderBizObj, result, context);

			return result;
		}

		void ExportOrderPlannedContainers(Order orderBizObj, Xsd.OrderOrderDetailShipmentPlanning shipmentPlanningValue, IValueObjectExportContext context)
		{
			foreach (OrderContainer orderContainer in orderBizObj.PlannedContainers)
			{
				Xsd.PlannedContainer plannedContainer = shipmentPlanningValue.PlannedContainers.AddNew();
				plannedContainer.Number = orderContainer.J1_ContainerNumber;

				if (orderContainer.J1_ContainerCount > 0)
				{
					plannedContainer.Quantity = orderContainer.J1_ContainerCount;
				}

				ContainerTypeValueObjectDataAdapter typeAdapter = new ContainerTypeValueObjectDataAdapter();
				plannedContainer.Type = typeAdapter.ExportToValueObject(orderContainer.Container, context);
			}
		}

		Xsd.OrderOrderDetailMilestones ExportOrderMilestones(Order orderBizObj)
		{
			Xsd.OrderOrderDetailMilestones result = new Xsd.OrderOrderDetailMilestones();

			result.Arrival = XsdOrderMilestoneDateHelper.FromEstimatedActualDates(orderBizObj, Events.Arrival);
			result.CartageAdvised = XsdOrderMilestoneDateHelper.FromEstimatedActualDates(orderBizObj, Events.DeliveryCartageAdvised);
			result.CustomsCommenced = XsdOrderMilestoneDateHelper.FromEstimatedActualDates(orderBizObj, Events.CustomsCommenced);
			result.CustomsFinalised = XsdOrderMilestoneDateHelper.FromEstimatedActualDates(orderBizObj, Events.CustomsCleared);
			result.Delivery = XsdOrderMilestoneDateHelper.FromEstimatedActualDates(orderBizObj, Events.DeliveryCartageCompleteFinalised);
			result.Departure = XsdOrderMilestoneDateHelper.FromEstimatedActualDates(orderBizObj, Events.Departure);
			result.ExFactory = XsdOrderMilestoneDateHelper.FromEstimatedActualDates(orderBizObj, Events.ExWorks);
			result.OriginReceival = XsdOrderMilestoneDateHelper.FromEstimatedActualDates(orderBizObj, Events.GateIn);

			result.Unpacked.IsSpecified = false;
			if (orderBizObj.Shipment == null)
			{
				result.Unpacked = XsdOrderMilestoneDateHelper.FromEstimatedActualDates(orderBizObj, Events.CargoAvailable);
			}

			if (orderBizObj.JD_EstimateUserDate1.IsValid || orderBizObj.JD_EstimateUserDate2.IsValid || orderBizObj.JD_ActualUserDate1.IsValid || orderBizObj.JD_ActualUserDate2.IsValid)
			{
				result.UserDate = new Xsd.MilestoneDatesCollection();

				Xsd.MilestoneDates userDate1 = Xsd.MilestoneDates.FromEstimatedAndActual(orderBizObj.JD_EstimateUserDate1, orderBizObj.JD_ActualUserDate1);
				if (userDate1.IsSpecified)
				{
					result.UserDate.Add(userDate1);
				}

				Xsd.MilestoneDates userDate2 = Xsd.MilestoneDates.FromEstimatedAndActual(orderBizObj.JD_EstimateUserDate2, orderBizObj.JD_ActualUserDate2);
				if (userDate2.IsSpecified)
				{
					result.UserDate.Add(userDate2);
				}
			}

			return result;
		}

		ZDecimal ExportOrderLines(Order orderBizObj, Xsd.Order orderValue, IValueObjectExportContext context, string errorContext)
		{
			ZDecimal totalValue = 0;

			foreach (OrderLine orderLine in orderBizObj.OrderLines)
			{
				totalValue += orderLine.JO_LinePrice;

				if (orderValue.OrderLines == null)
				{
					orderValue.OrderLines = new Xsd.OrderOrderLineCollection();
				}

				Xsd.OrderOrderLine orderLineValue = orderValue.OrderLines.AddNew();
				orderLineValue.OrderLineNo = orderLine.JO_LineNo;

				if (orderLine.JO_SubLineNo > 0)
				{
					orderLineValue.OrderSubLineNo = orderLine.JO_SubLineNo;
				}

				if (orderLine.JO_LineSplitNumber > 0)
				{
					orderLineValue.OrderLineSplitNo = orderLine.JO_LineSplitNumber;
				}

				orderLineValue.OrderLineDetail = new Xsd.OrderOrderLineOrderLineDetail();

				if (!orderLine.JO_Description.IsEmpty)
				{
					orderLineValue.OrderLineDetail.Description = orderLine.JO_Description;
				}
				else if (orderLine.Product != null)
				{
					orderLineValue.OrderLineDetail.Description = orderLine.Product.OP_Desc;
				}

				if (orderLine.JO_LineDropDate.IsValid)
				{
					orderLineValue.OrderLineDetail.DropDate = orderLine.JO_LineDropDate.ToDateTime();
				}

				orderLineValue.OrderLineDetail.AdditionalInformation = orderLine.JO_AdditionalInformation;
				orderLineValue.OrderLineDetail.SpecialInstructions = orderLine.JO_SpecialInstructions;
				orderLineValue.OrderLineDetail.InnerPacks = Xsd.DimensionValue.FromAmountAndUnit(orderLine.JO_InnerPacks, orderLine.JO_InnerPacksUQ);
				orderLineValue.OrderLineDetail.ItemPrice.Value = orderLine.JO_ItemPrice;
				orderLineValue.OrderLineDetail.ItemPrice.IsSpecified = true;
				orderLineValue.OrderLineDetail.LinePrice.Value = orderLine.JO_LinePrice;
				orderLineValue.OrderLineDetail.LinePrice.IsSpecified = true;
				orderLineValue.OrderLineDetail.OuterPacks = Xsd.DimensionValue.FromAmountAndUnit(orderLine.JO_OuterPacks, orderLine.JO_OuterPacksUQ);
				orderLineValue.OrderLineDetail.Weight = orderLine.JO_ActualWeight;
				orderLineValue.OrderLineDetail.WeightType = orderLine.JO_UnitOfWeight;
				orderLineValue.OrderLineDetail.WeightTypeSpecified = true;
				orderLineValue.OrderLineDetail.Volume = orderLine.JO_ActualVolume;
				orderLineValue.OrderLineDetail.VolumeType = orderLine.JO_UnitOfVolume;
				orderLineValue.OrderLineDetail.VolumeTypeSpecified = true;
				orderLineValue.OrderLineDetail.Product = (orderLine.JO_Partno.IsEmpty) ? new ZString(Res.GetString("e92db5a0-fc26-4fe4-a2f6-d3d58d471663", "*EMPTY*")) : orderLine.JO_Partno;

				orderLineValue.OrderLineDetail.QtyOrdered = Xsd.DimensionValue.FromAmountAndUnit(orderLine.JO_Quantity, orderLine.JO_F3_NKPackType);
				orderLineValue.OrderLineDetail.QtyInvoiced = Xsd.DimensionValue.FromAmountAndUnit(orderLine.JO_QtyInvoiced, orderLine.JO_F3_NKPackType);
				orderLineValue.OrderLineDetail.QtyReceived = Xsd.DimensionValue.FromAmountAndUnit(orderLine.JO_QtyReceived, orderLine.JO_F3_NKPackType);
				orderLineValue.OrderLineDetail.QtyReceivedToDate = Xsd.DimensionValue.FromAmountAndUnit(orderLine.QtyReceivedToDate, orderLine.JO_F3_NKPackType);

				orderLineValue.OrderLineDetail.LineStatus = GetValidOrderOrOrderLineStatusCode(orderLine.JO_LineStatus_List, orderLine.JO_LineStatus, context);
				orderLineValue.OrderLineDetail.LineStatusSpecified = true;
				orderLineValue.OrderLineDetail.PartAttrib1 = orderLine.JO_PartAttrib1;
				orderLineValue.OrderLineDetail.PartAttrib2 = orderLine.JO_PartAttrib2;
				orderLineValue.OrderLineDetail.PartAttrib3 = orderLine.JO_PartAttrib3;

				orderLineValue.OrderLineDeliveries = ExportOrderLineDeliveries(orderLine, context, errorContext);
				orderLineValue.OrderLineDetail.Custom = ExportCustomOrderLineDetails(orderLine);

				orderLineValue.OrderLineDetail.ContainerNumber = orderLine.JO_ContainerNumber;
				if (orderLine.JO_ContainerPackingOrder > 0)
				{
					orderLineValue.OrderLineDetail.ContainerPackingOrder = orderLine.JO_ContainerPackingOrder;
				}

				if (!orderLine.JO_RN_NKCountryOfOrigin.IsEmpty)
				{
					orderLineValue.OrderLineDetail.CountryOfOrigin = orderLine.JO_RN_NKCountryOfOrigin;
				}
				else
				{
					orderLineValue.OrderLineDetail.CountryOfOrigin = orderLine.Order.JD_RN_NKCountryOfSupply;
				}

				orderLineValue.OrderLineDetail.CommercialInvoiceNo = orderLine.JO_CommercialInvoiceNo;

				orderLineValue.OrderLineDetail.DangerousGoods.ExportFromUNDGDataItems(orderLine.UNDGs.ToArray(), errorContext, context);

				if (!orderLine.JO_INCO.IsEmpty)
				{
					orderLineValue.OrderLineDetail.Incoterm = orderLine.JO_INCO;
				}

				if (!orderLine.JO_AdditionalTerms.IsEmpty)
				{
					orderLineValue.OrderLineDetail.AdditionalTerms = orderLine.JO_AdditionalTerms;
				}

				if (!orderLine.JO_ConfirmationNum.IsEmpty)
				{
					orderLineValue.OrderLineDetail.ConfirmNumber = orderLine.JO_ConfirmationNum;
				}

				if (orderLine.JO_ConfirmationDate.IsValid)
				{
					orderLineValue.OrderLineDetail.ConfirmDate = orderLine.JO_ConfirmationDate.ToDateTime();
				}

				if (orderLine.JO_ExWorksDate.IsValid)
				{
					orderLineValue.OrderLineDetail.ExWorksRequiredBy = orderLine.JO_ExWorksDate.ToDateTime();
				}

				// Legacy
				foreach (UNDGDataItem dgItem in orderLine.UNDGs)
				{
					if (dgItem.Substance != null)
					{
						orderLineValue.OrderLineDetail.DGFlashPointSpecified = true;
						orderLineValue.OrderLineDetail.DGFlashPoint = dgItem.DI_DGFlashPoint;
						orderLineValue.OrderLineDetail.DGSubstanceCode = dgItem.Substance.DG_Code;
					}
				}
			}

			return totalValue;
		}

		ZString GetValidOrderOrOrderLineStatusCode(ReadOnlyCodeDescriptionPairList orderOrOrderLineStatusList, ZString orderOrOrderLineStatus, INotifications notifications)
		{
			if (orderOrOrderLineStatusList.ContainsCode(orderOrOrderLineStatus))
			{
				return orderOrOrderLineStatus;
			}
			else
			{
				notifications.Notify(new WarningNotification(Res.GetString("b204b5d6-ed2a-426a-87e2-bf99b736cc7f", "Order Status or Order Line Status {0} is unknown. It not will be mapped.", orderOrOrderLineStatus)));
				return "";
			}
		}

		Xsd.OrderOrderLineOrderLineDetailCustom ExportCustomOrderLineDetails(OrderLine orderLine)
		{
			Xsd.OrderOrderLineOrderLineDetailCustom result = new Xsd.OrderOrderLineOrderLineDetailCustom();

			if (orderLine.JO_CustomDate1.IsValid)
			{
				result.Date1 = orderLine.JO_CustomDate1;
			}
			if (orderLine.JO_CustomDate2.IsValid)
			{
				result.Date2 = orderLine.JO_CustomDate2;
			}
			if (orderLine.JO_CustomDate3.IsValid)
			{
				result.Date3 = orderLine.JO_CustomDate3;
			}
			if (orderLine.JO_CustomDate4.IsValid)
			{
				result.Date4 = orderLine.JO_CustomDate4;
			}
			if (orderLine.JO_CustomDate5.IsValid)
			{
				result.Date5 = orderLine.JO_CustomDate5;
			}

			CustomLabelInfoList list = OrderLine.NewCustomLabelsProvider(orderLine.Order.Buyer).GetCustomFields(orderLine.Order.Buyer, orderLine.Order.Factory);

			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomDecimal1.Name).IsEnabled)
			{
				result.Decimal1 = orderLine.JO_CustomDecimal1;
			}
			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomDecimal2.Name).IsEnabled)
			{
				result.Decimal2 = orderLine.JO_CustomDecimal2;
			}
			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomDecimal3.Name).IsEnabled)
			{
				result.Decimal3 = orderLine.JO_CustomDecimal3;
			}
			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomDecimal4.Name).IsEnabled)
			{
				result.Decimal4 = orderLine.JO_CustomDecimal4;
			}
			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomDecimal5.Name).IsEnabled)
			{
				result.Decimal5 = orderLine.JO_CustomDecimal5;
			}

			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomFlag1.Name).IsEnabled)
			{
				result.Flag1 = orderLine.JO_CustomFlag1;
				result.Flag1Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomFlag2.Name).IsEnabled)
			{
				result.Flag2 = orderLine.JO_CustomFlag2;
				result.Flag2Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomFlag3.Name).IsEnabled)
			{
				result.Flag3 = orderLine.JO_CustomFlag3;
				result.Flag3Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomFlag4.Name).IsEnabled)
			{
				result.Flag4 = orderLine.JO_CustomFlag4;
				result.Flag4Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderLineSchema.JO_CustomFlag5.Name).IsEnabled)
			{
				result.Flag5 = orderLine.JO_CustomFlag5;
				result.Flag5Specified = true;
			}

			result.Text1 = orderLine.JO_CustomAttrib1.IsEmpty ? null : orderLine.JO_CustomAttrib1;
			result.Text2 = orderLine.JO_CustomAttrib2.IsEmpty ? null : orderLine.JO_CustomAttrib2;
			result.Text3 = orderLine.JO_CustomAttrib3.IsEmpty ? null : orderLine.JO_CustomAttrib3;
			result.Text4 = orderLine.JO_CustomAttrib4.IsEmpty ? null : orderLine.JO_CustomAttrib4;
			result.Text5 = orderLine.JO_CustomAttrib5.IsEmpty ? null : orderLine.JO_CustomAttrib5;
			result.Text6 = orderLine.JO_CustomAttrib6.IsEmpty ? null : orderLine.JO_CustomAttrib6;

			result.CustomText1 = orderLine.JO_CustomTextBlob1.IsEmpty ? null : orderLine.JO_CustomTextBlob1;

			return result;
		}

		Xsd.OrderOrderLineOrderLineDeliveryCollection ExportOrderLineDeliveries(OrderLine orderLine, IValueObjectExportContext context, string errorContext)
		{
			Xsd.OrderOrderLineOrderLineDeliveryCollection result = null;

			foreach (OrderLineDelivery delivery in orderLine.Deliveries)
			{
				if (result == null)
				{
					result = new Xsd.OrderOrderLineOrderLineDeliveryCollection();
				}
				Xsd.OrderOrderLineOrderLineDelivery deliveryValue = result.AddNew();
				deliveryValue.DeliveryDetails = new Xsd.OrderOrderLineOrderLineDeliveryDeliveryDetails();

				if (delivery.DeliveryPoint == null)
				{
					deliveryValue.DeliveryDetails.AddressFreeText = delivery.J4_OA_NKDeliveryPoint;
				}
				else
				{
					AddressValueObjectHelper addressHelper = new AddressValueObjectHelper(Res.GetString("5eaacc22-b377-4114-b654-76a3a9d5a343", "Delivery Point on Order #/Product # {0}/{1}", orderLine.Order.JD_OrderNumber, orderLine.JO_Partno));
					deliveryValue.DeliveryDetails.Address = addressHelper.ToAddressReference(delivery.DeliveryPoint, context);
				}

				if (delivery.DestinationPort != null)
				{
					deliveryValue.DeliveryDetails.DelPort = Xsd.UNLOCO.FromPort(delivery.DestinationPort);
				}

				deliveryValue.DeliveryDetails.QtyAllocated = delivery.J4_Allocated;

				deliveryValue.DeliveryDetails.Custom = ExportOrderLineDeliveryCustomDetails(delivery);
				deliveryValue.DeliveryContainers = ExportOrderLineDeliveryContainers(delivery, context, errorContext);
			}

			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
#if DEBUG
		protected
#endif
 Xsd.OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom ExportOrderLineDeliveryCustomDetails(OrderLineDelivery delivery)
		{
			Xsd.OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom custom = new Xsd.OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom();

			CustomLabelInfoList list = OrderLineDelivery.NewCustomLabelsProvider(delivery.Order.Buyer).GetCustomFields(delivery.Order.Buyer, delivery.Order.Factory);

			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDate1.Name).IsEnabled && delivery.J4_CustomDate1.IsValid)
			{
				custom.Date1 = delivery.J4_CustomDate1;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDate2.Name).IsEnabled && delivery.J4_CustomDate2.IsValid)
			{
				custom.Date2 = delivery.J4_CustomDate2;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDate3.Name).IsEnabled && delivery.J4_CustomDate3.IsValid)
			{
				custom.Date3 = delivery.J4_CustomDate3;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDate4.Name).IsEnabled && delivery.J4_CustomDate4.IsValid)
			{
				custom.Date4 = delivery.J4_CustomDate4;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDate5.Name).IsEnabled && delivery.J4_CustomDate5.IsValid)
			{
				custom.Date5 = delivery.J4_CustomDate5;
			}

			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDecimal1.Name).IsEnabled)
			{
				custom.Decimal1 = delivery.J4_CustomDecimal1;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDecimal2.Name).IsEnabled)
			{
				custom.Decimal2 = delivery.J4_CustomDecimal2;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDecimal3.Name).IsEnabled)
			{
				custom.Decimal3 = delivery.J4_CustomDecimal3;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDecimal4.Name).IsEnabled)
			{
				custom.Decimal4 = delivery.J4_CustomDecimal4;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomDecimal5.Name).IsEnabled)
			{
				custom.Decimal5 = delivery.J4_CustomDecimal5;
			}

			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomFlag1.Name).IsEnabled)
			{
				custom.Flag1 = delivery.J4_CustomFlag1;
				custom.Flag1Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomFlag2.Name).IsEnabled)
			{
				custom.Flag2 = delivery.J4_CustomFlag2;
				custom.Flag2Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomFlag3.Name).IsEnabled)
			{
				custom.Flag3 = delivery.J4_CustomFlag3;
				custom.Flag3Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomFlag4.Name).IsEnabled)
			{
				custom.Flag4 = delivery.J4_CustomFlag4;
				custom.Flag4Specified = true;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomFlag5.Name).IsEnabled)
			{
				custom.Flag5 = delivery.J4_CustomFlag5;
				custom.Flag5Specified = true;
			}

			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomAttribute1.Name).IsEnabled && !delivery.J4_CustomAttribute1.IsEmpty)
			{
				custom.Text1 = delivery.J4_CustomAttribute1;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomAttribute2.Name).IsEnabled && !delivery.J4_CustomAttribute2.IsEmpty)
			{
				custom.Text2 = delivery.J4_CustomAttribute2;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomAttribute3.Name).IsEnabled && !delivery.J4_CustomAttribute3.IsEmpty)
			{
				custom.Text3 = delivery.J4_CustomAttribute3;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomAttribute4.Name).IsEnabled && !delivery.J4_CustomAttribute4.IsEmpty)
			{
				custom.Text4 = delivery.J4_CustomAttribute4;
			}
			if (list.GetFieldByPropertyName(JobOrderLineDeliverySchema.J4_CustomAttribute5.Name).IsEnabled && !delivery.J4_CustomAttribute5.IsEmpty)
			{
				custom.Text5 = delivery.J4_CustomAttribute5;
			}
			return custom;
		}

		Xsd.OrderOrderLineOrderLineDeliveryDeliveryContainerCollection ExportOrderLineDeliveryContainers(OrderLineDelivery delivery, IValueObjectExportContext context, string errorContext)
		{
			Xsd.OrderOrderLineOrderLineDeliveryDeliveryContainerCollection result = null;

			foreach (OrderLineDeliverContainer container in delivery.Containers)
			{
				if (result == null)
				{
					result = new Xsd.OrderOrderLineOrderLineDeliveryDeliveryContainerCollection();
				}
				Xsd.OrderOrderLineOrderLineDeliveryDeliveryContainer containerValue = result.AddNew();
				containerValue.Container = new Xsd.Container();
				containerValue.Container.ContainerNumber = container.J5_ContainerNum;
				containerValue.Container.ContainerType = new ContainerTypeValueObjectDataAdapter().ExportToValueObject(container.ContainerType, context);
				containerValue.Container.PackingMode = ContainerModeToXmlCodeMappings.Instance.GetExternalCode(delivery.OrderLine.Order.JD_ContainerMode, errorContext, context);
				containerValue.Container.Seal = container.J5_ContainerSeal;
				if (container.J5_ETA.IsValid)
				{
					containerValue.ETA = container.J5_ETA.ToDateTime();
				}
				if (container.J5_ETD.IsValid)
				{
					containerValue.ETD = container.J5_ETD.ToDateTime();
				}
				containerValue.LoadPort = Xsd.UNLOCO.FromPort(container.LoadPort);
				containerValue.MasterBillNo = container.J5_MasterBill.IsEmpty ? null : (string)container.J5_MasterBill;
				containerValue.PackCount = container.J5_PackCount;
				containerValue.PackType = PkgUnitXmlCodeMappings.Instance.GetExternalCode(container.J5_F3_NKPackType, errorContext, context);
				containerValue.Vessel = container.J5_RV_NKArrivalVessel.IsEmpty ? null : (string)container.J5_RV_NKArrivalVessel;
				containerValue.Volume = Xsd.DimensionValue.FromAmountAndUnit(container.J5_Volume, VolumeUQXmlCodeMappings.Instance.GetExternalCode(container.J5_VolumeUQ, errorContext, context));
				containerValue.Weight = Xsd.DimensionValue.FromAmountAndUnit(container.J5_Weight, WeightUQXmlCodeMappings.Instance.GetExternalCode(container.J5_WeightUQ, errorContext, context));
				containerValue.Voyage = container.J5_Voyage.IsEmpty ? null : (string)container.J5_Voyage;
				containerValue.QtyInvoiced = container.J5_QuantityInvoiced;
				containerValue.QtyDelivered = container.J5_QuantityInStore;
			}

			return result;
		}

		#endregion
	}
}

