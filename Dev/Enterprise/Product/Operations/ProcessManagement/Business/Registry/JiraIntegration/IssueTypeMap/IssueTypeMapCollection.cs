using System.Xml.Serialization;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class IssueTypeMapCollection : JiraEntityClassificationMapItemCollection<IssueTypeMapItem>
	{
		protected override JiraEntityClassificationMapItemCollection<IssueTypeMapItem> CreateNewCollectionForClone()
		{
			return new IssueTypeMapCollection();
		}
	}
}
