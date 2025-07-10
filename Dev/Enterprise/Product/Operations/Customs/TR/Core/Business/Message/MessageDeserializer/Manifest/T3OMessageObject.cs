using System.Xml;
using CargoWise.Common;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.TR.Business
{
	public class T3OMessageObject : SoapMessageObject
	{
		public T3OMessageObject(string messageText, TRBaseMessage message, LoggingInformation logger) : base(messageText, message, logger) { }

		protected override (string ElementName, string NamespaceUrl)[] InnerMessagePath => new (string, string)[]
		{
			(TRMessageConstants.Xml.DiffGramElementName, TRMessageConstants.Xml.DiffGramNamespace),
			(TRMessageConstants.Xml.ResultElementName, string.Empty),
		};

		public override bool IsValidMessage => true;

		protected override InnerMessageObjectBase GetInnerObjectFromElement(XmlElement element)
		{
			InnerMessageObjectBase result = null;

			if (element[TRMessageConstants.Xml.CustomsResponseElementName] is XmlElement responseXmlElement && XmlUtils.IsValidXml(responseXmlElement.InnerText, out var responseXmlDocument))
			{
				result = base.GetInnerObjectFromDocument(responseXmlDocument);
			}
			else if (element[InnerMessageObjectBase.Constants.DiffGramResultProcess] is XmlElement processElemen)
			{
				result = new InnerMessageObjectBase(processElemen.InnerText);
			}

			return result;
		}
	}
}
