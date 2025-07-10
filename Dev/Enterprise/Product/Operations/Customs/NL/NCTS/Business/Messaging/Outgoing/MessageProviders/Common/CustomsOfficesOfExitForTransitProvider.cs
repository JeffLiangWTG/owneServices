using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CustomsOfficesOfExitForTransitProvider : ICustomsOfficeOfExitForTransit
{
	public CustomsOfficesOfExitForTransitProvider(string officeCode, ZInt sequence)
	{
		SequenceNumeric = sequence;
		ReferenceNumber = officeCode;
	}

	public int SequenceNumeric { get; }

	public string ReferenceNumber { get; }
}
