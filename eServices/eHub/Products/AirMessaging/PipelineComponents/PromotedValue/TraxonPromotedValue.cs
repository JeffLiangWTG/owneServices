
namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	class TraxonPromotedValue : PromotedValue
	{
		public TraxonPromotedValue()
		{
			Add("SenderPIMA", "/*[local-name()='Message' and namespace-uri()='http://CargoWise.eHub.Products.Traxon.Schemas.TraxonReplyMessage']/*[local-name()='InterchangeHeader' and namespace-uri()='']/*[local-name()='Sender' and namespace-uri()='']/*[local-name()='ID' and namespace-uri()='']");
			Add("RecipientPIMA", "/*[local-name()='Message' and namespace-uri()='http://CargoWise.eHub.Products.Traxon.Schemas.TraxonReplyMessage']/*[local-name()='InterchangeHeader' and namespace-uri()='']/*[local-name()='Recipient' and namespace-uri()='']/*[local-name()='ID' and namespace-uri()='']");
			Add("MessageType", "/*[local-name()='Message' and namespace-uri()='http://CargoWise.eHub.Products.Traxon.Schemas.TraxonReplyMessage']/*[local-name()='MessageHeader' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']");
			Add("MessageVersion", "/*[local-name()='Message' and namespace-uri()='http://CargoWise.eHub.Products.Traxon.Schemas.TraxonReplyMessage']/*[local-name()='MessageHeader' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']/*[local-name()='Version' and namespace-uri()='']");
			Add("InternalMessage", "/*[local-name()='Message' and namespace-uri()='http://CargoWise.eHub.Products.Traxon.Schemas.TraxonReplyMessage']/*[local-name()='Data' and namespace-uri()='']");
		}
	}
}
