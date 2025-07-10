using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection()
		{
		}

		public AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new AutoratingIntercompanyTariffsForGatewayJobConfiguration this[int index]
		{
			get { return (AutoratingIntercompanyTariffsForGatewayJobConfiguration)Elements[index]; }
		}

		public new AutoratingIntercompanyTariffsForGatewayJobConfiguration AddNew()
		{
			return (AutoratingIntercompanyTariffsForGatewayJobConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AutoratingIntercompanyTariffsForGatewayJobConfiguration();
		}
	}
}

