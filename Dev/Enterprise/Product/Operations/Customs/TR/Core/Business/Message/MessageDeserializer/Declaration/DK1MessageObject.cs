using System.Xml;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.TR.Business
{
	public class DK1MessageObject : SoapMessageObject
	{
		public DK1MessageObject(string messageText, TRBaseMessage message, LoggingInformation logger) : base(messageText, message, logger) { }
		public override bool IsValidMessage => true;
		protected override InnerMessageObjectBase GetInnerObjectFromDocument(XmlDocument innerDocument) => new InnerXmlControlAnswerObject(innerDocument);
	}
}
