using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderJobNumberStrategy : BaseContainerYardNumberFountainsStrategy
	{
		public MNRWorkOrderJobNumberStrategy(BusinessObjectFactory factory)
			: base(factory, Env.Instance.NumberFountains.MNRWorkOrderJobNumber) { }

		public string GetJobNumber()
		{
			return GetNumber();
		}
	}
}
