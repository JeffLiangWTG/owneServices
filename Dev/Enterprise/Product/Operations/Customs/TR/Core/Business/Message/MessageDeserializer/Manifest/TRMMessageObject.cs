using System.Xml;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.TR.Business
{
	public class TRMMessageObject : SoapMessageObject
	{
		public TRMMessageObject(string messageText, TRBaseMessage message, LoggingInformation logger) : base(messageText, message, logger) { }

		public override bool IsValidMessage => true;

		protected override (string ElementName, string NamespaceUrl)[] InnerMessagePath => new (string, string)[]
		{
			(TRMessageConstants.Xml.DiffGramElementName, TRMessageConstants.Xml.DiffGramNamespace),
			(TRMessageConstants.Xml.ResultElementName, string.Empty),
		};

		protected override InnerMessageObjectBase GetInnerObjectFromElement(XmlElement element) => new InnerXmlInspectionClerkObject(element);
	}
}
