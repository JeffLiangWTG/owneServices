using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class SupplyTypeConfigurationByChargeGroup : ChargeGroupSetup<SupplyTypeConfigurationCollection>
	{
		public SupplyTypeConfigurationByChargeGroup()
			: base()
		{
		}

		public SupplyTypeConfigurationByChargeGroup(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SupplyTypeConfigurationByChargeGroup(fallbackLevel);
		}
	}
}
