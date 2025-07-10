using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ShipmentOrdersDataAdapterHelper
	{
		#region Ctor

		public ShipmentOrdersDataAdapterHelper(EventsWithSourceType triggeredByEvents, IValueObjectDataAdapter orderValueObjectDataAdapter = null)
		{
			TriggeredByEvents = triggeredByEvents;
			OrderValueObjectDataAdapter = orderValueObjectDataAdapter;
		}

		#endregion

		#region Nested Types

		public abstract class Args
		{
			public ForwardingShipment Shipment
			{
				get;
				set;
			}

			public Xsd.Shipment ShipmentValue
			{
				get;
				set;
			}

			public virtual bool IsValid
			{
				get { return Shipment != null && ShipmentValue != null; }
			}
		}

		public class ImportArgs : Args
		{
			public IValueObjectImportContext Context
			{
				get;
				set;
			}

			public override bool IsValid
			{
				get { return base.IsValid && Context != null; }
			}
		}

		public class ExportArgs : Args
		{
			public Order OrderFilter
			{
				get;
				set;
			}

			public IValueObjectExportContext Context
			{
				get;
				set;
			}

			public override bool IsValid
			{
				get { return base.IsValid && Context != null; }
			}
		}

		#endregion

		#region Properties

		EventsWithSourceType TriggeredByEvents
		{
			get;
			set;
		}

		protected IValueObjectDataAdapter OrderValueObjectDataAdapter
		{
			set { orderValueObjectDataAdapter = value; }
			get { return orderValueObjectDataAdapter ?? (orderValueObjectDataAdapter = new OrderValueObjectDataAdapter(TriggeredByEvents)); }
		}
		IValueObjectDataAdapter orderValueObjectDataAdapter;

		#endregion

		#region Import

		public void ImportOrdersAndOrderReferences(ImportArgs importArgs)
		{
			if (importArgs == null || !importArgs.IsValid)
			{
				return;
			}

			if (importArgs.ShipmentValue.Orders != null)
			{
				ImportOrders(importArgs);
			}

			if (importArgs.ShipmentValue.ShipmentDetails.OrderReferences != null)
			{
				importArgs.Shipment.DocsAndCartage.OrderItems.RemoveAndDeleteAll();
				foreach (ZString orderReference in importArgs.ShipmentValue.ShipmentDetails.OrderReferences)
				{
					OrderItem item = importArgs.Shipment.DocsAndCartage.OrderItems.AddNew();
					item.JT_OrderReference = orderReference.SubstringSafe(0, JobOrderItemSchema.JT_OrderReference.MaxLength);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		protected virtual bool CanAttachToOrder(ImportArgs importArgs, Xsd.Order orderValue, out Order order)
		{
			bool canAttachOrder = false;

			order = importArgs.Context.Factory.LoadTop1<Order>(GetOrderFilter(orderValue, importArgs.Context));

			if (order == null || order.CanBeUpdatedByImport)
			{
				order = (Order)OrderValueObjectDataAdapter.CreateOrUpdateFromValueObject(orderValue, importArgs.Context);
				canAttachOrder = order != null;
			}
			else if (!order.IsShipmentAttached && !order.IsDeclarationAttached)
			{
				canAttachOrder = true;
			}

			return canAttachOrder;
		}

		void ImportOrders(ImportArgs importArgs)
		{
			var helper = new OrdersOnShipmentLimitHelper(importArgs.Shipment);

			foreach (Xsd.Order orderValue in importArgs.ShipmentValue.Orders)
			{
				if (orderValue.OrderDetail.ReferenceNumber.Value.IsEmpty)
				{
					Order order = null;

					bool canAttachOrder = CanAttachToOrder(importArgs, orderValue, out order);

					if (!canAttachOrder)
					{
						importArgs.Context.Notify(new WarningNotification(Res.GetString("7c12bdde-a6cd-4a19-b961-efae64ebe935", "{0} is already attached and cannot be updated.", order.HumanReadableName)));
					}
					else if (!importArgs.Shipment.AttachedOrders.Contains(order))
					{
						importArgs.Shipment.AttachedOrders.Add(order);
						importArgs.Shipment.OnOrderAttached(order);

						importArgs.Context.Notify(new InfoNotification(importArgs.Shipment.JS_IsForwardRegistered ?
							Res.GetString("7960c492-b434-451e-b3cc-12e39caf6719", "{0} has been attached to shipment.", order.HumanReadableName) :
							Res.GetString("f5e185a8-e970-4954-b2e8-897ae09416c5", "{0} has been attached to booking.", order.HumanReadableName)));

						CheckOrderLimitNotExceeded(importArgs.Context, helper);
					}
				}
				else
				{
					importArgs.Context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("ddb919cc-d7d2-4141-b829-bd66159c9450", "Order {0}: Reference Number should only be provided when importing a stand-alone order.", orderValue.OrderIdentifier.OrderNumber)));
				}
			}
		}

		public static void CheckOrderLimitNotExceeded(IValueObjectImportContext context, OrdersOnShipmentLimitHelper helper)
		{
			Argument.NotNull(helper, "helper");

			var notification = helper.CreateNotification();

			if (notification != null && notification.Type == CargoWise.ComponentModel.NotificationType.Error)
			{
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, notification.Message));
			}
		}

		#endregion

		#region Export

		public void ExportOrdersAndOrderReferences(ExportArgs exportArgs)
		{
			if (exportArgs == null || !exportArgs.IsValid)
			{
				return;
			}

			List<string> orderReferences = new List<string>();

			foreach (OrderItem item in exportArgs.Shipment.DocsAndCartage.OrderItems)
			{
				orderReferences.Add((string)item.JT_OrderReference);
			}

			if (orderReferences.Count > 0)
			{
				exportArgs.ShipmentValue.ShipmentDetails.OrderReferences = orderReferences.ToArray();
			}

			if (exportArgs.Shipment != null && exportArgs.Shipment.AttachedOrders.Count > 0)
			{
				SetShipmentOrders(exportArgs);
			}
		}

		void SetShipmentOrders(ExportArgs exportArgs)
		{
			if (exportArgs.OrderFilter == null)
			{
				foreach (Order order in exportArgs.Shipment.AttachedOrders)
				{
					exportArgs.ShipmentValue.Orders.Add((Xsd.Order)OrderValueObjectDataAdapter.ExportToValueObject(order, exportArgs.Context));
				}
			}
			else if (exportArgs.Shipment.AttachedOrders.Contains(exportArgs.OrderFilter))
			{
				exportArgs.ShipmentValue.Orders.Add((Xsd.Order)OrderValueObjectDataAdapter.ExportToValueObject(exportArgs.OrderFilter, exportArgs.Context));
			}
		}

		#endregion

		#region Implementation

		protected ZQuery GetOrderFilter(Xsd.Order orderValue, IValueObjectImportContext context)
		{
			var filter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, orderValue.OrderIdentifier.OrderNumber);
			filter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, orderValue.OrderIdentifier.OrderNumberSplit);

			var buyer = context.FindOrganisation(orderValue.OrderDetail.Buyer, null, OrganisationTypes.None);
			filter.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyer == null ? ZGuid.Empty : buyer.Addresses.Select(x => x.PK));

			return filter;
		}

		#endregion
	}
}

