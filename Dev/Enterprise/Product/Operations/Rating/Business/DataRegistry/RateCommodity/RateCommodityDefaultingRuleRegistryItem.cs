using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public class RateCommodityDefaultingRuleRegistryItem : StronglyTypedRegistryItem<RateCommodityDefaultingRuleCollection>
	{
		public RateCommodityDefaultingRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new RateCommodityDefaultingRuleRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.RateCommodityDefaultingRuleRegistryItemEditor, Enterprise.Rating.GUI")]
	class RateCommodityDefaultingRuleRegistryDataType : NonPersistentBusinessObjectRegistryDataType<RateCommodityDefaultingRuleCollection>
	{
		public RateCommodityDefaultingRuleRegistryDataType()
		{
		}
	}
}

