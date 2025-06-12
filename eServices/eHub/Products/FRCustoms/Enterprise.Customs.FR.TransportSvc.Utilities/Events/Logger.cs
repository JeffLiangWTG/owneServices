using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.eHub.Adapter;

namespace Enterprise.Customs.FR.TransportSvc.Utilities
{
	[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
	[SuppressMessage("CargoWiseOne", "CW1060:Do not use System.DateTime.Now Rule")]
	public class Logger
	{
		public Logger()
		{
			ManageLogFile();
		}

		public static string CurrentDateLogFilePath => GetCurrentDateLogFilePath();

		static string GetCurrentDateLogFilePath() => Path.Combine(ApplicationConfig.Instance.LogPath, string.Format("{0}.log", string.Format("{0:yyyy-MM-dd}", DateTime.Now)).Replace("/", "-"));

		static void GetOrCreateCurrentLogFilePath()
		{
			var fullLogFileName = CurrentDateLogFilePath;
			var rootPath = Path.GetFullPath(ApplicationConfig.Instance.LogPath);

			if (!Directory.Exists(rootPath))
			{
				Directory.CreateDirectory(rootPath);
			}

			if (!System.IO.File.Exists(fullLogFileName))
			{
				var f = System.IO.File.Create(fullLogFileName);
				f.Close();
			}
		}

		static void ManageLogFile()
		{
			if (LogHasReachedMaxSize())
			{
				BackUpCurrentLogFile();
			}

			GetOrCreateCurrentLogFilePath();
		}

		static void BackUpCurrentLogFile()
		{
			var fileToBackup = GetCurrentDateLogFilePath();
			var shortTime = string.Format("{0:HH-mm-ss}", DateTime.Now);
			var backupFileName = Path.Combine(ApplicationConfig.Instance.LogPath, Path.GetFileNameWithoutExtension(fileToBackup)) + "-" + shortTime + ".log";
			File.Move(fileToBackup, backupFileName);
		}

		static bool LogHasReachedMaxSize() => File.Exists(CurrentDateLogFilePath) && new FileInfo(CurrentDateLogFilePath).Length / 1024 > LogMaxSize;

		static int LogMaxSize => GetLogMaxSize();

		public static int GetLogMaxSize()
		{
			int.TryParse(ApplicationConfig.Instance.Taille_max_logs, out var result);
			return result;
		}

		public void AddNotification(Notification.MessageType type, Notification.Events myEvent, bool verboseModeOnly)
		{
			if (verboseModeOnly && !IsVerboseMode)
			{
				return;
			}
			new Notification(type, myEvent).Add(IsConsoleOut, this);
		}

		public void AddNotification(string message, Notification.MessageType type, Notification.Events myEvent, bool verboseModeOnly)
		{
			if (verboseModeOnly && !IsVerboseMode)
			{
				return;
			}
			new Notification(message, type, myEvent).Add(IsConsoleOut, this);
		}

		public void AddMessageListNotification(List<IeHubMessage> messageList)
		{
			if (messageList != null)
			{
				var messageCount = messageList.Count;
				var plural = messageCount > 0 ? "s" : string.Empty;
				AddNotification($"{messageCount} eHub message{plural} file{plural} received.", Notification.MessageType.Information, Notification.Events.MessageReception, messageCount == 0);
			}
		}

		public void WriteToFile(string message)
		{
			ManageLogFile();

			lock (helperLock)
			{
				var index = 0;
				var maxtry = 3;

				while (index < maxtry)
				{
					try
					{
						using (var writer = new StreamWriter(CurrentDateLogFilePath, true))
						{
							writer.WriteLine(message);
							break;
						}
					}
					catch
					{
					}
					finally
					{
						index++;
					}
				}
			}

			if (message != null && message.Contains("OnShutdown"))
			{
				BackUpCurrentLogFile();
			}
		}

		public static void DumpLog()
		{
			File.WriteAllText(CurrentDateLogFilePath, string.Empty);
		}

		static bool IsConsoleOut => ApplicationConfig.Instance.DisplayMessagesInConsole == "1";
		static bool IsVerboseMode => ApplicationConfig.Instance.VerboseMode == "1";

		readonly object helperLock = new object();
	}
}
