using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class GatewayChargeDefaultDebtorConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public GatewayChargeDefaultDebtorConfigurationCollection()
		{
		}

		public GatewayChargeDefaultDebtorConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new GatewayChargeDefaultDebtorConfiguration this[int index]
		{
			get { return (GatewayChargeDefaultDebtorConfiguration)Elements[index]; }
		}

		public new GatewayChargeDefaultDebtorConfiguration AddNew()
		{
			return (GatewayChargeDefaultDebtorConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GatewayChargeDefaultDebtorConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GatewayChargeDefaultDebtorConfiguration();
		}
	}
}
