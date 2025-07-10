using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class CommonShipmentTransportSupporter<T> : TransportSupporter<T>
		where T : CommonShipment
	{
		public CommonShipmentTransportSupporter(T shipment)
			: base(shipment)
		{
			this.shipment = shipment;
		}

		#region Container Mode

		public override ZString ContainerMode
		{
			get { return shipment.JS_PackingMode; }
		}

		public override bool IsArrivalContainerModeFCLorULD
		{
			get
			{
				return base.IsArrivalContainerModeFCLorULD || shipment.ArrivalConsol != null && shipment.ArrivalConsol.JK_ConsolMode == Constants.ContainerModes.BuyersConsol;
			}
		}

		#endregion

		public override ZString Description
		{
			get { return shipment.JS_UniqueConsignRef; }
		}
		public override ZString ConsignmentRef
		{
			get { return shipment.JS_UniqueConsignRef; }
		}
		public override ZString TransportMode
		{
			get { return shipment.JS_TransportMode; }
		}
		public override ZString BillOfLading
		{
			get { return shipment.JS_HouseBill; }
		}
		public override ZGuid ShippingLine
		{
			get { return ZGuid.Empty; }
			set { }
		}

		public override void SetConsignmentRefIfNotSet()
		{
			shipment.PopulateBillAndShipmentNumberIfNeeded();
		}
		public override JobConsolTransportValidation GetNewTransportValidator(Transport transport)
		{
			return new ShipmentTransportValidation(transport);
		}

		readonly CommonShipment shipment;

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		protected override void NotifyVoyageUpdatedCore(Transport transport)
		{
			if (shipment.DocsAndCartage != null)
			{
				shipment.Logs.CreateRecreateOrUpdateEventLog(
					Events.CargoAvailable,
					EstimateActual.Actual,
					shipment.DocsAndCartage.AvailableDate.ToOffset(),
					ZString.Empty,
					shipment.GetParametersForEvent(Events.CargoAvailable).ToArray());
			}
		}

		protected override void NotifyTransportModeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyTransportModeChangedCore(transport, previousValue);

			if (Parent.JS_TransportMode == TransportModes.Air
				&& (previousValue == TransportModes.Air || transport.JW_TransportMode == TransportModes.Air)
				&& shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(shipment.JS_InspectionTypeCode))
			{
				Parent.SetApprovedShipperStatus(Res.GetString("2f283c62-3e55-40c5-b7aa-ec145b9093a7", "{0} has been changed", transport.JW_TransportModeInfo.HumanReadableName));
			}
		}

		protected override void NotifyLoadChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyLoadChangedCore(transport, previousValue);

			if (Parent.JS_TransportMode == TransportModes.Air
				&& previousValue.SubstringSafe(0, 2) != transport.JW_RL_NKLoadPort.SubstringSafe(0, 2)
				&& shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(shipment.JS_InspectionTypeCode))
			{
				Parent.SetApprovedShipperStatus(Res.GetString("16f770f8-58c8-4ce4-9d17-e55a9553805e", "{0} has been changed", transport.JW_RL_NKLoadPortInfo.HumanReadableName));
			}
		}
	}
}
