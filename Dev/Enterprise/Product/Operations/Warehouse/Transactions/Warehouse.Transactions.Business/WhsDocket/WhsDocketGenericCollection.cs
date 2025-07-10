#if DEBUG

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketGenericCollection : WhsDocketCollection
	{
		public WhsDocketGenericCollection(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(WhsDocket)))
		{
		}

		protected override bool AllowNewDocket
		{
			get { return false; }
		}

		public virtual new WhsDocket AddNew()
		{
			throw new NotSupportedException("Non supported operation");
		}

		protected override IEnumerable<string> DocketTypes
		{
			get
			{
				return new[]
				{
					DocketType.Codes.Adjustment,
					DocketType.Codes.Order,
					DocketType.Codes.Receive,
					DocketType.Codes.Transfer,
					DocketType.Codes.WorkOrder,
				};
			}
		}
	}
}
#endif
