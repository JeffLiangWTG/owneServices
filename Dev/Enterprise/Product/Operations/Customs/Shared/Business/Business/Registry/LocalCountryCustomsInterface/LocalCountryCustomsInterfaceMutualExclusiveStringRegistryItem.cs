using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem : StronglyTypedRegistryItem<string>, IRegistryItemWithOtherChangedItems
	{
		public LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options = RegistryOptions.Default)
			: base(new LocalCountryCustomsInterfaceMutualExclusiveStringImpl(name, category, caption, hint, storage, options))
		{
		}

		public IEnumerable<IRegistryItemInternals> OtherChangedItems { get; set; }

		class LocalCountryCustomsInterfaceMutualExclusiveStringImpl : NonBuiltInOnlyCountryRegistryItemImpl
		{
			public LocalCountryCustomsInterfaceMutualExclusiveStringImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new LocalCountryCustomsInterfaceMutualExclusiveStringRegistryDataType(), storage, options, "")
			{
			}

			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				var countryCode = (string)Db.Connection.ExecuteScalar("select GC_RN_NKCountryCode from dbo.GlbCompany where GC_PK = '" + companyPK.ToString() + "'");
				return IntegratedCountryHelper.CustomsWareInstallations(countryCode);
			}
		}
	}

	public class LocalCountryCustomsInterfaceMutualExclusiveStringRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			registryItem.AddErrorIfCustomsInterfaceConfigured(companyPK);
		}
	}
}
