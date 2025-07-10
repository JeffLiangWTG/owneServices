using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsRMAOrderLineCollection : NonPersistentBusinessObjectCollection<WhsRMAOrderLine>
	{
		public WhsRMAOrderLineCollection(BusinessObjectFactory factory, IEnumerable<WhsRMAOrderLine> rmaOrderLines)
			: base(factory)
		{
			Argument.NotNull(rmaOrderLines, nameof(rmaOrderLines));

			AddRange(rmaOrderLines);
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
