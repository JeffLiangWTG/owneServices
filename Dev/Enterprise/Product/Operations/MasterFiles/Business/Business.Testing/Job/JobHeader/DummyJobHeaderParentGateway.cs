using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyJobHeaderParentGateway : DummyJobHeaderParent, IGateway
	{
		public DummyJobHeaderParentGateway(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public IGatewayBillingSupporter GatewayBillingSupporter => new DummyGatewayBillingSupporter();

		public List<ZGuid> SortedGatewayAgentPKs => throw new NotImplementedException();

		public List<ZGuid> GatewayAgentPKsForIntercompanyTariff => throw new NotImplementedException();

		public IDictionary<ZString, IList<ZGuid>> LoginGatewayAgentRoles => throw new NotImplementedException();

		public ZString GatewayLoginRole => throw new NotImplementedException();

		public bool ContinueWithDefaultCosting(BillingType billingType) => throw new NotImplementedException();

		public ZString GatewayAgentTypeFilteredReason(string agentType, ZGuid gatewayAgentPk, BillingType billingType, CostSell costSell) => throw new NotImplementedException();

		public ZString GatewayServiceLevelFilteredReason(ZString gatewayServiceLevel, ZGuid gatewayAgentPk) => throw new NotImplementedException();

		public ZString ShipmentGatewayServiceLevel => throw new NotImplementedException();

		public bool IsIntercompanyTariffApplicable(BillingType billingType, CostSell costSell) => throw new NotImplementedException();

		public ZBool ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType billingType) => throw new NotImplementedException();

		public ZBool IsGatewaySellApplicableToGatewayConsol(CostSell costSell) => throw new NotImplementedException();

		public ZBool IsContainerNegotiatedCostApplicable(CostSell costSell) => throw new NotImplementedException();

		public List<ILocation> SortedOverridenPlannedLoad => throw new NotImplementedException();

		public List<ILocation> SortedOverridenPlannedDischarge => throw new NotImplementedException();
		public List<ZGuid> SortedControllingCustomerPKs => throw new NotImplementedException();
	}
}
