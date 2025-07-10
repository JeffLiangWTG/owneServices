using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class TransportNonDependentCollection : BusinessObjectCollection<TransportNonDependent>
	{
		public TransportNonDependentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
