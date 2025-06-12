namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	class ARINCPromotedValue : PromotedValue
	{
		public ARINCPromotedValue()
		{
			Add("Priority-SenderAddress", "/*[local-name()='ARINC' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.ARINCReplyMessage']/*[local-name()='Header1' and namespace-uri()='']/*[local-name()='Priority-SenderAddress' and namespace-uri()='']");
            Add("RecipientAddress", "/*[local-name()='ARINC' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.ARINCReplyMessage']/*[local-name()='Header2' and namespace-uri()='']/*[local-name()='RecipientAddress' and namespace-uri()='']");
			Add("RecipientPIMA", "/*[local-name()='ARINC' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.ARINCReplyMessage']/*[local-name()='Header2' and namespace-uri()='']/*[local-name()='ClientPIMA' and namespace-uri()='']");
			Add("InternalMessage", "/*[local-name()='ARINC' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.ARINCReplyMessage']/*[local-name()='Data' and namespace-uri()='']");
            Add("Reference", "/*[local-name()='ARINC' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.ARINCReplyMessage']/*[local-name()='Header2' and namespace-uri()='']/*[local-name()='Signature-Date' and namespace-uri()='']");
		}
	}
}
