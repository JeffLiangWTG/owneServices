//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccOrgTaxConfigurationLookups
//
//    This class should be used for overriding collections in AutoAccOrgTaxConfigurationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxConfigurationLookups : AutoAccOrgTaxConfigurationLookups
	{
		public AccOrgTaxConfigurationLookups(AutoAccOrgTaxConfiguration parent) : base(parent)
		{
		}

		new AccOrgTaxConfiguration Parent
		{
			get { return (AccOrgTaxConfiguration)base.Parent; }
		}

		public override AccTaxConfigurationCollection TaxConfigurations
		{
			get
			{
				var company = Parent.GetParentCompany();
				if (company != null)
				{
					var query = new ZQuery(AccTaxConfigurationSchema.ETC_Ledger, Parent.Ledger);

					return DependencyFactory.GetTaxFrameworkConfigurationHelper().GetCompanyTaxConfigurations(Factory, company, query);
				}
				else
				{
					return new AccTaxConfigurationCollection(Factory, new ZQuery() { IsNoResultQuery = true });
				}
			}
		}

		IAccountingMasterFilesDependencyFactory DependencyFactory => ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>();
	}
}
