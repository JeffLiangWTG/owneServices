using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.Common;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class UnloadingRemarkProviderTest : DataProviderTestCase<UnloadingRemarkProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsArrivalMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new UnloadingRemarkProvider(null));
	}

	public void TestConform()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_NoChangesToReport = true;
			AssertEquals("true", NCTSIndicator.YES, GetProvider().Conform);

			movementHeader.BM_NoChangesToReport = false;
			AssertEquals("false", NCTSIndicator.NO, GetProvider().Conform);
		});
	}

	public void TestUnloadingCompletion()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_UnloadingCompleted = true;
			AssertEquals("true", NCTSIndicator.YES, GetProvider().UnloadingCompletion);

			movementHeader.BM_UnloadingCompleted = false;
			AssertEquals("false", NCTSIndicator.NO, GetProvider().UnloadingCompletion);
		});
	}

	public void TestUnloadingDate() => AssertType<DateTime>(GetProvider().UnloadingDate);

	public void TestStateOfSeals()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_StateOfSealsBoolean = true;
			AssertEquals("true", NCTSIndicator.YES, GetProvider().StateOfSeals);

			movementHeader.BM_StateOfSealsBoolean = false;
			AssertEquals("false", NCTSIndicator.NO, GetProvider().StateOfSeals);
		});
	}

	public void TestUnloadingRemark() => AssertType<string>(GetProvider().UnloadingRemark);

	protected override UnloadingRemarkProvider GetProvider() => new UnloadingRemarkProvider(movementHeader);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		movementHeader = nctsHeader.ArrivalMovementHeader;
	}

	NctsHeader nctsHeader;
	NctsArrivalMovementHeader movementHeader;
}
