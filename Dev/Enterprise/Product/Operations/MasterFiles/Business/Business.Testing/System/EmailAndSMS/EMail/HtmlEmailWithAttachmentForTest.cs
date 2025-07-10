using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class HtmlEmailWithAttachmentForTest : HtmlEmailWithAttachment
	{
		public HtmlEmailWithAttachmentForTest(ISendEmailSource source)
			: base(source)
		{
		}

		public string ParseMessage_Exposed(ZString message)
		{
			return base.ParseMessage(message);
		}

		public string NormaliseWhitespaceCharactersForHtml_Exposed(ZString text)
		{
			return base.NormaliseWhitespaceCharactersForHtml(text);
		}

		public string NormaliseNewLine_Exposed(ZString text)
		{
			return base.NormaliseNewLine(text);
		}
	}
}
