using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public class FBaseMessageDetails : IFBaseMessageDetailsProvider
	{
		public FBaseMessageDetails(ExportAWBHeader parent)
		{
			this.parent = parent;
		}

		protected readonly ExportAWBHeader parent;

		RefAirline Airline
		{
			get
			{
				if (airline == null)
				{
					if (!parent.EH_By1st.IsEmpty)
					{
						airline = RefAirline.LoadFromAirline2LetterCode(parent.Factory, parent.EH_By1st);
					}

					if (airline == null && !parent.EH_AirlinePrefix.IsEmpty)
					{
						airline = RefAirline.LoadFromAirlinePrefix(parent.Factory, parent.EH_AirlinePrefix);
					}
				}

				return airline;
			}
		}

		RefAirline airline;

		#region IFBaseMessageDetailsProvider Members

		public ZString Currency => parent.EH_Currency;

		public ZString ChargesCode => parent.EH_ChargesCode;

		public ZString WeightVPPDCOL => parent.EH_WeightVPPDCOL;

		public ZString OtherPPDCOL => parent.EH_OtherPPDCOL;

		public ZDecimal DeclaredValue => parent.EH_DeclaredValue;

		public ZDecimal CustomsValue => parent.EH_CustomsValue;

		public ZDecimal InsuranceValue => parent.EH_InsuranceValue;

		public ZString HouseCustomsValueCurrency => parent.EH_HouseCustomsValueCurrency;

		public ZString HouseInsuranceValueCurrency => parent.EH_HouseInsuranceValueCurrency;

		public ZString HouseDeclaredValueCurrency => parent.EH_HouseDeclaredValueCurrency;

		public ZString ShipperTraderNo => parent.EH_ShipperTraderNo;

		public ZString ShipperTraderNoType => parent.EH_ShipperTraderNoType;

		public ZString ShipperTraderNoCountryCode => parent.EH_ShipperTraderNoCountryCode;

		public ZString ShipperCountryCode => parent.EH_ShipperCountryCode;

		public ZString ShipperContactName => parent.EH_ShipperContactName;

		public ZString ShipperContactCode => parent.EH_ShipperContactCode;

		public ZString ShipperContactDetail => parent.EH_ShipperContactDetail;

		public ZString ShipperAccount => parent.EH_ShipperAccount;

		public ZString ShipperName => parent.EH_ShipperName;

		public ZString ShipperAddress => parent.EH_ShipperAddress;

		public ZString ShipperAddress2 => parent.EH_ShipperAddress2;

		public ZString ShipperPlace => parent.EH_ShipperPlace;

		public ZString ShipperState => parent.EH_ShipperState;

		public ZString ShipperPostCode => parent.EH_ShipperPostCode;

		public ZString DestinationShipperComment => parent.DestinationShipperComment;

		public ZString ConsigneeTraderNo => parent.EH_ConsigneeTraderNo;

		public ZString ConsigneeTraderNoType => parent.EH_ConsigneeTraderNoType;

		public ZString ConsigneeTraderNoCountryCode => parent.EH_ConsigneeTraderNoCountryCode;

		public ZString ConsigneeCountryCode => parent.EH_ConsigneeCountryCode;

		public ZString ConsigneeContactName => parent.EH_ConsigneeContactName;

		public ZString ConsigneeContactCode => parent.EH_ConsigneeContactCode;

		public ZString ConsigneeContactDetail => parent.EH_ConsigneeContactDetail;

		public ZString ConsigneeAccount => parent.EH_ConsigneeAccount;

		public ZString ConsigneeName => parent.EH_ConsigneeName;

		public ZString ConsigneeAddress => parent.EH_ConsigneeAddress;

		public ZString ConsigneeAddress2 => parent.EH_ConsigneeAddress2;

		public ZString ConsigneePlace => parent.EH_ConsigneePlace;

		public ZString ConsigneeState => parent.EH_ConsigneeState;

		public ZString ConsigneePostCode => parent.EH_ConsigneePostCode;

		public ZString AlsoNotifyTraderNo => parent.EH_AlsoNotifyTraderNo;

		public ZString AlsoNotifyTraderNoType => parent.EH_AlsoNotifyTraderNoType;

		public ZString AlsoNotifyTraderNoCountryCode => parent.EH_AlsoNotifyTraderNoCountryCode;

		public ZString AlsoNotifyCountryCode => parent.EH_AlsoNotifyCountryCode;

		public ZString AlsoNotifyContactName => parent.EH_AlsoNotifyContactName;

		public ZString AlsoNotifyContactCode => parent.EH_AlsoNotifyContactCode;

		public ZString AlsoNotifyContactDetail => parent.EH_AlsoNotifyContactDetail;

		public ZString FreightForwarderOrCarrierCode => parent.FreightForwarderOrCarrierCode;

		public ZString AWBOriginCode => parent.EH_AWBOriginCode;

		public ZString AirportOfDestinationCode => parent.EH_AirportOfDestinationCode;

		public ZInt TotalNoOfPieces => parent.EH_TotalNoOfPieces;

		public ZDecimal TotalGrossWeight => parent.EH_TotalGrossWeight;

		public ZString? AirlineContactNameOCIIdentifier => Airline?.RM_ContactNameOCIIdentifier;

		public ZString? AirlineContactPhoneOCIIdentifier => Airline?.RM_ContactPhoneOCIIdentifier;

		public ZBool IsDeclarantForAdvancedCargoReporting => parent.IsHouseAirWayBill ? false : parent.EH_IsConsigneeDeclarantForAdvanceCargoReporting;

		public ZString ShipperContactEmail => parent.EH_ShipperContactEmail;

		public ZString ConsigneeContactEmail => parent.EH_ConsigneeContactEmail;

		public ZString HandlingInformation => parent.EH_HandlingInformation;

		public IReadOnlyCollection<ZString> DGUNNOValues() => parent.DGUNNOValues.ToList().AsReadOnly();

		public IReadOnlyCollection<IAWBRateLineMessageDetailsProvider> AWBRateLines => parent.AWBRateLines.Cast<ExportAWBRateLine>().ToList().AsReadOnly();

		public IReadOnlyCollection<IVATCountryHandler> VATCountryHandlers => parent.GetVATCountryHandlers(this);

		public IReadOnlyCollection<IContactNumberCountryHandler> ContactNumberCountryHandlers => parent.GetContactNumberCountryHandlers(this);

		public IReadOnlyCollection<IAWBExportStatementDetailsProvider> ExportStatements => parent.ExportStatements_CargoIMP.AsReadOnly();

		public IACASCountryHandler ACASCountryHandler => parent.GetACASCountryHandler();

		public IReadOnlyCollection<IAWBEntryNumberMessageDetailsProvider> CustomsEntryNumbers => parent.CustomsEntryNumbers.ToList().AsReadOnly();

		public IReadOnlyCollection<IAWBMovementReferenceNumberMessageDetailsProvider> MovementReferenceNumbers => parent.MovementReferenceNumbers.ToList().AsReadOnly();

		public IReadOnlyCollection<IAWBGoodsDeclarationReferenceNumberMessageDetailsProvider> GoodsDeclarationReferenceNumbers => parent.GoodsDeclarationReferenceNumbers.ToList().AsReadOnly();

		#endregion
	}
}
