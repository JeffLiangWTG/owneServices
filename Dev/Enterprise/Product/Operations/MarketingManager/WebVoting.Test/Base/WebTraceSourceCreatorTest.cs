using System.Collections.Specialized;
using System.Diagnostics;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	internal class WebTraceSourceCreatorTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Creates Event Log Source on Application")]
		public void TestTraceSourceCreation()
		{
			var session = new StringCollection();

			var traceSource = WebTraceSourceCreator.Initialise(session);
			AssertNull("Trace Source Null", traceSource);

			session.Add(WebTraceSourceCreator.EnableTracingVariable);

			traceSource = WebTraceSourceCreator.Initialise(session);
			AssertNotNull("Trace Source Not Null", traceSource);

			AssertEquals("Trace Source Listener Count", 1, traceSource.Listeners.Count);
			AssertType<EventLogTraceListener>(traceSource.Listeners[0]);
			AssertEquals("Trace Source Switch Level", SourceLevels.All, traceSource.Switch.Level);
		}
	}
}
