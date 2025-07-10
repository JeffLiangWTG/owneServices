using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxConfigurationCollection : ActiveBusinessObjectCollection<AccOrgTaxConfiguration>
	{
		public AccOrgTaxConfigurationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		public AccOrgTaxConfigurationCollection(BusinessObjectFactory factory, BusinessObject master)
			: base(factory, master)
		{
		}
	}
}
