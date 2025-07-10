using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.Business.XmlCredential
{
	public interface IXmlCredentialConfigurationHandler
	{
		bool Process(Configuration configuration);
	}
}
