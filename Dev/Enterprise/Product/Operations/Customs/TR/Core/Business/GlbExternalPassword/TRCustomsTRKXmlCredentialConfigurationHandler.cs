using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.XmlCredential;

namespace Enterprise.Customs.TR.Business
{
	public class TRCustomsTRKXmlCredentialConfigurationHandler : GlbExternalPasswordConfigurationHandler
	{
		public TRCustomsTRKXmlCredentialConfigurationHandler(LoggingInformation logger)
		: base(logger)
		{ }
	}
}
