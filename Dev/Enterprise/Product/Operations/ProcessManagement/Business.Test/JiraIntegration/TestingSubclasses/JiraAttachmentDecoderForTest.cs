using System;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class JiraAttachmentDecoderForTest : JiraAttachmentDecoder
	{
		public JiraAttachmentDecoderForTest(IDocumentFactory factory, JiraCredentials credentials, IJiraImporterProgressTracker progressTracker)
			: base(factory, credentials, progressTracker)
		{
		}

		public Func<JiraAttachment, byte[]> IssueAttachmentContentGetter { get; set; }

		protected override byte[] GetDocData(JiraAttachment attachment)
		{
			return IssueAttachmentContentGetter != null
				? IssueAttachmentContentGetter.Invoke(attachment)
				: base.GetDocData(attachment);
		}
	}
}
