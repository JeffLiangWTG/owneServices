using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxConfigurationTemplateItemValidation : AccOrgTaxConfigurationValidation
	{
		public AccOrgTaxConfigurationTemplateItemValidation(AccOrgTaxConfigurationTemplateItem parent) : base(parent)
		{
		}

		protected override void CheckOTC_ETC()
		{
			base.CheckOTC_ETC();

			if (Parent.OTC_ETC.IsValid && (Parent.OrgTaxConfigurationTemplate?.AccOrgTaxConfigurations.Any(x => x.OTC_ETC == Parent.OTC_ETC && x.PK != Parent.PK) ?? false))
			{
				Parent.OTC_ETCInfo.AddError(Res.GetString("34F3BCD9-E918-4F6F-ACDA-C1C18ACA6B35", "Same Tax Configuration can only be listed in Template once."));
			}
		}

		protected new AccOrgTaxConfigurationTemplateItem Parent
		{
			get { return (AccOrgTaxConfigurationTemplateItem)base.Parent; }
		}
	}
}
