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
	public class ForwardingConsolGatePassMovementProvider : IGatePassMovementProvider
	{
		internal ForwardingConsolGatePassMovementProvider(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
		}

		public EnterpriseBusinessObject BusinessObject => consol;

		public BusinessObjectFactory Factory => consol.Factory;

		public EDIMessageCollection Messages => consol.Messages;

		public ZString MessageReferenceNumber { get => consol.JK_GMN; set => consol.JK_GMN = value; }

		public ZString SourceType => nameof(ForwardingConsol);

		public ZString SourceID => consol.JK_UniqueConsignRef;

		public ZString ProcessType => GatePassMovementHelper.GetProcessType(consol.DischargePort?.Country.Code.ToString());

		public ZString OriginSiteCode => consol.DischargePort is RefUNLOCO refUNLOCODestination ? refUNLOCODestination.Code : ZString.Empty;

		public ZString DestinationSiteCode => ZString.Empty;

		public ZString CargoTypeCode => consol.JK_ConsolMode;

		public ZString CargoIdentifierTypeCode => GatePassMovementHelper.GetCargoIdentifierTypeCode(TransportMode);

		public ZString CargoIdentifierKey1 => GatePassMovementHelper.GetCargoIdentifierKey1(consol, TransportMode);

		public ZString CargoIdentifierKey2
			=> TransportMode.ToString() switch
			{
				Constants.TransportModes.Sea => consol.Numbers.Cast<CusEntryNumber>().FirstOrDefault(s => s.CE_EntryType == IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber)?.CE_EntryNum ?? ZString.Empty,
				Constants.TransportModes.Air => GatePassMovementHelper.GetAirCargoIdentifierKey2(consol),
				_ => ZString.Empty
			};

		public ZString CargoIdentifierKey3 => ZString.Empty;

		public ZBool CargoIdentifierKey3IsVisible => false;

		public ZString TransportMode => consol.TransportMode;

		public CodeDescriptionPairList CargoTypeCodeCollection => consol.JK_ConsolMode_List;

		readonly ForwardingConsol consol;
	}
}
