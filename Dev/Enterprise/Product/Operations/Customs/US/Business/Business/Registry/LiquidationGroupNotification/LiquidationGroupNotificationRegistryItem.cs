using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class LiquidationGroupNotificationRegistryItem : StronglyTypedRegistryItem<LiquidationGroupNotification>
	{
		public LiquidationGroupNotificationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, LiquidationGroupNotification defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new LiquidationGroupNotificationRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
