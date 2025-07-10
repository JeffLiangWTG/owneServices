using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

public static class LoggingInformationExtensions
{
	public static void AssertHasLogMessagePart(this ISimpleLogResult logsProvider, string assertionDescription, LogType eventType, string expectedLogMessage)
		=> Assertion.AssertCollectionContains("Service has fault log message", logsProvider.Logs,
			x => x.Type == eventType && x.Message.Contains(expectedLogMessage.Substring(0, Math.Min(expectedLogMessage.Length, 1000))));

	public static void AssertHasNoLogMessagePart(this ISimpleLogResult logsProvider, string assertionDescription, LogType eventType, string expectedLogMessage)
		=> Assertion.AssertCollectionNotContains("Service has fault log message", logsProvider.Logs,
			x => x.Type == eventType && x.Message.Contains(expectedLogMessage.Substring(0, Math.Min(expectedLogMessage.Length, 1000))));

	public static void AssertHasExactLogMessage(this ISimpleLogResult logsProvider, string assertionDescription, LogType eventType, string expectedLogMessage)
		=> Assertion.AssertCollectionContains("Service has fault log message", logsProvider.Logs,
			x => x.Type == eventType && x.Message == expectedLogMessage.Substring(0, Math.Min(expectedLogMessage.Length, 1000)));

	public static void AssertHasLogMessageRegEx(this ISimpleLogResult logsProvider, string assertionDescription, LogType eventType, string expectedLogRegExPattern)
	{
		var expectedLogReg = new Regex(expectedLogRegExPattern);
		Assertion.AssertCollectionContains("Service has fault log message", logsProvider.Logs,
			x => x.Type == eventType && expectedLogReg.IsMatch(x.Message));
	}
}
