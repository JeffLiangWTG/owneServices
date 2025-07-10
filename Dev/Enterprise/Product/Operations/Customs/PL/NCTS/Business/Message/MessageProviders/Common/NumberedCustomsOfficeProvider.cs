using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NumberedCustomsOfficeProvider : CustomsOfficeProvider, INumberedCustomsOffice
{
	public NumberedCustomsOfficeProvider(int sequenceNumber, NctsPLOfficeCode customsOffice)
		: base(customsOffice)
	{
		SequenceNumber = sequenceNumber.ToString();
	}

	public string SequenceNumber { get; }
}
