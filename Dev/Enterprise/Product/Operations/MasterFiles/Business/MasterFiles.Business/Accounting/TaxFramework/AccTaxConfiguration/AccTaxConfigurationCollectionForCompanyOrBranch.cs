using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxConfigurationCollectionForCompanyOrBranch : AccTaxConfigurationCollection
	{
		public AccTaxConfigurationCollectionForCompanyOrBranch(GlbCompany parent) : base(parent.Factory, parent, null, AccTaxConfigurationSchema.ETC_ParentId)
		{
			this.parent = parent;
			SetReadOnlyIncludingChildren(!Env.Security.CompaniesModifyTaxConfiguration.IsAllowed);
		}

		public AccTaxConfigurationCollectionForCompanyOrBranch(GlbBranch parent) : base(parent.Factory, parent, null, AccTaxConfigurationSchema.ETC_ParentId)
		{
			this.parent = parent;
			SetReadOnlyIncludingChildren(!Env.Security.BranchModifyTaxConfiguration.IsAllowed);
		}

		protected override void SetDefaultsForNewElementCore(AccTaxConfiguration newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.ETC_ParentTableCode = parent.TablePrefix;

			if (parent is GlbCompany company)
			{
				newElement.ETC_RN_NKCountry = AccTaxConfiguration.GetCountryFromCompany(company);
			}
			else if (parent is GlbBranch branch)
			{
				newElement.ETC_RN_NKCountry = AccTaxConfiguration.GetCountryFromBranch(branch);
			}
		}

		readonly BusinessObject parent;
	}
}
