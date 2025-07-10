using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.NO.Business.XmlSerializers")]
	public class NodiRegistryCollection : RegistryBusinessObjectCollectionTemplate<NodiRegistry>
	{
		public NodiRegistryCollection()
			: this(null, null)
		{
		}

		public NodiRegistryCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public NodiRegistryCollection DefaultCollection
		{
			get
			{
				if (defaultCollection == null)
				{
					defaultCollection = new NodiRegistryCollection()
					{
						new NodiRegistry() { SystemName = NodiRegistry.CustomsProductionSystemName, NodiNumber = NodiRegistry.CustomsProductionDefaultNodiNumber },
						new NodiRegistry() { SystemName = NodiRegistry.CustomsTestSystemName, NodiNumber = NodiRegistry.CustomsTestDefaultNodiNumber },
						new NodiRegistry() { SystemName = NodiRegistry.NctsProductionSystemName, NodiNumber = NodiRegistry.NctsProductionDefaultNodiNumber },
						new NodiRegistry() { SystemName = NodiRegistry.NctsTestSystemName, NodiNumber = NodiRegistry.NctsTestDefaultNodiNumber }
					};
				}
				return defaultCollection;
			}
		}
		NodiRegistryCollection defaultCollection;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new NodiRegistry(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new NodiRegistryCollection(fallbackLevel, factory);

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
