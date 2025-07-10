using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class TaxFrameworkAccTaxRateLoader : NonPersistentBusinessObject
	{
		public TaxFrameworkAccTaxRateLoader()
			: base(new BusinessObjectFactory())
		{
		}

		public TaxFrameworkAccTaxRateCollection TaxFrameworkAccTaxRates
		{
			get
			{
				if (taxFrameworkAccTaxRates == null)
				{
					taxFrameworkAccTaxRates = new TaxFrameworkAccTaxRateCollection(Factory);
					RegisterEditableChildObject(taxFrameworkAccTaxRates);
				}
				return taxFrameworkAccTaxRates;
			}
		}
		TaxFrameworkAccTaxRateCollection taxFrameworkAccTaxRates;
	}
}
