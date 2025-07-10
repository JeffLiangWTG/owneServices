using System;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public interface IExchangeRateData
	{
		string Currency { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
		decimal InRate { get; }
		decimal ExRate { get; }
	}
}
