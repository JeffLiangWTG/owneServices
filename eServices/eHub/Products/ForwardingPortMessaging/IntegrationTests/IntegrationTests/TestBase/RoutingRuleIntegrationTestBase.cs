using System;
using System.Collections.Generic;
using System.Linq;
using eServices.eHubRoutingRuleEngine;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;
using Assert = NUnit.Framework.Assert;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.IntegrationTests
{
	[Property("DAT:CapabilityRequirements", "BIZTALK2020,VM,SQL,SQLFILESTREAM")]
	public abstract class RoutingRuleIntegrationTestBase : GenericTestBase
	{
		#region Implementation

		protected string Log
		{
			get
			{
				return string.Join("\r\n", logEntries);
			}
		}

		protected void AssertClient(string clientID, System.Collections.ObjectModel.Collection<Result> result)
		{
			Assert.AreEqual(1, result.Count());
			Assert.IsNotNull(result.First().Recipient, "Error: " + result.First().ToString());
			Assert.AreEqual(clientID, result.First().Recipient.CC_ID);
		}

		protected void AssertError(string errorCode, string errorMessage, System.Collections.ObjectModel.Collection<Result> result)
		{
			Assert.AreEqual(1, result.Count());
			var recipient = result.First().Recipient == null ? "<Empty>" : result.First().Recipient.CC_ID;
			Assert.Null(result.First().Recipient, result.First().Recipient == null ? "" : "Actual Recipient: " + result.First().Recipient.CC_ID);
			Assert.AreEqual(errorCode, result.First().ErrorCode, "ErrorCode");
			Assert.AreEqual(errorMessage, result.First().ErrorDescription, "ErrorDescription");
		}

		List<string> logEntries;
		protected ILog mockLogger;

		protected override void SetUpCore()
		{
			logEntries = new List<string>();
			mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
		}

		protected void InitialiseLoggerStubs(ILog mockLogger, List<string> logEntries)
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
		#endregion
	}

	sealed class DisposableAction : IDisposable
	{
		internal DisposableAction(Action create, Action dispose)
		{
			if (create == null) throw new ArgumentNullException("create");
			if (dispose == null) throw new ArgumentNullException("dispose");
			this.dispose = dispose;
			create();
		}

		readonly Action dispose;

		public void Dispose()
		{
			dispose();
		}
	}
}