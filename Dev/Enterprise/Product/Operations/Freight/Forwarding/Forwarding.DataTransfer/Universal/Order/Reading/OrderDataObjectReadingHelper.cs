using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderDataObjectReadingHelper
	{
		public OrderDataObjectReadingHelper(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IAttachOrders parent, IOrderLineLinkManager orderLineLinkManager = null)
		{
			this.dataObject = Argument.NotNull(dataObject, "dataObject");
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.parent = Argument.NotNull(parent, "parent");
			this.orderLineLinkManager = orderLineLinkManager;
			SetupData();
		}

		readonly UniversalShipment dataObject;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly IOrderLineLinkManager orderLineLinkManager;
		readonly IAttachOrders parent;

		public void PopulateOrders()
		{
			if (dataObject.RelatedShipmentCollection != null)
			{
				var invalidOrderWithDataObjects = new Dictionary<ZGuid, UniversalShipment>();
				var importErrorType = OrderImportErrorTypes.None;

				foreach (var orderDataObject in dataObject.RelatedShipmentCollection.Where(x => x.GetMatchingDataTarget(DataContextType.OrderManagerOrder) != null))
				{
					var reader = new OrderDataObjectReader(orderDataObject, logger, factory, orderLineLinkManager, parent);
					var order = reader.ReadIntoBusinessObject();

					if (order != null && order.IsInDatabase && order.IsAttachedToSupplierBooking)
					{
						logger.Log(LogType.Warning, Res.GetString("626EA830-C380-43A6-B33A-515590A851B9", "The order containing in this shipment XML has associated supplier bookings and cannot be linked via XML."));
					}
					else if (order != null && !parent.AttachedOrders.Contains(order) && !isOrderAlreadyAttached(order))
					{
						parent.AttachedOrders.Add(order);
						LogOrderAttachedAndUpdateParentFromOrder(order);
						CheckShipmentOrdersLimitNotExceeded(limitHelper);
					}

					if (reader.ImportErrorType == OrderImportErrorTypes.CutOffDateHasPassed || reader.ImportErrorType == OrderImportErrorTypes.CutOffDateNotSet)
					{
						var key = order.PK;

						if (invalidOrderWithDataObjects.ContainsKey(key))
						{
							invalidOrderWithDataObjects[key] = orderDataObject;
						}
						else
						{
							invalidOrderWithDataObjects.Add(key, orderDataObject);
						}

						importErrorType = reader.ImportErrorType;
					}
				}

				if (invalidOrderWithDataObjects.Any())
				{
					var notificationEmailManager = (logger as IXmlSessionTracker)?.NotificationEmailManager;
					if (notificationEmailManager != null)
					{
						var orderEmailProcessor = new ImportedOrderEmailProcessor(invalidOrderWithDataObjects, importErrorType);
						notificationEmailManager.Register(orderEmailProcessor);
					}
				}
			}
		}

		void CheckShipmentOrdersLimitNotExceeded(CollectionLimitHelperForPotentialHVLV helper)
		{
			var notification = helper?.CreateNotification();

			if (notification != null && notification.Type == CargoWise.ComponentModel.NotificationType.Error)
			{
				throw new DataObjectReadFailureException(notification.Message);
			}
		}

		void LogOrderAttachedAndUpdateParentFromOrder(Order order)
		{
			// Do not log Attaching if we are creating a new Order on a new Shipment/Declaration
			if (order.IsInDatabase || parent.IsInDatabase)
			{
				// If Order is in database but Shipment/Declaration is not, the Existing Order would have been updated to the values in Universal XML
				// and the new Shipmen/Declarationt should thus already have all the correct data from Universal XML
				if (parent.IsInDatabase)
				{
					parent.OnOrderAttached(order);
				}

				logger.LogBoth(LogType.Information, Res.GetString("c3d5d4b3-9267-4291-bfc3-50b6d847e067", "{0} has been attached to {1}.", order.HumanReadableName, parentHumanReadableName));
			}
		}

		void SetupData()
		{
			if (parent is ForwardingShipment shipment)
			{
				limitHelper = new OrdersOnShipmentLimitHelper(shipment);
				parentHumanReadableName = Res.GetString("54da286f-365a-45a2-8064-b47b2d799a38", "shipment");
				isOrderAlreadyAttached = (o) => o.IsShipmentAttached;
			}
			else if (parent is IBaseJobDeclaration declaration)
			{
				limitHelper = new OrdersOnDeclarationLimitHelper(declaration);
				parentHumanReadableName = Res.GetString("b8f3ac35-57e6-4eaa-99fe-80a451e4b6e9", "declaration");
				isOrderAlreadyAttached = (o) => o.IsDeclarationAttached;
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("{DE0FDD91-F51A-47BF-BEBD-1EAECC9380DD}", "parent must be either Shipment or Declaration."));
			}
		}
		CollectionLimitHelperForPotentialHVLV limitHelper;
		string parentHumanReadableName;
		Func<Order, bool> isOrderAlreadyAttached;
	}
}
