namespace Enterprise.Freight.Integration
{
	public interface IBookingCostBreakdownCharge
	{
		decimal Amount { get; }
		string Currency { get; }
		string ChargeCode { get; }
		string ChargeCodeDescription { get; }
	}
}
