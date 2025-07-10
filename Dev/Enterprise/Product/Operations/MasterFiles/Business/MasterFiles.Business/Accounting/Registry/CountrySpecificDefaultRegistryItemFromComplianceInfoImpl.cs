using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	// This class is OBSOLETE. If you wish to use a registry implementation that is country sepcific, with caching, then please use CountrySpecificDefaultValueRegistryItemImpl.
	// While using CountrySpecificDefaultValueRegistryItemImpl, remember to implement country specific AccountingCountryFactory (if already doesn't exist) and country specific 
	// DefaultValuesForCountrySpecificRegistryItems.
	//
	// As an example, Please refer to implementation of ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency registry and SingaporeRegistryItemsDefaultValues class.

	/// <summary>
	/// Defines a strongly typed registry item implementation with default value based on ICountryComplianceInfo from GlbCompany.GC_RN_NKCountryCode.
	/// Note that the ICountryComplianceInfo may be null; consumers MUST do null checks in defaultValueGetter.
	/// Note that the ICountryComplianceInfo may be cast to a sub-type interface; consumes MUST check types in defaultValueGetter.
	/// </summary>
	public class CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<T> : CompanySpecificDefaultWithCacheRegistryItemImpl<T>
	{
		public CountrySpecificDefaultRegistryItemFromComplianceInfoImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions option, Func<ICountryComplianceInfo, T> defaultValueGetter, T fallbackDefault)
			: this(name, category, caption, hint, dataType, null, storage, option, defaultValueGetter, fallbackDefault)
		{
		}

		public CountrySpecificDefaultRegistryItemFromComplianceInfoImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions option, Func<ICountryComplianceInfo, T> defaultValueGetter, T fallbackDefault)
			: base(name, category, caption, hint, dataType, editorInfo, storage, option, fallbackDefault)
		{
			this.defaultValueGetter = Argument.NotNull(defaultValueGetter, nameof(defaultValueGetter));
		}

		readonly Func<ICountryComplianceInfo, T> defaultValueGetter;

		protected override T GetDefaultValueByCompany(GlbCompany company)
			=> defaultValueGetter(GetComplianceInfoFromCompany(company));

		protected static ICountryComplianceInfo GetComplianceInfoFromCompany(GlbCompany company)
			=> ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(company.GC_RN_NKCountryCode);
	}
}
