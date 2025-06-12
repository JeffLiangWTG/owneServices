using Dat.Integration;
using Microsoft.Build.Framework;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using eHub.DatImplementation.TestObject.Deployment;

namespace eHub.DatImplementation.Deployment.Tests
{
    [TestFixture]
    public class BuildLoggerFixtures
    {
        [Test]
        public void TestBuildLogger_OnEvenetSourceEvents_LogExpectedMessages()
        {
            var stringBuilder = new StringBuilder();
            try
            {
                using (var writer = new StringWriter(stringBuilder))
                {
                    Console.SetOut(writer);
                    Console.SetError(writer);

                    var logger = new BuildLogger(new TaskLogger());
                    var eventSource = new TestEventSource();
                    logger.Initialize(eventSource);

                    var senderName = "BuildLoggerFixtures";
                    eventSource.RaiseEventMessageRaised(this, new BuildMessageEventArgs("RaiseEventMessageRaised", "help", senderName, MessageImportance.High));
                    eventSource.RaiseEventErrorRaised(this, new BuildErrorEventArgs("subcategory", "code", "file", 999, 99, 999, 99, "RaiseEventErrorRaised", "help", senderName));
                    eventSource.RaiseEventWarningRaised(this, new BuildWarningEventArgs("subcategory", "code", "file", 999, 99, 999, 99, "RaiseEventWarningRaised", "help", senderName));

                    eventSource.RaiseEventBuildStarted(this, new BuildStartedEventArgs("RaiseEventBuildStarted", "help"));
                    eventSource.RaiseEventBuildFinished(this, new BuildFinishedEventArgs("RaiseEventBuildStarted", "help", true));

                    eventSource.RaiseEventProjectStarted(this, new ProjectStartedEventArgs("RaiseEventProjectStarted", "help", senderName, "testTarget", new string[] { }, new string[] { }));
                    eventSource.RaiseEventProjectFinished(this, new ProjectFinishedEventArgs("RaiseEventProjectFinished", "help", senderName, true));

                    eventSource.RaiseEventTargetStarted(this, new TargetStartedEventArgs("RaiseEventTargetStarted", "help", "targetTest", senderName, "targetFile"));
                    eventSource.RaiseEventTargetFinished(this, new TargetFinishedEventArgs("RaiseEventTargetFinished", "help", "targetTest", senderName, "targetFile", true));

                    eventSource.RaiseEventTaskStarted(this, new TaskStartedEventArgs("RaiseEventTaskStarted", "help", senderName, "taskFile", "taskName"));
                    eventSource.RaiseEventTaskFinished(this, new TaskFinishedEventArgs("RaiseEventTaskFinished", "help", senderName, "taskFile", "taskName", true));

                    eventSource.RaiseEventCustomEventRaised(this, new TestCustomBuildEventArgs("RaiseEventCustomEventRaised", "help", senderName));
                    eventSource.RaiseEventStatusEventRaised(this, new TestBuildStatusEventArgs("RaiseEventStatusEventRaised", "help", senderName));
                    eventSource.RaiseEventAnyEventRaised(this, new TestBuildEventArgs("RaiseEventAnyEventRaised", "help", senderName));
                }

                var logs = stringBuilder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                Assert.AreEqual(14, logs.Length);
                Assert.IsTrue(logs.Any(x => x == "Info: BuildLoggerFixtures, RaiseEventMessageRaised"));
                Assert.IsTrue(logs.Any(x => x == "Error: BuildLoggerFixtures, RaiseEventErrorRaised"));
                Assert.IsTrue(logs.Any(x => x == "Warn: BuildLoggerFixtures, RaiseEventWarningRaised"));

                Assert.IsTrue(logs.Any(x => x.Contains("Starting	Build:")));
                Assert.IsTrue(logs.Any(x => x.Contains("Finished	Build:")));
                Assert.IsTrue(logs.Any(x => x.Contains("Starting	Project:")));
                Assert.IsTrue(logs.Any(x => x.Contains("Finished	Project:")));
                Assert.IsTrue(logs.Any(x => x.Contains("Starting	Target:")));
                Assert.IsTrue(logs.Any(x => x.Contains("Finished	Target:")));
                Assert.IsTrue(logs.Any(x => x.Contains("Starting	Task:")));
                Assert.IsTrue(logs.Any(x => x.Contains("Finished	Task:")));

                Assert.IsTrue(logs.Any(x => x == "Event: BuildLoggerFixtures, RaiseEventCustomEventRaised"));
                Assert.IsTrue(logs.Any(x => x == "Status: BuildLoggerFixtures, RaiseEventStatusEventRaised"));
                Assert.IsTrue(logs.Any(x => x == "Event: BuildLoggerFixtures, RaiseEventAnyEventRaised"));
            }
            finally
            {
                Console.SetOut(TextWriter.Null);
                Console.SetError(TextWriter.Null);
            }
        }
    }
}