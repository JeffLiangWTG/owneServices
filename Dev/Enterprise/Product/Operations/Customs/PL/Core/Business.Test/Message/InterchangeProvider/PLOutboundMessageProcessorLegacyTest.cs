using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.Business.Testing;

public abstract class OutboundMessageProcessorTestBase : TestCaseWithFactory
{
	protected void AssertProcessMessage(string caseName, bool expectedToBeProcessed, EnterpriseEDIMessage message)
	{
		var messageInitialStatus = message.EM_Status;

		var logger = new LoggingInformation();
		var processor = new PLOutboundMessageProcessorLegacy(logger);
		processor.ProcessMessage(CancellationToken.None);

		Factory.Save();

		message.Reload();

		CombineAssertions("RESULT", () =>
		{
			if (expectedToBeProcessed)
			{
				AssertEquals($"{caseName}: Status is changed to sent", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertNotNull($"{caseName}: Interchange is linked", message.Interchange);
			}
			else
			{
				AssertEquals($"{caseName}: Status is not changed", messageInitialStatus, message.EM_Status);
				AssertNull($"{caseName}: Interchange is not linked", message.Interchange);
			}
		});
	}
}

sealed class PLOutboundMessageProcessorLegacyTest : OutboundMessageProcessorTestBase
{
	public void TestProcessMessageImport() => TestProcessMessageCore(EUJobMessageTypeList.Codes.Import);

	public void TestProcessMessageExport() => TestProcessMessageCore(EUJobMessageTypeList.Codes.Export);

	void TestProcessMessageCore(ZString messageType)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var message = Factory.CreateCoreMessage(messageType: messageType, linkedObject: entryHeader);
		Factory.Save();

		AssertProcessMessage($"Correct {messageType} Message", expectedToBeProcessed: true, message);
	}

	public void TestProcessMessageIncorrectApplicationCode()
	{
		const string incorrectApplicationCode = "AAA";
		var message = Factory.CreateCoreMessage(applicationCode: incorrectApplicationCode);
		Factory.Save();

		AssertProcessMessage("Message with incorrect ApplicationCode", false, message);
	}

	public void TestProcessMessageIncorrectDirection()
	{
		var message = Factory.CreateCoreMessage(direction: EDIInterchange.Direction.Receive);
		Factory.Save();

		AssertProcessMessage("Message with incorrect Direction", false, message);
	}

	public void TestProcessMessageIncorrectStatus()
	{
		var message = Factory.CreateCoreMessage(status: EDIMessageStatusList.Codes.Withdrawn);
		Factory.Save();

		AssertProcessMessage("Message with incorrect Status", false, message);
	}

	protected override void SetUp()
	{
		base.SetUp();
		CertificateHelper.SetUpTestCertificate();
	}
}
