using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Warehouse.Yard.Business
{
	public class PickupIDNumberFoutainStrategy : BaseContainerYardNumberFountainsStrategy
	{
		public PickupIDNumberFoutainStrategy(BusinessObjectFactory factory)
			: base(factory, Env.Instance.NumberFountains.CYDPickupID) { }

		public string GetPickupID()
		{
			return GetNumber();
		}
	}
}
