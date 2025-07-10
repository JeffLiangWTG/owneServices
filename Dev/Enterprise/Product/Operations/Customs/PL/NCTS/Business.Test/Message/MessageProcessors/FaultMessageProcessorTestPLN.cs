using CargoWise.Common;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Testing.FaultMessageTestHelper;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using MessageTypes = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(FaultMessageProcessor))]
sealed class FaultMessageProcessorTestPLN : FaultMessageProcessorTestBase<FaultMessageProcessor, EDIMessage>
{
	public override void TestApplicationCode() => AssertEquals(ApplicationCodes.PLCustomsNCTS, MessageProcessor.ApplicationCode);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendNctsErrors;

	public void TestDepartureFault() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		FaultsForTest.ForEach(AssertMovementHeaderFault);

		return;

		void AssertMovementHeaderFault(FaultTestInfo faultTestInfo)
		{
			nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
			var (processedOk, _, _) = AssertCommonProcessingResults(
				faultTestInfo,
				ApplicationCodes.PLCustomsNCTS,
				messageType: MessageTypes.NctsDeparture,
				messageSubType: Constants.MessageSubTypeCodes.IE015,
				transmitMessageLinkedObject: nctsHeader.MovementHeader);
			if (!processedOk)
			{
				return;
			}

			AssertEquals($"{faultTestInfo}: Validating NctsHeader status", NctsMessageStatusList.Codes.Rejected, nctsHeader.MovementHeader.BM_MessageStatus);

			var expectedLogMessage = $"Updated corresponding MovementHeader status to [{NctsMessageStatusList.Codes.Rejected}]. {faultTestInfo.ExpectedFaultName}. Error code = [{faultTestInfo.ExpectedFaultCode}], Description = [{faultTestInfo.ExpectedFaultDescription}].";
			nctsHeader.MovementHeader.AssertHasLogMessagePart($"{faultTestInfo}: Checking the CusEntryHeader log message", Events.InterchangeAcknowledged, expectedLogMessage);
		}
	});

	public void TestArrivalFault() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		FaultsForTest.ForEach(AssertNctsHeaderFault);

		return;

		void AssertNctsHeaderFault(FaultTestInfo faultTestInfo)
		{
			nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
			var (processedOk, _, _) = AssertCommonProcessingResults(
				faultTestInfo,
				ApplicationCodes.PLCustomsNCTS,
				messageType: MessageTypes.NctsArrivalNotification,
				messageSubType: Constants.MessageSubTypeCodes.IE007,
				transmitMessageLinkedObject: nctsHeader);
			if (!processedOk)
			{
				return;
			}

			AssertEquals($"{faultTestInfo}: Validating NctsHeader status", NctsMessageStatusList.Codes.Rejected, nctsHeader.BH_MessageStatus);

			var expectedLogMessage = $"Updated corresponding NctsHeader status to [{NctsMessageStatusList.Codes.Rejected}]. {faultTestInfo.ExpectedFaultName}. Error code = [{faultTestInfo.ExpectedFaultCode}], Description = [{faultTestInfo.ExpectedFaultDescription}].";
			nctsHeader.AssertHasLogMessagePart($"{faultTestInfo}: Checking the CusEntryHeader log message", Events.InterchangeAcknowledged, expectedLogMessage);
		}
	});

	protected override FaultMessageProcessor MessageProcessor => messageProcessor ??= new (serviceLogger);
	new FaultMessageProcessor messageProcessor;
}
