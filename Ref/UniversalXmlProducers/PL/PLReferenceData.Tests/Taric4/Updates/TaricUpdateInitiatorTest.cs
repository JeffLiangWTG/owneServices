using System;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4.Updates;

[TestFixture]
sealed class TaricUpdateInitiatorTest
{
	[Test]
	public void TestGenerateTariffUpdateRequest()
	{
		var expectedResult = "sysRef";
		var expected = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4.TestFiles.Output.test_updateTariff.xml"));
		var startDate = new DateTime(Configuration.Taric.StartYear, Configuration.Taric.StartMonth, Configuration.Taric.StartDay, Configuration.Taric.StartHour, Configuration.Taric.StartMinute, 0);
		var endDate = new DateTime(Configuration.Taric.EndYear, Configuration.Taric.EndMonth, Configuration.Taric.EndDay, Configuration.Taric.EndHour, Configuration.Taric.EndMinute, 59);
		var messageSendingMock = new Mock<IMessageSender>();
		messageSendingMock.Setup(x => x.SendTariffUpdateRequest(It.IsAny<XDocument>())).Returns(expectedResult);
		var updateRequest = new UpdateRequest(startDate, endDate);
		var result = new TaricUpdateInitiator(messageSendingMock.Object).GenerateAndSendTariffUpdateRequest(updateRequest);
		messageSendingMock.Verify(x => x.SendTariffUpdateRequest(It.Is<XDocument>(y => expected.ToString() == y.ToString())));

		Assert.AreEqual(expectedResult, result);
	}
}
