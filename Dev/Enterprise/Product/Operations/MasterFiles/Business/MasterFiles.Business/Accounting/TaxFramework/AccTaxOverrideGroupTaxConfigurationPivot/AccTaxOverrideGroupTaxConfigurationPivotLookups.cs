//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxOverrideGroupTaxConfigurationPivotLookups
//
//    This class should be used for overriding collections in AutoAccTaxOverrideGroupTaxConfigurationPivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxOverrideGroupTaxConfigurationPivotLookups : AutoAccTaxOverrideGroupTaxConfigurationPivotLookups
	{
		public AccTaxOverrideGroupTaxConfigurationPivotLookups(AutoAccTaxOverrideGroupTaxConfigurationPivot parent) : base(parent)
		{
		}

		ZString ParentCountryCode => Parent.TaxOverrideGroup?.AX_RN_NKCountry ?? ZString.Empty;

		public override AccTaxRateCollection TaxIDs
		{
			get
			{
				return new NonVATAccTaxRateCollection(Factory, ParentCountryCode, Parent.TaxConfiguration);
			}
		}

		public override AccInvMsgCollection DefaultVATClasses
		{
			get { return new AccInvMsgCollection(Factory, ParentCountryCode); }
		}

		public override AccTaxConfigurationCollection TaxConfigurations
		{
			get
			{
				var company = GlbCompany.CurrentCompany;
				if (company != null)
				{
					var query = new ZQuery();
					return DependencyFactory.GetTaxFrameworkConfigurationHelper().GetCompanyTaxConfigurations(Factory, company, query);
				}
				else
				{
					return new AccTaxConfigurationCollection(Factory, new ZQuery() { IsNoResultQuery = true });
				}
			}
		}

		new AccTaxOverrideGroupTaxConfigurationPivot Parent
		{
			get { return (AccTaxOverrideGroupTaxConfigurationPivot)base.Parent; }
		}

		IAccountingMasterFilesDependencyFactory DependencyFactory => dependencyFactory ?? (dependencyFactory = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>());
		IAccountingMasterFilesDependencyFactory dependencyFactory;
	}
}
