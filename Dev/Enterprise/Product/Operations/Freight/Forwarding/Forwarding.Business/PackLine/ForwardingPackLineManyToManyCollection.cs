using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingPackLineManyToManyCollection : PackLineManyToManyCollection, Integration.Forwarding.IForwardingPackLineManyToManyCollection
	{
		public ForwardingPackLineManyToManyCollection(ForwardingContainer container) : base(container)
		{
		}

		public new ForwardingPackLine this[int index]
		{
			get { return (ForwardingPackLine)(Elements[index]); }
		}

		public new ForwardingPackLine AddNew()
		{
			return (ForwardingPackLine)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ForwardingPackLine);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override void Remove(BusinessObject bizObj)
		{
			using (((ForwardingPackLine)bizObj).Shipment?.MonitorRequireTEUChange(new CO2eStatusChangedReason(freeTextReason: (NoResString)"PackLine Container allocation changed")))
			{
				base.Remove(bizObj);
			}
		}
	}
}
