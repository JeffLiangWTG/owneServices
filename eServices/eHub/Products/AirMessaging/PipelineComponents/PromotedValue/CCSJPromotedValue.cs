
namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	class CCSJPromotedValue : PromotedValue
	{
		public CCSJPromotedValue()
		{
			Add("SenderPIMA", "/*[local-name()='CCSJ']/Header/SenderAirline");
			Add("RecipientPIMA", "/*[local-name()='CCSJ']/Header/Recipient");
			Add("InternalMessage", "/*[local-name()='CCSJ']/Data");
		}
	}
}
