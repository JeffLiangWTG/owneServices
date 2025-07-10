using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondMoveHeaderConsolDataCalculator : CusInBondMoveHeaderDataCalculator
	{
		readonly ForwardingConsol consol;
		readonly MovementHeaderWrapper moveHeaderWrapper;
		public CusInBondMoveHeaderConsolDataCalculator(ForwardingConsol consol, MovementHeaderWrapper moveHeaderWrapper, CusInBondMoveHeader moveHeader) : base(moveHeader)
		{
			this.consol = Argument.NotNull(consol, "consol");
			this.moveHeaderWrapper = Argument.NotNull(moveHeaderWrapper, "moveHeaderWrapper");
		}

		protected override ZString MoveHeaderEntryType => moveHeaderWrapper.EntryType;

		protected override List<Transport> Transports => new List<Transport>(consol.Transports.ToArray<Transport>());
	}
}
