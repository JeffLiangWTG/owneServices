using System;
using System.Collections.Generic;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace eServices.eHubRoutingRuleEngine.Tests
{
	public abstract class TestBase
	{
		protected static void InitialiseLoggerStubs(ILog mockLogger, List<string> logEntries)
		{
			mockLogger.Stub(x => x.IsInfoEnabled).Return(true);
			mockLogger.Stub(x => x.Info(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[INFO ] " + (string)m)));
			mockLogger.Stub(x => x.Info(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[INFO ] " + (string)m + " [Exception:] " + e.Message)));
			mockLogger.Stub(x => x.InfoFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[INFO ] " + String.Format(m, o))));
			mockLogger.Stub(x => x.InfoFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[INFO ] " + String.Format(m, o) + " [Exception:] " + e.Message)));

			mockLogger.Stub(x => x.IsWarnEnabled).Return(true);
			mockLogger.Stub(x => x.Warn(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[WARN ] " + (string)m)));
			mockLogger.Stub(x => x.Warn(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[WARN ] " + (string)m + " [Exception:] " + e.Message)));
			mockLogger.Stub(x => x.WarnFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[WARN ] " + String.Format(m, o))));
			mockLogger.Stub(x => x.WarnFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[WARN ] " + String.Format(m, o) + " [Exception:] " + e.Message)));

			mockLogger.Stub(x => x.IsErrorEnabled).Return(true);
			mockLogger.Stub(x => x.Error(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[ERROR] " + (string)m)));
			mockLogger.Stub(x => x.Error(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[ERROR] " + (string)m + " [Exception:] " + e.Message)));
			mockLogger.Stub(x => x.ErrorFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[ERROR] " + String.Format(m, o))));
			mockLogger.Stub(x => x.ErrorFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[ERROR] " + String.Format(m, o) + " [Exception:] " + e.Message)));

			mockLogger.Stub(x => x.IsDebugEnabled).Return(true);
			mockLogger.Stub(x => x.Debug(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[DEBUG] " + (string)m)));
			mockLogger.Stub(x => x.Debug(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[DEBUG] " + (string)m + " [Exception:] " + e.Message)));
			mockLogger.Stub(x => x.DebugFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[DEBUG] " + String.Format(m, o))));
			mockLogger.Stub(x => x.DebugFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[DEBUG] " + String.Format(m, o) + " [Exception:] " + e.Message)));

			mockLogger.Stub(x => x.IsTraceEnabled).Return(true);
			mockLogger.Stub(x => x.Trace(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[TRACE] " + (string)m)));
			mockLogger.Stub(x => x.Trace(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[TRACE] " + (string)m + " [Exception:] " + e.Message)));
			mockLogger.Stub(x => x.TraceFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[TRACE] " + String.Format(m, o))));
			mockLogger.Stub(x => x.TraceFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[TRACE] " + String.Format(m, o) + " [Exception:] " + e.Message)));
		}

		protected static void AssertException<T>(Action action, Predicate<T> predicate = null) where T : Exception
		{
			try
			{
				action();
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(T));
				if (predicate != null) Assert.IsTrue(predicate((T)ex), "Exception did not meet specified criteria.");
				return;
			}

			Assert.Fail("Expected exception of type '{0}' was not thrown.", typeof(T));
		}
	}
}
