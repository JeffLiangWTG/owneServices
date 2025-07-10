using System.Xml.Serialization;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class ProjectCategoryMap : JiraEntityClassificationMap<ProjectCategoryMapCollection, ProjectCategoryMapItem>
	{
		protected override JiraEntityClassificationMap<ProjectCategoryMapCollection, ProjectCategoryMapItem> CreateNewMapForClone()
		{
			return new ProjectCategoryMap();
		}
	}
}
