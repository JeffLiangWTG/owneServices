using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class CashFlowActivityConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CashFlowActivityConfigurationCollection()
		{
		}

		public CashFlowActivityConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new CashFlowActivityConfiguration this[int x]
		{
			get { return (CashFlowActivityConfiguration)base[x]; }
		}

		public new CashFlowActivityConfiguration AddNew()
		{
			var configuration = (CashFlowActivityConfiguration)base.AddNew();
			configuration.SetCustomizedDataCaptionSource(AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration);
			return configuration;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CashFlowActivityConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CashFlowActivityConfiguration();
		}
	}
}
