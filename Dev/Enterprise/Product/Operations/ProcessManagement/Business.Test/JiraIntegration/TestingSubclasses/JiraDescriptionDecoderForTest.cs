using Enterprise.MasterFiles.Integration;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class JiraDescriptionDecoderForTest : JiraDescriptionDecoder
	{
		public JiraDescriptionDecoderForTest(IDocumentFactory docFactory)
			: base(docFactory)
		{
		}

		public bool ReturnBadDocData { get; set; }

		protected override byte[] GetDocData(JiraAttachment attachment)
		{
			return ReturnBadDocData
				? null
				: base.GetDocData(attachment);
		}
	}
}
