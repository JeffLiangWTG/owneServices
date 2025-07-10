using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class DefaultStatementPrintDateRegistryItem : StronglyTypedRegistryItem<DefaultStatementPrintDate>
	{
		public DefaultStatementPrintDateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultStatementPrintDateRegistryDataType(), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter))
		{
		}
	}
}
