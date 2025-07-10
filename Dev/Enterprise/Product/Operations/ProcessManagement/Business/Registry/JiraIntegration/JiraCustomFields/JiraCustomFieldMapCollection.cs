using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class JiraCustomFieldMapCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new JiraCustomFieldMapItem this[int index] => (JiraCustomFieldMapItem)Elements[index];

		public new JiraCustomFieldMapItem AddNew() => (JiraCustomFieldMapItem)base.AddNew();

		protected JiraCustomFieldMapCollection CreateNewCollectionForClone()
		{
			return new JiraCustomFieldMapCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JiraCustomFieldMapItem();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return CreateNewCollectionForClone();
		}
	}
}
