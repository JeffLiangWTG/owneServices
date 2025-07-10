using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business
{
	partial class ContainerDetention : IEDocsProvider
	{
		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return new DocManagerInfo(this, Constants.DocManagerCodes.DetentionInvoice); }
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new ContainerDetentionDocumentSupporter(this); }
		}

		#endregion
	}
}
