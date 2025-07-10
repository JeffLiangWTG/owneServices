using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.MasterFiles.Business
{
	public interface IEDocsProvider : IDocumentSupportable, IDocManagerSupport
	{
		EDocsProviderSupporter GetEDocsProviderSupporter();
	}
}
