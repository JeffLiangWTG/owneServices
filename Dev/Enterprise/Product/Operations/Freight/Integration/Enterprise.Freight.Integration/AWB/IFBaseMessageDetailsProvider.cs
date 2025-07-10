using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IFBaseMessageDetailsProvider
	{
		ZString Currency { get; }
		ZString ChargesCode { get; }
		ZString WeightVPPDCOL { get; }
		ZString OtherPPDCOL { get; }
		ZDecimal DeclaredValue { get; }
		ZDecimal CustomsValue { get; }
		ZDecimal InsuranceValue { get; }
		ZString HouseCustomsValueCurrency { get; }
		ZString HouseInsuranceValueCurrency { get; }
		ZString HouseDeclaredValueCurrency { get; }

		ZString ShipperTraderNo { get; }
		ZString ShipperTraderNoType { get; }
		ZString ShipperTraderNoCountryCode { get; }
		ZString ShipperCountryCode { get; }
		ZString ShipperContactName { get; }
		ZString ShipperContactCode { get; }
		ZString ShipperContactDetail { get; }
		ZString ShipperContactEmail { get; }
		ZString DestinationShipperComment { get; }

		ZString ShipperAccount { get; }
		ZString ShipperName { get; }
		ZString ShipperAddress { get; }
		ZString ShipperAddress2 { get; }
		ZString ShipperPlace { get; }
		ZString ShipperState { get; }
		ZString ShipperPostCode { get; }

		ZString ConsigneeTraderNo { get; }
		ZString ConsigneeTraderNoType { get; }
		ZString ConsigneeTraderNoCountryCode { get; }
		ZString ConsigneeCountryCode { get; }
		ZString ConsigneeContactName { get; }
		ZString ConsigneeContactCode { get; }
		ZString ConsigneeContactDetail { get; }
		ZString ConsigneeContactEmail { get; }

		ZString ConsigneeAccount { get; }
		ZString ConsigneeName { get; }
		ZString ConsigneeAddress { get; }
		ZString ConsigneeAddress2 { get; }
		ZString ConsigneePlace { get; }
		ZString ConsigneeState { get; }
		ZString ConsigneePostCode { get; }

		ZString AlsoNotifyTraderNo { get; }
		ZString AlsoNotifyTraderNoType { get; }
		ZString AlsoNotifyTraderNoCountryCode { get; }
		ZString AlsoNotifyCountryCode { get; }
		ZString AlsoNotifyContactName { get; }
		ZString AlsoNotifyContactCode { get; }
		ZString AlsoNotifyContactDetail { get; }

		ZString FreightForwarderOrCarrierCode { get; }

		ZString AWBOriginCode { get; }
		ZString AirportOfDestinationCode { get; }

		ZInt TotalNoOfPieces { get; }
		ZDecimal TotalGrossWeight { get; }

		ZString? AirlineContactNameOCIIdentifier { get; }
		ZString? AirlineContactPhoneOCIIdentifier { get; }

		ZBool IsDeclarantForAdvancedCargoReporting { get; }

		IReadOnlyCollection<ZString> DGUNNOValues();

		IReadOnlyCollection<IAWBRateLineMessageDetailsProvider> AWBRateLines { get; }

		IReadOnlyCollection<IVATCountryHandler> VATCountryHandlers { get; }
		IReadOnlyCollection<IContactNumberCountryHandler> ContactNumberCountryHandlers { get; }
		IReadOnlyCollection<IAWBExportStatementDetailsProvider> ExportStatements { get; }

		IACASCountryHandler ACASCountryHandler { get; }
		ZString HandlingInformation { get; }
		IReadOnlyCollection<IAWBEntryNumberMessageDetailsProvider> CustomsEntryNumbers { get; }
		IReadOnlyCollection<IAWBMovementReferenceNumberMessageDetailsProvider> MovementReferenceNumbers { get; }
		IReadOnlyCollection<IAWBGoodsDeclarationReferenceNumberMessageDetailsProvider> GoodsDeclarationReferenceNumbers { get; }
	}
}
