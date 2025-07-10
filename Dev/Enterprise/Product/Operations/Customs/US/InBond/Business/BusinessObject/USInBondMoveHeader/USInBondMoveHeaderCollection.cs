using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.InBond.Business
{
	public class USInBondMoveHeaderCollection : ActiveBusinessObjectCollection<USInBondMoveHeader>
	{
		public USInBondMoveHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
