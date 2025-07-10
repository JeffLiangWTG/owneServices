using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class SupplyTypeConfigurationCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public SupplyTypeConfigurationCollection()
		{
		}

		public SupplyTypeConfigurationCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new SupplyTypeConfiguration this[int i]
		{
			get { return (SupplyTypeConfiguration)Elements[i]; }
		}

		public new SupplyTypeConfiguration AddNew()
		{
			return (SupplyTypeConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SupplyTypeConfigurationCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SupplyTypeConfiguration(CurrentFallbackLevel);
		}
	}
}
