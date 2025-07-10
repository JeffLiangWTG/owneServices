using System.Xml.Serialization;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class ProjectCategoryMapCollection : JiraEntityClassificationMapItemCollection<ProjectCategoryMapItem>
	{
		protected override JiraEntityClassificationMapItemCollection<ProjectCategoryMapItem> CreateNewCollectionForClone()
		{
			return new ProjectCategoryMapCollection();
		}
	}
}
