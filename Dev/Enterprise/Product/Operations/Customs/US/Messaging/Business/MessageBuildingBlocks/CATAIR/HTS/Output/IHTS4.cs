using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	public interface IHTS4
	{
		ZString TariffNumber { get; }
		ZString ValueEditCode { get; }
		ZDecimal ValueLowBounds { get; }
		ZDecimal ValueHighBounds { get; }
		ZString EntryDateRestrictionCode1 { get; }
		ZShort BeginRestrictionDate1 { get; }
		ZShort EndRestrictionDate1 { get; }
		ZString EntryDateRestrictionCode2 { get; }
		ZShort BeginRestrictionDate2 { get; }
		ZShort EndRestrictionDate2 { get; }
		ZString ISOCountryOfOriginEditCode { get; }
		ZString QuantityEditCode { get; }
		ZDecimal QuantityEditLowerBound { get; }
		ZDecimal QuantityEditUpperBound { get; }
	}
}
