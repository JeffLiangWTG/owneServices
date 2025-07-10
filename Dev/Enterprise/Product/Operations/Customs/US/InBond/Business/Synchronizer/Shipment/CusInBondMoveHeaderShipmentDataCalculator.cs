using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondMoveHeaderShipmentDataCalculator : CusInBondMoveHeaderDataCalculator
	{
		readonly ForwardingShipment shipment;
		public CusInBondMoveHeaderShipmentDataCalculator(ForwardingShipment shipment, CusInBondMoveHeader moveHeader) : base(moveHeader)
		{
			this.shipment = shipment;
		}

		public ForwardingConsol Consol => moveHeader.IsDeleted ? null : moveHeader.BM_InBondEntryType == InbondCommonTypeList.Codes._1ImmediateTransport ? shipment.ArrivalConsol : shipment.DepartureConsol;

		protected override ZString MoveHeaderEntryType => moveHeader.BM_InBondEntryType;

		protected override List<Transport> Transports
		{
			get
			{
				var transports = new List<Transport>();
				if (Consol != null)
				{
					transports.AddRange(Consol.Transports.ToArray<Transport>());
				}
				transports.AddRange(shipment.Transports.ToArray<Transport>());
				return transports;
			}
		}

		#region BM_DestinationPortCode

		public ZString GetDestinationPort()
		{
			var result = ZString.Empty;
			var entryType = MoveHeaderEntryType;
			if (entryType == InbondCommonTypeList.Codes._1ImmediateTransport || entryType == InbondCommonTypeList.Codes._2TransportandExport)
			{
				var mostInterestingLeg = GetMostInterestingLeg();
				if (mostInterestingLeg != null)
				{
					result = USScheduleResolver.GetScheduleCode(Schedule.D, mostInterestingLeg.JW_RL_NKDiscPort, mostInterestingLeg.JW_TransportMode, moveHeader.Factory);
				}
			}
			return result.Left(CusInBondMoveHeader.Schema.BM_DestinationPortCodeMaxLength);
		}

		Transport GetMostInterestingLeg()
		{
			var transports = GetSortedTransports();

			Transport mostInterestingLeg = null;
			foreach (Transport transport in transports)
			{
				if (mostInterestingLeg == null && transport.JW_RL_NKDiscPort.Left(2) == Core.Constants.CountryCodes.UnitedStates)
				{
					mostInterestingLeg = transport;
				}

				if (mostInterestingLeg != null &&
					transport.JW_RL_NKLoadPort == mostInterestingLeg.JW_RL_NKDiscPort &&
					transport.JW_RL_NKDiscPort.Left(2) == Core.Constants.CountryCodes.UnitedStates &&
					GetFallBackDate(transport.JW_ATD, transport.JW_ETD) >= GetFallBackDate(mostInterestingLeg.JW_ATA, mostInterestingLeg.JW_ETA))
				{
					mostInterestingLeg = transport;
				}
			}
			return mostInterestingLeg;
		}

		#endregion
	}
}
