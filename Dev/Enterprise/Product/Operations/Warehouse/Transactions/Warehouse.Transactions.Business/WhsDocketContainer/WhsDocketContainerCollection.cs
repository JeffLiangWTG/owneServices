using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketContainerCollection : ActiveBusinessObjectCollection<WhsDocketContainer>
	{
		public WhsDocketContainerCollection(WhsDocket master, BusinessObjectFactory factory)
			: base(factory, master)
		{
		}
	}
}
