using System.Linq;
using System.Net;
using CargoWise.Common;
using Enterprise.Messaging.Business.MessageProcessor;
using NUnit.Framework;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.PL.Business.Testing;

public static class InterchangeUnpackResultExtensions
{
	public static void Assert(this IUniversalCustomsInterchangeUnpackerResult unpackResult,
		ExpectedUnpackResult expectedResult,
		LogExpectedOn? logExpectedOnByDefault = null,
		string assertionsPrefix = "")
	{
		AssertEquals(assertionsPrefix + (expectedResult.ExpectedSuccess ? " Success is expected" : " Failure is expected"), expectedResult.ExpectedSuccess, unpackResult.IsSuccess);
		if (unpackResult.IsSuccess != expectedResult.ExpectedSuccess)
		{
			return;
		}

		if (!expectedResult.ExpectedSuccess)
		{
			AssertEquals(assertionsPrefix + " Unpack error reason is expected", expectedResult.ExpectedErrorReason, unpackResult.ErrorReason);
		}
		else
		{
			var expectedMessages = expectedResult.ExpectedMessages ?? [];
			var unpackedMessages = unpackResult.EdiMessages ?? [];

			var foundExpectedUnpackedMessages = unpackedMessages.Where(message
				=> expectedResult.ExpectedMessages.Any(expected => message == expected)).ToList();
			if (foundExpectedUnpackedMessages.Count != unpackedMessages.Count)
			{
				var unexpectedUnpackedMessages = unpackedMessages.Except(foundExpectedUnpackedMessages).ToList();
				Assertion.HtmlFail(WebUtility.HtmlEncode(
					$"{assertionsPrefix} {unexpectedUnpackedMessages.Count} unexpected messages created"));
				foreach (var unexpectedUnpackedMessage in unexpectedUnpackedMessages)
				{
					Assertion.HtmlFail(WebUtility.HtmlEncode(
						$"{assertionsPrefix} non expected message: {unexpectedUnpackedMessage.GetAssertionTextRepresentation()}"));
				}
			}
			if (foundExpectedUnpackedMessages.Count != expectedMessages.Count)
			{
				var notFoundExpectedUnpackedMessages = expectedMessages.Where(expected => foundExpectedUnpackedMessages.All(message => message != expected)).ToList();
				Assertion.HtmlFail($"{assertionsPrefix} {notFoundExpectedUnpackedMessages.Count} expected messages not created");
				foreach (var notFoundExpectedUnpackedMessage in notFoundExpectedUnpackedMessages)
				{
					Assertion.HtmlFail(WebUtility.HtmlEncode(
						$"{assertionsPrefix} not found message: {notFoundExpectedUnpackedMessage.ToString()}"));
				}
			}
		}

		foreach (var expectedLog in expectedResult.ExpectedLogs ?? [])
		{
			var logExpectedOn = expectedLog.ExpectedOn ?? logExpectedOnByDefault;
			if (!logExpectedOn.HasValue)
			{
				Assertion.HtmlFail(WebUtility.HtmlEncode(
					$"Expected log {expectedLog.ToString()} does not have log source and the default source not provided!"));
			}

			if (expectedLog.EventCode != null)
			{
				foreach (var logsProvider in logExpectedOn.Value.BusinessObjects.WhereNotNull() ?? [])
				{
					expectedLog.AssertLog(assertionsPrefix + $" expected log: {expectedLog.Text}", logsProvider);
				}
			}

			if (expectedLog.LogType.HasValue && logExpectedOn.Value.ServiceLog is { } serviceLogs)
			{
				expectedLog.AssertLog(assertionsPrefix + $" expected log: {expectedLog.Text}", serviceLogs);
			}
		}
	}
}
