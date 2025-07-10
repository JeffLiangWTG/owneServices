using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class OrderDataObject : DocDataObject
	{
		public OrderDataObject(object identifier) : base(identifier) { }

		#region OrderNumber

		public ZString OrderNumber
		{
			get => orderNumber;
			set
			{
				if (SetNonPersistentPropertyValue(OrderNumberInfo, ref orderNumber, value))
				{
					Validate(OrderNumberInfo);
				}
			}
		}

		ZString orderNumber;

		public ZPropertyInfo OrderNumberInfo => GetZPropertyInfo(nameof(OrderNumber));

		#endregion

		#region OrderDate

		public ZDateTime OrderDate
		{
			get => orderDate;
			set
			{
				if (SetNonPersistentPropertyValue(OrderDateInfo, ref orderDate, value))
				{
					Validate(OrderDateInfo);
				}
			}
		}

		ZDateTime orderDate;

		public ZPropertyInfo OrderDateInfo => GetZPropertyInfo(nameof(OrderDate));

		#endregion

		#region OrderLines

		public IReadOnlyCollection<OrderLineDataObject> OrderLines
		{
			get => orderLines;
			set => orderLines = SetChildCollection(orderLines, value);
		}

		IReadOnlyCollection<OrderLineDataObject> orderLines;

		#endregion
	}
}
