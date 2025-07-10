using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsPackageConfiguration))]
sealed class NctsPackageConfigurationTest : EU.NCTS.Business.Testing.NctsPackageConfigurationAbstractTest
{
	protected override Type NctsPackageDeparturePhase5ValidationDeciderForTest => typeof(NctsPackageDeparturePhase5ValidationDecider);

	protected override Type NctsPackageArrivalPhase5ValidationDeciderForTest => typeof(NctsPackageArrivalPhase5ValidationDecider);
}
