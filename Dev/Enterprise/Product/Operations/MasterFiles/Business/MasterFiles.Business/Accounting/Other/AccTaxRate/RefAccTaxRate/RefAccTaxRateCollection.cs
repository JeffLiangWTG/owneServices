using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefAccTaxRateCollection : ActiveBusinessObjectCollection<RefAccTaxRate>
	{
		public RefAccTaxRateCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefAccTaxRateCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}
	}
}
