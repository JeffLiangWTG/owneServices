using System.Xml;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.TR.Business
{
	public class TRIMessageObject : SoapMessageObject
	{
		public TRIMessageObject(string messageText, TRBaseMessage message, LoggingInformation logger) : base(messageText, message, logger)
		{
		}

		public override bool IsValidMessage => true;

		protected override (string ElementName, string NamespaceUrl)[] InnerMessagePath => new (string, string)[]
		{
			(TRMessageConstants.Xml.InspectionClerkQueryResponse , TRMessageConstants.Xml.TempuriNamespace),
		};

		protected override InnerMessageObjectBase GetInnerObjectFromDocument(XmlDocument innerXmlDocument) => new TRIMessageResponseObject(innerXmlDocument);
	}
}
