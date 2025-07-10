using System.Xml;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.TR.Business
{
	public class DT3MessageObject : SoapMessageObject
	{
		public DT3MessageObject(string messageText, TRBaseMessage message, LoggingInformation logger) : base(messageText, message, logger) { }
		public override bool IsValidMessage => true;
		protected override InnerMessageObjectBase GetInnerObjectFromDocument(XmlDocument innerDocument) => new InnerXmlRegisterAnswerObject(innerDocument);
	}
}
