namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	class CargonautPromotedValue : PromotedValue
	{
		public CargonautPromotedValue()
		{
			Add("ClientPIMA", "/*[local-name()='Cargonaut' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.CargonautReplyMessage']/*[local-name()='Header' and namespace-uri()='']/*[local-name()='ClientPIMA' and namespace-uri()='']");
            Add("MessageType", "/*[local-name()='Cargonaut' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.CargonautReplyMessage']/*[local-name()='Header' and namespace-uri()='']/*[local-name()='MessageType' and namespace-uri()='']");
			Add("MessageVersion", "/*[local-name()='Cargonaut' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.CargonautReplyMessage']/*[local-name()='Header' and namespace-uri()='']/*[local-name()='MessageVersion' and namespace-uri()='']");
			Add("InternalMessage", "/*[local-name()='Cargonaut' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.CargonautReplyMessage']/*[local-name()='InternalMessage' and namespace-uri()='']");
		}
	}
}
