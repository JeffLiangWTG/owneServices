using Enterprise.Customs.Common.EU;
using NUnit.Framework;
using PLEntryStatus = Enterprise.Customs.PL.Business.Declaration.PLEntryStatusList.Codes;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CC507MessageSender))]
sealed class CC507MessageSenderTest : ExitControlMessageSenderTest<CC507MessageSender>
{
	public void TestMessageStatusUpdatedOnSend() => CombineAssertions(() =>
	{
		var testData = new ExitControlTestData(Factory);
		var exitReport = testData.ExitReport;
		var entryHeader = testData.EntryHeader;
		var messageSender = CreateSender(exitReport);

		exitReport.CER_MessageStatus = LogicalStatusList.Codes.Error;
		entryHeader.CH_EntryStatus = PLEntryStatus.ReleasedForExport;
		if (!SendMessageAndAssertResult(messageSender, exitReport, out _, "CH_EntryStatus = ReleasedForExport"))
		{
			return;
		}
		AssertEquals("CH_EntryStatus = ReleasedForExport", LogicalStatusList.Codes.Sent, exitReport.CER_MessageStatus);

		exitReport.CER_MessageStatus = LogicalStatusList.Codes.Error;
		entryHeader.CH_EntryStatus = PLEntryStatus.CAN;
		if (!SendMessageAndAssertResult(messageSender, exitReport, out _, "CH_EntryStatus = CAN"))
		{
			return;
		}
		AssertEquals("CH_EntryStatus = CAN", LogicalStatusList.Codes.Error, exitReport.CER_MessageStatus);
	});
}
