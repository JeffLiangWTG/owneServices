using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryItem : StronglyTypedRegistryItem<bool>, IRegistryItemWithOtherChangedItems
	{
		public LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, bool defaultValue = false)
			: base(new LocalCountryCustomsInterfaceMutualExclusiveBooleanImpl(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue))
		{
		}

		public IEnumerable<IRegistryItemInternals> OtherChangedItems { get; set; }

		class LocalCountryCustomsInterfaceMutualExclusiveBooleanImpl : NonBuiltInOnlyCountryRegistryItemImpl
		{
			public LocalCountryCustomsInterfaceMutualExclusiveBooleanImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue)
				: base(name, category, caption, hint, new LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryDataType(), storage, options, defaultValue)
			{
			}

			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				var countryCode = (string)Db.Connection.ExecuteScalar("select GC_RN_NKCountryCode from dbo.GlbCompany where GC_PK = '" + companyPK.ToString() + "'");
				return IntegratedCountryHelper.CustomsWareInstallations(countryCode);
			}
		}
	}

	public class LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryDataType : BooleanRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, bool proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			registryItem.AddErrorIfCustomsInterfaceConfigured(companyPK);
		}
	}
}
