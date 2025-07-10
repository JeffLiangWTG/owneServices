using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class AutoBillingGroupNotificationRegistryItem : StronglyTypedRegistryItem<AutoBillingGroupNotification>
	{
		public AutoBillingGroupNotificationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			   : base(new RegistryItemImpl(name, category, caption, hint, new AutoBillingGroupNotificationRegistryItemDataType(), storage, RegistryOptions.Default))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.DataRegistry.GUI.AutoBillingGroupNotificationRegistryItemEditor, Enterprise.Customs.GUI")]
	public class AutoBillingGroupNotificationRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<AutoBillingGroupNotification>
	{
	}
}
