using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class TaxIdAndTaxMessageCombinationRulesRegistryItem : StronglyTypedRegistryItem<TaxIdAndTaxMessageCombinationRulesConfiguration>
	{
		public TaxIdAndTaxMessageCombinationRulesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new TaxIdAndTaxMessageCombinationRulesRegistryItemDataType(), storage, options, new TaxIdAndTaxMessageCombinationRulesConfiguration()))
		{
		}
	}
}
