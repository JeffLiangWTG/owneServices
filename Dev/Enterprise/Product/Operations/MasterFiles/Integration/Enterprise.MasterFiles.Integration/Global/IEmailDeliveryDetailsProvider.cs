using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IEmailDeliveryDetailsProvider
	{
		ZString ContactName { get; }
		ZString EmailAddress { get; }
		ZString BounceBackEmail { get; }
	}
}
