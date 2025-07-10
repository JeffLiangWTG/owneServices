using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class OrderInvoiceImporter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrderInvoiceImporter(BaseJobDeclaration declaration)
			: base(new BusinessObjectFactory())
		{
			this.Declaration = declaration;
			fQuantityType = ComInvReconciliationQuantityType.OrderQuantity;
		}

		#region Schema

		public static class Schema
		{
			public const string IsOrderedQuantityImport = "IsOrderedQuantityImport";
			public const string IsInvoicedQuantityImport = "IsInvoicedQuantityImport";
			public const string IsReceivedQuantityImport = "IsReceivedQuantityImport";
			public const string InvoiceNumber = "InvoiceNumber";
			public const string InvoiceDate = "InvoiceDate";
		}

		#endregion

		public void ValidateOrders()
		{
			foreach (Order order in OrdersToImport)
			{
				order.ClearAllNotifications();
				if (order.OrderLines.Count == 0 && !order.HasErrors)
				{
					order.AddRowError(Res.GetString("90273dcb-235f-4064-a7d6-3dd139152b00", "This Order has no Order Lines. Data will not be imported"));
				}
				else
				{
					if (IsOrderAlreadyImported(order))
					{
						order.AddRowWarning(Res.GetString("f3d6c17d-1a5c-4dea-aa30-70061129b896", "Some order lines on this Order have previously been imported. These will override the existing invoice line"));
					}
				}
			}
		}

		#region Collections

		public OrderCollection OrdersToImport
		{
			get
			{
				if (fOrdersToImport == null)
				{
					ZQuery filter = new ZQuery();
					if (Declaration.Shipment != null)
					{
						var relatedShipments = GetRelatedShipments();
						if (relatedShipments.Any())
						{
							filter.AddToFilter(JoinCondition.Or, JobOrderHeaderSchema.JD_JS, relatedShipments.Select(x => x.PK));
						}
					}
					else
					{
						filter.AddToFilter(JobOrderHeaderSchema.JD_JE, Declaration.PK);
					}

					fOrdersToImport = new OrderCollection(Factory, filter);
				}

				return fOrdersToImport;
			}
		}

		OrderCollection fOrdersToImport;

		public IEnumerable<ForwardingShipment> GetRelatedShipments()
		{
			var result = new List<ForwardingShipment>();
			var shipment = Declaration.Shipment;
			if (shipment != null)
			{
				result.Add(shipment);
				var consol = Declaration.RelevantConsol;
				if (consol == null || !consol.IsDirect)
				{
					result.AddRange(shipment.CoLoadShipments.Cast<ForwardingShipment>());
				}
			}
			return result;
		}

		#endregion

		#region Properties

		public BaseJobDeclaration Declaration
		{
			get { return fDeclaration; }
			set { fDeclaration = value; }
		}

		BaseJobDeclaration fDeclaration;

		#region IsOrderedQuantityImport

		[BusinessObjectTestExclude]
		public ZBool IsOrderedQuantityImport
		{
			get { return QuantityType == ComInvReconciliationQuantityType.OrderQuantity; }
			set
			{
				if (value)
				{
					fQuantityType = ComInvReconciliationQuantityType.OrderQuantity;
				}
				IsOrderedQuantityImportInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsOrderedQuantityImportInfo
		{
			get { return GetZPropertyInfo(Schema.IsOrderedQuantityImport); }
		}

		#endregion

		#region IsInvoicedQuantityImport

		[BusinessObjectTestExclude]
		public ZBool IsInvoicedQuantityImport
		{
			get { return QuantityType == ComInvReconciliationQuantityType.InvoiceQuantity; }
			set
			{
				if (value)
				{
					fQuantityType = ComInvReconciliationQuantityType.InvoiceQuantity;
				}
				IsInvoicedQuantityImportInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsInvoicedQuantityImportInfo
		{
			get { return GetZPropertyInfo(Schema.IsInvoicedQuantityImport); }
		}

		#endregion

		#region IsReceivedQuantityImport

		[BusinessObjectTestExclude]
		public ZBool IsReceivedQuantityImport
		{
			get { return QuantityType == ComInvReconciliationQuantityType.ReceivedQuantity; }
			set
			{
				if (value)
				{
					fQuantityType = ComInvReconciliationQuantityType.ReceivedQuantity;
				}
				IsReceivedQuantityImportInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsReceivedQuantityImportInfo
		{
			get { return GetZPropertyInfo(Schema.IsReceivedQuantityImport); }
		}

		#endregion

		#region InvoiceNumber

		[MaxLength(35)]
		public ZString InvoiceNumber
		{
			get { return fInvoiceNumber; }
			set
			{
				if (fInvoiceNumber != value)
				{
					CheckMaximumLength(InvoiceNumberInfo, value);
					SetNonPersistentPropertyValue(InvoiceNumberInfo, ref fInvoiceNumber, value);
				}
			}
		}
		ZString fInvoiceNumber;

		public ZPropertyInfo InvoiceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceNumber); }
		}

		#endregion

		#region InvoiceDate

		public ZDateTime InvoiceDate
		{
			get { return fInvoiceDate; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceDateInfo, ref fInvoiceDate, value);
			}
		}
		ZDateTime fInvoiceDate;

		public ZPropertyInfo InvoiceDateInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceDate); }
		}

		#endregion

		#region QuantityType

		public ComInvReconciliationQuantityType QuantityType
		{
			get { return fQuantityType; }
		}

		ComInvReconciliationQuantityType fQuantityType;

		#endregion

		#endregion

		#region DefaultInvoiceNumberAndDate

		public void DefaultInvoiceNumberAndDate(Order selectedOrder)
		{
			foreach (Order order in OrdersToImport)
			{
				if (order.PK == selectedOrder.PK)
				{
					order.JD_InvoiceNumber = InvoiceNumber;
					order.JD_InvoiceDate = InvoiceDate;
				}
			}
		}

		#endregion

		#region Implementation

		bool IsOrderAlreadyImported(Order order)
		{
			bool result = false;
			if (order != null)
			{
				foreach (BaseJobComInvoiceLine line in Declaration.InvoiceLines)
				{
					foreach (OrderLine orderLine in order.OrderLines)
					{
						if (line.JI_JO == orderLine.PK)
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		#endregion
	}
}
