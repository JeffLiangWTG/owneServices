using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class OnHoldTermsRegistryItem : StronglyTypedRegistryItem<OnHoldTerms>
	{
		public OnHoldTermsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new OnHoldTermsRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.OnHoldTermsRegistryItemEditor, Enterprise.Registry.GUI")]
	class OnHoldTermsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OnHoldTerms>
	{
		public OnHoldTermsRegistryDataType()
		{
		}
	}
}
