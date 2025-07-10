using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	public interface IHTS1
	{
		ZString TariffNumber { get; }
		ZString TransactionCode { get; }
		ZDate RecordBeginEffectiveDate { get; }
		ZDate RecordEndEffectiveDate { get; }
		ZInt NumberOfReportingUnits { get; }
		ZString Unit1 { get; }
		ZString Unit2 { get; }
		ZString Unit3 { get; }
		ZString DutyComputationCode { get; }
		ZString CommodityDescription { get; }
		ZDecimal Column1SpecificRate { get; }
		ZString BaseRateIndicator { get; }
	}
}
