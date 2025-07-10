using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface IStrategicGoodsData
	{
		ZString RequestedTariff { get; } //this is what tariff was passed in and is the source tariff of the request

		ZString Tariff { get; } //this is a reference to the tariff or nomenclature record where we found the risk data

		ZString TariffTypeDescription { get; }//this is human readable name for the tariff type e.g. World Customs Organization Tariff

		ZString ConditionType { get; } //points to one or another agreement e.g.wassenaar, cwc etc

		ZString ConditionTypeDescription { get; } //the description of the aggreement

		ZDateTime StartDate { get; } //the start date '1 1 1900' means no start date

		ZDateTime EndDate { get; } //when known else empty

		ZString Comment { get; } //additional comments when included in the reference data set

		ZString Source { get; }
	}
}
