using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Warehouse.Yard.Business
{
	public class DeliveryIDNumberFoutainStrategy : BaseContainerYardNumberFountainsStrategy
	{
		public DeliveryIDNumberFoutainStrategy(BusinessObjectFactory factory)
			: base(factory, Env.Instance.NumberFountains.CYDDeliveryID) { }

		public string GetDeliveryID()
		{
			return GetNumber();
		}
	}
}
