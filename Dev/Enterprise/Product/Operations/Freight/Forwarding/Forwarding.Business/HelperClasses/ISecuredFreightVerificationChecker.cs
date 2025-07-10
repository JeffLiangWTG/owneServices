namespace Enterprise.Freight.Forwarding.Business
{
	public interface ISecuredFreightVerificationChecker
	{
		bool FreightIsVerifiedToBeSecure(bool mawbValueMayNotBeSynced);
	}
}
