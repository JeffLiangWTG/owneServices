
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.PermitPrinting
{
	public interface IPrintPermitTN4 : IPrintPermit
	{
		ICConditions[] CAConditions { get; }
		ICConditions[] CustomsConditions { get; }
	}

	public interface IPrintPermitTN41 : IPrintPermit
	{
		ZString ImporterNameLine1 { get; }
		ZString ImporterNameLine2 { get; }
		ZString ImporterUEN { get; }

		ZString ExporterNameLine1 { get; }
		ZString ExporterNameLine2 { get; }
		ZString ExporterUEN { get; }

		ZString HandlingAgentNameLine1 { get; }
		ZString HandlingAgentNameLine2 { get; }
		ZString HandlingAgentNameLine3 { get; }
		ZString HandlingAgentUEN { get; }

		ZString PlaceOfReleaseName { get; }
		ZString PlaceOfReleaseCode { get; }

		ZString PlaceOfReceiptName { get; }
		ZString PlaceOfReceiptCode { get; }

		ZString InwardCarrierAgentNameLine1 { get; }
		ZString InwardCarrierAgentNameLine2 { get; }
		ZString InwardCarrierAgentNameLine3 { get; }

		ZString OutwardCarrierAgentNameLine1 { get; }
		ZString OutwardCarrierAgentNameLine2 { get; }
		ZString OutwardCarrierAgentNameLine3 { get; }

		ITN41PermitConditions[] CAPermitConditions { get; }
		ITN41PermitConditions[] CustomsPermitConditions { get; }
	}

	public interface IPrintPermit
	{
		ZString TradeNetVersion { get; }
		ZString PermitNumber { get; }
		ZString UniqueRef { get; }

		#region Page 1

		ZString MessageType { get; }
		ZString DeclarationType { get; }
		ZString Importer { get; }
		ZString Exporter { get; }
		ZString HandlingAgent { get; }
		ZString PortOfLoading { get; }
		ZString NextPortOfCall { get; }
		ZString PortOfDischarge { get; }
		ZString FinalPortOfCall { get; }
		ZString CountryOfFinalDest { get; }
		ZString InwardCarrierAgent { get; }
		ZString OutwardCarrierAgent { get; }
		ZString PlaceOfRelease { get; }
		ZDate ValidityPeriodFrom { get; }
		ZDate ValidityPeriodTo { get; }
		ZString TotalGrossWt { get; }
		ZString TotalOuterPack { get; }
		ZDecimal TotalCustomsDUTPayable { get; }
		ZDecimal TotalOtherTaxPayable { get; }
		ZDecimal TotalExciseDUTPayable { get; }
		ZDecimal TotalGstAmount { get; }
		ZDecimal TotalAmountPayable { get; }
		ZString CargoPackingType { get; }
		ZString InVesName { get; }
		ZString InVoyageFlightNumber { get; }
		ZString InOBLMawbNb { get; }
		ZDate ArrivalDate { get; }
		ZString OutVesName { get; }
		ZString OutVesLocation { get; }
		ZString OutVoyageFlightNumber { get; }
		ZString TowingVesselName { get; }
		ZString OutOBLMawbNb { get; }
		ZDate DepartureDate { get; }
		ZString LicenceNo { get; }
		ZString CertificateNo { get; }
		ZString PlaceOfReceipt { get; }
		ZString CustomsProcedureCodes { get; }

		#endregion

		#region Page2
		ZBool HideMawbLine { get; }
		ZBool HideHawbLine { get; }
		ZBool HideCifFobValue { get; }
		ZBool HideLspValue { get; }
		ZBool HideGstValue { get; }
		ZBool HideDutQtyWtVolValue { get; }
		ZBool HideUnitPriceValue { get; }
		ZBool HideExciseValue { get; }
		ZBool HideDutyValue { get; }
		ZBool HideOtherTaxValue { get; }
		IPrintPermitConsignment[] ConsignmentDetails { get; }
		#endregion

		#region Page3
		ZString[] TradersRemark { get; }
		IPrintPermitContainers[] ContainerIdentifiers { get; }
		ZString NameOfCompany { get; }
		ZString EntityIdentOfCompany { get; }
		ZString DeclarantName { get; }
		ZString DeclarantCode { get; }
		ZString TelNb { get; }
		ZString ManufacturerName { get; }
		#endregion

		ZDate AmendDate { get; }
		ZString[] AmendFields { get; }
	}

	public interface IPrintPermitConsignment
	{
		ZString SerialNb { get; }
		ZString HSCode { get; }
		ZString BrandName { get; }
		ZString HSQuantity { get; }
		ZString HSQuantityUnit { get; }
		ZString Marking { get; }
		ZString CityOfOrigin { get; }
		ZString Model { get; }
		ZString DutQuantity { get; }
		ZString DutQuantityUnit { get; }
		ZString InwardMawbObl { get; }
		ZString InwardHawbHbl { get; }
		ZString OutwardMawbObl { get; }
		ZString OutwardHawbHbl { get; }
		ZString GoodsDescription { get; }
		ZDecimal UnitPrice { get; }
		ZString UnitPriceCurrency { get; }
		ZDecimal CustomsDutyPayable { get; }
		ZDecimal ExciseDutyPayable { get; }
		ZDecimal OtherTaxPayable { get; }
		ZString CurrentLotNb { get; }
		ZString PreviousLotNb { get; }
		ZDecimal CifFobLspValue { get; }
		ZDecimal LspAmount { get; }
		ZDecimal GstAmount { get; }
		ZString CASCProductCode { get; }
		ZDecimal CASCProductQty { get; }
		ZString CASCProductUQ { get; }
		ZString EngineNbChassisNb { get; }
		ZInt OuterPackQty { get; }
		ZString OuterPackUQ { get; }
		ZInt InPackQty { get; }
		ZString InPackUQ { get; }
		ZInt InnerPackQty { get; }
		ZString InnerPackUQ { get; }
		ZInt InmostPackQty { get; }
		ZString InmostPackUQ { get; }
		ZString ManufacturerName { get; }

		ZString LineValue1 { get; }
		ZString LineUnit1 { get; }
		ZString LineValue2 { get; }
		ZString LineUnit2 { get; }
		ZString LineValue3 { get; }
		ZString LineUnit3 { get; }
		ZString LineValue4 { get; }
		ZString LineUnit4 { get; }
		ZString LineValue5 { get; }
		ZString LineUnit5 { get; }
		ZString LineValue6 { get; }
		ZString LineValue7 { get; }
		ZString LineValue8 { get; }

		ICASCProductCode[] CASCProductCodes { get; }
		IEngineOrChassisNumber[] EngineOrChassisNumbers { get; }
	}

	public interface ICASCProductCode
	{
		ZString SequenceNumber { get; }
		ZString ProductCode { get; }
		ZString ProductQuantity { get; }
	}

	public interface IEngineOrChassisNumber
	{
		ZString SequenceNumber { get; }
		ZString Number { get; }
	}

	public interface ICConditions
	{
		ZString Code { get; }
		ZString Message { get; }
	}

	public interface ITN41PermitConditions
	{
		ZString Condition { get; }
	}

	public interface IPrintPermitContainers
	{
		ZString ContainerSequenceNumber1 { get; }
		ZString ContainerIdentifier1 { get; }

		ZString ContainerSequenceNumber2 { get; }
		ZString ContainerIdentifier2 { get; }
	}
}
