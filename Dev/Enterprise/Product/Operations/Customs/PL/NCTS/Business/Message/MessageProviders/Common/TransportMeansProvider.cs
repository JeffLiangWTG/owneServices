using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class TransportMeansProvider : ITransportMeans
{
	public TransportMeansProvider(ZString typeOfIdentification, ZString identificationNumber, ZString nationality)
	{
		TypeOfIdentification = typeOfIdentification;
		IdentificationNumber = identificationNumber;
		Nationality = nationality;
	}

	public string TypeOfIdentification { get; }

	public string IdentificationNumber { get; }

	public string Nationality { get; }
}
