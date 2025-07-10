using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruitment.Registry
{
	[XmlSerializerAssembly("Enterprise.Recruitment.Registry.XmlSerializers")]
	public class WorkItemTemplatePropertiesCollection : RegistryBusinessObjectCollectionTemplate
	{
		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new WorkItemTemplatePropertiesCollection();

		public new WorkItemTemplateProperties this[int i] => (WorkItemTemplateProperties)base[i];

		public WorkItemTemplateProperties Add(string friendlyName) => Add(friendlyName, ZGuid.Empty);

		public WorkItemTemplateProperties Add(string friendlyName, ZGuid pk)
		{
			var result = AddNew();
			result.FriendlyName = friendlyName;
			result.WKI_PK = pk;
			return result;
		}
		public new WorkItemTemplateProperties AddNew() => (WorkItemTemplateProperties)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new WorkItemTemplateProperties();
	}
}
