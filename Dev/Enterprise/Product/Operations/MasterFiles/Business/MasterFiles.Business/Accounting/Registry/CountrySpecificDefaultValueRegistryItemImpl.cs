using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.Foundation.FrameworkExtensions.Functional;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Defines a strongly typed registry item implementation with default value based on GlbCompany.GC_RN_NKCountryCode.
	/// The default value is retrieved from country specific implementation IDefaultValuesForCountrySpecificRegistryItems (or DefaultValuesForCountrySpecificRegistryItems class if no specific implementation for a country).
	/// For more help please refer to : https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/4341/Accounting-registry-with-country-specific-default-values
	/// </summary>
	public class CountrySpecificDefaultValueRegistryItemImpl<T> : RegistryItemImpl
	{
		public CountrySpecificDefaultValueRegistryItemImpl(string name, MultilingualString category, MultilingualString hint, IRegistryDataType dataType, RegistryOptions option, BusinessObjectFactory factoryForDefaultValues, CountrySpecificDefaultRegistryDescriptor<T> countrySpecificDefaultRegistryDescriptor, IRegistryEditorInfo editorInfo = null, object defaultValue = null, bool useDefaultDefaultValue = true)
			: base(name, category, countrySpecificDefaultRegistryDescriptor?.GetDefaultTypeCaption(), hint, dataType, editorInfo, RegistryStorageFlags.Company, option, defaultValue, useDefaultDefaultValue)
		{
			this.countrySpecificDefaultRegistryDescriptor = Argument.NotNull(countrySpecificDefaultRegistryDescriptor, nameof(countrySpecificDefaultRegistryDescriptor));
			this.factory = Argument.NotNull(factoryForDefaultValues, nameof(factoryForDefaultValues));
		}

		readonly CountrySpecificDefaultRegistryDescriptor<T> countrySpecificDefaultRegistryDescriptor;
		readonly BusinessObjectFactory factory;

		protected override sealed object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var companyProvider = ObjectFactory.Get<ICompanyProvider>().WithFactory(factory);
			var companyOption = companyPK == Guid.Empty ? Option.None<ICompany>() : companyProvider.Get(companyPK);
			var countryCodeOption = companyOption.GetCountryCode();
			if (countryCodeOption.TryGet(out var countryCode))
			{
				return countrySpecificDefaultRegistryDescriptor.GetDefaultValue(countryCode);
			}

			return DataType.DefaultValue;
		}
	}
}
