using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Packing.Business
{
	[XmlSerializerAssembly("Enterprise.Packing.Business.XmlSerializers")]
	[XmlRoot("Types")]
	public class PalletTypeCollection : RegistryBusinessObjectCollection
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PalletType();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PalletTypeCollection();
		}

		public new PalletType AddNew()
		{
			return (PalletType)base.AddNew();
		}

		public new PalletType this[int i]
		{
			get { return (PalletType)base[i]; }
		}
	}
}
