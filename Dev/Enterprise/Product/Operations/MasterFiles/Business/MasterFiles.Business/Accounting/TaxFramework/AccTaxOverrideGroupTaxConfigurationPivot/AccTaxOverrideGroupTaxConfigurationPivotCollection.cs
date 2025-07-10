using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxOverrideGroupTaxConfigurationPivotCollection : ActiveBusinessObjectCollection<AccTaxOverrideGroupTaxConfigurationPivot>
	{
		public AccTaxOverrideGroupTaxConfigurationPivotCollection(AccTaxOverrideGroup parent)
			: base(parent.Factory, parent, new ZQuery(), AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_AX_TaxOverrideGroup)
		{
		}
	}
}
