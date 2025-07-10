using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Warehouse.Yard.Business
{
	public class TransportationUnitJobNumberStrategy : BaseContainerYardNumberFountainsStrategy
	{
		public TransportationUnitJobNumberStrategy(BusinessObjectFactory factory)
			: base(factory, Env.Instance.NumberFountains.CYDTransportationUnitID) { }

		public string GetJobNumber()
		{
			return GetNumber();
		}
	}
}
