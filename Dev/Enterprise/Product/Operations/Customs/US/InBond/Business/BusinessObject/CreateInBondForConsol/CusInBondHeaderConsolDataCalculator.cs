using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondHeaderConsolDataCalculator : CusInBondHeaderDataCalculator
	{
		readonly ForwardingConsol consol;

		public CusInBondHeaderConsolDataCalculator(ForwardingConsol consol, CusInBondHeader header)
			: base(header)
		{
			this.consol = Argument.NotNull(consol, "consol");
		}

		public override ForwardingConsol RelevantConsol => consol;

		protected override BusinessObjectFactory SourceFactory => consol.Factory;

		protected override ZString InBondTransportMode => consol.TransportMode;

		protected override bool IsSourceAir => consol.IsAir;

		protected override ForwardingConsol ArrivalConsol => consol;

		protected override ZString SourceTransportMode => consol.JK_TransportMode;

		protected override ZString SourceLoadingPort => consol.JK_RL_NKLoadPort;

		public override ZDateTime GetETADate() => consol.LastLegDischargePortETAForBinding;

		public override ZDateTime GetSailingDate() => consol.FirstLegLoadPortETDForBinding;

		protected override (ZString transportMode, ZString packingMode) GetTransportAndPacking() => (consol.JK_TransportMode, consol.JK_ConsolMode);

		protected override List<Transport> Transports => new List<Transport>(consol.Transports.ToArray<Transport>());

		protected override ForwardingConsol InBondParentConsol => consol;
	}
}
