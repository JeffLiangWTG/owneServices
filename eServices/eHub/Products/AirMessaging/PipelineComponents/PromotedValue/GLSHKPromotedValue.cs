namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	class GLSHKPromotedValue : PromotedValue
	{
		public GLSHKPromotedValue()
		{
			Add("SenderPIMA", "/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='InterchangeHeader' and namespace-uri()='']/*[local-name()='Sender' and namespace-uri()='']/*[local-name()='PIMA' and namespace-uri()='']");
			Add("RecipientPIMA", "/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='InterchangeHeader' and namespace-uri()='']/*[local-name()='Recipient' and namespace-uri()='']/*[local-name()='PIMA' and namespace-uri()='']");
			Add("InternalMessage", "/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='Body' and namespace-uri()='']/*[local-name()='Message' and namespace-uri()='']");
			Add("MessageType", "/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='MessageHeader' and namespace-uri()='']/*[local-name()='MessageType' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']");
			Add("MessageVersion", "/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='MessageHeader' and namespace-uri()='']/*[local-name()='MessageType' and namespace-uri()='']/*[local-name()='Version' and namespace-uri()='']");
			Add("Reference", "/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='MessageHeader' and namespace-uri()='']/*[local-name()='CommonAccessReference' and namespace-uri()='']/*[local-name()='Reference' and namespace-uri()='']");
		}
	}
}
