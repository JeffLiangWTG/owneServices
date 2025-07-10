using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class SupplyTypeConfigurationByChargeGroupCollection : RegistryBusinessObjectCollectionTemplate
	{
		public SupplyTypeConfigurationByChargeGroupCollection()
		{
		}

		public SupplyTypeConfigurationByChargeGroupCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new SupplyTypeConfigurationByChargeGroup this[int i]
		{
			get { return (SupplyTypeConfigurationByChargeGroup)Elements[i]; }
		}

		public new SupplyTypeConfigurationByChargeGroup AddNew()
		{
			return (SupplyTypeConfigurationByChargeGroup)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SupplyTypeConfigurationByChargeGroupCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SupplyTypeConfigurationByChargeGroup(CurrentFallbackLevel);
		}
	}
}
