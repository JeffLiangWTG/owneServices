using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketCollectionAdHoc : ActiveBusinessObjectCollection<WhsDocket>
	{
		public WhsDocketCollectionAdHoc(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(WhsDocket)))
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
