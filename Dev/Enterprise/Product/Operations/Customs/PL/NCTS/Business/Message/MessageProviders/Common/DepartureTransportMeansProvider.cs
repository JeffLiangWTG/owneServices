using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class DepartureTransportMeansProvider : IDepartureTransportMeans
{
	public DepartureTransportMeansProvider(int sequenceNumber, ZString typeOfIdentification, ZString identificationNumber, string nationality)
	{
		SequenceNumber = sequenceNumber.ToString();
		TypeOfIdentification = typeOfIdentification;
		IdentificationNumber = identificationNumber;
		Nationality = nationality;
	}

	public string SequenceNumber { get; }

	public string TypeOfIdentification { get; }

	public string IdentificationNumber { get; }

	public string Nationality { get; }
}
