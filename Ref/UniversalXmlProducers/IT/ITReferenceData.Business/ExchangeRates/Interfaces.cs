namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public interface IExchangeRateMetaDataFetcher
	{
		ExchangeRateMetaData Fetch();
		ExchangeRateMetaData FetchPublicationTime();
	}

	public interface IExchangeRateDataFetcher
	{
		ExchangeRateData Fetch();
	}

	public interface IExchangeRateMetaDataParser
	{
		ExchangeRateMetaData Parse();
	}

	public interface IExchangeRateDataParser
	{
		ExchangeRateData Parse();
	}

	public interface IExchangeRateDataExporter
	{
		void Export();
	}
}
