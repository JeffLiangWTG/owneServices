using Urs.Api.Integration.Interfaces;

namespace Enterprise.Rating.Business;

public interface IUrsCharge : IBaseChargeDto
{
	ITradeServiceDto TradeService { get; }
}
