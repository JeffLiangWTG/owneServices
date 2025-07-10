using System.Xml.Serialization;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class IssueTypeMap : JiraEntityClassificationMap<IssueTypeMapCollection, IssueTypeMapItem>
	{
		protected override JiraEntityClassificationMap<IssueTypeMapCollection, IssueTypeMapItem> CreateNewMapForClone()
		{
			return new IssueTypeMap();
		}
	}
}
