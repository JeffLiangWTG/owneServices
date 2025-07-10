namespace Enterprise.Freight.Integration
{
	public interface IConfirmationPlugin
	{
		void SetStrategy(ConfirmationType strategy, string tabPageCaption);
	}

	public enum ConfirmationType
	{
		None,
		OriginPickup,
		DestinationDelivery,
	}
}
