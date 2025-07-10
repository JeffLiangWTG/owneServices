using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class RatingDocRollupOrGroupRegistryCollection : RegistryBusinessObjectCollectionTemplate
	{
		public RatingDocRollupOrGroupRegistryCollection()
		{
		}

		public RatingDocRollupOrGroupRegistryCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new RatingDocRollupOrGroupRegistry this[int x] => (RatingDocRollupOrGroupRegistry)Elements[x];

		public new RatingDocRollupOrGroupRegistry AddNew() => (RatingDocRollupOrGroupRegistry)base.AddNew();

		protected override bool AllowRemoveCore => Count > 1 && base.AllowRemoveCore;

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new RatingDocRollupOrGroupRegistryCollection(fallbackLevel, factory);

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new RatingDocRollupOrGroupRegistry(CurrentFallbackLevel, CurrentFactory);
	}
}
