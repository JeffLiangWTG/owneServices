using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketReferenceCollection : DependentBusinessObjectCollection<WhsDocketReference, WhsDocket>
	{
		public WhsDocketReferenceCollection(WhsDocket master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}
	}
}
