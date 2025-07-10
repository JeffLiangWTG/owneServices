using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class BillConfigurationTest : BillConfigurationAbstractTest<BillConfiguration>
{
	protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsBillDeparturePhase5ValidationDecider);

	protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(NctsBillArrivalPhase5ValidationDecider);
}
