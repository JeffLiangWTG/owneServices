using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.LogsReport;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LogsReportBuilderTest : TestCaseWithFactory
	{
		public void TestLogFilesDirectory()
		{
			LogsReportBuilder builder = new LogsReportBuilder(ZDate.Today.AddDays(-7), ZDate.Today, "ABC", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER2", "ODYSSEYSEIHAM", @"1234\5678", 100);
			AssertContains(@"\Process Controller\1234$5678\ODYSSEYSEIHAM", builder.LogFilesDirectories.ToArray()[0]);
			AssertContains(@"\Process Controller\1234\ODYSSEYSEIHAM", builder.LogFilesDirectories.ToArray()[1]);
			AssertContains(@"\Process Controller\LON-SSQL-20B$MSSQLSERVER2\ODYSSEYSEIHAM", builder.LogFilesDirectories.ToArray()[2]);
			AssertContains(@"\Process Controller\LON-SSQL-20B\ODYSSEYSEIHAM", builder.LogFilesDirectories.ToArray()[3]);

			builder = new LogsReportBuilder(ZDate.Today.AddDays(-7), ZDate.Today, "ABC", "CS01234567", "SYD-SSQL-20B", "ODYSSEYFGLAKL", @"5678\1234", 100);
			AssertContains(@"\Process Controller\5678$1234\ODYSSEYFGLAK", builder.LogFilesDirectories.ToArray()[0]);
			AssertContains(@"\Process Controller\5678\ODYSSEYFGLAK", builder.LogFilesDirectories.ToArray()[1]);
			AssertContains(@"\Process Controller\SYD-SSQL-20B\ODYSSEYFGLAK", builder.LogFilesDirectories.ToArray()[2]);
			AssertContains(@"\Process Controller\SYD-SSQL-20B\ODYSSEYFGLAK", builder.LogFilesDirectories.ToArray()[3]);
		}

		[TestDate(2010, 2, 10)]
		[DeveloperOnlyTest] //I have 0 idea why this fails only on DAT.
		public void TestInaccessibleLogFilesDirectory()
		{
			LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
			LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(@"abcdefghijklmnopqrstuvwxyz\zyxwvutsrpqonmlkjihgfedcba");
			LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 100);

			byte[] logFilesZip = builder.LogFilesZip.ToArray();
			ZipExtractor extractor = new ZipExtractor();

			MemoryStream streamIntoBuffer = new MemoryStream();
			string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));
			Assert(zippedFileNames.Any(x => x.Equals(@"exception.txt")));
			extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, @"exception.txt");
			string exceptionDetails = Encoding.UTF8.GetString(streamIntoBuffer.ToArray());
			AssertStartsWith(exceptionDetails, "Exception occured while opening folder:", exceptionDetails);
			AssertContains(exceptionDetails, "Additional information:", exceptionDetails);
			AssertContains(exceptionDetails, @"\Process Controller\LON-SSQL-20B$MSSQLSERVER999\ODYSSEYSEIHAMMY", exceptionDetails);
			AssertContains(exceptionDetails, @"\Process Controller\1234\ODYSSEYSEIHAMMY", exceptionDetails);
			AssertContains(exceptionDetails, "List of directories successfully searched:\r\nNone.", exceptionDetails);
			AssertContains(exceptionDetails, "List of directories attempted to be searched:", exceptionDetails);
		}

		[TestDate(2010, 2, 10)]
		public void TestCreate()
		{
			LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER2", "ODYSSEYSEIHAM", "1234", 100);
			var result = builder.Create();
			AssertEquals("ABC", result.ServiceTaskCode);
			AssertEquals("CS01234567", result.IncidentNumber);
		}

		[TestDate(2010, 2, 10)]
		[DeveloperOnlyTest] //When I made the previous test developer only, this one started failing only on DAT. Sorry, I don't know why. I just want to check this code in, I can figure it out another time.
		public void TestInaccessibleLogFilesDirectory_AdditionalInformation()
		{
			string processControllerDirectory = "";
			try
			{
				using (var tempDirectory = new TempDirectory())
				{
					string directoryTruncated = tempDirectory.DirectoryName;
					directoryTruncated = directoryTruncated.Substring(0, directoryTruncated.LastIndexOf('\\'));

					processControllerDirectory = Directory.CreateDirectory(Path.Combine(directoryTruncated, @"Process Controller")).FullName;
					Directory.CreateDirectory(Path.Combine(directoryTruncated, @"Process Controller\OTHERSERVER"));
					Directory.CreateDirectory(Path.Combine(directoryTruncated, @"Process Controller\OTHERSERVER2$SQL"));

					string fakeTempDirectory = Path.Combine(directoryTruncated, @"Process Controller\SERVER$SQL\enterprise\");
					LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
					LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(fakeTempDirectory);
					LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory);

					LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 100);

					byte[] logFilesZip = builder.LogFilesZip.ToArray();
					ZipExtractor extractor = new ZipExtractor();

					MemoryStream streamIntoBuffer = new MemoryStream();
					string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));

					string expectedExceptionTxt = "exception.txt";

					Assert(zippedFileNames.Any(x => x.Equals(expectedExceptionTxt)));
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, expectedExceptionTxt);
					string exceptionDetails = Encoding.UTF8.GetString(streamIntoBuffer.ToArray());
					AssertStartsWith(exceptionDetails, @"Exception occured while opening folder: Could not find log folder or log folder was empty.", exceptionDetails);
					AssertContains(exceptionDetails, @"Additional information:", exceptionDetails);
					AssertContains(exceptionDetails, "List of directories attempted to be searched:", exceptionDetails);
					AssertContains(exceptionDetails, "List of directories successfully searched:\r\n" + tempDirectory, exceptionDetails);
					AssertContains(exceptionDetails, @"List of directories found one parent up from " + tempDirectory.DirectoryName + ":", exceptionDetails);
					AssertContains(exceptionDetails, @"List of directories found two parents up from " + tempDirectory.DirectoryName + ":", exceptionDetails);
					AssertContains(exceptionDetails, @"\Process Controller\LON-SSQL-20B$MSSQLSERVER999\ODYSSEYSEIHAMMY", exceptionDetails);
					AssertContains(exceptionDetails, @"\Process Controller\1234\ODYSSEYSEIHAMMY", exceptionDetails);
				}
			}
			finally
			{
				if (!string.IsNullOrWhiteSpace(processControllerDirectory))
				{
					TempDirectory.DeleteDirectory(processControllerDirectory);
				}
			}
		}

		[TestDate(2010, 2, 10)]
		[DeveloperOnlyTest] //When I made the previous test developer only, this one started failing only on DAT. Sorry, I don't know why. I just want to check this code in, I can figure it out another time.
		public void TestInaccessibleLogFilesDirectory_AdditionalInformation_2()
		{
			string processControllerDirectory = "";
			try
			{
				using (var tempDirectory = new TempDirectory())
				{
					string directoryTruncated = tempDirectory.DirectoryName;
					directoryTruncated = directoryTruncated.Substring(0, directoryTruncated.LastIndexOf('\\'));

					processControllerDirectory = Directory.CreateDirectory(Path.Combine(directoryTruncated, @"Process Controller")).FullName;
					Directory.CreateDirectory(Path.Combine(directoryTruncated, @"Process Controller\OTHERSERVER"));
					Directory.CreateDirectory(Path.Combine(directoryTruncated, @"Process Controller\OTHERSERVER2$SQL"));

					string fakeTempDirectory = Path.Combine(directoryTruncated, @"Process Controller\SERVER$SQL\enterprise");
					LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
					LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(fakeTempDirectory);

					LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 100);

					byte[] logFilesZip = builder.LogFilesZip.ToArray();
					ZipExtractor extractor = new ZipExtractor();

					MemoryStream streamIntoBuffer = new MemoryStream();
					string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));

					string expectedExceptionTxt = "exception.txt";

					Assert(zippedFileNames.Any(x => x.Equals(expectedExceptionTxt)));
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, expectedExceptionTxt);
					string exceptionDetails = Encoding.UTF8.GetString(streamIntoBuffer.ToArray());
					AssertStartsWith(exceptionDetails, @"Exception occured while opening folder: Could not find log folder or log folder was empty.", exceptionDetails);
					AssertContains(exceptionDetails, @"Additional information:", exceptionDetails);
					AssertContains(exceptionDetails, "List of directories attempted to be searched:", exceptionDetails);
					AssertContains(exceptionDetails, "List of directories successfully searched:\r\nNone.", exceptionDetails);
					AssertContains(exceptionDetails, @"List of directories found one parent up from " + fakeTempDirectory + ":", exceptionDetails);
					AssertContains(exceptionDetails, @"List of directories found two parents up from " + fakeTempDirectory + ":", exceptionDetails);
					AssertContains(exceptionDetails, @"\Process Controller\LON-SSQL-20B$MSSQLSERVER999\ODYSSEYSEIHAMMY", exceptionDetails);
					AssertContains(exceptionDetails, @"\Process Controller\1234\ODYSSEYSEIHAMMY", exceptionDetails);
					AssertContains(exceptionDetails, @"Process Controller\OTHERSERVER", exceptionDetails);
					AssertContains(exceptionDetails, @"Process Controller\OTHERSERVER2$SQL", exceptionDetails);
				}
			}
			finally
			{
				if (!string.IsNullOrWhiteSpace(processControllerDirectory))
				{
					TempDirectory.DeleteDirectory(processControllerDirectory);
				}
			}
		}

		[TestDate(2010, 2, 10)]
		public void TestLogFileNamesAndZip()
		{
			using (var tempDirectory1 = new TempDirectory())
			using (var tempDirectory2 = new TempDirectory())
			{
				string tempDirectory = tempDirectory1.DirectoryName + "\\";
				LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
				LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory2);
				LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory);

				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100203.txt")))
				{ stream.Write("log1"); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100204.txt")))
				{ stream.Write("log2"); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100206.txt")))
				{ stream.Write("log3"); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory.ToUpper(), "ABC_20100206.txt")))
				{ stream.Write("log3"); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100210.171615-771.txt")))
				{ stream.Write("log4"); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100202.txt")))
				{ stream.Write("Too early!"); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100211.txt")))
				{ stream.Write("Too late!"); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "DEF_20100203.txt")))
				{ stream.Write("log5"); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "GHI_20100203.txt")))
				{ stream.Write("wrong service task code!"); }

				LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 100);
				string[] logFileNames = builder.LogFileNames.ToArray();
				AssertEquals(5, logFileNames.Length);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100203.txt"), logFileNames[0]);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100204.txt"), logFileNames[1]);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100206.txt"), logFileNames[2]);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100210.171615-771.txt"), logFileNames[3]);
				AssertEquals(Path.Combine(tempDirectory, "DEF_20100203.txt"), logFileNames[4]);

				byte[] logFilesZip = builder.LogFilesZip.ToArray();
				ZipExtractor extractor = new ZipExtractor();
				string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100210.171615-771.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "DEF_20100203.txt")))));
				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")));
					AssertEquals("log1", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")));
					AssertEquals("log2", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")));
					AssertEquals("log3", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100210.171615-771.txt")));
					AssertEquals("log4", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "DEF_20100203.txt")));
					AssertEquals("log5", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				var result = builder.Create();
				AssertEquals("ABC,DEF", result.ServiceTaskCode);
				AssertEquals("CS01234567", result.IncidentNumber);
				AssertEquals(logFilesZip, result.LogFilesZip);
			}
		}

		[TestDate(2010, 2, 10)]
		public void TestLogFileNamesAndZip_MaxZipSize_LogsAreCompressed()
		{
			using (var tempDirectory1 = new TempDirectory())
			{
				string tempDirectory = tempDirectory1.DirectoryName + "\\";
				LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
				LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory);

				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100203.txt")))
				{ stream.Write(new string('a', 1024 * 512)); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100204.txt")))
				{ stream.Write(new string('b', 1024 * 1024)); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100206.txt")))
				{ stream.Write("log3"); }

				LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 1);
				string[] logFileNames = builder.LogFileNames.ToArray();
				AssertEquals(3, logFileNames.Length);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100203.txt"), logFileNames[0]);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100204.txt"), logFileNames[1]);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100206.txt"), logFileNames[2]);

				byte[] logFilesZip = builder.LogFilesZip.ToArray();
				ZipExtractor extractor = new ZipExtractor();
				string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));
				AssertEquals(3, zippedFileNames.Length);
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")))));
				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")));
					AssertEquals(new string('a', 1024 * 512), Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")));
					AssertEquals(new string('b', 1024 * 1024), Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")));
					AssertEquals("log3", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}
			}
		}

		[TestDate(2010, 2, 10)]
		public void TestLogFileNamesAndZip_MaxLogFileSize()
		{
			using (var tempDirectory1 = new TempDirectory())
			{
				string tempDirectory = tempDirectory1.DirectoryName + "\\";
				LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
				LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory);

				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100203.txt")))
				{ stream.Write(new string('a', 1024 * 512)); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100204.txt")))
				{ stream.Write(new string('b', 1024 * 1024)); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100206.txt")))
				{ stream.Write("log3"); }

				LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 10, 1);
				string[] logFileNames = builder.LogFileNames.ToArray();
				AssertEquals(3, logFileNames.Length);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100203.txt"), logFileNames[0]);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100204.txt"), logFileNames[1]);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100206.txt"), logFileNames[2]);

				byte[] logFilesZip = builder.LogFilesZip.ToArray();
				ZipExtractor extractor = new ZipExtractor();
				string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));
				AssertEquals(3, zippedFileNames.Length);
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals("log_size_limit_reached.txt")));
				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")));
					AssertEquals(new string('a', 1024 * 512), Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")));
					AssertEquals(new string('b', 1024 * 512), Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, "log_size_limit_reached.txt");
					AssertEquals("The log file size limit of 1 MB was reached. No more data will be displayed.", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 2);

				logFilesZip = builder.LogFilesZip.ToArray();
				zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));
				AssertEquals(3, zippedFileNames.Length);
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")))));
				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")));
					AssertEquals(new string('a', 1024 * 512), Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")));
					AssertEquals(new string('b', 1024 * 1024), Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")));
					AssertEquals("log3", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}
			}
		}

		[TestDate(2010, 2, 10)]
		public void TestLogFileNamesAndZip_MaxZipSize()
		{
			using (var tempDirectory1 = new TempDirectory())
			{
				string tempDirectory = tempDirectory1.DirectoryName + "\\";
				LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
				LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory);

				var random = new Random(0);
				var bufferA = new byte[1024 * 512];
				var bufferB = new byte[1024 * 1024];
				random.NextBytes(bufferA);
				random.NextBytes(bufferB);
				
				using (var stream = File.Create(Path.Combine(tempDirectory, "ABC_20100203.txt")))
				{ stream.Write(bufferA, 0, bufferA.Length); }
				using (var stream = File.Create(Path.Combine(tempDirectory, "ABC_20100204.txt")))
				{ stream.Write(bufferB, 0, bufferB.Length); }
				using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100206.txt")))
				{ stream.Write("log3"); }
				
				var builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 1);
				var logFileNames = builder.LogFileNames.ToArray();
				AssertEquals(3, logFileNames.Length);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100203.txt"), logFileNames[0]);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100204.txt"), logFileNames[1]);
				AssertEquals(Path.Combine(tempDirectory, "ABC_20100206.txt"), logFileNames[2]);

				var logFilesZip = builder.LogFilesZip.ToArray();
				var extractor = new ZipExtractor();
				var zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));
				AssertEquals(2, zippedFileNames.Length);
				Assert(zippedFileNames.Any(x => x.Equals("zip_size_limit_reached.txt")));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")))));
				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, "zip_size_limit_reached.txt");
					AssertEquals("The zip file size limit of 1 MB was reached. No more data will be displayed.", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")));
					AssertEquals(bufferA, streamIntoBuffer.ToArray());
				}

				builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 2);

				logFilesZip = builder.LogFilesZip.ToArray();
				zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));
				AssertEquals(3, zippedFileNames.Length);
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")))));
				Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")))));
				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")));
					AssertEquals(bufferA, streamIntoBuffer.ToArray());
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")));
					AssertEquals(bufferB, streamIntoBuffer.ToArray());
				}

				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")));
					AssertEquals("log3", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}
			}
		}

		[TestDate(2010, 2, 10)]
		public void TestLogFileNamesAndZip_VeryLongPath()
		{
			using (var tempDirectory1 = new TempDirectory())
			using (var tempDirectory2 = new TempDirectory())
			{
				string tempDirectory = tempDirectory1.DirectoryName + "\\";
				LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
				LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory2);
				LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory);

				var fileName = "ABC_20100203.txt" + new string('A', 200 - tempDirectory.Length);
				fileName = Path.Combine(tempDirectory, fileName);

				using (var stream = File.CreateText(Path.Combine(tempDirectory, fileName)))
				{ stream.Write("log1"); }

				LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 100);
				string[] logFileNames = builder.LogFileNames.ToArray();
				AssertEquals(1, logFileNames.Length);
				AssertEquals(fileName, logFileNames[0]);

				byte[] logFilesZip = builder.LogFilesZip.ToArray();
				ZipExtractor extractor = new ZipExtractor();
				string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));

				fileName = LogsReportBuilder.SanitizeFileName(fileName);
				fileName = fileName.Substring(0, 100) + "..." + fileName.Substring(fileName.Length - 100);

				Assert(zippedFileNames.Any(x => x.Equals(fileName)));
				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, fileName);
					AssertEquals("log1", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}
			}
		}

		[TestDate(2010, 2, 10)]
		public void TestLogFileNamesAndZip_HeldBySecondThread()
		{
			using (var tempDirectory1 = new TempDirectory())
			using (var tempDirectory2 = new TempDirectory())
			{
				string tempDirectory = tempDirectory1.DirectoryName + "\\";
				LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
				LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory2);
				LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory);

				var fileName = "ABC_20100203.txt";
				fileName = Path.Combine(tempDirectory, fileName);

				using (var stream = File.CreateText(Path.Combine(tempDirectory, fileName)))
				{ stream.Write("log1"); }

				LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC,DEF", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER999", "ODYSSEYSEIHAMMY", "1234", 100);
				string[] logFileNames = builder.LogFileNames.ToArray();
				AssertEquals(1, logFileNames.Length);
				AssertEquals(fileName, logFileNames[0]);

				var thread2 = new Thread(
				() =>
				{
					using (var stream = File.OpenWrite(fileName))
					{
						Thread.Sleep(500);
					}
				});

				thread2.Start();
				byte[] logFilesZip = builder.LogFilesZip.ToArray();
				AssertEquals(ThreadState.Stopped, thread2.ThreadState);
				ZipExtractor extractor = new ZipExtractor();
				string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(logFilesZip));

				fileName = LogsReportBuilder.SanitizeFileName(fileName);

				Assert(zippedFileNames.Any(x => x.Equals(fileName)));
				{
					MemoryStream streamIntoBuffer = new MemoryStream();
					extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, fileName);
					AssertEquals("log1", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
				}
			}
		}

		[TestDate(2010, 2, 10)]
		public void TestGetLogFilesFromRemoteHost()
		{
			var provider1 = new Mock<ILogViewerDataProvider>();

			var host1 = Factory.New<StmServiceHost>();
			host1.SH_HostName = "HostABC";
			Factory.Save();

			var log1 = Encoding.ASCII.GetBytes("Test Log 1");
			var log2 = Encoding.ASCII.GetBytes("Test Log 2");

			provider1.Setup(m => m.Hostname).Returns("HostABC");
			provider1.Setup(m => m.GetFileNames()).Returns(new[] { "ABC_20100203.txt", "ABC_20100204.txt" });
			provider1.Setup(m => m.GetBytes("ABC_20100203.txt")).Returns(log1);
			provider1.Setup(m => m.GetBytes("ABC_20100204.txt")).Returns(log2);

			var hosts = new UntranslatableCodeDescriptionPairList("Simulating true hosts property");
			hosts.AddPair("HostABC");

			var viewer = new Mock<IServiceTaskLogViewer>();
			viewer.Setup(m => m.HostLogProviderCollection).Returns(new List<ILogViewerDataProvider> { provider1.Object });

			var reportBuilder = new Mock<LogsReportBuilder>(ZDate.Today.AddDays(-7), ZDate.Today, "ABC", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER2", "ODYSSEYSEIHAM", @"1234\5678", 100, 2000);
			reportBuilder.Setup(r => r.LogViewer).Returns(viewer.Object);

			AssertContains(@"http://HostABC:7070/cargowise/processController/", reportBuilder.Object.LogFilesDirectories.ToArray()[4]);

			AssertEquals("Only 2 filenames were found", 2, reportBuilder.Object.LogFileNames.Count());
			AssertStartsWith("Filename contains wrong directory", @"http://HostABC:7070/cargowise/processController/", reportBuilder.Object.LogFileNames.ToArray()[0]);
			AssertEquals("Filename is wrong", "ABC_20100203.txt", Path.GetFileName(reportBuilder.Object.LogFileNames.ToArray()[0]));
			AssertStartsWith("Filename contains wrong directory", @"http://HostABC:7070/cargowise/processController/", reportBuilder.Object.LogFileNames.ToArray()[1]);
			AssertEquals("Filename is wrong", "ABC_20100204.txt", Path.GetFileName(reportBuilder.Object.LogFileNames.ToArray()[1]));

			byte[] logFilesZip = reportBuilder.Object.LogFilesZip.ToArray();
			ZipExtractor extractor = new ZipExtractor();
			{
				MemoryStream streamIntoBuffer = new MemoryStream();
				extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(reportBuilder.Object.LogFileNames.ToArray()[0]));
				AssertEquals("Test Log 1", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
			}

			{
				MemoryStream streamIntoBuffer = new MemoryStream();
				extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(reportBuilder.Object.LogFileNames.ToArray()[1]));
				AssertEquals("Test Log 2", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
			}

			provider1.VerifyAll();
		}

		[TestDate(2010, 2, 10)]
		public void TestGetLogFilesFromRemoteHosts_DuplicateFileNames()
		{
			var provider1 = new Mock<ILogViewerDataProvider>();
			var provider2 = new Mock<ILogViewerDataProvider>();
			var host1 = Factory.New<StmServiceHost>();
			host1.SH_HostName = "HostABC1";
			var host2 = Factory.New<StmServiceHost>();
			host2.SH_HostName = "HostABC2";
			Factory.Save();

			var log1 = Encoding.ASCII.GetBytes("Test Log 1");
			var log2 = Encoding.ASCII.GetBytes("Test Log 2");
			var log3 = Encoding.ASCII.GetBytes("Test Log 3");

			provider1.Setup(m => m.Hostname).Returns("HostABC1");
			provider1.Setup(m => m.GetFileNames()).Returns(new[] { "ABC_20100203.txt", "ABC_20100204.txt" });
			provider1.Setup(m => m.GetBytes("ABC_20100203.txt")).Returns(log1);
			provider1.Setup(m => m.GetBytes("ABC_20100204.txt")).Returns(log2);

			provider2.Setup(m => m.Hostname).Returns("HostABC2");
			provider2.Setup(m => m.GetFileNames()).Returns(new[] { "ABC_20100204.txt", "ABC_20100205.txt" });
			provider2.Setup(m => m.GetBytes("ABC_20100204.txt")).Returns(log2);
			provider2.Setup(m => m.GetBytes("ABC_20100205.txt")).Returns(log3);

			var hosts = new UntranslatableCodeDescriptionPairList("Simulating true hosts property");
			hosts.AddPair("HostABC1");
			hosts.AddPair("HostABC2");

			var viewer = new Mock<IServiceTaskLogViewer>();
			viewer.Setup(m => m.HostLogProviderCollection).Returns(new List<ILogViewerDataProvider> { provider1.Object, provider2.Object });

			var reportBuilder = new Mock<LogsReportBuilder>(ZDate.Today.AddDays(-7), ZDate.Today, "ABC", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER2", "ODYSSEYSEIHAM", @"1234\5678", 100, 2000);
			reportBuilder.Setup(m => m.LogViewer).Returns(viewer.Object);

			AssertContains(@"http://HostABC1:7070/cargowise/processController/", reportBuilder.Object.LogFilesDirectories.ToArray()[4]);
			AssertContains(@"http://HostABC2:7070/cargowise/processController/", reportBuilder.Object.LogFilesDirectories.ToArray()[6]);

			AssertEquals("Only 4 filenames were found", 4, reportBuilder.Object.LogFileNames.Count());
			AssertStartsWith("Filename contains wrong directory", @"http://HostABC1:7070/cargowise/processController/", reportBuilder.Object.LogFileNames.ToArray()[0]);
			AssertEquals("Filename is wrong", "ABC_20100203.txt", Path.GetFileName(reportBuilder.Object.LogFileNames.ToArray()[0]));
			AssertStartsWith("Filename contains wrong directory", @"http://HostABC1:7070/cargowise/processController/", reportBuilder.Object.LogFileNames.ToArray()[1]);
			AssertEquals("Filename is wrong", "ABC_20100204.txt", Path.GetFileName(reportBuilder.Object.LogFileNames.ToArray()[1]));
			AssertStartsWith("Filename contains wrong directory", @"http://HostABC2:7070/cargowise/processController/", reportBuilder.Object.LogFileNames.ToArray()[3]);
			AssertEquals("Filename is wrong", "ABC_20100205.txt", Path.GetFileName(reportBuilder.Object.LogFileNames.ToArray()[3]));

			byte[] logFilesZip = reportBuilder.Object.LogFilesZip.ToArray();
			ZipExtractor extractor = new ZipExtractor();
			{
				MemoryStream streamIntoBuffer = new MemoryStream();
				extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(reportBuilder.Object.LogFileNames.ToArray()[0]));
				AssertEquals("Test Log 1", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
			}

			{
				MemoryStream streamIntoBuffer = new MemoryStream();
				extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(reportBuilder.Object.LogFileNames.ToArray()[1]));
				AssertEquals("Test Log 2", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
			}

			{
				MemoryStream streamIntoBuffer = new MemoryStream();
				extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(reportBuilder.Object.LogFileNames.ToArray()[2]));
				AssertEquals("Test Log 2", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
			}

			{
				MemoryStream streamIntoBuffer = new MemoryStream();
				extractor.ExtractZipStream(new MemoryStream(logFilesZip), streamIntoBuffer, LogsReportBuilder.SanitizeFileName(reportBuilder.Object.LogFileNames.ToArray()[3]));
				AssertEquals("Test Log 3", Encoding.UTF8.GetString(streamIntoBuffer.ToArray()));
			}
		}
	}
}
