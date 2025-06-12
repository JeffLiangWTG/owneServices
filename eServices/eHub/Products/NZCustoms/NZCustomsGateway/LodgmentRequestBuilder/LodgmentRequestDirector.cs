using Common.Logging;

namespace CargoWise.eHub.Products.NZCustoms.Gateway
{
	class LodgmentRequestDirector
	{
		public static LodgmentRequestBuilder Create(ILog logger, string messageType, string authentication, string message)
		{
			if (messageType == MessageType.XmlWithAttachments.ToString())
			{
				return new LodgmentRequestBuilderForXmlWithAttachments(logger, message, authentication);
			}
			else if (messageType == MessageType.Xml.ToString())
			{
				return new LodgmentRequestBuilderForXml(logger, message);
			}
			else
			{
				return new LodgmentRequestBuilderForLegacy(logger, message);
			}
		}

		public enum MessageType
		{
			Text,
			Xml,
			XmlWithAttachments
		}
	}
}
