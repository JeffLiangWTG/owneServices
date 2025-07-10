using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module
{
	public class AccChargeCodeModuleForRegistry : AccChargeCodeModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccChargeCodeCollectionForRegistry(Factory);
		}
	}
}
