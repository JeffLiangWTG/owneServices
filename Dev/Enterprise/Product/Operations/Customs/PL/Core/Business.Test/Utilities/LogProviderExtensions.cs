using System;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

public static class LogProviderExtensions
{
	public static void AssertHasLogMessagePart(this IStmALogProvider logsProvider, string assertionDescription, Event eventType, string expectedLogMessage)
		=> Assertion.AssertCollectionContains(assertionDescription,
			logsProvider.Logs.GetAllLogs().Cast<StmALog>(),
			x => x.SL_Reference.Contains(expectedLogMessage.Substring(0, Math.Min(expectedLogMessage.Length, 1000))) && x.SL_SE_NKEvent == eventType.Code);

	public static void AssertHasNoLogMessagePart(this IStmALogProvider logsProvider, string assertionDescription, Event eventType, string expectedLogMessage)
		=> Assertion.AssertCollectionNotContains(assertionDescription,
			logsProvider.Logs.GetAllLogs().Cast<StmALog>(),
			x => x.SL_Reference.Contains(expectedLogMessage.Substring(0, Math.Min(expectedLogMessage.Length, 1000))) && x.SL_SE_NKEvent == eventType.Code);

	public static void AssertHasExactLogMessage(this IStmALogProvider logsProvider, string assertionDescription, Event eventType, string expectedLogMessage)
		=> Assertion.AssertCollectionContains(assertionDescription,
			logsProvider.Logs.GetAllLogs().Cast<StmALog>(),
			x => x.SL_Reference == expectedLogMessage.Substring(0, Math.Min(expectedLogMessage.Length, 1000)) && x.SL_SE_NKEvent == eventType.Code);

	public static void AssertHasLogMessageRegEx(this IStmALogProvider logsProvider, string assertionDescription, Event eventType, string expectedLogRegExPattern)
	{
		var expectedLogReg = new Regex(expectedLogRegExPattern);
		Assertion.AssertCollectionContains(assertionDescription,
			logsProvider.Logs.GetAllLogs().Cast<StmALog>(),
			x => expectedLogReg.IsMatch(x.SL_Reference) && x.SL_SE_NKEvent == eventType.Code);
	}
}
