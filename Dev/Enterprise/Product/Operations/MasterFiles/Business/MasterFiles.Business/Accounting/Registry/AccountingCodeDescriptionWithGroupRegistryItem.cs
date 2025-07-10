using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class AccountingCodeDescriptionWithGroupRegistryItem<T, T2> : CodeDescriptionWithGroupRegistryItem
		where T : CodeDescriptionWithGroupCollection
		where T2 : CodeDescriptionWithGroup
	{
		public AccountingCodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionWithGroupRegistryEditorInfo editorInfo, T defaultValue, Func<CodeDescriptionWithGroupCollection, T> valueConverter)
			: base(new RegistryItemImpl(name, category, caption, hint, new AccountingCodeDescriptionWithGroupRegistryDataType<T>(), storage, options, defaultValue), editorInfo)
		{
			ValueConverter = Argument.NotNull(valueConverter, nameof(valueConverter));
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			=> ValueConverter(base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as CodeDescriptionWithGroupCollection);

		Func<CodeDescriptionWithGroupCollection, T> ValueConverter { get; }
	}

	public sealed class AccountingCodeDescriptionWithGroupRegistryDataType<T> : NonPersistentBusinessObjectRegistryDataType<T>
		where T : CodeDescriptionWithGroupCollection
	{ }
}