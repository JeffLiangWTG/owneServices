using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public sealed class DefaultCurrencyToLocalCurrencyRegistryItem : RegistryItemImpl
	{
		public DefaultCurrencyToLocalCurrencyRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlag)
			: base(name, category, caption, hint, RegistryDataTypes.BoolType, storageFlag)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var factory = new BusinessObjectFactory();
			var company = factory.Load<IGlbCompany>(companyPK);
			var country = company?.GC_RN_NKCountryCode ?? ZString.Empty;
			return
				country == Core.Constants.CountryCodes.UnitedStates ||
				country == Core.Constants.CountryCodes.PuertoRico ||
				country == Core.Constants.CountryCodes.VirginIslands ||
				country == Core.Constants.CountryCodes.NewZealand ||
				country == Core.Constants.CountryCodes.Singapore;
		}
	}
}
