using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class AirlineConfigCommodityBusinessObjectCollection : NonPersistentBusinessObjectCollection<AirlineConfigCommodityBusinessObject>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException();

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
