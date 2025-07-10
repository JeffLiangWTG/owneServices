using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingPackLineManyToManyCollection : ForwardingPackLineManyToManyCollection
	{
		public TrackingPackLineManyToManyCollection(TrackingContainer container) : base(container)
		{
		}

		public new TrackingPackLine this[int index]
		{
			get { return (TrackingPackLine)Elements[index]; }
		}

		public new TrackingPackLine AddNew()
		{
			return (TrackingPackLine)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TrackingPackLine);
		}
	}
}
