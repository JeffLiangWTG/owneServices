using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefExchangeRateTypes
	{
		string GetDescriptionFromRateType(ExchangeRateType rateType);
	}
}
