using System;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsTypesProvider))]
sealed class NctsTypesProviderTest : EU.NCTS.Business.Testing.NctsTypesProviderAbstractTest<NctsTypesProvider>
{
	protected override Type ExpectedNctsAdditionalInfoType => typeof(NctsAdditionalInfo);
	protected override Type ExpectedNctsArrivalCargoDescType => typeof(NctsArrivalCargoDesc);
	protected override Type ExpectedNctsDepartureCargoDescType => typeof(EU.NCTS.Business.NctsDepartureCargoDesc);
}
