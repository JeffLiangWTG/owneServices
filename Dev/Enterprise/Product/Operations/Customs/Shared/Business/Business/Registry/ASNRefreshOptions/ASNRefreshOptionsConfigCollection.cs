using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public class ASNRefreshOptionsConfigCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ASNRefreshOptionsConfigCollection()
		{ }

		public ASNRefreshOptionsConfigCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ASNRefreshOptionsConfig this[int i] => (ASNRefreshOptionsConfig)Elements[i];

		public new ASNRefreshOptionsConfig AddNew()
		{
			return (ASNRefreshOptionsConfig)base.AddNew();
		}

		#region Override

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ASNRefreshOptionsConfig(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ASNRefreshOptionsConfigCollection(fallbackLevel, factory);
		}

		#endregion
	}
}
