using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IQPFirstAddressSegment
	{
		ZString CompanyName { get; set; }
		ZString AddressLine1 { get; set; }
	}

	public interface IQPSecondAddressSegment
	{
		ZString AddressLine2 { get; set; }
		ZString AddressLine3 { get; set; }
	}

	public interface IQPPhoneAddressSegment
	{
		ZString PhoneNumber { get; set; }
	}
}