using System.Xml;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.TR.Business
{
	public class TROMessageObject : SoapMessageObject
	{
		static class Constants
		{
			public const string Response = nameof(Response);
			public const string Error = nameof(Error);
		}

		public TROMessageObject(string messageText, TRBaseMessage message, LoggingInformation logger) : base(messageText, message, logger) { }

		public override bool IsValidMessage => true;

		protected override (string ElementName, string NamespaceUrl)[] InnerMessagePath => new (string, string)[]
		{
			(TRMessageConstants.Xml.SummaryDeclarationResponseElementName, TRMessageConstants.Xml.CustomsBizTalkNamespace),
			(Constants.Response, string.Empty)
		};

		protected override InnerMessageObjectBase GetInnerObjectFromElement(XmlElement element) => new SOAPLevelRefIDAndGuidObject(element);
	}
}
