using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Telematics.Business.Registry
{
	public class OverspeedAlertRegistryItem : StronglyTypedRegistryItem<OverspeedAlertConfiguration>
	{
		public OverspeedAlertRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
		: base(new RegistryItemImpl(name, category, caption, hint, new OverspeedAlertRegistryDataType(), storage, option))
		{
		}
	}

	[RegistryEditor("Enterprise.Telematics.GUI.Registry.OverspeedAlertRegistryItemEditor, Enterprise.Telematics.GUI")]
	public class OverspeedAlertRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OverspeedAlertConfiguration>
	{
		public OverspeedAlertRegistryDataType()
		{
		}
	}
}
