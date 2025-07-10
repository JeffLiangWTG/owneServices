using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPickFaceCommittedStockViewCollection : ActiveBusinessObjectCollection<WhsPickFaceCommittedStockView>
	{
		public WhsPickFaceCommittedStockViewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsPickFaceCommittedStockViewCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override bool AllowNew => false;
	}
}
