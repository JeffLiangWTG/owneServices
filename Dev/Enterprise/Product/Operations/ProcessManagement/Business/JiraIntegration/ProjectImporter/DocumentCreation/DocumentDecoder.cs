using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ProcessManagement.Business.JiraIntegration
{
	public abstract class DocumentDecoder
	{
		protected DocumentDecoder(IDocumentFactory docFactory)
		{
			BatchDocFactory = docFactory;
		}

		public void ProcessDocumentsAndAddToWorkItem(WorkItem workItem, List<JiraAttachment> attachments)
		{
			ConfigureDecoderToCurrentWorkItem(workItem, attachments);
			ProcessDocumentsAndAddToWorkItemCore();
		}

		protected void ProcessDocumentsAndAddToWorkItemCore()
		{
			foreach (var attachment in DocsToDecode)
			{
				ProcessDocument(attachment);
			}
		}

		protected IeDoc GetDocAddedToDocManager(byte[] fileContent, JiraAttachment attachment)
		{
			return DocManagerInfo.AddFileOrDocument(fileContent, attachment.FileName, documentType: Core.Constants.RefDocTypes.MiscellaneousDocument);
		}

#if DEBUG
		public
#endif
		IDocumentFactory BatchDocFactory
		{ get; }
		DocManagerInfo DocManagerInfo { get; set; }
		List<JiraAttachment> DocsToDecode { get; set; }
		protected WorkItem WorkItemToPopulate { get; set; }

		void ConfigureDecoderToCurrentWorkItem(WorkItem workItem, List<JiraAttachment> attachments)
		{
			DocManagerInfo = workItem.DocManagerInfo;
			DocManagerInfo.MasterFactory = BatchDocFactory;

			DocsToDecode = attachments;
			WorkItemToPopulate = workItem;
		}

		protected abstract byte[] GetDocData(JiraAttachment attachment);
		protected abstract void ProcessDocument(JiraAttachment attachment);
	}
}
