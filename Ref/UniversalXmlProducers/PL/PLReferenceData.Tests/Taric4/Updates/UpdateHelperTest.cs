using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4.Updates;

[TestFixture]
sealed class UpdateHelperTest
{
	[TestCase("2024-01-01", "2024-01-07", 6, "more than 1 day difference")]
	[TestCase("2024-01-01", "2024-01-02", 1, "1 day difference")]
	[TestCase("2024-01-02T00:00:00", "2024-01-01T23:59:59", 0, "until date and start date are within the same day")]
	[TestCase("2024-01-02T01:00:00", "2024-01-01T00:00:00", 0, "Time difference startDate > untilDate by 1 hour")]
	[TestCase("2024-01-01T00:00:00", "2024-01-01T00:00:00", 0, "exact date and time")]
	[TestCase("2024-01-02", "2024-01-01", 0, "start date is in the future")]
	public void TestGenerateUpdateRequests(string startDateInput, string untilDateInput, int expectedAmount, string message)
	{
		CultureInfo.CurrentCulture = new CultureInfo("pl-PL", false);
		var startDate = DateTime.Parse(startDateInput, CultureInfo.CurrentCulture);
		var untilDate = DateTime.Parse(untilDateInput, CultureInfo.CurrentCulture);

		var result = UpdateHelper.GenerateUpdateRequests(startDate, untilDate).ToArray();

		Assert.Multiple(() =>
		{
			Assert.AreEqual(expectedAmount, result.Length, $"{message} - Requests count");
			var requestDate = startDate;
			for(var i = 0; i < expectedAmount; i++)
			{
				var expectedStartDate = new DateTime(requestDate.Year, requestDate.Month, requestDate.Day, 0, 0, 0);
				var expectedEndDate = expectedStartDate.AddDays(1).AddTicks(-1);
				Assert.AreEqual(expectedStartDate, result[i].StartDate, $"{message} - {i} - StartDate");
				Assert.AreEqual(expectedEndDate, result[i].EndDate, $"{message} - {i} - EndDate");
				requestDate = requestDate.AddDays(1);
			}
		});
	}

	[Test]
	public void TestSendAndGetTaricUpdatesFromRequestList_Succeeded()
	{
		CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
		var utcNow = DateTime.UtcNow;

		var getTaricUpdate = Mock.Of<ITaricUpdateCollector>(x => x.GenerateAndSendTariffGetRequest(It.IsAny<string>()) == new byte[] { 11, 02 });
		var parseTaricUpdate = Mock.Of<ITaricUpdateParser>(x => x.ParseAndSave(It.IsAny<byte[]>()) == true);
		var messageSenderMock = Mock.Of<IMessageSender>(x => x.SendTariffUpdateRequest(It.IsAny<XDocument>()) == string.Empty);
		var requestTaricUpdate = new TaricUpdateInitiator(messageSenderMock);
		var requests = new [] { new UpdateRequest(utcNow.AddDays(-1), utcNow.AddTicks(-1)) };

		var result = UpdateHelper.SendAndGetTaricUpdatesFromRequestList(requests, requestTaricUpdate, getTaricUpdate, parseTaricUpdate, shouldWaitForResponse: false);
		Assert.AreEqual(true, result, "Send and retrieve succeeded");
	}

	[Test]
	public void TestSendAndGetTaricUpdatesFromRequestList_Failed()
	{
		CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
		var utcNow = DateTime.UtcNow;

		var getTaricUpdate = Mock.Of<ITaricUpdateCollector>(x => x.GenerateAndSendTariffGetRequest(It.IsAny<string>()) == null);
		var parseTaricUpdate = Mock.Of<ITaricUpdateParser>(x => x.ParseAndSave(It.IsAny<byte[]>()) == true);
		var messageSenderMock = Mock.Of<IMessageSender>(x => x.SendTariffUpdateRequest(It.IsAny<XDocument>()) == string.Empty);
		var requestTaricUpdate = new TaricUpdateInitiator(messageSenderMock);
		var requests = new [] { new UpdateRequest(utcNow.AddDays(-1), utcNow.AddTicks(-1)) };

		using var consoleOutput = new StringWriter();
		Console.SetError(consoleOutput);

		var result = UpdateHelper.SendAndGetTaricUpdatesFromRequestList(requests, requestTaricUpdate, getTaricUpdate, parseTaricUpdate, shouldWaitForResponse: false);
		Assert.AreEqual(false, result, "Send and retrieve failed");
	}

	[TestCaseSource(nameof(TestGetTaricUpdatesCases))]
	public void TestGetTaricUpdates(byte[] tariffGetRequestData, bool expectedParseAndSaveResult, string expectedOutput)
	{
		var getTaricUpdateProvider = Mock.Of<ITaricUpdateCollector>(x => x.GenerateAndSendTariffGetRequest(It.IsAny<string>()) == tariffGetRequestData);
		var parseTaricUpdateProvider = Mock.Of<ITaricUpdateParser>(x => x.ParseAndSave(It.IsAny<byte[]>()) == expectedParseAndSaveResult);
		using var consoleOutput = new StringWriter();
		var updateRequestResponseList = new UpdateRequestResponse[]
		{
			new(new UpdateRequest(new DateTime(2024, 1, 1), new DateTime(2024, 1, 2, 23, 59, 59)), string.Empty),
			new(new UpdateRequest(new DateTime(2024, 1, 2, 4, 05, 08), new DateTime(2024, 1, 3, 21, 59, 13)), string.Empty),
			new(new UpdateRequest(new DateTime(2024, 1, 3, 22, 03, 59), new DateTime(2024, 1, 4)), string.Empty),
		};

		Console.SetError(consoleOutput);

		UpdateHelper.GetTaricUpdates(getTaricUpdateProvider, parseTaricUpdateProvider, updateRequestResponseList, shouldWaitForResponse: false);

		Assert.AreEqual(expectedOutput, consoleOutput.ToString());
	}

	static IEnumerable<TestCaseData> TestGetTaricUpdatesCases
	{
		get
		{
			yield return new TestCaseData(
				null,
				true,
				"Failed getting update for request 01-01-2024 00:00:00 - 02-01-2024 23:59:59 - Maximum tries exceeded\r\n" +
				"Failed getting update for request 02-01-2024 04:05:08 - 03-01-2024 21:59:13 - Maximum tries exceeded\r\n" +
				"Failed getting update for request 03-01-2024 22:03:59 - 04-01-2024 00:00:00 - Maximum tries exceeded\r\n");

			yield return new TestCaseData(
				new byte[] { },
				true,
				string.Empty);

			yield return new TestCaseData(
				new byte[] { },
				false,
				"Failed getting update for request 01-01-2024 00:00:00 - 02-01-2024 23:59:59 - Maximum tries exceeded\r\n" +
				"Failed getting update for request 02-01-2024 04:05:08 - 03-01-2024 21:59:13 - Maximum tries exceeded\r\n" +
				"Failed getting update for request 03-01-2024 22:03:59 - 04-01-2024 00:00:00 - Maximum tries exceeded\r\n");
		}
	}
}
