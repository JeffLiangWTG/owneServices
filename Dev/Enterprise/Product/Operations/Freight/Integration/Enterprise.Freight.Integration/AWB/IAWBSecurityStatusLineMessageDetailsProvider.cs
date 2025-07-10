using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IAWBSecurityStatusLineMessageDetailsProvider
	{
		ZString ApprovalCategory { get; }
		ZString ApprovalNumber { get; }
		ZString CountryCode { get; }
		ZDateTime ApprovalExpiryDate { get; }
	}
}
