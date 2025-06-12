using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.OceanCarrierMessaging.PipelineComponents;
using Common.Logging;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Newtonsoft.Json;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	/// <summary>
	/// The following tests will let you check your change again a real eHubTransaction database. The log of these tests will be saved in Clipboard so you just need to Ctrl+V to your editor to see the logs.
	/// Please comment the Ignore line below if you want to run the test.
	/// </summary>
	[TestFixture]
	[Ignore("Please comment this Test Ignore line.")]
	public class RuleTests
	{
		const string eHubTransactionsContext_ConnectionString = "Data Source=ehubtransactions.db.wisegrid.net;Initial Catalog=eHubTransactions;App=eHub Gateway;User Id=ehubreader;Password=ehubrocks;ApplicationIntent=ReadOnly";
		[Test, Apartment(ApartmentState.STA)]
		public void Rule_Evaluate_SHIPPING_INSTRUCTION()
		{
			string testContent;
            var fileTestContent = @"CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests.TestRealDatabase.TestFiles.TestMessage.xml";
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileTestContent))
			{
				testContent = stream.ReadToEnd();
			}
			var message = new TestingMessage("HYEDCNUAT", "SHIPPING_INSTRUCTION", testContent);

			var results = Evaluate(message);

            var json = JsonConvert.SerializeObject(results, Formatting.Indented);
            Assert.AreEqual("", json, "result: " + json);
		}
		
		protected object Evaluate(TestingMessage message)
		{
			var pipelineContext = new PipelineContext();
            var component = new InboxDisassembleAndRoute();
			component.Disassemble(pipelineContext, message);
            int id = 0;
            var messages = component.MessageQueue.Select(x =>
            {
                List<object> list = new List<object>();
                IBaseMessageContext ctx = x.Context;
                
                for (int loop = 0; loop < ctx.CountProperties; loop++)
                {
                    string name;
                    string nspace;
                    ctx.ReadAt(loop, out name, out nspace);
                    var value = ctx.Read(name, nspace);
                    list.Add(new { property = nspace + "#" + name, value = value == null ? "" : value.ToString()});
                }
                return new { id = id++, list };
            }).ToArray();

            return new {count = messages.Count(), messages};
        }

		private static string GetLogAndCopyToClipBoard(List<string> logEntries)
		{
			var log = string.Join("\r\n", logEntries);
			Clipboard.SetText(log);
			return log;
		}

		List<string> logEntries;
		ILog mockLogger;

		[SetUp]
		public void Initialize()
		{
			logEntries = new List<string>();
			mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			InboxDisassembleAndRoute.GetLogger = (p) => mockLogger;
			var settings = ConfigurationManager.ConnectionStrings["eHubTransactionsContext"];
			var fi = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
			fi.SetValue(settings, false);
			settings.ConnectionString = eHubTransactionsContext_ConnectionString;
		}

		[TearDown]
		public void TearDown()
		{
			GetLogAndCopyToClipBoard(logEntries);
		}

		protected static void InitialiseLoggerStubs(ILog mockLogger, List<string> logEntries)
		{
			mockLogger.Stub(x => x.IsInfoEnabled).Return(true);
			mockLogger.Stub(x => x.Info(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[INFO ] " + (string)m)));
			mockLogger.Stub(x => x.Info(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[INFO ] " + (string)m + " [Exception:] " + e.ToString())));
			mockLogger.Stub(x => x.InfoFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[INFO ] " + String.Format(m, o))));
			mockLogger.Stub(x => x.InfoFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[INFO ] " + String.Format(m, o) + " [Exception:] " + e.ToString())));

			mockLogger.Stub(x => x.IsWarnEnabled).Return(true);
			mockLogger.Stub(x => x.Warn(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[WARN ] " + (string)m)));
			mockLogger.Stub(x => x.Warn(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[WARN ] " + (string)m + " [Exception:] " + e.ToString())));
			mockLogger.Stub(x => x.WarnFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[WARN ] " + String.Format(m, o))));
			mockLogger.Stub(x => x.WarnFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[WARN ] " + String.Format(m, o) + " [Exception:] " + e.ToString())));

			mockLogger.Stub(x => x.IsErrorEnabled).Return(true);
			mockLogger.Stub(x => x.Error(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[ERROR] " + (string)m)));
			mockLogger.Stub(x => x.Error(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[ERROR] " + (string)m + " [Exception:] " + e.ToString())));
			mockLogger.Stub(x => x.ErrorFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[ERROR] " + String.Format(m, o))));
			mockLogger.Stub(x => x.ErrorFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[ERROR] " + String.Format(m, o) + " [Exception:] " + e.ToString())));

			mockLogger.Stub(x => x.IsDebugEnabled).Return(true);
			mockLogger.Stub(x => x.Debug(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[DEBUG] " + (string)m)));
			mockLogger.Stub(x => x.Debug(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[DEBUG] " + (string)m + " [Exception:] " + e.ToString())));
			mockLogger.Stub(x => x.DebugFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[DEBUG] " + String.Format(m, o))));
			mockLogger.Stub(x => x.DebugFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[DEBUG] " + String.Format(m, o) + " [Exception:] " + e.ToString())));

			mockLogger.Stub(x => x.IsTraceEnabled).Return(true);
			mockLogger.Stub(x => x.Trace(Arg<object>.Is.Anything))
				.Do(new Action<object>(m => logEntries.Add("[TRACE] " + (string)m)));
			mockLogger.Stub(x => x.Trace(Arg<object>.Is.Anything, Arg<Exception>.Is.Anything))
				.Do(new Action<object, Exception>((m, e) => logEntries.Add("[TRACE] " + (string)m + " [Exception:] " + e.ToString())));
			mockLogger.Stub(x => x.TraceFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, object[]>((m, o) => logEntries.Add("[TRACE] " + String.Format(m, o))));
			mockLogger.Stub(x => x.TraceFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((m, e, o) => logEntries.Add("[TRACE] " + String.Format(m, o) + " [Exception:] " + e.ToString())));
		}
	}
}
