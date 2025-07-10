using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IVisaQuery
	{
		ZString TariffNumber { get; }
		ZString OriginCountry { get; }
		ZString SecondTariffNumber { get; }
	}
}
