using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RevenueRecognitionByChargeGroupRegistryItem : StronglyTypedRegistryItem<RevenueRecognitionByChargeGroupCollection>
	{
		public RevenueRecognitionByChargeGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RevenueRecognitionByChargeGroupCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RevenueRecognitionByChargeGroupRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.RevenueRecognitionByChargeGroupRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class RevenueRecognitionByChargeGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<RevenueRecognitionByChargeGroupCollection>
	{
		public RevenueRecognitionByChargeGroupRegistryDataType()
		{
		}
	}
}
