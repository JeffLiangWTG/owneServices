using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceSubTypeAttributionRuleConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceSubTypeAttributionRuleConfigurationCollection()
		{
		}

		public ComplianceSubTypeAttributionRuleConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ComplianceSubTypeAttributionRuleConfiguration this[int x]
		{
			get { return (ComplianceSubTypeAttributionRuleConfiguration)base[x]; }
		}

		public new ComplianceSubTypeAttributionRuleConfiguration AddNew()
		{
			return (ComplianceSubTypeAttributionRuleConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceSubTypeAttributionRuleConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceSubTypeAttributionRuleConfiguration();
		}
	}
}
