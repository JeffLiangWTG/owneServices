using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	// This class is OBSOLETE. If you wish to use a registry implementation that is country sepcific, with caching, then please use CountrySpecificDefaultValueRegistryItemImpl.
	// While using CountrySpecificDefaultValueRegistryItemImpl, remember to implement country specific AccountingCountryFactory (if already doesn't exist) and country specific 
	// DefaultValuesForCountrySpecificRegistryItems.
	//
	// As an example, Please refer to implementation of ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency registry and SingaporeRegistryItemsDefaultValues class.

	/// <summary>
	/// Defines a strongly typed registry item implementation with default value based on GlbCompany.GC_RN_NKCountryCode.
	/// </summary>
	public class CountrySpecificDefaultRegistryItemImpl<T> : CompanySpecificDefaultWithCacheRegistryItemImpl<T>
	{
		public CountrySpecificDefaultRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions option, Func<ZString, T> defaultValueGetter, T fallbackDefault)
			: this(name, category, caption, hint, dataType, null, storage, option, defaultValueGetter, fallbackDefault)
		{
		}

		public CountrySpecificDefaultRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions option, Func<ZString, T> defaultValueGetter, T fallbackDefault)
			: base(name, category, caption, hint, dataType, editorInfo, storage, option, fallbackDefault)
		{
			this.defaultValueGetter = Argument.NotNull(defaultValueGetter, nameof(defaultValueGetter));
		}

		readonly Func<ZString, T> defaultValueGetter;

		protected override T GetDefaultValueByCompany(GlbCompany company)
			=> defaultValueGetter(company.GC_RN_NKCountryCode);
	}
}
