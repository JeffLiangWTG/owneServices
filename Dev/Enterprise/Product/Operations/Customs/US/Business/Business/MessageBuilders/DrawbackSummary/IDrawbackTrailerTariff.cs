
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IDrawbackTrailerTariff
	{
		ZString FirstTariffNumber { get; }
		ZString AdditionalTariffNumber { get; }
		ZString AdditionalTariffNumber1 { get; }
		ZString AdditionalTariffNumber2 { get; }
		ZString AdditionalTariffNumber3 { get; }
	}
}
