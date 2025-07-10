using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class MaximumCreditLimitCollectionRegistryItem : StronglyTypedRegistryItem<MaximumCreditLimitCollection>
	{
		public MaximumCreditLimitCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOption, MaximumCreditLimitCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new MaximumCreditLimitDataType(), storage, registryOption, defaultValue))
		{
		}
	}
}
