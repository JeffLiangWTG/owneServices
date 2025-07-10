using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class GatewayChargeDefaultInvoiceTargetJobConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public GatewayChargeDefaultInvoiceTargetJobConfigurationCollection()
		{
		}

		public GatewayChargeDefaultInvoiceTargetJobConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new GatewayChargeDefaultInvoiceTargetJobConfiguration this[int index]
		{
			get { return (GatewayChargeDefaultInvoiceTargetJobConfiguration)Elements[index]; }
		}

		public new GatewayChargeDefaultInvoiceTargetJobConfiguration AddNew()
		{
			return (GatewayChargeDefaultInvoiceTargetJobConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GatewayChargeDefaultInvoiceTargetJobConfiguration();
		}
	}
}
