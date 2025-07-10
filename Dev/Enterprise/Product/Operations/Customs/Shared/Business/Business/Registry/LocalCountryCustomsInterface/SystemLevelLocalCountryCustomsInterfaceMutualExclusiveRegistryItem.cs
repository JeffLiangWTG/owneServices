using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.Business.Res;

namespace Enterprise.Customs.DataRegistry.Business
{
	public abstract class SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryItem : StronglyTypedRegistryItem<string>, IRegistryItemWithOtherChangedItems
	{
		protected SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryDataType registryDataType, RegistryStorageFlags storage, object defaultValue)
			: base(new SystemLevelLocalCountryCustomsInterfaceMutualExclusiveImpl(name, category, caption, hint, registryDataType, storage, defaultValue))
		{
		}

		public IEnumerable<IRegistryItemInternals> OtherChangedItems { get; set; }

		class SystemLevelLocalCountryCustomsInterfaceMutualExclusiveImpl : RegistryItemImpl
		{
			public SystemLevelLocalCountryCustomsInterfaceMutualExclusiveImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryDataType registryDataType, RegistryStorageFlags storage, object defaultValue)
				: base(name, category, caption, hint, registryDataType, storage, defaultValue)
			{
			}
		}
	}

	public abstract class SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			var registryItemWithOtherChangedItems = registryItem as IRegistryItemWithOtherChangedItems;
			if (registryItemWithOtherChangedItems != null && registryItemWithOtherChangedItems.IsRegistryItemOverriden(Guid.Empty))
			{
				foreach (var company in GlbCompany.GetActiveCompanies(CountryCode))
				{
					if (registryItemWithOtherChangedItems.IsRegistryItemConfiguredInCustomsInterface(company.PK.ToGuid()))
					{
						throw new RegistryValidationException(CannotConfigureRegistryForCountry(company.GC_Code));
					}
				}
			}
		}

		protected abstract ZString CountryCode { get; }
		protected abstract ZString CountryName { get; }

		public string CannotConfigureRegistryForCountry(ZString companyCode)
		{
			return Res.GetString("c3882920-6426-42a9-ac6e-578bdbcf879e", "Cannot configure for {0} because a configuration already exists under '{1}' for the company {2}.", CountryName, RegistryItemExtensions.LocationOfLocalCountryCustomsInterface, companyCode);
		}
	}
}
