using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Text.Json;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using NLog;
using NLog.Targets;
using static System.FormattableString;

namespace Enterprise.Freight.Forwarding.Logging
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer")]
	[SuppressMessage("Performance", "CA1810:Inline static fields initialization")]
	public static partial class PerformanceLogger
	{
		static PerformanceLogger()
		{
			_ = lazyProcessExitHandlerRegistered.Value;

			var productRegistration = ObjectFactory.Get<IProductRegistration>();
			var productRegistrationKey = productRegistration.Key;
			Code = $"{productRegistrationKey.EnterpriseCode}{productRegistrationKey.ServerCode}";

			ConfigNLog();
		}

		internal static void ConfigNLog()
		{
			var productRegistration = ObjectFactory.Get<IProductRegistration>();
			var productRegistrationKey = productRegistration.Key;
			var logFilePrefix = Invariant($"{productRegistrationKey.EnterpriseCode}{productRegistrationKey.ServerCode}_{Process.GetCurrentProcess().Id}");
			var logFilePath = Path.Combine(LogFileDir, Invariant($"{logFilePrefix}_${{date:format=yyyyMMdd}}.log"));

			var config = LogManager.Configuration ?? new NLog.Config.LoggingConfiguration();
			var fileTarget = config.FindTargetByName<FileTarget>(PerformanceLogTargetName);
			if (fileTarget == null)
			{
				fileTarget = new FileTarget
				{
					Name = PerformanceLogTargetName,
					FileName = logFilePath,
					CreateDirs = true,
					Layout = "${message}",
					KeepFileOpen = true,
					OpenFileCacheTimeout = 5,
					OpenFileCacheSize = 1,
					ConcurrentWrites = true,
					ConcurrentWriteAttemptDelay = 5,
					ArchiveAboveSize = 1024 * 1024 * 10,
					MaxArchiveFiles = 3,
					ArchiveFileName = Path.Combine(LogFileDir, "${logger}_{#}.txt"),
					ArchiveDateFormat = "yyyyMMdd.HHmmss-fff",
					ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
				};

				config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget, "*");
			}

			LogManager.Configuration = config;
		}

		internal static string LogFileDir => CargoWise.Common.CommonProgramData.GetCargoWiseDirectory("PerformanceTracker", Db.ServerName, Db.DatabaseName);

		internal static void Info(string message, string operation, string objectType, string pk, int numberOfRecords)
		{
			Info(message, operation, objectType, pk, numberOfRecords, TimeSpan.Zero);
		}

		internal static void Info(string message, string operation, string objectType, string pk, int numberOfRecords, TimeSpan duration)
		{
			var performanceInfo = new PerformanceInfo()
			{
				Timestamp = ZDateTime.UtcNow.ToDateTime(),
				Domain = DomainName,
				Host = HostName,
				Pid = currentProcessId,
				Server = Db.ServerName,
				Database = Db.DatabaseName,
				Code = Code,
				Object = objectType,
				Pk = pk,
				Records = numberOfRecords,
				Duration = Convert.ToInt32(duration.TotalMilliseconds),
				Op = operation,
				Version = ReleaseInfo.Instance.VersionNumber.ToVersion(),
				Message = message,
			};

			var jsonMessage = JsonSerializer.Serialize(performanceInfo);
			var logEventInfo = new LogEventInfo
			{
				Level = LogLevel.Info,
				Message = jsonMessage,
			};

			GetLogger().Log(logEventInfo);
		}

		internal static Logger GetLogger() => LogManager.GetLogger(PerformanceLogTargetName);

		public static void Flush()
		{
			LogManager.Flush();
		}

		public static void Shutdown()
		{
			LogManager.Shutdown();
		}

		static void OnProcessExit(object? sender, EventArgs e)
		{
			AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;

			try
			{
				Flush();
				Shutdown();
			}
			catch
			{
				// all exceptions are ignored
			}
		}

		static Lazy<bool> lazyProcessExitHandlerRegistered { get; } =
			new Lazy<bool>(() =>
			{
				AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
				return true;
			},
			LazyThreadSafetyMode.ExecutionAndPublication);

		internal static readonly string PerformanceLogTargetName = "Enterprise.Freight.Forwarding.PerformanceLog";
		readonly static string Code = string.Empty;
		readonly static string HostName = System.Environment.MachineName;
		readonly static string DomainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
		readonly static int currentProcessId = Process.GetCurrentProcess().Id;
	}
}
