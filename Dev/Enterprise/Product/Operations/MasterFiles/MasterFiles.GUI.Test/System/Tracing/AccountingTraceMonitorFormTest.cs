using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Diagnostics;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AccountingTraceMonitorFormTest : TestCaseWithFactory
	{
		public void TestMonitorLogsOnlyAllowedEvents()
		{
			var monitor = new TraceMonitor();
			using (var form = new TraceMonitorForm(monitor))
			{
				try
				{
					form.Show();

					foreach (var mappingItem in GetTraceLevelToEventMapping())
					{
						var traceSetting = monitor.TraceSourceSettingsList.Cast<TraceSourceSettings>()
							.Single(x => x.TraceSourceName.Equals(AccountingTraceSourceCodes.Http));
						traceSetting.TraceLevel = mappingItem.Key;

						AssertEquals("Start tracking", form.ButtonToggleStartStop.Text);
						AssertEquals("Should not be tracing", false, monitor.IsTracing);
						AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);

						//Start Tracking
						form.ButtonToggleStartStop.PerformClick();

						AssertEquals("Stop tracking", form.ButtonToggleStartStop.Text);
						AssertEquals("Should be tracking", true, monitor.IsTracing);
						AssertContains("Should have message", "Start tracking at", form.TextBoxStackTrace.Text);

						CreateTraceEvents();
						foreach (TraceEventType eventType in mappingItem.Value)
						{
							AssertContains("Should contain", eventType.ToString(), form.TextBoxStackTrace.Text);
						}

						//Clearing everyting
						form.ButtonClear.PerformClick();
						AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);

						//Stopping Tracking
						form.ButtonToggleStartStop.PerformClick();

						AssertEquals("Start tracking", form.ButtonToggleStartStop.Text);
						AssertEquals("Should not be tracking", false, monitor.IsTracing);
						AssertContains("Should have message", "Tracking stopped at", form.TextBoxStackTrace.Text);

						CreateTraceEvents();
						foreach (TraceEventType eventType in mappingItem.Value)
						{
							AssertNotContains("Should not contain", eventType.ToString(), form.TextBoxStackTrace.Text);
						}

						//Clearing everyting
						form.ButtonClear.PerformClick();
						AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);
					}
				}
				finally
				{
					monitor.ClearTraceSources();
					monitor.MessageWriter = null;
				}
			}
		}

		[RequiresSTA]
		public void TestOnlyAllowedComponentsAreAddedToTheLog()
		{
			var monitor = new TraceMonitor();
			var traceSetting = monitor.TraceSourceSettingsList.Cast<TraceSourceSettings>()
				.Single(x => x.TraceSourceName.Equals(AccountingTraceSourceCodes.Http));
			traceSetting.TraceLevel = TraceSourceLevels.Codes.All;
			var tracer = ObjectFactory.Get<ITracer>();

			using (var form = new TraceMonitorForm(monitor))
			{
				try
				{
					form.Show();

					foreach (var options in GetLoggingOptionsCombinations())
					{
						monitor.LogCallStack = options.LogCallStack;
						monitor.LogDateTime = options.LogDateTime;
						monitor.LogProcessId = options.LogProcessId;
						monitor.LogThreadId = options.LogThreadId;

						AssertEquals("Start tracking", form.ButtonToggleStartStop.Text);
						AssertEquals("Should not be tracing", false, monitor.IsTracing);
						AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);

						//Start Tracking
						form.ButtonToggleStartStop.PerformClick();

						AssertEquals("Stop tracking", form.ButtonToggleStartStop.Text);
						AssertEquals("Should be tracking", true, monitor.IsTracing);
						AssertContains("Should have message", "Start tracking at", form.TextBoxStackTrace.Text);

						tracer.TraceInformation(AccountingTraceSourceCodes.Http, () => "Adding Info");
						AssertOptionComponents(monitor.LogCallStack, "Callstack");
						AssertOptionComponents(monitor.LogDateTime, "DateTime");
						AssertOptionComponents(monitor.LogProcessId, "ProcessId");
						AssertOptionComponents(monitor.LogThreadId, "ThreadId");

						//Clearing everyting
						form.ButtonClear.PerformClick();
						AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);

						//Stopping Tracking
						form.ButtonToggleStartStop.PerformClick();

						AssertEquals("Start tracking", form.ButtonToggleStartStop.Text);
						AssertEquals("Should not be tracking", false, monitor.IsTracing);
						AssertContains("Should have message", "Tracking stopped at", form.TextBoxStackTrace.Text);

						//Clearing everyting
						form.ButtonClear.PerformClick();
						AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);
					}
				}
				finally
				{
					monitor.ClearTraceSources();
					monitor.MessageWriter = null;
				}

				void AssertOptionComponents(bool componentAllowed, string expectedText)
				{
					if (componentAllowed)
					{
						AssertContains(expectedText, form.TextBoxStackTrace.Text);
					}
					else
					{
						AssertNotContains(expectedText, form.TextBoxStackTrace.Text);
					}
				}
			}
		}

		void CreateTraceEvents()
		{
			var tracer = ObjectFactory.Get<ITracer>();
			foreach (var eventType in Enum.GetNames(typeof(TraceEventType)))
			{
				var eventTypeAsEnum = (TraceEventType)Enum.Parse(typeof(TraceEventType), eventType);

				switch (eventTypeAsEnum)
				{
					case TraceEventType.Critical:
						tracer.TraceCriticalEvent(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					case TraceEventType.Error:
						tracer.TraceErrorEvent(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					case TraceEventType.Warning:
						tracer.TraceWarningEvent(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					case TraceEventType.Information:
						tracer.TraceInformation(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					case TraceEventType.Verbose:
						tracer.TraceVerbose(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					case TraceEventType.Start:
						tracer.TraceStartingOfActivity(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					case TraceEventType.Stop:
						tracer.TraceStoppingOfActivity(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					case TraceEventType.Suspend:
						tracer.TraceSuspensionOfActivity(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					case TraceEventType.Resume:
						tracer.TraceResumptionOfActivity(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					case TraceEventType.Transfer:
						tracer.TraceTransferEvent(AccountingTraceSourceCodes.Http, () => eventType);
						break;
					default:
						break;
				}
			}
		}

		Dictionary<string, TraceEventType[]> GetTraceLevelToEventMapping()
		{
			return new Dictionary<string, TraceEventType[]>()
			{
				{ TraceSourceLevels.Codes.ActivityTracing, new TraceEventType[] { TraceEventType.Start, TraceEventType.Stop, TraceEventType.Suspend, TraceEventType.Transfer, TraceEventType.Resume } },
				{ TraceSourceLevels.Codes.All, new TraceEventType[] { TraceEventType.Critical, TraceEventType.Error, TraceEventType.Information, TraceEventType.Start, TraceEventType.Stop, TraceEventType.Suspend, TraceEventType.Transfer, TraceEventType.Resume, TraceEventType.Verbose, TraceEventType.Warning } },
				{ TraceSourceLevels.Codes.Critical, new TraceEventType[] { TraceEventType.Critical } },
				{ TraceSourceLevels.Codes.Error, new TraceEventType[] { TraceEventType.Critical, TraceEventType.Error } },
				{ TraceSourceLevels.Codes.Information, new TraceEventType[] { TraceEventType.Critical, TraceEventType.Error, TraceEventType.Warning,TraceEventType.Information  } },
				{ TraceSourceLevels.Codes.Off, Array.Empty<TraceEventType>() },
				{ TraceSourceLevels.Codes.Verbose, new TraceEventType[] { TraceEventType.Critical, TraceEventType.Error, TraceEventType.Warning, TraceEventType.Information, TraceEventType.Verbose } },
				{ TraceSourceLevels.Codes.Warning, new TraceEventType[] { TraceEventType.Critical, TraceEventType.Error, TraceEventType.Warning } }
			};
		}

		(bool LogCallStack, bool LogDateTime, bool LogThreadId, bool LogProcessId)[] GetLoggingOptionsCombinations()
		{
			return new (bool LogCallStack, bool LogDateTime, bool LogThreadId, bool LogProcessId)[]
			{
				(true, false, false, false),
				(true, true, false, false),
				(true, true, true, false),
				(true, true, true, true),
			};
		}
	}
}
