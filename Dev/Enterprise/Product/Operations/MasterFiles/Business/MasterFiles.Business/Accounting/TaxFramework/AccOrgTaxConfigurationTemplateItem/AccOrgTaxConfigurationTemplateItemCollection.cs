using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxConfigurationTemplateItemCollection : ActiveBusinessObjectCollection<AccOrgTaxConfigurationTemplateItem>
	{
		public AccOrgTaxConfigurationTemplateItemCollection(AccOrgTaxConfigurationTemplate parentTemplate) : base(parentTemplate.Factory, parentTemplate)
		{
		}
	}
}
