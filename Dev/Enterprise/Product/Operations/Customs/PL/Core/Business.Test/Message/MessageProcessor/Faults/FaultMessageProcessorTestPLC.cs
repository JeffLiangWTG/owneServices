using CargoWise.Common;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using EntryHeaderStatus = Enterprise.Customs.Common.Shared.MessageStatusList.Codes;
using FaultTestInfo = Enterprise.Customs.PL.Business.Testing.FaultMessageTestHelper.FaultTestInfo;
using MessageTypes = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(FaultMessageProcessor))]
sealed class FaultMessageProcessorTestPLC : FaultMessageProcessorTestBase<FaultMessageProcessor, EDIMessage>
{
	public void TestCusEntryHeaderFault() => CombineAssertions(() =>
	{
		var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		FaultsForTest.ForEach(AssertCusEntryHeaderFault);

		return;

		void AssertCusEntryHeaderFault(FaultTestInfo faultTestInfo)
		{
			cusEntryHeader.CH_Status = EntryHeaderStatus.Sent;
			var (processedOk, _, _) = AssertCommonProcessingResults(
				faultTestInfo,
				ApplicationCodes.PLCustoms,
				messageType: MessageTypes.Export,
				messageSubType: EDIMessageSubType.Declaration,
				transmitMessageLinkedObject: cusEntryHeader);
			if (!processedOk)
			{
				return;
			}

			AssertEquals($"{faultTestInfo}: Validating CusEntryHeader status", EntryHeaderStatus.ErrorReplace, cusEntryHeader.CH_Status);

			var expectedLogMessage = $"Updated corresponding CusEntryHeader status to [{EntryHeaderStatus.ErrorReplace}]. {faultTestInfo.ExpectedFaultName}. Error code = [{faultTestInfo.ExpectedFaultCode}], Description = [{faultTestInfo.ExpectedFaultDescription}].";
			cusEntryHeader.AssertHasLogMessagePart($"{faultTestInfo}: Checking the CusEntryHeader log message", Events.InterchangeAcknowledged, expectedLogMessage);
		}
	});

	protected override FaultMessageProcessor MessageProcessor => messageProcessor ??= new (serviceLogger);
	new FaultMessageProcessor messageProcessor;
}
