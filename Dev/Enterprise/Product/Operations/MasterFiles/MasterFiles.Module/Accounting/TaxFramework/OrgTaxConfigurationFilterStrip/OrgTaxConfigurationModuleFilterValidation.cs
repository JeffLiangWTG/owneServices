using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgTaxConfigurationModuleFilterValidation : ModuleTextFilterValidation
	{
		public OrgTaxConfigurationModuleFilterValidation(OrgTaxConfigurationModuleFilter parent) : base(parent)
		{
			Parent = parent;
		}

		new readonly OrgTaxConfigurationModuleFilter Parent;

		public void ValidateTaxConfiguration()
		{
			ValidateCalculatedProperty(Parent.TaxConfigurationInfo);
		}

		protected void CheckTaxConfiguration()
		{
			if (!Parent.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.TaxConfigurationInfo);
			}
			ListValidation.ErrorIfInvalidPK(Parent.TaxConfigurationInfo);
		}

		public void ValidateConfigurationStatus()
		{
			ValidateCalculatedProperty(Parent.ConfigurationStatusInfo);
		}

		protected void CheckConfigurationStatus()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ConfigurationStatusInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateTaxConfiguration();
			ValidateConfigurationStatus();
		}
	}
}
