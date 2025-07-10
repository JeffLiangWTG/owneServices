using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC007CSenderTest : NCTSMessageSenderTest<CC007CSender, ICC007C>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CC007CSender(null));
	}

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Arrival;

	protected override ZString EntryType => NctsMessageTypeListNL.Codes.ArrivalNotification;

	protected override ZString ParentTableName => Customs.Business.AutoCusInBondHeader.Schema.TableName;
}
