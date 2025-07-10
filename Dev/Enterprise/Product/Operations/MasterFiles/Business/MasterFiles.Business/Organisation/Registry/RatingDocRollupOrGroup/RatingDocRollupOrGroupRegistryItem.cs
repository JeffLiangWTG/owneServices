using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RatingDocRollupOrGroupRegistryItem : StronglyTypedRegistryItem<RatingDocRollupOrGroupRegistryCollection>
	{
		public RatingDocRollupOrGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, RatingDocRollupOrGroupRegistryCollection chargeGroupingAndRollUp)
			: base(new RegistryItemImpl(name, category, caption, hint, new RatingDocRollupOrGroupDataType(), storage, options, chargeGroupingAndRollUp))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.RatingDocumentsChargeGroupingAndRollUpRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	public class RatingDocRollupOrGroupDataType : NonPersistentBusinessObjectRegistryDataType<RatingDocRollupOrGroupRegistryCollection>
	{
		public RatingDocRollupOrGroupDataType()
		{
		}
	}
}
