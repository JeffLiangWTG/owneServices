using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class CountrySpecificRegistryDefaultValueCollection : NonPersistentBusinessObjectCollection<DefaultRegistryValueDisplay>
	{
		public CountrySpecificRegistryDefaultValueCollection()
		{
			//To add new registries, the need to be refactored as per: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/4341/Accounting-registry-with-country-specific-default-values
			//Please add new items to the list alphabetically
			listOfCountrySpecificDefaultRegistryValues.Add(new DelayTimeForRequeueInvoices_RegistryDescriptor());
			listOfCountrySpecificDefaultRegistryValues.Add(new EInvoicingAmendmentCodes_RegistryDescriptor());
			listOfCountrySpecificDefaultRegistryValues.Add(new EInvoicingReversalCodes_RegistryDescriptor());
			listOfCountrySpecificDefaultRegistryValues.Add(new EnableGovernmentAllocatedNumberBehavior_RegistryDescriptor());
			listOfCountrySpecificDefaultRegistryValues.Add(new PrintWatermarkForTransactionAwaitingApproval_RegistryDescriptor());
			listOfCountrySpecificDefaultRegistryValues.Add(new SAFTGroupingCategory_RegistryDescriptor());
			listOfCountrySpecificDefaultRegistryValues.Add(new ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency_RegistryDescriptor());
			listOfCountrySpecificDefaultRegistryValues.Add(new TaxMessageIsMandatoryPayables_RegistryDescriptor());
			listOfCountrySpecificDefaultRegistryValues.Add(new TaxMessageIsMandatoryReceivables_RegistryDescriptor());
			listOfCountrySpecificDefaultRegistryValues.Add(new ThirdPartyEInvoiceDocType_RegistryDescriptor());
			//Please add new items to the list alphabetically
		}

		internal void LoadCountrySpecificRegistryDefaultValue(ZString countryCode)
		{
			RemoveAll();

			foreach (var item in listOfCountrySpecificDefaultRegistryValues)
			{
				Add(new DefaultRegistryValueDisplay(item.GetCaption(), item.GetDefaultValueForDisplay(countryCode)));
			}
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		readonly List<ICountrySpecificDefaultRegistryValue> listOfCountrySpecificDefaultRegistryValues = new List<ICountrySpecificDefaultRegistryValue>();
	}
}
