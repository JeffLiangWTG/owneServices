using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	abstract class CusInBondMoveHeaderDataCalculator
	{
		protected readonly CusInBondMoveHeader moveHeader;

		public CusInBondMoveHeaderDataCalculator(CusInBondMoveHeader moveHeader)
		{
			this.moveHeader = Argument.NotNull(moveHeader, "moveHeader");
		}

		#region BM_ForeignDestPortKCode

		public ZString GetForeignDestPort()
		{
			var result = ZString.Empty;
			var entryType = MoveHeaderEntryType;
			if (entryType == InbondCommonTypeList.Codes._2TransportandExport || entryType == InbondCommonTypeList.Codes._3ImmediateExport)
			{
				var transports = GetSortedTransports();
				var leg = (from Transport transport in transports
						   where transport.JW_RL_NKLoadPort.Left(2) == Core.Constants.CountryCodes.UnitedStates
								&& transport.JW_RL_NKDiscPort.Left(2) != Core.Constants.CountryCodes.UnitedStates
						   select transport).FirstOrDefault();

				if (leg != null)
				{
					result = USScheduleResolver.GetScheduleCode(Schedule.K, leg.JW_RL_NKDiscPort, leg.JW_TransportMode, moveHeader.Factory);
				}
			}
			return result;
		}

		protected abstract ZString MoveHeaderEntryType { get; }

		public List<Transport> GetSortedTransports()
		{
			var transports = Transports;
			transports.Sort((x, y) =>
			{
				return GetFallBackDate(x.JW_ATD, x.JW_ETD) > GetFallBackDate(y.JW_ATD, y.JW_ETD) ? 1 : -1;
			});
			return transports;
		}

		protected abstract List<Transport> Transports { get; }

		protected ZDateTime GetFallBackDate(ZDateTime actual, ZDateTime estimate)
		{
			return actual.IsEmpty ? estimate : actual;
		}

		#endregion
	}
}
