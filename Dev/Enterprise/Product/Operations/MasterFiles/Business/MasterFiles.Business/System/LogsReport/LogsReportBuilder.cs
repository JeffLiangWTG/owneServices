using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.Abstractions;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business.LogsReport
{
	public class LogsReportBuilder
	{
		public LogsReportBuilder(ZDate dateFromUtc, ZDate dateToUtc, string serviceTaskCode, string incidentNumber, string hostServerName, string hostDBName, string hostConnectionServerName, int maxZipSize = 100, int maxLogFileSize = 2000)
		{
			this.DateFromUtc = dateFromUtc;
			this.DateToUtc = dateToUtc;
			this.ServiceTaskCode = serviceTaskCode;
			this.IncidentNumber = incidentNumber;
			this.HostServerName = hostServerName.Replace("\\", "$");
			this.HostConnectionServerName = hostConnectionServerName.Replace("\\", "$");
			//HostServerName is in format SYD-SSQL-20B or LON-SSQL-20B\MSSQLSERVER2 or SC001801\SQL_SC001801, because we are sending contents of Host Server Name field.
			//The \ gets replaced by a $. This is correct - e.g. the folder might look like ...\Process Controller\SYD-WDRS-6$V2014
			this.HostDBName = hostDBName;
			this.MaxZipSize = maxZipSize;
			this.MaxLogFileSize = maxLogFileSize;
		}

		public ZDate DateFromUtc { get; set; }
		public ZDate DateToUtc { get; set; }
		public string ServiceTaskCode { get; set; }
		public string IncidentNumber { get; set; }
		public string HostServerName { get; set; }
		public string HostDBName { get; set; }
		public string HostConnectionServerName { get; set; }
		public int MaxZipSize { get; set; }
		public int MaxLogFileSize { get; set; }

		public IEnumerable<string> LogFilesDirectories
		{
			get
			{
#if DEBUG
				if (LogFilesDirectoriesForTest.Value != null)
				{
					foreach (string directory in LogFilesDirectoriesForTest.Value)
					{
						yield return directory;
					}
				}
#endif
				if (!string.IsNullOrWhiteSpace(HostConnectionServerName))
				{
					yield return ServiceManagerHelper.GetLogFilesDirectory(HostConnectionServerName, HostDBName);
					yield return Regex.Replace(ServiceManagerHelper.GetLogFilesDirectory(HostConnectionServerName, HostDBName), @"\$[^\\/]+", "");
				}

				if (!string.IsNullOrWhiteSpace(HostServerName))
				{
					yield return ServiceManagerHelper.GetLogFilesDirectory(HostServerName, HostDBName);
					yield return Regex.Replace(ServiceManagerHelper.GetLogFilesDirectory(HostServerName, HostDBName), @"\$[^\\/]+", "");
				}

				foreach (var hostLogProvider in LogViewer.HostLogProviderCollection)
				{
					var host = hostLogProvider.Hostname;
					var logFilesUri = ServiceManagerHelper.GetLogFilesUri(host).OriginalString;
					// This code expect the host to be equal, although the host in Uri is case-insensitive: https://datatracker.ietf.org/doc/html/rfc3986#section-3.2.2
					logFilesUri = logFilesUri.Replace(host.ToLower(), host);
					yield return logFilesUri;
					yield return Regex.Replace(logFilesUri, @"\$[^\\/]+", "");
				}

				yield return ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName);
			}
		}

		bool success;

#if DEBUG
		public virtual
#endif
		IServiceTaskLogViewer LogViewer
		{ get => logViewer ?? (logViewer = ObjectFactory.Get<IServiceTaskLogViewer>()); }
		IServiceTaskLogViewer logViewer;

#if DEBUG
		[ThreadSafe]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "For test only")]
		public static Overridable<List<string>> LogFilesDirectoriesForTest = new Overridable<List<string>>();
#endif

		public IEnumerable<string> LogFileNames
		{
			get
			{
				const string dateFormat = "yyyyMMdd";
				List<string> result = new List<string>();

				foreach (string logFileDirectory in LogFilesDirectories.Distinct())
				{
					if (Directory.Exists(logFileDirectory))
					{
						try
						{
							foreach (string currentServiceTaskCode in ServiceTaskCode.Split(','))
							{
								ZDate currentDate = DateFromUtc;
								while (currentDate <= DateToUtc)
								{
									string fileSearchPattern = currentServiceTaskCode + "_" + currentDate.ToString(dateFormat) + "*";
									foreach (string file in Directory.EnumerateFiles(logFileDirectory, fileSearchPattern, SearchOption.TopDirectoryOnly)) //search pattern seems correct to me
									{
										result.Add(file);
										success = true;
									}
									currentDate = currentDate.AddDays(1);
								}
							}
							//don't break early - more than one folder might exist, so we need to check all of them
						}
						catch (IOException e)
						{
							exceptionForFolder = e;
							continue;
						}

						searchedDirectories.Add(logFileDirectory);
					}
					else if (logFileDirectory.StartsWith(ServiceManagerHelper.GetLogFilesUri(string.Empty).OriginalString.Substring(0, 7), StringComparison.OrdinalIgnoreCase))
					{
						foreach (var taskCode in ServiceTaskCode.Split(','))
						{
							LogViewer.TaskType = taskCode;

							var hostName = GetHostNameFromURI(logFileDirectory);
							var logProvider = LogViewer.HostLogProviderCollection.FirstOrDefault(p => p.Hostname == hostName);

							if (logProvider != null)
							{
								try
								{
									var currentDate = DateFromUtc;
									while (currentDate <= DateToUtc)
									{
										var regexPattern = currentDate.ToString(dateFormat);
										foreach (string fileName in logProvider.GetFileNames())
										{
											var fileWithDirectory = FormattableString.Invariant($"{logFileDirectory}/{fileName}");
											if (Regex.IsMatch(fileWithDirectory, regexPattern))
											{
												result.Add(fileWithDirectory);
												success = true;
											}
										}
										currentDate = currentDate.AddDays(1);
									}
								}
								catch (IOException e)
								{
									exceptionForFolder = e;
									continue;
								}
							}
						}

						searchedDirectories.Add(logFileDirectory);
					}
				}

				if (!success)
				{
					additionalInformation = DirectoryStructureReport();
				}

#if NET
				return System.Linq.Enumerable.Distinct(result, StringComparer.OrdinalIgnoreCase);
#else
				return result.DistinctBy(x => x.ToLower());
#endif
			}
		}

		readonly List<string> searchedDirectories = new List<string>();

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "It's already error-handling code...")]
		string DirectoryStructureReport()
		{
			if (!LogFilesDirectories.Any())
			{
				return (NoResString)"No directories found!";
			}

			StringBuilder result = new StringBuilder();

			result.Append((NoResString)"List of directories attempted to be searched:\r\n");
			result.Append(LogFilesDirectories.Any() ? LogFilesDirectories.Distinct().Aggregate((x, y) => x + "\r\n" + y) + "\r\n" : "None.\r\n");

			result.Append((NoResString)"List of directories successfully searched:\r\n");
			result.Append(searchedDirectories.Count > 0 ? searchedDirectories.Distinct().Aggregate((x, y) => x + "\r\n" + y) + "\r\n" : "None.\r\n");

			string logFileDirectory = searchedDirectories.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? LogFilesDirectories.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

			//remove ending \
			if (logFileDirectory[logFileDirectory.Length - 1] == Path.DirectorySeparatorChar)
			{
				logFileDirectory = logFileDirectory.Substring(0, logFileDirectory.Length - 1);
			}

			try
			{
				result.AppendFormat((NoResString)"List of directories found one parent up from {0}:\r\n", logFileDirectory);

				//first parent - remove everything until next \
				string logFilesDirectory1stParent = logFileDirectory.Substring(0, logFileDirectory.LastIndexOf(Path.DirectorySeparatorChar) + 1);

				if (Directory.Exists(logFilesDirectory1stParent))
				{
					foreach (string directory in Directory.GetDirectories(logFilesDirectory1stParent))
					{
						result.Append(directory + "\r\n");
					}
				}

				result.AppendFormat((NoResString)"List of directories found two parents up from {0}:\r\n", logFileDirectory);

				//remove ending \
				logFilesDirectory1stParent = logFilesDirectory1stParent.Substring(0, logFilesDirectory1stParent.Length - 1);

				//second parent - remove everything until next \

				string logFilesDirectory2ndParent = logFilesDirectory1stParent.Substring(0, logFilesDirectory1stParent.LastIndexOf(Path.DirectorySeparatorChar) + 1);

				if (Directory.Exists(logFilesDirectory2ndParent))
				{
					foreach (string directory in Directory.GetDirectories(logFilesDirectory2ndParent))
					{
						result.Append(directory + "\r\n");
					}
				}
			}
			catch (Exception e)
			{
				if (e.IsCriticalException())
				{ throw; }
				result.Append((NoResString)"Exception while inspecting parent directories: " + e);
			}

			return result.ToString();
		}

		Exception exceptionForFolder;
		string additionalInformation;

		public static string SanitizeFileName(string fileName)
		{
			return Regex.Replace(fileName, @"[:\$\\\/]", "_");
		}

		void DisposeAllStreams(List<ZipStream> streams)
		{
			foreach (var zipStream in streams)
			{
				zipStream.Stream.Dispose();
			}
		}

		public MemoryStream LogFilesZip
		{
			get
			{
				//Log files can look like SGC_20140724.txt or like SGC_20140724.171615-771.txt
				var files = ConstructLogFilesZipFiles(MaxLogFileSize);
				if (!string.IsNullOrWhiteSpace(additionalInformation))
				{
					//If I don't substitute \ for # the file name gets truncated.
					//Substituting : for ; because it gets stripped (looks like a root directory).
					files.Add(new ZipStream("exception.txt", new MemoryStream(
						Encoding.UTF8.GetBytes(
							string.Format(CultureInfo.InvariantCulture, (NoResString)"Exception occured while opening folder: {0}\r\nAdditional information:\r\n{1}",
							exceptionForFolder != null ? exceptionForFolder.ToString() : (NoResString)"Could not find log folder or log folder was empty.",
							additionalInformation)
							)
						)));
				}

				using (var zipStream = new MemoryStream())
				{
					var creator = new ZipCreator();
					creator.ZipStream(files, zipStream, leaveOpen: true);
					if (zipStream.Length < (1024L * 1024L * MaxZipSize))
					{
						DisposeAllStreams(files);
						return zipStream;
					}
					else
					{
						files.Insert(0, new ZipStream("zip_size_limit_reached.txt",
  new MemoryStream(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, (NoResString)"The zip file size limit of {0} MB was reached. No more data will be displayed.", MaxZipSize)))));

						//zip file doesn't fit in a single eDoc size, but we can't predict how large a zip file will be before zipping it, so linearly step down until it's small enough.
						//take steps of size (1+floor(count/1000)) to bound computation.
						//thought about binary search but it's not actually a guarantee that zip size is monotonic, I think, and it was getting annoying to write/test.
						//we can improve this later if it's defective.
						var index = files.Count - 1;
						var step = 1 + files.Count / 1000;
						while (index >= 0)
						{
							using (var zipStream2 = new MemoryStream())
							{
								var creator2 = new ZipCreator();
								creator2.ZipStream(files.Take(index), zipStream2, leaveOpen: true);
								if (zipStream2.Length < (1024L * 1024L * MaxZipSize))
								{
									DisposeAllStreams(files);
									return zipStream2;
								}
							}
							index -= step;
						}
						throw new Exception("Unreachable");
					}
				}
			}
		}

		List<ZipStream> ConstructLogFilesZipFiles(int maxLogFileSize)
		{
			long cumulativeBytes = 0;
			var files = new List<ZipStream>();
			var sleeps = new int[] { 0, 1, 10, 100, 1000, 10000 };

			foreach (var logFileName in LogFileNames)
			{
				//show 200 characters of entire file path, sanitized. if we go over 200 characters, show first and last 100
				var fileName = SanitizeFileName(logFileName);
				if (fileName.Length > 200)
				{
					fileName = fileName.Substring(0, 100) + "..." + fileName.Substring(fileName.Length - 100);
				}

				var max_retries = sleeps.Length + 1;
				for (var i = 0; i < max_retries; ++i)
				{
					if (i > 0)
					{
						Thread.Sleep(sleeps[i - 1]);
					}

					try
					{
						var nextFile = RetrieveFile(logFileName);
						using (nextFile)
						{
							if (nextFile.Length + cumulativeBytes < (1024L * 1024L * maxLogFileSize))
							{
								cumulativeBytes += nextFile.Length;
								files.Add(new ZipStream(fileName, new MemoryStream(nextFile.ReadFully())));
							}
							else
							{
								int diff = (int)((1024L * 1024L * MaxLogFileSize) - cumulativeBytes);
								var result = new byte[diff];
								_ = nextFile.Read(result, 0, diff);
								nextFile.Close();
								files.Add(new ZipStream(fileName, new MemoryStream(result)));
								cumulativeBytes += diff;
								files.Add(new ZipStream("log_size_limit_reached.txt",
								new MemoryStream(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, (NoResString)"The log file size limit of {0} MB was reached. No more data will be displayed.", maxLogFileSize)))));
								return files;
							}
						}
						break;
					}
					catch (Exception e)
					{
						if (e.IsCriticalException())
						{ throw; }
						if (i >= (max_retries - 1))
						{
							files.Add(new ZipStream("exception_" + logFileName,
							new MemoryStream(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, (NoResString)"Exception occured during retrieval: {0}", e)))));
						}
					}
				}
			}
			return files;
		}

		public LogsReport Create()
		{
			return new LogsReport(IncidentNumber, ServiceTaskCode, LogFilesZip.ToArray());
		}

		public void CreateAndSend()
		{
			LogsReport report = Create();
			LogsReportEHubBuilder.Send(report);
		}

		Stream RetrieveFile(string fileName)
		{
			if (fileName.StartsWith(ServiceManagerHelper.GetLogFilesUri(string.Empty).OriginalString.Substring(0, 7), StringComparison.OrdinalIgnoreCase))
			{
				var hostName = GetHostNameFromURI(fileName);
				var logProvider = LogViewer.HostLogProviderCollection.FirstOrDefault(p => p.Hostname == hostName);
				var bytes = logProvider.GetBytes(Path.GetFileName(fileName));

				MemoryStream memStream = new MemoryStream(bytes);
				return memStream;
			}
			else
			{
				return new FileStream(fileName, FileMode.Open);
			}
		}

		static string GetHostNameFromURI(string directory)
		{
			if (directory.StartsWith(ServiceManagerHelper.GetLogFilesUri(string.Empty).OriginalString.Substring(0, 7), StringComparison.OrdinalIgnoreCase))
			{
				return directory.Split('/')[2].Split(':')[0];
			}
			return "";
		}
	}
}
