using System.Diagnostics;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Common.ErrorReporting;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner
{
	public static class ProcessorRunnerHelper
	{
		public static int RunAppAsProcessor(ProcessorInfo info, ILogHelper logHelper, string jobName, string exePath, string args, string[] configPaths, Action<IProcessWrapper>[] onStarted = null)
		{
			Argument.NotNull(logHelper, nameof(logHelper));
			Argument.NotNullOrEmpty(jobName, nameof(jobName));
			Argument.NotNullOrEmpty(exePath, nameof(exePath));
			Argument.NotNull(info, nameof(info));

			var exitCode = 0;
			using var errorReportingWrapper = new ErrorReportingWrapper();
			RunProcessor(info, logHelper, errorReportingWrapper, () =>
			{
				exitCode = RunApp(logHelper, jobName, exePath, args, configPaths, onStarted);
				return Tuple.Create(0, exitCode == 0);
			});
			return exitCode;
		}

		public static int RunApp(ILogHelper logHelper, string jobName, string exePath, string args, string[] configPaths, Action<IProcessWrapper>[] onStarted = null)
		{
			Argument.NotNull(logHelper, nameof(logHelper));
			Argument.NotNullOrEmpty(jobName, nameof(jobName));
			Argument.NotNullOrEmpty(exePath, nameof(exePath));

			using var errorReportingWrapper = new ErrorReportingWrapper();
			return RunApp(logHelper, jobName, exePath, args, errorReportingWrapper, configPaths, onStarted);
		}

		public static int RunApp(ILogHelper logHelper, string jobName, string exePath, string args, IErrorReportingWrapper errorReportingWrapper, string[] configPaths, Action<IProcessWrapper>[] onStarted = null)
		{
			Argument.NotNull(logHelper, nameof(logHelper));
			Argument.NotNullOrEmpty(jobName, nameof(jobName));
			Argument.NotNullOrEmpty(exePath, nameof(exePath));
			Argument.NotNull(errorReportingWrapper, nameof(errorReportingWrapper));

			List<string> appNameLogs = [];
			var appName = string.Empty;
			errorReportingWrapper.AppendBaseKey(jobName);

			var process = new Process();
			process.StartInfo.FileName = exePath;
			process.StartInfo.Arguments = args ?? string.Empty;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
			process.OutputDataReceived += (sender, d) =>
			{
				if (d.Data != null)
				{
					if (d.Data.StartsWith(FlagHelper.GetFlag(UXMLProducerHelper.AppContext), StringComparison.OrdinalIgnoreCase))
					{
						if (!logHelper.TrySetAppContext(d.Data.Split(':')[1], out var message))
						{
							errorReportingWrapper.AppendDescription(message);
							errorReportingWrapper.PostCrashReport();
							logHelper.LogError(message);
						}
					}
					if (d.Data.StartsWith(FlagHelper.GetFlag(UXMLProducerHelper.SourceDataAppName), StringComparison.OrdinalIgnoreCase))
					{
						appNameLogs.Add(d.Data);
						errorReportingWrapper.AppendBaseKey(d.Data);
					}
					if (d.Data.StartsWith(FlagHelper.GetFlag(UXMLProducerHelper.SubSource), StringComparison.OrdinalIgnoreCase))
					{
						if (!logHelper.TrySetSubSource(d.Data.Split(':')[1], out var message))
						{
							errorReportingWrapper.AppendDescription(message);
							errorReportingWrapper.PostCrashReport();
							logHelper.LogError(message);
						}
						errorReportingWrapper.AppendBaseKey(d.Data);
					}
					if (d.Data.StartsWith(FlagHelper.GetFlag(UXMLProducerHelper.SqlPerformance), StringComparison.OrdinalIgnoreCase))
					{
						logHelper.LogSqlDurationInfo(d.Data.Split(':')[1], double.Parse(d.Data.Split(':')[2], CultureInfo.InvariantCulture));
						return;
					}

					logHelper.LogInfo(d.Data);
				}
			};
			bool hasUnhandledExceptionOccurred = false;
			process.ErrorDataReceived += (sender, d) =>
			{
				if (d.Data != null && !hasUnhandledExceptionOccurred)
				{
					if (d.Data.StartsWith(FlagHelper.GetFlag(UXMLProducerHelper.AppException), StringComparison.OrdinalIgnoreCase))
					{
						var exceptionJson = d.Data.Substring(17);
						LogExceptionMessage(logHelper, errorReportingWrapper, exceptionJson);
					}
					else if (d.Data.StartsWith(FlagHelper.GetFlag(UXMLProducerHelper.UnhandledException), StringComparison.OrdinalIgnoreCase))
					{
						var exceptionJson = d.Data.Substring(23);
						LogExceptionMessage(logHelper, errorReportingWrapper, exceptionJson);
						hasUnhandledExceptionOccurred = true;
					}
					else
					{
						errorReportingWrapper.AppendDescription(d.Data);
						logHelper.LogError(d.Data);
					}
				}
			};

			var startTime = DateTime.UtcNow;
			process.Start();
			logHelper.SetProcessId(process.Id);
			if (onStarted != null)
			{
				Array.ForEach(onStarted, x => x(new ProcessWrapper(process)));
			}

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit();

			logHelper.LogMonitorInfo(startTime, (DateTime.UtcNow - startTime).TotalMinutes, process.ExitCode == 0);
			if (appNameLogs.Count > 0)
			{
				appName = appNameLogs.FirstOrDefault()!.Split(':')[1];
			}
			var appPath = exePath;
			if (!string.IsNullOrEmpty(appName))
			{
				foreach (var configPath in configPaths)
				{
					appPath = Path.Combine(configPath, appName);
					if (File.Exists(appPath))
					{
						break;
					}
				}
			}

			if (errorReportingWrapper.HasErrorToReport ||
				(process.ExitCode != 0 && UXMLProducerHelper.ExitWIthProducerStatus(process.ExitCode) && ShouldReportIssue(appPath, process.ExitCode)))
			{
				if (!errorReportingWrapper.HasErrorToReport)
				{
					errorReportingWrapper.AppendDescription($"{jobName} exits with status {process.ExitCode}.");
				}
				errorReportingWrapper.PostCrashReport();
			}

			var executeResult = process.ExitCode == 0 ? "succeeded" : "failed";
			logHelper.LogInfo($"Process executed {executeResult}.");
			return process.ExitCode;
		}

		static void LogExceptionMessage(ILogHelper logHelper, IErrorReportingWrapper errorReportingWrapper, string exceptionJson)
		{
			Exception exception = null;
			if (!ExceptionExtensions.TryDeserialiseException(exceptionJson, ref exception))
			{
				logHelper.LogWarn("Fail to deserialize the Exception.");
				exception = ExceptionExtensions.CreateException(exceptionJson);
			}
			errorReportingWrapper.AddException(exception);
			errorReportingWrapper.AppendDescription(exception.GetUnWrappedMessage());
			logHelper.LogError(exception.GetUnWrappedMessage(), exception);
		}

		public static void RunProcessor(ProcessorInfo info, Func<int> action)
		{
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(info, nameof(info));
			using var logWrapper = new LogWrapper(info.JobName);
			var logHelper = new LogHelper(logWrapper, info.JobName);
			using var errorReportingWrapper = new ErrorReportingWrapper();
			RunProcessor(info, logHelper, errorReportingWrapper, action);
		}

		public static void RunProcessor(ProcessorInfo info, ILogHelper logHelper, IErrorReportingWrapper errorReportingWrapper, Func<int> action)
		{
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(info, nameof(info));
			Argument.NotNull(logHelper, nameof(logHelper));
			Argument.NotNull(errorReportingWrapper, nameof(errorReportingWrapper));

			RunProcessor(info, logHelper, errorReportingWrapper, () => Tuple.Create(action(), true));
		}

		static void RunProcessor(ProcessorInfo info, ILogHelper logHelper, IErrorReportingWrapper errorReportingWrapper, Func<Tuple<int, bool>> action)
		{
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(info, nameof(info));
			Argument.NotNull(logHelper, nameof(logHelper));
			Argument.NotNull(errorReportingWrapper, nameof(errorReportingWrapper));

			using (var stage = new StagingRepository(DbConnectionStringManager.StagingConnectionString))
			{
				new ProcessorRunner(logHelper, stage, errorReportingWrapper).Run(info, action);
			}
		}

#if DEBUG
		public
#endif
		static bool ShouldReportIssue(string appPath, int failureCode)
		{
			var configValue = new IssueReportingConfigProvider().GetIssueReportingExitCodeValue(appPath);
			var value = int.Parse(configValue, CultureInfo.InvariantCulture);
			return (value & failureCode) > 0;
		}
	}
}
