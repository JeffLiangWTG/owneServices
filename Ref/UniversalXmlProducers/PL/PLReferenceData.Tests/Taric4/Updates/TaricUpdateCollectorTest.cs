using CargoWise.RefDbRepo.PLReferenceData.Business.Message;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

[TestFixture]
sealed class TaricUpdateCollectorTest
{

	[Test]
	public void TestGenerateAndSendTariffGetRequest()
	{
		var sysRef = "sysRef";
		var messageSendingMock = new Mock<IMessageSender>();
		new TaricUpdateCollector(messageSendingMock.Object).GenerateAndSendTariffGetRequest(sysRef);
		messageSendingMock.Verify(x => x.GetTariffUpdate(sysRef));
	}
}
