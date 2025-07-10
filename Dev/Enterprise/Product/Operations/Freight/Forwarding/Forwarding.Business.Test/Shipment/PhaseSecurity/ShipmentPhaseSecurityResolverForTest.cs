using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShipmentPhaseSecurityResolverForTest : ShipmentPhaseSecurityResolver
	{
		public ShipmentPhaseSecurityResolverForTest(ForwardingShipment master)
			: base(master)
		{
		}

		public new ZString PhaseCode
		{
			get { return base.PhaseCode; }
		}

		public IPhaseSecurity PhaseSecurity_Exposed
		{
			get { return GetPhaseSecurity(); }
		}

		protected override IPhaseSecurity GetPhaseSecurity()
		{
			return GetPhaseSecurityDelegate?.Invoke() ?? base.GetPhaseSecurity();
		}
		public Func<IPhaseSecurity> GetPhaseSecurityDelegate { get; set; }

		public new IEnumerable<ZString> ResolveUNLOCOs(ZString locationCode)
		{
			return base.ResolveUNLOCOs(locationCode);
		}
	}
}
