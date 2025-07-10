namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public interface IExchangeRateDetails
	{
		string Code { get; }
		decimal Rate { get; }
	}
}
