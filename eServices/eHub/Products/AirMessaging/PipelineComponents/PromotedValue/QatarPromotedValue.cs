namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	class QatarPromotedValue : PromotedValue
	{
		public QatarPromotedValue()
		{
			Add("SenderAddress", "/*[local-name()='Qatar' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.QatarReplyMessage']/*[local-name()='Header1' and namespace-uri()='']/*[local-name()='SenderAddress' and namespace-uri()='']");
			Add("MessagePriority", "/*[local-name()='Qatar' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.QatarReplyMessage']/*[local-name()='Header1' and namespace-uri()='']/*[local-name()='MessagePriority' and namespace-uri()='']");
			Add("RecipientAddress", "/*[local-name()='Qatar' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.QatarReplyMessage']/*[local-name()='Header2' and namespace-uri()='']/*[local-name()='RecipientAddress' and namespace-uri()='']");
			Add("RecipientPIMA", "/*[local-name()='Qatar' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.QatarReplyMessage']/*[local-name()='Header2' and namespace-uri()='']/*[local-name()='ClientPIMA' and namespace-uri()='']");
			Add("InternalMessage", "/*[local-name()='Qatar' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.QatarReplyMessage']/*[local-name()='Data' and namespace-uri()='']");
			Add("Reference", "/*[local-name()='Qatar' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.QatarReplyMessage']/*[local-name()='Header2' and namespace-uri()='']/*[local-name()='Signature-Date' and namespace-uri()='']");
		}
	}
}
