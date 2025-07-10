using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class TaxFrameworkAccTaxRateCollection : ActiveBusinessObjectCollection<TaxFrameworkAccTaxRate>
	{
		public TaxFrameworkAccTaxRateCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool MatchesFilterCore(TaxFrameworkAccTaxRate taxRate, bool fetchOnlyFromLocalCache)
		{
			return !taxRate.IsInDatabase || taxRate.AT_TaxSystemCode != ZString.Empty;
		}
	}
}
