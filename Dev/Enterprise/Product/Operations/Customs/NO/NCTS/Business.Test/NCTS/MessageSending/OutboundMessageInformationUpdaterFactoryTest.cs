using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.Testing;

[TestedType(typeof(OutboundMessageInformationUpdaterFactory))]
sealed class OutboundMessageInformationUpdaterFactoryTest : TestCaseWithFactory
{
	public void TestGetInformationUpdater_Parameter()
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null",
			() => OutboundMessageInformationUpdaterFactory.GetInformationUpdater(null));
	}

	public void TestGetInformationUpdater() => CombineAssertions(() =>
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertType<ArrivalOutboundMessageInformationUpdater>("When Arrival Movement", OutboundMessageInformationUpdaterFactory.GetInformationUpdater(header));

		header.BH_HeaderType = ZString.Empty;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertExceptionThrown<NotSupportedException>("When Departure Movement", () => OutboundMessageInformationUpdaterFactory.GetInformationUpdater(header));
	});
}
