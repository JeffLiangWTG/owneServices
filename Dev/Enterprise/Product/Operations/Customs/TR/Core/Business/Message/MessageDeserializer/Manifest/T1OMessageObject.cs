using System.Xml;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.TR.Business
{
	public class T1OMessageObject : SoapMessageObject
	{
		public T1OMessageObject(string messageText, TRBaseMessage message, LoggingInformation logger) : base(messageText, message, logger) { }

		public override bool IsValidMessage => true;

		protected override InnerMessageObjectBase GetInnerObjectFromDocument(XmlDocument innerDocument) =>
			InnerXmlRegistrationNumberObject.IsMatch(innerDocument) ? new InnerXmlRegistrationNumberObject(innerDocument) : base.GetInnerObjectFromDocument(innerDocument);
	}
}
