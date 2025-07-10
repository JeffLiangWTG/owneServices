using System;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(BillConfiguration))]
sealed class BillConfigurationTest : EU.NCTS.Business.Testing.BillConfigurationAbstractTest<BillConfiguration>
{
	protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsBillDeparturePhase5ValidationDecider);

	protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(NctsBillArrivalPhase5ValidationDecider);

	protected override Type GetBillAdditionalDocumentValidationPhase5DeciderForTest() => typeof(NctsBillAdditionalDocumentPhase5ValidationDecider);
}
