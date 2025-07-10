using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class PackageVersionOverrideCollection : RegistryBusinessObjectCollection
	{
		public new PackageVersionOverride this[int index] => (PackageVersionOverride)Elements[index];

		public new PackageVersionOverride AddNew() => (PackageVersionOverride)base.AddNew();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new PackageVersionOverrideCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new PackageVersionOverride(this);
	}
}
