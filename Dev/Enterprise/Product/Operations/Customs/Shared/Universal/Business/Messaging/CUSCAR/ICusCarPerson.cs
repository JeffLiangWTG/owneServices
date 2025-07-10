using CargoWise.Types;

namespace Enterprise.Customs.Universal.Messaging.CUSCAR
{
	public interface ICusCarPerson
	{
		PartyType PartyType { get; }
		ZString Surname { get; }
		ZString FullName { get; }
		ZString PassportNumber { get; }
		ZString TravelDocumentType { get; }
		ZString DrivingLicenceNumber { get; }
		ZString AdditionalInformationOne { get; }
		ZString AdditionalInformationTwo { get; }
		ZString AdditionalInformationTwoFor16A { get; }
	}
}
