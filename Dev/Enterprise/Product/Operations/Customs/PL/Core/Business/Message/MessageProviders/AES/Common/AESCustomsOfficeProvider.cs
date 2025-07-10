using CargoWise.Customs.PL.MessageContracts.Interfaces;

namespace Enterprise.Customs.PL.Business;

public class AESCustomsOfficeProvider : ICustomsOffice
{
	public AESCustomsOfficeProvider(string referenceNumber)
	{
		ReferenceNumber = referenceNumber;
	}

	public string ReferenceNumber { get; }
}
