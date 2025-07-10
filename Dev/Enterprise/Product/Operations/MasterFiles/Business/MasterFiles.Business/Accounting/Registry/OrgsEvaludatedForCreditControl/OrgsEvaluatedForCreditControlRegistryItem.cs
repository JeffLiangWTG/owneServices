using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class OrgsEvaluatedForCreditControlRegistryItem : StronglyTypedRegistryItem<OrgsEvaluatedForCreditControlCollection>
	{
		public OrgsEvaluatedForCreditControlRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, OrgsEvaluatedForCreditControlCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new OrgsEvaluatedForCreditControlRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.OrgsEvaluatedForCreditControlRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	public class OrgsEvaluatedForCreditControlRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OrgsEvaluatedForCreditControlCollection>
	{
		public OrgsEvaluatedForCreditControlRegistryDataType()
		{
		}
	}
}
