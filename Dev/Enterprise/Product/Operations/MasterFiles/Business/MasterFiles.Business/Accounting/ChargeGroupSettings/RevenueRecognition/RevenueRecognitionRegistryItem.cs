using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RevenueRecognitionRegistryItem : StronglyTypedRegistryItem<RevenueRecognitionCollection>
	{
		public RevenueRecognitionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RevenueRecognitionCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RevenueRecognitionRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.RevenueRecognitionRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class RevenueRecognitionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<RevenueRecognitionCollection>
	{
		public RevenueRecognitionRegistryDataType()
		{
		}
	}
}
