using System.ServiceModel;

namespace Enterprise.Services.ServiceHost
{
	// NOTE: If you change the interface name "IOrganizationMergeService" here, you must also update the reference to "IOrganizationMergeService" in Web.config.
	[ServiceContract]
	public interface IOrganizationMerge
	{
		[OperationContract]
		string MergeFromXmlAsUser(string username, string password, string xml);

		[OperationContract]
		string MergeAsUser(string username, string password, string[] oldOrgCodes, string newOrgCode);
	}
}
