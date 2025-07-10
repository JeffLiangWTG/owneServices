using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[ModuleID(ModuleId.OrderLine)]
	[FreezeSortOnGridCollectionElementModify(false)]
	public class OrderLineCollection : ActiveBusinessObjectCollection<OrderLine>
	{
		public OrderLineCollection(Order parentOrder)
			: base(parentOrder)
		{
		}

		public OrderLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrderLineCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		public OrderLineCollection(BusinessObjectFactory factory, ICollectionRelationship collectionRelationship, bool isAllowNewOverriden = false)
			: base(factory, collectionRelationship)
		{
			IsAllowNewOverriden = isAllowNewOverriden;
		}

		public Order ParentOrder
		{
			get { return Relationship.Master as Order; }
		}

		public bool HasOrHadOrderLinesWithQuantityReceived()
		{
			foreach (OrderLine orderLine in this)
			{
				if (orderLine.JO_QtyReceived > 0 || orderLine.JO_LineStatus == Constants.OrderStatus.PartDeliveredQuantityAmendedToZero)
				{
					return true;
				}
			}
			return false;
		}

		#region Properties

		public OrderLine Find(ZInt orderlineNumber)
		{
			OrderLine result = null;

			foreach (OrderLine orderline in this)
			{
				if (orderline.JO_LineNo == orderlineNumber)
				{
					result = orderline;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Gets a value indicating whether or not this Order's OrderLines are partially filled, that is either:
		///  - some OrderLines are fulfilled but not others
		///	 - a given OrderLine is only partially fulfilled
		/// </summary>
		public bool IsOrderPartiallyComplete
		{
			get { return !AreAllOrderLinesEmpty && !AreAllOrderLinesFulfilled; }
		}

		/// <summary>
		/// Gets a value indicating whether the quantity remaining is equal to total quantity for all lines.
		/// </summary>
		internal bool AreAllOrderLinesEmpty
		{
			get { return this.All(orderLine => orderLine.JO_QuantityRemaining == orderLine.JO_Quantity); }
		}

		bool AreAllOrderLinesFulfilled
		{
			get { return this.All(orderLine => orderLine.JO_QuantityRemaining == 0); }
		}

		#endregion

		#region Copy

		public void AddToCopiedCollection(OrderLineCollection copiedOrderLines)
		{
			if (this.Count > 0)
			{
				foreach (OrderLine line in ToArray())
				{
					OrderLine newOrderLine = Factory.New<OrderLine>();
					newOrderLine.CopyPersistentValuesFrom(line, new BusinessObjectCloneArgs(new[] { OrderLine.Schema.JO_JD }));
					copiedOrderLines.Add(newOrderLine);

					newOrderLine.JO_LineStatus = Constants.OrderStatus.Incomplete;
					newOrderLine.JO_QtyReceived = 0m;
					newOrderLine.JO_QtyInvoiced = 0m;
					newOrderLine.JO_CommercialInvoiceNo = ZString.Empty;
					newOrderLine.ManufacturerNameOrPK = line.ManufacturerNameOrPK;
					line.Deliveries.AddToCopiedCollection(newOrderLine.Deliveries);

					foreach (ZPropertyInfo propertyInfo in newOrderLine.ZPropertyInfoHash)
					{
						if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
						{
							propertyInfo.Value = ZDateTime.Empty;
						}
					}
				}
			}
		}

		#endregion

		#region Delete

		public event EventHandler ExporterOrderLineDeleteAttempt;

		void OnExporterOrderLineDeleteAttempt()
		{
			if (ExporterOrderLineDeleteAttempt != null)
			{
				ExporterOrderLineDeleteAttempt(this, EventArgs.Empty);
			}
		}

		public override void Delete(OrderLine elementToDelete)
		{
			if (ParentOrder != null && !AllowExportedOrderLinesToBeDeleted && ParentOrder.HasBeenExported)
			{
				OnExporterOrderLineDeleteAttempt();
			}
			else
			{
				base.Delete(elementToDelete);
			}
		}

		bool AllowExportedOrderLinesToBeDeleted
		{
			get { return OrdersDataRegistry.Instance.AllowExportedOrderLinesToBeDeleted.Value; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewElementCore(OrderLine newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (ParentOrder != null)
			{
				if (AdvOrmFeatureHelper.IsEnabled)
				{
					if (ParentOrder.JD_OrderStatus == Constants.OrderStatus.Incomplete)
					{
						newElement.JO_LineStatus = Constants.OrderStatus.Incomplete;
					}
					else
					{
						newElement.JO_LineStatus = ZString.Empty;
					}
				}
				else
				{
					newElement.JO_LineStatus = Constants.OrderStatus.Open;
				}

				newElement.JO_LineNo = OrderLineHighestLineNo + 1;
			}
		}

		protected override bool AllowNew
		{
			get { return !IsAllowNewOverriden && base.AllowNew; }
		}

		readonly bool IsAllowNewOverriden;

		protected ZInt OrderLineHighestLineNo
		{
			get
			{
				int highestLineNo = 0;

				foreach (OrderLine orderLine in this)
				{
					if (orderLine.JO_LineNo > highestLineNo)
					{
						highestLineNo = orderLine.JO_LineNo;
					}
				}

				return highestLineNo;
			}
		}

		#endregion

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new OrderLineFindBoxListProvider(this); }
		}

		class OrderLineFindBoxListProvider : FindBoxListProvider
		{
			public OrderLineFindBoxListProvider(OrderLineCollection collection)
				: base(collection)
			{
			}

			bool ParseOrderLineNumber(ZString code, out ZString orderNumber, out int lineNumber)
			{
				Regex reg = new Regex("^(.+) - (\\d+)$");
				Match match = reg.Match(code);
				if (match.Success && match.Groups.Count == 3)
				{
					orderNumber = match.Groups[1].Value;
					return int.TryParse(match.Groups[2].Value, out lineNumber);
				}

				orderNumber = ZString.Empty;
				lineNumber = 0;
				return false;
			}

			ZString ParsePartialOrderLineNumber(ZString code)
			{
				Regex reg = new Regex("^(?<OrderNumber>\\S+)([ ](-[ ]?)?)?$");
				Match match = reg.Match(code);
				return match.Success ? (ZString)match.Groups["OrderNumber"].Value : code;
			}

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				ZString orderNumber;
				int lineNumber;
				if (ParseOrderLineNumber(code, out orderNumber, out lineNumber))
				{
					ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(OrderLine));
					dbQuery.AddToFilter(JobOrderLineSchema.JO_LineNo, lineNumber);
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);
					subQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
					dbQuery.AddSubQuery(JobOrderLineSchema.JO_JD, subQuery, JoinCondition.And);
					query.AddToFilter(dbQuery);
				}
				else
				{
					query.IsNoResultQuery = true;
				}
			}

			protected override void AddCodeStartsWithFilter(ZQuery query, string code)
			{
				ZString orderNumber;
				int lineNumber;
				ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(OrderLine));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);
				if (ParseOrderLineNumber(code, out orderNumber, out lineNumber))
				{
					dbQuery.AddToFilter(JobOrderLineSchema.JO_LineNo, SQLComparisonOperator.Equal, lineNumber);
					subQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
				}
				else
				{
					orderNumber = ParsePartialOrderLineNumber(code);
					subQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.StartsWith, orderNumber);
				}

				dbQuery.AddSubQuery(JobOrderLineSchema.JO_JD, subQuery, JoinCondition.And);
				query.AddToFilter(dbQuery);
				query.OrderBy = JobOrderLineSchema.JO_LineNo.Name;
			}
		}

		#endregion
	}
}
