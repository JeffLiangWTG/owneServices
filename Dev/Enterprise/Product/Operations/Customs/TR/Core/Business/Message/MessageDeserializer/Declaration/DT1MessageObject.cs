using System.Xml;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.TR.Business
{
	public class DT1MessageObject : SoapMessageObject
	{
		public DT1MessageObject(string messageText, TRBaseMessage message, LoggingInformation logger) : base(messageText, message, logger) { }
		public override bool IsValidMessage => true;
		protected override InnerMessageObjectBase GetInnerObjectFromDocument(XmlDocument innerDocument) => new InnerXmlRegisterAnswerObject(innerDocument);
	}
}
