using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public sealed class DeclarationLockConfigCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DeclarationLockConfigCollection()
		{
		}

		public DeclarationLockConfigCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public DeclarationLockConfigCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new DeclarationLockConfig this[int i] => (DeclarationLockConfig)Elements[i];

		public new DeclarationLockConfig AddNew() => (DeclarationLockConfig)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new DeclarationLockConfig(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DeclarationLockConfigCollection(fallbackLevel, factory);
	}
}
