
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Warehouse.Yard.Business
{
	public class ReleaseOrderJobNumberStrategy : BaseContainerYardNumberFountainsStrategy
	{
		public ReleaseOrderJobNumberStrategy(BusinessObjectFactory factory)
			: base(factory, Env.Instance.NumberFountains.CYDReleaseAdviceJobNumber) { }

		public string GetJobNumber()
		{
			return GetNumber();
		}
	}
}
