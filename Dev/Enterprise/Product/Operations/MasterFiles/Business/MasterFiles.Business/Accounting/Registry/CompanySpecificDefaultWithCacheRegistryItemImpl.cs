using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	// This class is OBSOLETE. If you wish to use a registry implementation that is country sepcific, with caching, then please use CountrySpecificDefaultValueRegistryItemImpl.
	// While using CountrySpecificDefaultValueRegistryItemImpl, remember to implement country specific AccountingCountryFactory (if already doesn't exist) and country specific 
	// DefaultValuesForCountrySpecificRegistryItems.
	//
	// As an example, Please refer to implementation of ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency registry and SingaporeRegistryItemsDefaultValues class.

	/// <summary>
	/// Defines a strongly typed registry item implementation that maintains a cache of default values by GlbCompany.PK.
	/// This class is abstract; see CountrySpecificDefaultRegistryItemImpl and CountrySpecificDefaultRegistryItemFromComplianceInfoImpl for concrete types.
	/// </summary>
	/// <remarks>
	/// This class is NOT safe to use for dynamic values; the defaults for each country / company MUST be constant.
	/// </remarks>
	public abstract class CompanySpecificDefaultWithCacheRegistryItemImpl<T> : RegistryItemImpl
	{
		protected CompanySpecificDefaultWithCacheRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions option, T fallbackDefault)
			: this(name, category, caption, hint, dataType, null, storage, option, fallbackDefault)
		{
		}

		protected CompanySpecificDefaultWithCacheRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions option, T fallbackDefault)
			: base(name, category, caption, hint, dataType, editorInfo, storage, option, fallbackDefault)
		{
			if (storage != RegistryStorageFlags.Company)
			{
				throw new ArgumentException("RegistryStorageFlags must be Company. Note this could be expanded with additional logic and unit tests.", nameof(storage));
			}

			this.fallbackDefault = fallbackDefault;
		}

		protected readonly T fallbackDefault;
		Dictionary<Guid, T> defaultValueCacheByCompanyPk;

		protected abstract T GetDefaultValueByCompany(GlbCompany company);

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override sealed object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (defaultValueCacheByCompanyPk == null)
			{
				defaultValueCacheByCompanyPk = new Dictionary<Guid, T>();
			}

			if (defaultValueCacheByCompanyPk.TryGetValue(companyPK, out var result))
			{
				return result;
			}

			UpdateCache();
			if (defaultValueCacheByCompanyPk.TryGetValue(companyPK, out result))
			{
				return result;
			}
			else
			{
				return fallbackDefault;
			}
		}

		void UpdateCache()
		{
			foreach (var company in GetListOfCompanies())
			{
				if (company != null
					&& !defaultValueCacheByCompanyPk.ContainsKey(company.PK.ToGuid()))
				{
					var companyCountryValue = GetDefaultValueByCompany(company);
					defaultValueCacheByCompanyPk.Add(company.PK.ToGuid(), companyCountryValue);
				}
			}
		}

		protected virtual IReadOnlyCollection<GlbCompany> GetListOfCompanies()
			=> new BusinessObjectFactory().Load<GlbCompany>(new ZQuery());

#if DEBUG
		public void ClearCache_ForTestOnly()
		{
			defaultValueCacheByCompanyPk = null;
		}
#endif
	}
}
