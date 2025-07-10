using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondShipmentWrapperCollection : NonPersistentBusinessObjectCollection<CusInBondShipmentWrapper>
	{
		public CusInBondShipmentWrapperCollection(ForwardingConsol consol)
		{
			if (consol != null)
			{
				this.consol = consol;
				Load();
			}
		}
		readonly ForwardingConsol consol;

		public void MoveBOToTargetCollection(CusInBondShipmentWrapperCollection targetCollection, CusInBondShipmentWrapper wrapper)
		{
			if (!targetCollection.Contains(wrapper) && this.Contains(wrapper))
			{
				targetCollection.Add(wrapper);
				Remove(wrapper);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public override void Load()
		{
			using (SuspendSettingHasChanges())
			{
				RemoveAll();
				var shipments = consol.Shipments.Cast<ForwardingShipment>().Where(x => x.InBondHeader == null);
				shipments.ForEach(x => Add(new CusInBondShipmentWrapper(x)));
			}
		}
	}
}
