using System.Diagnostics;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class EventLogTest
{
	[Test]
	public void SafeWriteToEventLogDoesNotThrowExceptionsTest()
	{
		SafeEventLogExtensions.SafeWriteEntryToApplicationLog("Test Message", EventLogEntryType.Information);
	}
}
