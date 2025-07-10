
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class PackLineForShipmentAndContainerCollection : OuterPackLineCollection
	{
		public PackLineForShipmentAndContainerCollection(PackUnpackShipment master, BusinessObjectFactory factory) : base(master, factory)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			PackLine newPack = (PackLine)child;
			PackUnpackShipment shipment = (PackUnpackShipment)Master;

			base.SetDefaultsForNewChild(newPack);

			if (shipment.ParentContainerRegistration != null)
			{
				newPack.SetContainer(shipment.ParentContainerRegistration.PK);
			}
		}

		public new PackLine AddNew()
		{
			return base.AddNew();
		}
	}
}
