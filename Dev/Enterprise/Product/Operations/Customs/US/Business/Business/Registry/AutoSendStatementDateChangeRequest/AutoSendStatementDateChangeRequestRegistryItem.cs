using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class AutoSendStatementDateChangeRequestRegistryItem : StronglyTypedRegistryItem<AutoSendStatementDateChangeRequest>
	{
		public AutoSendStatementDateChangeRequestRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(name, category, caption, hint, new AutoSendStatementDateChangeRequestRegistryDataType(), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default))
		{
		}
	}
}
