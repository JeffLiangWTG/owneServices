using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public abstract class JiraEntityClassificationMapItemCollection<T> : RegistryBusinessObjectCollectionTemplate
		where T : JiraEntityClassificationMapItem, new()
	{
		public new T this[int index] => (T)Elements[index];

		public new T AddNew() => (T)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new T();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return CreateNewCollectionForClone();
		}

		protected abstract JiraEntityClassificationMapItemCollection<T> CreateNewCollectionForClone();
	}
}
