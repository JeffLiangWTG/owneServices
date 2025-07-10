using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.LogsReport;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LogsReportEHubBuilderTest : TestCaseWithFactory
	{
		[TestDate(2010, 2, 10)]
		public void TestSend()
		{
			try
			{
				GlbCompany.CurrentCompany.GC_Code = "COM";
				var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				registrationKey.EnterpriseCodeForTest = "ENT";
				registrationKey.ServerCodeForTest = "SRV";

				using (var tempDirectory1 = new TempDirectory())
				{
					string tempDirectory = tempDirectory1.DirectoryName + "\\";
					LogsReportBuilder.LogFilesDirectoriesForTest.Value = new List<string>();
					LogsReportBuilder.LogFilesDirectoriesForTest.Value.Add(tempDirectory);

					using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100203.txt")))
					{ stream.Write("log1"); }
					using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100204.txt")))
					{ stream.Write("log2"); }
					using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100206.txt")))
					{ stream.Write("log3"); }
					using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100210.txt")))
					{ stream.Write("log4"); }
					using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100202.txt")))
					{ stream.Write("Too early!"); }
					using (var stream = File.CreateText(Path.Combine(tempDirectory, "ABC_20100211.txt")))
					{ stream.Write("Too late!"); }

					var outgoing = new DebugOnlyOutgoingSystemMessage();
					ObjectFactory.Substitute<IOutgoingSystemMessage>(outgoing);

					LogsReportBuilder builder = new LogsReportBuilder(new ZDate(2010, 2, 3), new ZDate(2010, 2, 10), "ABC", "CS01234567", @"LON-SSQL-20B\MSSQLSERVER2", "ODYSSEYSEIHAM", "1234");
					LogsReport.LogsReport reportToSend = builder.Create();
					LogsReportEHubBuilder.Send(reportToSend);

					AssertEquals("MessageName", SystemMessageList.Descriptions.LogsReport, DebugOnlyOutgoingSystemMessage.MessageName);
					string bodyText = Encoding.UTF8.GetString(DebugOnlyOutgoingSystemMessage.MessageStream);
					LogsReport.LogsReport report = new LogsReport.LogsReport(bodyText);

					AssertEquals("ABC", report.ServiceTaskCode);
					AssertEquals("CS01234567", report.IncidentNumber);

					ZipExtractor extractor = new ZipExtractor();
					string[] zippedFileNames = extractor.GetFileNames(new MemoryStream(report.LogFilesZip));
					Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100203.txt")))));
					Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100204.txt")))));
					Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100206.txt")))));
					Assert(zippedFileNames.Any(x => x.Equals(LogsReportBuilder.SanitizeFileName(Path.Combine(tempDirectory, "ABC_20100210.txt")))));
				}
			}
			finally
			{
				DebugOnlyOutgoingSystemMessage.Initialize();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "HYEMELJKW");
		}
	}
}
