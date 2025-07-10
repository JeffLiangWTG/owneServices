using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.Business
{
	public class StampDutyLedgerNumberCustomizationRegistryItem : StronglyTypedRegistryItem<StampDutyLedgerNumberCustomizationRegistrySetting>
	{
		public StampDutyLedgerNumberCustomizationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, StampDutyLedgerNumberCustomizationRegistrySetting defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new StampDutyLedgerNumberCustomizationRegistryItemDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.TR.GUI.StampDutyLedgerNumberCustomizationRegistryItemEditor, Enterprise.Customs.TR.GUI")]
	public class StampDutyLedgerNumberCustomizationRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<StampDutyLedgerNumberCustomizationRegistrySetting>
	{
	}
}
