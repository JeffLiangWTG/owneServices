using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business.JiraIntegration;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public class JiraAttachmentDecoder : DocumentDecoder
	{
		public JiraAttachmentDecoder(IDocumentFactory factory, JiraCredentials credentials, IJiraImporterProgressTracker progressTracker)
			: base(factory)
		{
			Credentials = credentials;
			ProgressTracker = progressTracker;
		}

#if DEBUG
		public
#endif
		JiraCredentials Credentials
		{ get; }
		IJiraImporterProgressTracker ProgressTracker { get; }

		protected override byte[] GetDocData(JiraAttachment attachment)
		{
			return JiraWebHelper.DownloadFile(Credentials, attachment.ContentURL);
		}

		protected override void ProcessDocument(JiraAttachment attachment)
		{
			ProgressTracker.UpdateCurrentLowLevelActivity(ResString.GetMultilingualString("7f94bd57-2e6d-4037-91fa-d079f2530aee", "Downloading attachment {0}", attachment.FileName));
			var fileContent = GetDocData(attachment);

			if (fileContent != null)
			{
				var doc = GetDocAddedToDocManager(fileContent, attachment);

				if (doc != null && attachment.CreateTime.IsValid)
				{
					doc.DateAdded = attachment.CreateTime;
				}
			}
		}
	}
}
