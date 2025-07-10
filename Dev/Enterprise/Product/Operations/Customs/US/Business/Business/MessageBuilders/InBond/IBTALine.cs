using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IFDALine : IOGALine
	{
		ZString CommercialDescription { get; }

		ZInt FDALineNumber { get; set; }

		ZString FDAProductCode { get; }
		ZString CargoStorageStatus { get; }
		ZString CountryOfProduction { get; }

		IReadOnlyList<AffirmationCode> AffirmationCodes { get; }

		ZString ManufacturerNumber { get; }
		ZString SupplierOrShipperNumber { get; }

		List<FDAQtyUQPair> OrderedQtyUQs { get; }

		ZDecimal ValueInWholeDollars { get; set; }
		ZString ConsigneeFEI { get; }
		ZString TradeOrBrandName { get; }

		ZDecimal FirstDimension { get; }
		ZDecimal SecondDimension { get; }
		ZDecimal ThirdDimension { get; }
		ZString DimensionUQ { get; }

		ZString ContactName { get; }
		ZString ContactPhone { get; }
		ZString ContactEmail { get; }
	}

	public interface IPriorNoticeLine : IFDALine
	{
		IPriorNoticeHeader PriorNoticeHeader { get; }

		ZBool RequiresPriorNotice { get; }
		ZBool IsDisclaimed { get; }
		ZString ConfirmationNumber { get; }
		ZString CountryOfShipping { get; }
		ZString FoodFacilityRegistrationExemption { get; }
		ZString FoodFacilityRegistrationNumber { get; }
		ZString OwnerFirmType { get; }
		ZString ProducerFirmType { get; }
		ZString ShipperRegistrationNumber { get; }

		IEnumerable<IMasterHouse> Bills { get; }
		IEnumerable<ZString> RailCarNumbers { get; }
		IEnumerable<ZString> ContainerNumbers { get; }

		ZString HarmonizedTariffNumber { get; }
		OrgHeader Importer { get; }
		OrgHeader Consignee { get; }
	}

	public interface IMasterHouse
	{
		ZString MasterBillSCAC { get; }
		ZString MasterBill { get; }

		ZString HouseBillSCAC { get; }
		ZString HouseBill { get; }
	}

	public interface IStandAlonePriorNoticeHeader
	{
		ZBool RelatedBillRequired { get; }
		ZString HumanReadableName { get; }
		ZString ReferenceQualifierCode { get; }
		ZString FilerOrIssuerCode { get; }
		ZString ReferenceIdentifierNumber { get; }
		ZString BillTypeIndicator { get; }
		ZString ImportingCarrierSCAC { get; }
		ZString EntryType { get; }
		ZString ModeOfTransportationCode { get; }
		BusinessObjectFactory Factory { get; }
		CBPEDIMessageCollection Messages { get; }
		IEnumerable<IBillOfLadingDetail> Bills { get; }
		IEnumerable<IACEPriorNoticeLine> ACEStandalonePriorNoticeLines { get; }
	}

	public interface IPriorNoticeHeader : IMessageAttacheeInDeclaration
	{
		OrgHeader Submitter { get; }

		ZString PortOfArrival { get; }
		ZDateTime DateOfArrival { get; }
		ZDateTime TimeOfArrival { get; }

		ZString AnticipatedPortOfCrossing { get; }
		ZString VoyageFlightNumber { get; }
		ZBool IsSurface { get; }
		ZString CarrierType { get; }
		ZString CarrierName { get; }
		ZString CarrierCountry { get; }

		ZString EntryNumberLast8Digits { get; }
		ZString EntryType { get; }
		ZString ModeOfTransportationCode { get; }
		ZString LocationOfGoodsFIRMSCode { get; }
		ZString FTZAdmissionNumber { get; }
		ZString ImportingCarrierSCAC { get; }
		ZString ImportingAirCarrier3LetterCode { get; }

		ZString FDAMsgStatus { set; }

		IReadOnlyList<IPriorNoticeLine> StandalonePriorNoticeLines { get; }
	}
}
