using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module
{
	public class AccTaxRateModuleForRegistry : AccTaxRateModule
	{
		public AccTaxRateModuleForRegistry()
			: base()
		{
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccTaxRateCollectionForRegistry(Factory);
		}
	}
}
