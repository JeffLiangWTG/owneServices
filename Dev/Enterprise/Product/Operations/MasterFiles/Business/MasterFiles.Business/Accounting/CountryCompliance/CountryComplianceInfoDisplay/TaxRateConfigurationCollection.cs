using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay
{
	public class TaxRateConfigurationCollection : NonPersistentBusinessObjectCollection<TaxRateConfiguration>
	{
		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		internal void LoadTaxIds(string countryCode = null)
		{
			RemoveAll();
			var allTaxIDs = new TaxRateXmlParser().BuildTaxRatesDictionaryBasedOnCountry();
			if (countryCode != null)
			{
				if (allTaxIDs.TryGetValue(countryCode, out IEnumerable<ITaxRateConfiguration> countryTaxIDs))
				{
					AddRange(countryTaxIDs);
				}
			}
			else
			{
				AddRange(allTaxIDs.Values.SelectMany(x => x));
			}
		}
	}
}
