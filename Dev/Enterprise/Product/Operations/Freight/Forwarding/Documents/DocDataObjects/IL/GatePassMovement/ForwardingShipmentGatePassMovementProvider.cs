using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class ForwardingShipmentGatePassMovementProvider : IGatePassMovementProvider
	{
		internal ForwardingShipmentGatePassMovementProvider(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			arrivalConsol = shipment.ArrivalConsol;
		}

		public EnterpriseBusinessObject BusinessObject => shipment;

		public BusinessObjectFactory Factory => shipment.Factory;

		public EDIMessageCollection Messages => shipment.Messages;

		public ZString MessageReferenceNumber { get => shipment.JS_GMN; set => shipment.JS_GMN = value; }

		public ZString SourceType => nameof(ForwardingShipment);

		public ZString SourceID => shipment.JS_UniqueConsignRef;

		public ZString ProcessType
			=> GatePassMovementHelper.GetProcessType(shipment.Destination?.Country.Code.ToString());

		public ZString OriginSiteCode
			=> shipment.Destination is RefUNLOCO refUNLOCODestination
			? refUNLOCODestination.Code
			: ZString.Empty;

		public ZString DestinationSiteCode
			=> shipment.ImportReleaseDepot is OrgAddress importReleaseDepot && importReleaseDepot.Header is OrgHeader orgHeaderImportReleaseDepot
			? orgHeaderImportReleaseDepot.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, Constants.CountryCodes.Israel)
			: ZString.Empty;

		public ZString CargoTypeCode => shipment.JS_PackingMode;

		public ZString CargoIdentifierTypeCode => GatePassMovementHelper.GetCargoIdentifierTypeCode(TransportMode);

		public ZString CargoIdentifierKey1 => GatePassMovementHelper.GetCargoIdentifierKey1(arrivalConsol, TransportMode);

		public ZString CargoIdentifierKey2
			=> TransportMode.ToString() switch
			{
				Constants.TransportModes.Sea => shipment.Numbers.Cast<CusEntryNumber>().FirstOrDefault(s => s.CE_EntryType == IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber)?.CE_EntryNum ?? ZString.Empty,
				Constants.TransportModes.Air => GatePassMovementHelper.GetAirCargoIdentifierKey2(arrivalConsol),
				_ => ZString.Empty
			};

		public ZString CargoIdentifierKey3
			=> CargoIdentifierKey3IsVisible
			? shipment.JS_HouseBill
			: ZString.Empty;

		public ZBool CargoIdentifierKey3IsVisible => shipment.IsAir;

		public ZString TransportMode => shipment.TransportMode.ToString();

		public CodeDescriptionPairList CargoTypeCodeCollection => shipment.Lookups.JS_PackingMode_List;

		readonly ForwardingShipment shipment;
		readonly ForwardingConsol arrivalConsol;
	}
}
