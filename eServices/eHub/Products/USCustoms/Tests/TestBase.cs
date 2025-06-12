using System;
using System.Collections.Generic;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eServices.USCustoms.Tests
{
	public abstract class TestBase
	{
		protected static void InitialiseLoggerStubs(ILog mockLogger, List<string> logEntries)
		{
			mockLogger.Stub(x => x.IsDebugEnabled).Return(true);

			mockLogger.Stub(x => x.Info(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[INF] " + (string)m)));
			mockLogger.Stub(x => x.Info(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[INF] " + (string)m + " [Exception:] " + e.Message)));
			mockLogger.Stub(x => x.InfoFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[INF] " + String.Format(m, o))));
			mockLogger.Stub(x => x.InfoFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[INF] " + String.Format(m, o) + " [Exception:] " + e.Message)));

			mockLogger.Stub(x => x.Warn(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[WRN] " + (string)m)));
			mockLogger.Stub(x => x.Warn(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[WRN] " + (string)m + " [Exception:] " + e.Message)));
			mockLogger.Stub(x => x.WarnFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[WRN] " + String.Format(m, o))));
			mockLogger.Stub(x => x.WarnFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[WRN] " + String.Format(m, o) + " [Exception:] " + e.Message)));

			mockLogger.Stub(x => x.Error(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[ERR] " + (string)m)));
			mockLogger.Stub(x => x.Error(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[ERR] " + (string)m + " [Exception:] " + e.Message)));
			mockLogger.Stub(x => x.ErrorFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[ERR] " + String.Format(m, o))));
			mockLogger.Stub(x => x.ErrorFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[ERR] " + String.Format(m, o) + " [Exception:] " + e.Message)));

			mockLogger.Stub(x => x.Debug(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[DBG] " + (string)m)));
			mockLogger.Stub(x => x.Debug(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[DBG] " + (string)m + " [Exception:] " + e.Message)));
			mockLogger.Stub(x => x.DebugFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[DBG] " + String.Format(m, o))));
			mockLogger.Stub(x => x.DebugFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[DBG] " + String.Format(m, o) + " [Exception:] " + e.Message)));
		}

		protected internal static void AssertException<T>(Action action, Predicate<T> predicate = null, string failureMessage = null) 
            where T : Exception
		{
			try
			{
				action();
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(T));
                if (predicate != null)
                {
                    failureMessage = failureMessage ?? "Exception did not meet specified criteria.";
                    Assert.IsTrue(predicate((T)ex), String.Format("{0}\r\nActual Exception: {1}: {2}", failureMessage, ex.GetType(), ex.Message));
                }
				return;
			}

			Assert.Fail("Expected exception of type '{0}' was not thrown.", typeof(T));
		}
	}
}
