using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[ModuleID(Enterprise.ZArchitecture.Modules.ModuleId.Orders)]
	public class OrderCollection : ActiveBusinessObjectCollection<Order>, IFilterModuleExtraNotificationProvider
	{
		public OrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public OrderCollection(BusinessObjectFactory factory, IAttachOrders parent)
			: base(factory, (BusinessObject)parent)
		{
		}

		public OrderCollection(JobShipmentPreplanning preAdvice, ZQuery filter)
			: base(preAdvice.Factory, preAdvice, filter, JobOrderHeaderSchema.JD_EF_ShipmentPrePlanning)
		{
			CountChanged += (s, e) => preAdvice.RefreshBinding();
		}

		public OrderCollection(BusinessObject master)
			: base(master, typeof(GenPivot), new ZQuery(), GenPivotSchema.XX_Relation1ID, GenPivotSchema.XX_Relation2ID)
		{
		}

		#region Parents

		IAttachOrders Parent
		{
			get { return Relationship.Master as IAttachOrders; }
		}

		JobShipmentPreplanning PreAdviceParent
		{
			get { return Relationship.Master as JobShipmentPreplanning; }
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { Parent };
		}

		#endregion

		#region Defaults / Adding

		protected override void SetDefaultsForNewElementCore(Order newOrder)
		{
			base.SetDefaultsForNewElementCore(newOrder);
			if (Parent != null && !Parent.IsInDatabase && !newOrder.IsInDatabase)
			{
				OnNewOrderAdded(newOrder);
			}
		}

		protected override void SetRelationshipDefaultsForElementCore(Order addedOrder, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(addedOrder, throwIfRelationshipNotSupported);

			if (!addedOrder.IsInDatabase)
			{
				OnNewOrderAdded(addedOrder);
			}
			else
			{
				OnExistingOrderAdded(addedOrder);
			}
		}

		internal void OnNewOrderAdded(Order newOrder)
		{
			if (Parent != null)
			{
				Parent.SetDefaultsOnOrder(newOrder);
			}
			else if (PreAdviceParent != null)
			{
				PreAdviceParent.SetValuesOnOrder(newOrder);
			}
		}

		void OnExistingOrderAdded(Order addedOrder)
		{
			if (Parent != null)
			{
				ISupportDataImporting parentDataImporting = Parent as ISupportDataImporting;
				if (parentDataImporting == null || !parentDataImporting.IsImportingData)
				{
					Parent.OnOrderAttached(addedOrder);
				}
			}
			else if (PreAdviceParent != null)
			{
				PreAdviceParent.AppendContainersFromOrder(addedOrder);
			}
		}

		#endregion

		#region AddNotificationWhenAdditionalFilterNotMet

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			Order order = (Order)selectedBusinessObject;
			if (order.JD_IsCancelled)
			{
				errors.Add(Res.GetString("af429af9-7c3d-4582-9f00-d6e120646b1d", "This Order cannot be chosen here as it is inactive."));
			}
			else if (!order.JD_JS.IsEmpty || !order.JD_JE.IsEmpty)
			{
				errors.Add(Res.GetString("C654F3C7-998A-4853-BAD9-FC38C8AAC087", "This Order cannot be chosen here as it is already attached to a Shipment."));
			}
			else if (Parent != null) //Can exist without a current parent
			{
				if (order.JD_TransportMode != Parent.TransportMode)
				{
					errors.Add(Res.GetString("F4E22653-7085-4E9D-8910-C41052778D0C", "This Order cannot be chosen here as the Transport Mode is different to that of the Shipment."));
				}
				else if (order.JD_ContainerMode != Parent.ContainerMode)
				{
					errors.Add(Res.GetString("272daa0e-1355-4aaf-9d12-2d9e2eac3db9", "This Order cannot be chosen here as the Container Mode is different to that of the Shipment."));
				}
			}
			else if (order.IsAttachedToSupplierBooking)
			{
				errors.Add(Res.GetString("17cb60cc-0f06-4dc6-8b62-ee22a9b6f144", @"This Order is already linked to an active Supplier Booking. Orders in use in the Supplier Bookings module cannot be linked to Shipments directly.
Please cancel all active Supplier Bookings before attaching this Order directly, or proceed to the Supplier Booking and Container Load List process to create a new Shipment from this Order."));
			}
		}

		#endregion

		#region HasOrHadOrdersWithQuantityReceived

		public bool HasOrHadOrdersWithQuantityReceived()
		{
			return this.Any(order => order.OrderLines.HasOrHadOrderLinesWithQuantityReceived());
		}

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			if (GetExtraNotificationHanlder != null)
			{
				return GetExtraNotificationHanlder(businessObject);
			}

			return null;
		}

		public System.Func<BusinessObject, INotification> GetExtraNotificationHanlder;

		#endregion

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new OrderFindBoxListProvider(this); }
		}

		class OrderFindBoxListProvider : FindBoxListProvider
		{
			public OrderFindBoxListProvider(OrderCollection collection)
				: base(collection)
			{
			}

			bool ParseOrderNumberWithSplit(ZString code, out ZString orderNumber, out byte splitNumber)
			{
				Regex reg = new Regex("^(.+)-(\\d*)$");
				Match match = reg.Match(code);
				if (match.Success && match.Groups.Count == 3)
				{
					orderNumber = match.Groups[1].Value;
					return byte.TryParse(match.Groups[2].Value, out splitNumber);
				}

				orderNumber = code;
				splitNumber = 0;
				return false;
			}

			ZString ParsePartialOrderNumber(ZString code)
			{
				Regex reg = new Regex("^(?<OrderNumber>\\S+)-$");
				Match match = reg.Match(code);
				return match.Success ? (ZString)match.Groups["OrderNumber"].Value : code;
			}

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				ZString codeWithoutBuyerCode;
				ZString buyerCode;
				ZString orderNumber;
				byte splitNumber;

				if (ParseOrderNumberWithBuyerCode(code, out codeWithoutBuyerCode, out buyerCode))
				{
					var buyerQuery = new ZDBOnlyQuery(typeof(Order));
					var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobOrderHeaderSchema.JD_OA_BuyerAddress);
					var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
					orgSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, buyerCode);
					addressSubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
					buyerQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
					query.AddToFilter(buyerQuery);
				}

				if (ParseOrderNumberWithSplit(codeWithoutBuyerCode, out orderNumber, out splitNumber))
				{
					var splitQuery = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
					splitQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, splitNumber);
					splitQuery.AddToFilter(JoinCondition.Or, JobOrderHeaderSchema.JD_OrderNumber, codeWithoutBuyerCode);
					query.AddToFilter(splitQuery);
				}
				else
				{
					query.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, codeWithoutBuyerCode);
				}

				if (code != codeWithoutBuyerCode)
				{
					query.AddToFilter(JoinCondition.Or, JobOrderHeaderSchema.JD_OrderNumber, code);
				}
			}

			bool ParseOrderNumberWithBuyerCode(ZString code, out ZString orderNumber, out ZString buyerCode)
			{
				buyerCode = ZString.Empty;
				var lastIndex = code.LastIndexOf('|');
				if (lastIndex > -1)
				{
					orderNumber = code.SubstringSafe(0, lastIndex);
					buyerCode = code.SubstringSafe(lastIndex + 1);

					return true;
				}

				orderNumber = code;
				return false;
			}

			protected override void AddCodeStartsWithFilter(ZQuery query, string code)
			{
				ZString orderNumber;
				byte splitNumber;
				if (ParseOrderNumberWithSplit(code, out orderNumber, out splitNumber))
				{
					var splitQuery = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
					splitQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, SQLComparisonOperator.GreaterThanOrEqualTo, splitNumber);
					query.AddToFilter(splitQuery, JoinCondition.Or);
					var codeQuery = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.StartsWith, code);
					query.AddToFilter(codeQuery, JoinCondition.Or);
				}
				else
				{
					orderNumber = ParsePartialOrderNumber(code);
					query.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.StartsWith, orderNumber);
				}
				query.OrderBy = JobOrderHeaderSchema.JD_OrderNumberSplit.Name;
			}
		}

		#endregion
	}
}
