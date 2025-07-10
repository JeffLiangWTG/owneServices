namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawRateDailyRecord : IRawRateRecord
	{
		string OperationType { get; }
		int SequenceNumber { get; }
		string FileName { get; }
	}
}
