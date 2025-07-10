using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.InBond.Business
{
	public class MovementHeaderWrapperCollection : NonPersistentBusinessObjectCollection<MovementHeaderWrapper>
	{
		public MovementHeaderWrapperCollection(BusinessObjectFactory factory, CusInBondShipmentWrapperCollection shipmentsWithoutInBond)
		{
			this.factory = factory;
			this.shipmentsWithoutInBond = shipmentsWithoutInBond;
			AddNew();
		}
		readonly BusinessObjectFactory factory;
		readonly CusInBondShipmentWrapperCollection shipmentsWithoutInBond;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is MovementHeaderWrapper header)
			{
				header.InBondNumber = string.Format("NOT YET SPECIFIED {0}", Count + 1);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MovementHeaderWrapper(factory);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (bizO is MovementHeaderWrapper wrapper)
			{
				foreach (var allocatedShipmentsForMovement in wrapper.AllocatedShipmentsForMovement)
				{
					shipmentsWithoutInBond.Add(allocatedShipmentsForMovement);
				}

				if (Count > 0)
				{
					var index = 1;
					foreach (MovementHeaderWrapper moveHeader in this)
					{
						moveHeader.InBondNumber = string.Format("NOT YET SPECIFIED {0}", index++);
					}
				}
			}
		}
	}
}
