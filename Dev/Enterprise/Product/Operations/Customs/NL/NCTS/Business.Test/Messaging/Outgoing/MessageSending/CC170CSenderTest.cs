using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC170CSenderTest : NCTSMessageSenderTest<CC170CSender, ICC170C>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CC170CSender(null));
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override ZString EntryType => NctsMessageTypeListNL.Codes.PresentationNotification;

	protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Presentation;

	protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;
}
