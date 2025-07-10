using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPickFaceAwaitingReplenishmentViewCollection : ActiveBusinessObjectCollection<WhsPickFaceAwaitingReplenishmentView>
	{
		public WhsPickFaceAwaitingReplenishmentViewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsPickFaceAwaitingReplenishmentViewCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WhsPickFaceAwaitingReplenishmentViewCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected override bool AllowNew => false;
	}
}
