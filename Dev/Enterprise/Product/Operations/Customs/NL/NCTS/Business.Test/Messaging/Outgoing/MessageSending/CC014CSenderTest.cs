using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC014CSenderTest : NCTSMessageSenderTest<CC014CSender, ICC014C>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CC014CSender(null));
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Cancellation;

	protected override ZString EntryType => NctsMessageTypeListNL.Codes.InvalidationCancellation;

	protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;
}
