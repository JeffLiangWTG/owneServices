using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public interface IImporterSecurityFiling : IMessageAttachee, IBusiness
	{
		ZGuid PK { get; }
		ZString JobReference { get; }
		void UpdateBill(ZString billNumber, ZString status, ZDateTime statusDate);
		void UpdateAcceptedDate(ZDateTime acceptedDate);

		//SF10
		ZString SFSubmissionType { get; }
		ZString ActionReasonCode { get; }
		ZString ShipmentTypeCode { get; }
		ZString IORNumberQualifier { get; }
		ZString IORNumber { get; }
		ZDate DateOfBirth { get; }
		ZString ModeOfTransportation { get; }
		ZString SFTransactionNumber { get; set; }
		ZString SCAC { get; }
		ZString ISFImporterBondHolder { get; }
		ZBool ISFBondIndicator { get; }
		ZString CountryOfIssuance { get; }
		ZString ImporterFullName { get; }

		ZString ISFBondActivityCode { get; }
		ZString ISFBondType { get; }

		//SF13
		ZString ShipmentSubType { get; }
		ZDecimal EstimatedValue { get; }
		ZDecimal EstimatedQuantity { get; }
		ZString UnitOfMeasure { get; }
		ZDecimal EstimatedWeight { get; }
		ZString WeightQualifier { get; }

		//SF15
		IEnumerable<IShipmentReferenceID> ShipmentIDs { get; }

		//SF20
		IEnumerable<IReferenceData> ReferenceData { get; }

		//SF25
		IEnumerable<IContainerData> ContainerData { get; }

		ZString ConsigneeNumberQualifier { get; }
		ZString ConsigneeNumber { get; }
		ZString ConsigneePassportCountryOfIssue { get; }
		ZDate ConsigneePassportDateOfBirth { get; }
		ZString ConsigneeFullName { get; }

		//SELLER/OWNER, BUYER/OWNER, SHIP TO, STUFFING LOCATION, CONSOLIDATOR LOOP
		IEnumerable<IISFDocAddress> RelatedOrganizationData { get; }

		// ISF 5
		//SF40
		IEnumerable<ITariffData> Tariffs { get; }

		//SF50
		ZString CodeQualifier1 { get; }
		ZString ForeignPortOfUnlading { get; }
		ZString CodeQualifier2 { get; }
		ZString PlaceOfDelivery { get; }

		//ISF 10
		//MANUFACTURER LOOP
		IEnumerable<IManufacturerData> ManufacturerData { get; }
	}

	public interface IShipmentReferenceID
	{
		ZString CodeQualifier { get; }
		ZString ShipmentReferenceIdentifier { get; }
	}

	public interface IReferenceData
	{
		ZString CodeQualifier { get; }
		ZString ReferenceData { get; }
	}

	public interface IContainerData
	{
		ZString EquipmentDescriptionCode { get; }
		ZString EquipmentInitial { get; }
		ZString EquipmentNumber { get; }
		ZString EquipmentNumberCheckDigit { get; }
		ZString EquipmentSizeTypeCode { get; }
	}

	public interface IAddressingInformation
	{
		//SF35
		ZString AddressComponentQualifier1 { get; }
		ZString AddressInformation1 { get; }
		ZString AddressComponentQualifier2 { get; }
		ZString AddressInformation2 { get; }
	}

	public interface IManufacturerData
	{
		IISFDocAddress Manufacturer { get; }
		//SF40
		IEnumerable<ITariffData> Tariffs { get; }
	}

	public interface I30Blocks
	{
		//SF30
		ZString EntityCode { get; }
		ZString EntityName { get; }
		ZString EntityIdentifierQualifier { get; }
		ZString EntityIdentifier { get; }
		ZString LegalName { get; }
		ZString PassportCountryOfIssue { get; }
		ZDateTime PassportDateOfBirth { get; }
		ZDateTime SocialSecurityNumberDateOfBirth { get; }

		//SF31
		ZString SecondaryEntityCode { get; }
		ZString SecondaryEntityName { get; }

		IEnumerable<IAddressingInformation> AddressingInformation { get; }

		//SF36
		ZString City { get; }
		ZString CountrySubEntityCode { get; }
		ZString PostalCode { get; }
		ZString Country { get; }
	}

	public interface ITariffData
	{
		ZString HarmonizedTariffNumber { get; }
		ZString CountryOfOrigin { get; }
	}
}
