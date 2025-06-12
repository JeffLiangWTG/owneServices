using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.Billing.CollectorService.Plugin.Tests;
using CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccess;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using CargoWise.eServices.Billing.Tests.Common;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Tests.WiseCloudSQLAccessPluginTest
{
	[TestFixture]
	public class WiseCloudSQLAccessPluginTest : AbstractPluginTest<Plugin>
	{
		[OneTimeSetUp]
		public void Setup()
		{
			csvpath = Path.ChangeExtension(Path.GetTempFileName(), "csv");
			using (var infoStream = typeof(WiseCloudSQLAccessPluginTest).Assembly.GetManifestResourceStream(typeof(WiseCloudSQLAccessPluginTest).Namespace + ".WiseCloudSQLAccessReferenceFile.csv"))
			using (var fileStream = File.OpenWrite(csvpath))
			{
				infoStream.WriteTo(fileStream);
				fileStream.Flush();
			}
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			File.Delete(csvpath);
		}

		void OverwriteReferenceFileWith(string customerIPsWithHeader)
		{
			File.WriteAllText(csvpath, customerIPsWithHeader);
		}

		#region Implementation

		static void UpdateSettings(
			ref Mock<Plugin> plugin,
			string refFilePath = "",
			string serverUser = "",
			string serverPass = "",
			string serverDomain = "",
			string swUrl = "",
			string[] firewall = null,
			string user = "",
			string pass = "",
			string offset = "",
			string[] masks = null ,
			string portNumber = null,
			string[] portNumberRange = null,
			string ExcludingIPWhiteList = "",
			string ExcludingIPRegex = "",
			string AryakaPrivateIPs = "")
		{
			var pluginParams = new List<PluginParameter>()
			{
				new PluginParameter("ReferenceFilePath", refFilePath),
				new PluginParameter("ReferenceFileServerUsername", serverUser),
				new PluginParameter("ReferenceFileServerPassword", serverPass),
				new PluginParameter("ReferenceFileServerDomainName", serverDomain),
				new PluginParameter("SolarWindsServerUrl", swUrl),
				new PluginParameter("SolarWindsUserName", user),
				new PluginParameter("SolarWindsPassword", pass),
				new PluginParameter("BufferOffsetInHours", offset),
				new PluginParameter("SolarWindsPortNumber", portNumber),
				new PluginParameter("ExcludingIPWhiteList", ExcludingIPWhiteList),
				new PluginParameter("ExcludingIPRegex", ExcludingIPRegex),
				new PluginParameter("AryakaPrivateIPs", AryakaPrivateIPs)

			};
			if (firewall != null)
				pluginParams.AddRange(
					firewall.Select(x => new PluginParameter("SolarWindsFirewallDistinguishingMask", x)));
			if (masks != null)
				pluginParams.AddRange(
					masks.Select(x => new PluginParameter("SolarWindsInterfaceDistinguishingMask", x)));
			if (portNumberRange != null)
				pluginParams.AddRange(
					portNumberRange.Select(x => new PluginParameter("SolarWindsPortNumberRange", x)));
			plugin.Object.UpdateSettings(new PluginSettings
			{
				Parameters = pluginParams.ToArray()
			});
		}

		static void AssertBillingTransaction(TimeStampedTransaction transaction, int count, string clientID, string reference1, string reference2, string transactionTimeStr)
		{
			Assert.AreEqual(count, transaction.BillingTransaction.BillableCount);
			Assert.AreEqual(clientID, transaction.BillingTransaction.ClientID);
			Assert.AreEqual("HOS", transaction.BillingTransaction.Category);
			Assert.AreEqual("#HG", transaction.BillingTransaction.PriceItemCode);
			Assert.AreEqual("MSC", transaction.BillingTransaction.ReportingSource);
			Assert.AreEqual(reference1, transaction.BillingTransaction.Reference1);
			Assert.AreEqual(reference2, transaction.BillingTransaction.Reference2);
			Assert.AreEqual(Helper.ISO8601StringToDateTimeUtc(transactionTimeStr), transaction.BillingTransaction.ServiceOccuredUTC);
		}

		static IEnumerable<XElement> GetRowsFromSWResultXml(string xmlResultFromSolarwinds)
		{
			XNamespace ns = Helper.SolarwindsNamespace;
			return XElement.Parse(xmlResultFromSolarwinds).Descendants(ns + "row");
		}

		#endregion

		#region Test exceptions

		[Test]
		public void TestGetTransactions_StartDateIsLaterThanEndDate_ShouldThrow()
		{
			var utcStart = new DateTime(2021, 1, 29, 0, 0, 0, DateTimeKind.Utc);
			var utcEnd = new DateTime(2021, 1, 28, 0, 0, 0, DateTimeKind.Utc);

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Object.SetLoggerFactory(mockLoggerFactory.Object);

			var transactions = mockPlugin.Object.GetTransactions(utcStart, utcEnd);
			Assert.That(() => { transactions.Count(); },
				Throws.InstanceOf<ArgumentException>().With.Message.EqualTo($"The start time ({utcStart:u}) is later than the end time ({utcEnd:u})."));

			mockPlugin.VerifyAll();
		}

		[Test]
		public void TestGetTransactions_ThrowsExceptionWhenFileNotAvailable()
		{
			var settings = new PluginSettings();
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SampleMapping.csv");
			settings.Parameters = new PluginParameter[]
			{
				new PluginParameter { Name = "ReferenceFilePath", Value = path },
				new PluginParameter { Name = "SolarWindsServerUrl", Value = "SolarWindsServerUrl" },
				new PluginParameter { Name = "SolarWindsFirewallDistinguishingMask", Value = "SolarWindsFirewallDistinguishingMask" }
			};

			var utcStart = new DateTime(2021, 10, 15, 0, 0, 0, DateTimeKind.Utc);
			var utcEnd = new DateTime(2021, 10, 15, 1, 0, 0, DateTimeKind.Utc);

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Object.UpdateSettings(settings);
			mockPlugin.Object.SetLoggerFactory(mockLoggerFactory.Object);

			try
			{
				using (File.Open(path, FileMode.Open))
				{
					mockPlugin.Object.GetTransactions(utcStart, utcEnd).ToArray();
				}

				Assert.Fail("Should throw IOException");
			}
			catch (IOException exception)
			{
				Assert.True(Regex.IsMatch(exception.Message, @"^The process cannot access the file .* because it is being used by another process.$"));
			}

			mockPlugin.VerifyAll();
		}

		[Test]
		public void TestGetTransactions_ImpersonatorThrowsException_ShouldThrow()
		{
			var mockLogger = new Mock<ILogger>();
			var mockImpersonator = new Mock<Impersonator>(mockLogger.Object);

			var exception = new OperationCanceledException("Failed to impersonate user");
			mockImpersonator.Setup(m => m.Impersonate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(exception);

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Object.SetLoggerFactory(mockLoggerFactory.Object);
			mockPlugin.Setup(m => m.GetImpersonator()).Returns(mockImpersonator.Object);

			var utcStart = new DateTime(2021, 1, 29, 0, 0, 0, DateTimeKind.Utc);
			var utcEnd = new DateTime(2021, 1, 29, 1, 0, 0, DateTimeKind.Utc);

			var transactions = mockPlugin.Object.GetTransactions(utcStart, utcEnd);
			Assert.That(() => { transactions.Count(); },
				Throws.InstanceOf<OperationCanceledException>().With.Message.EqualTo("Failed to impersonate user"));

			mockPlugin.VerifyAll();
		}

		[Test]
		public void TestGetTransactions_ReferenceFilePathDoesNotExist_ShouldThrow()
		{
			var settings = new PluginSettings();
			settings.Parameters = new PluginParameter[]
			{
				new PluginParameter { Name = "ReferenceFilePath", Value = "ReferenceFilePath.csv" },
				new PluginParameter { Name = "SolarWindsServerUrl", Value = "SolarWindsServerUrl" },
				new PluginParameter { Name = "SolarWindsFirewallDistinguishingMask", Value = "SolarWindsFirewallDistinguishingMask" }
			};

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Object.UpdateSettings(settings);
			mockPlugin.Object.SetLoggerFactory(mockLoggerFactory.Object);

			var utcStart = new DateTime(2021, 1, 29, 0, 0, 0, DateTimeKind.Utc);
			var utcEnd = new DateTime(2021, 1, 29, 1, 0, 0, DateTimeKind.Utc);

			var transactions = mockPlugin.Object.GetTransactions(utcStart, utcEnd);
			Assert.That(() => { transactions.Count(); },
				Throws.InstanceOf<FileNotFoundException>().With.Property("FileName").EqualTo("ReferenceFilePath.csv"));

			mockPlugin.VerifyAll();
		}


		#endregion

		#region Test CreateBillingTransactions

		[Test]
		public void TestCreateBillingTransactions_OmitAmbiguousRecords_SameIPHavingMultipleCustomers()
		{
			var mockLog = new Mock<ILogger>(MockBehavior.Strict);
			mockLog.Setup(m => m.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Ambiguous records found: Customers bbbBBB, dddDDD share same IP 2.2.2.2"), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			SetLogger(mockPlugin, mockLog.Object);
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SameIPHavingMultipleCustomers.csv");
			
			var xmlResultFromSolarwindsAmbiguous = @"<queryResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""" + Helper.SolarwindsNamespace + @""">
  <template>
	<resultset>
	  <column name=""SourceIP"" type=""String"" ordinal=""0"" />
	  <column name = ""DestinationIP"" type=""String"" ordinal=""1"" />
	  <column name = ""EgressBytes"" type=""Single"" ordinal=""2"" />
	  <column name = ""TimeStamp"" type=""String"" ordinal=""3"" />
	</resultset>
  </template>
  <data>
	<row>
	  <c0>1.1.1.1</c0>
	  <c1>10.10.10.10</c1>
	  <c2>20000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>1.1.1.2</c0>
	  <c1>10.10.10.10</c1>
	  <c2>30000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>2.2.2.2</c0>
	  <c1>10.10.10.11</c1>
	  <c2>50000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>2.2.2.2</c0>
	  <c1>10.10.10.11</c1>
	  <c2>70000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>3.3.3.3</c0>
	  <c1>10.10.10.10</c1>
	  <c2>60000000</c2>  
	  <c3>2016-12-19T08:21:00.0000000Z</c3>
	</row>
 </data>
</queryResult>";

			var hasErrors = false;
			
			var customerCodeIPCollection = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, mockErrorReportingClient.Object, mockLog.Object);
			Assert.That(hasErrors, Is.False);
			File.Delete(path);

			var rows = GetRowsFromSWResultXml(xmlResultFromSolarwindsAmbiguous);
			var transactions = mockPlugin.Object.CreateBillingTransactions(customerCodeIPCollection, rows)
				.Select(billingTransaction => new TimeStampedTransaction(billingTransaction.ServiceOccuredUTC, billingTransaction)).ToList();

			Assert.AreEqual(3, transactions.ToArray().Length);
			AssertBillingTransaction(transactions[0], 20, "aaa???AAA", "1.1.1.1", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[1], 30, "aaa???AAA", "1.1.1.2", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[2], 60, "ccc???CCC", "3.3.3.3", "10.10.10.10", "2016-12-19T08:21:00.0000000Z");
			mockLog.VerifyAll();
		}


		[Test]
		public void TestCreateBillingTransactions_IgnoreInvalidDecimal()
		{
			var mockLog = new Mock<ILogger>(MockBehavior.Strict);

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			SetLogger(mockPlugin, mockLog.Object);
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "IgnoreInvalidDecimal.csv");

			var xmlResultFromSolarwindsAmbiguous = @"<queryResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""" + Helper.SolarwindsNamespace + @""">
  <template>
	<resultset>
	  <column name=""SourceIP"" type=""String"" ordinal=""0"" />
	  <column name = ""DestinationIP"" type=""String"" ordinal=""1"" />
	  <column name = ""EgressBytes"" type=""Single"" ordinal=""2"" />
	  <column name = ""TimeStamp"" type=""String"" ordinal=""3"" />
	</resultset>
  </template>
  <data>
	<row>
	  <c0>1.1.1.1</c0>
	  <c1>10.10.10.10</c1>
	  <c2>1.24693440452624E+17</c2>
	  <c3>2022-11-01T08:21:00.0000000Z</c3>
	</row>
 </data>
</queryResult>";

			var customerCodeIPCollection = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, mockErrorReportingClient.Object, mockLog.Object);
			File.Delete(path);

			var rows = GetRowsFromSWResultXml(xmlResultFromSolarwindsAmbiguous);
			var transactions = mockPlugin.Object.CreateBillingTransactions(customerCodeIPCollection, rows)
				.Select(billingTransaction => new TimeStampedTransaction(billingTransaction.ServiceOccuredUTC, billingTransaction)).ToList();

			Assert.AreEqual(0, transactions.ToArray().Length);
			mockLog.VerifyAll();
		}

		[Test]
		public void TestCreateBillingTransactions_SourceDestinationAreKeptAsOriginal()
		{
			var mockLog = new Mock<ILogger>(MockBehavior.Strict);
			var mockPlugin = new Mock<Plugin>() { CallBase = true };
			SetLogger(mockPlugin, mockLog.Object);
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SourceDestinationAreKeptAsOriginal.csv");
			var xmlResultFromSolarwindsSwap = @"<queryResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""" + Helper.SolarwindsNamespace + @""">
  <template>
	<resultset>
	  <column name=""SourceIP"" type=""String"" ordinal=""0"" />
	  <column name = ""DestinationIP"" type=""String"" ordinal=""1"" />
	  <column name = ""EgressBytes"" type=""Single"" ordinal=""2"" />
	  <column name = ""TimeStamp"" type=""String"" ordinal=""3"" />
	</resultset>
  </template>
  <data>
	<row>
	  <c0>1.1.1.1</c0>
	  <c1>10.10.10.10</c1>
	  <c2>20000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>1.1.1.2</c0>
	  <c1>10.10.10.10</c1>
	  <c2>30000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>10.10.10.10</c0>
	  <c1>1.1.1.2</c1>
	  <c2>40000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>3.3.3.3</c0>
	  <c1>10.10.10.10</c1>
	  <c2>60000000</c2>  
	  <c3>2016-12-19T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>10.10.10.10</c0>
	  <c1>3.3.3.3</c1>
	  <c2>15000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
 </data>
</queryResult>";

			var customerCodeIPCollection = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, mockErrorReportingClient.Object, mockLog.Object);

			var rows = GetRowsFromSWResultXml(xmlResultFromSolarwindsSwap);
			var transactions = mockPlugin.Object.CreateBillingTransactions(customerCodeIPCollection, rows)
				.Select(billingTransaction => new TimeStampedTransaction(billingTransaction.ServiceOccuredUTC, billingTransaction)).ToList();

			Assert.AreEqual(rows.Count(), transactions.ToArray().Length);
			AssertBillingTransaction(transactions[0], 20, "aaa???AAA", "1.1.1.1", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[1], 30, "aaa???AAA", "1.1.1.2", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[2], 40, "aaa???AAA", "10.10.10.10", "1.1.1.2", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[3], 60, "ccc???CCC", "3.3.3.3", "10.10.10.10", "2016-12-19T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[4], 15, "ccc???CCC", "10.10.10.10", "3.3.3.3", "2016-12-18T08:21:00.0000000Z");
		}

		[Test]
		public void TestCreateBillingTransactions_OverlappingIPShouldBeRemoved()
		{
			var mockLog = new Mock<ILogger>(MockBehavior.Strict);
			mockLog.Setup(m => m.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Ambiguous records found: Customers aaaAAA, bbbBBB share same IP 1.1.1.3, 1.1.1.4"), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			SetLogger(mockPlugin, mockLog.Object);
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "OverlappingIPShouldBeRemoved.csv");

			

			const string xmlResultFromSolarwindsOverlappingIP = @"<queryResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""" + Helper.SolarwindsNamespace + @""">
  <template>
	<resultset>
	  <column name=""SourceIP"" type=""String"" ordinal=""0"" />
	  <column name = ""DestinationIP"" type=""String"" ordinal=""1"" />
	  <column name = ""EgressBytes"" type=""Single"" ordinal=""2"" />
	  <column name = ""TimeStamp"" type=""String"" ordinal=""3"" />
	</resultset>
  </template>
  <data>
	<row>
	  <c0>1.1.1.1</c0>
	  <c1>10.10.10.10</c1>
	  <c2>20000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>1.1.1.2</c0>
	  <c1>10.10.10.10</c1>
	  <c2>30000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>1.1.1.3</c0>
	  <c1>10.10.10.10</c1>
	  <c2>10000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>1.1.1.4</c0>
	  <c1>10.10.10.11</c1>
	  <c2>11000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>1.1.1.6</c0>
	  <c1>10.10.10.10</c1>
	  <c2>12000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>3.3.3.3</c0>
	  <c1>10.10.10.10</c1>
	  <c2>60000000</c2>  
	  <c3>2016-12-19T08:21:00.0000000Z</c3>
	</row>
 </data>
</queryResult>";

			var customerCodeIPCollection = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, mockErrorReportingClient.Object, mockLog.Object);

			var rows = GetRowsFromSWResultXml(xmlResultFromSolarwindsOverlappingIP);
			var transactions = mockPlugin.Object.CreateBillingTransactions(customerCodeIPCollection, rows)
				.Select(billingTransaction => new TimeStampedTransaction(billingTransaction.ServiceOccuredUTC, billingTransaction)).ToList();

			Assert.AreEqual(4, transactions.ToArray().Length);
			AssertBillingTransaction(transactions[0], 20, "aaa???AAA", "1.1.1.1", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[1], 30, "aaa???AAA", "1.1.1.2", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[2], 12, "bbb???BBB", "1.1.1.6", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[3], 60, "ccc???CCC", "3.3.3.3", "10.10.10.10", "2016-12-19T08:21:00.0000000Z");

			mockLog.VerifyAll();
		}

		[Test]
		public void TestCreateBillingTransactions_SameCustomerHavingMultipleIPs()
		{
			var mockLog = new Mock<ILogger>(MockBehavior.Strict);
			var mockPlugin = new Mock<Plugin> { CallBase = true };
			SetLogger(mockPlugin, mockLog.Object);
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SameCustomerHavingMultipleIPs.csv");

			var xmlResultFromSolarWindsHavingMultipleIPs = @"<queryResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""" + Helper.SolarwindsNamespace + @""">
  <template>
	<resultset>
	  <column name=""SourceIP"" type=""String"" ordinal=""0"" />
	  <column name = ""DestinationIP"" type=""String"" ordinal=""1"" />
	  <column name = ""EgressBytes"" type=""Single"" ordinal=""2"" />
	  <column name = ""TimeStamp"" type=""String"" ordinal=""3"" />
	</resultset>
  </template>
  <data>
	<row>
	  <c0>1.1.1.2</c0>
	  <c1>10.10.10.10</c1>
	  <c2>30000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>1.1.1.3</c0>
	  <c1>10.10.10.10</c1>
	  <c2>10000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>2.2.2.2</c0>
	  <c1>10.10.10.11</c1>
	  <c2>11000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>2.2.2.10</c0>
	  <c1>10.10.10.10</c1>
	  <c2>12000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>3.3.3.3</c0>
	  <c1>10.10.10.12</c1>
	  <c2>60000000</c2>  
	  <c3>2016-12-19T08:21:00.0000000Z</c3>
	</row>
 </data>
</queryResult>";

			var customerCodeIPCollection = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, mockErrorReportingClient.Object, mockLog.Object);

			var rows = GetRowsFromSWResultXml(xmlResultFromSolarWindsHavingMultipleIPs);
			var transactions = mockPlugin.Object.
				CreateBillingTransactions(customerCodeIPCollection, rows).Select(billingTransaction => new TimeStampedTransaction(billingTransaction.ServiceOccuredUTC, billingTransaction)).ToList();

			Assert.AreEqual(5, transactions.ToArray().Length);
			AssertBillingTransaction(transactions[0], 30, "aaa???AAA", "1.1.1.2", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[1], 10, "aaa???AAA", "1.1.1.3", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[2], 11, "aaa???AAA", "2.2.2.2", "10.10.10.11", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[3], 12, "aaa???AAA", "2.2.2.10", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[4], 60, "bbb???BBB", "3.3.3.3", "10.10.10.12", "2016-12-19T08:21:00.0000000Z");
		}

		[Test]
		public void TestCreateBillingTransactions_CustomerIPContainingSlashShouldBeOmitted()
		{
			var mockLog = new Mock<ILogger>(MockBehavior.Strict);
			mockLog.Setup(m => m.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Please replace subnet in 2.2.2.2/24 with IP range. Customer code: Customer-bbbBBB-01"), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));
			mockLog.Setup(_ => _.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

			mockLog.Setup(m => m.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Reference file has invalid lines. Check log for which lines are invalid."), It.IsAny<FormatException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			SetLogger(mockPlugin, mockLog.Object);
			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "CustomerIPContainingSlashShouldBeOmitted.csv");

			var xmlResultFromSolarwindsAmbiguous = @"<queryResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""" + Helper.SolarwindsNamespace + @""">
  <template>
	<resultset>
	  <column name=""SourceIP"" type=""String"" ordinal=""0"" />
	  <column name = ""DestinationIP"" type=""String"" ordinal=""1"" />
	  <column name = ""EgressBytes"" type=""Single"" ordinal=""2"" />
	  <column name = ""TimeStamp"" type=""String"" ordinal=""3"" />
	</resultset>
  </template>
  <data>
	<row>
	  <c0>1.1.1.1</c0>
	  <c1>10.10.10.10</c1>
	  <c2>20000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>1.1.1.2</c0>
	  <c1>10.10.10.10</c1>
	  <c2>30000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>2.2.2.2</c0>
	  <c1>10.10.10.11</c1>
	  <c2>50000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>2.2.2.2</c0>
	  <c1>10.10.10.11</c1>
	  <c2>70000000</c2>
	  <c3>2016-12-18T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>3.3.3.3</c0>
	  <c1>10.10.10.10</c1>
	  <c2>60000000</c2>  
	  <c3>2016-12-19T08:21:00.0000000Z</c3>
	</row>
 </data>
</queryResult>";

			var customerCodeIPCollection = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, mockErrorReportingClient.Object, mockLog.Object);

			var rows = GetRowsFromSWResultXml(xmlResultFromSolarwindsAmbiguous);
			var transactions = mockPlugin.Object.CreateBillingTransactions(customerCodeIPCollection, rows)
				.Select(billingTransaction => new TimeStampedTransaction(billingTransaction.ServiceOccuredUTC, billingTransaction)).ToList();

			Assert.AreEqual(2, transactions.ToArray().Length);
			AssertBillingTransaction(transactions[0], 20, "aaa???AAA", "1.1.1.1", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[1], 30, "aaa???AAA", "1.1.1.2", "10.10.10.10", "2016-12-18T08:21:00.0000000Z");
			mockLog.VerifyAll();
		}

		[Test]
		public void TestCreateBillingTransactions_InternalIPShouldBeOmittedExceptWhiteListed()
		{
			var mockLog = new Mock<ILogger>(MockBehavior.Strict);

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(MockIInformationServiceClient().Object);

			SetLogger(mockPlugin, mockLog.Object);

			UpdateSettings(ref mockPlugin,
				refFilePath: csvpath,
				serverUser: "bennelong",
				serverPass: "southern-cross",
				serverDomain: "CORP",
				swUrl: "https://blahblahblah:1010/SolarWinds/InformationService/v3/OrionBasic",
				firewall: new []{"FROU-22"},
				masks:  new []{"Internet-Hot"},
				user: "ReportAPIReadOnly",
				pass: "99",
				ExcludingIPRegex : "^10\\.[0-9]{1,2}\\.[0-9]{1,3}\\.[0-9]{1,3}",
				ExcludingIPWhiteList : "10.2.64.132;10.3.64.160;10.4.64.134");

			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SampleWhiteListMapping.csv");

			var xmlResultFromSolarwindsAmbiguous = @"<queryResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""" + Helper.SolarwindsNamespace + @""">
  <template>
	<resultset>
	  <column name=""SourceIP"" type=""String"" ordinal=""0"" />
	  <column name = ""DestinationIP"" type=""String"" ordinal=""1"" />
	  <column name = ""EgressBytes"" type=""Single"" ordinal=""2"" />
	  <column name = ""TimeStamp"" type=""String"" ordinal=""3"" />
	</resultset>
  </template>
  <data>
	<row>
	  <c0>180.235.156.174</c0>
	  <c1>210.61.48.84</c1>
	  <c2>20000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
<row>
	  <c0>1.1.1.1</c0>
	  <c1>10.10.10.10</c1>
	  <c2>20000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>1.1.1.2</c0>
	  <c1>10.3.64.160</c1>
	  <c2>50000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>2.2.2.2</c0>
	  <c1>10.2.64.132</c1>
	  <c2>70000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>10.4.64.133</c0>
	  <c1>1.1.1.1</c1>
	  <c2>20000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>10.4.64.134</c0>
	  <c1>1.1.1.1</c1>
	  <c2>30000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
 </data>
</queryResult>";

			var customerCodeIPCollection = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, mockErrorReportingClient.Object, mockLog.Object);

			var rows = GetRowsFromSWResultXml(xmlResultFromSolarwindsAmbiguous);
			var transactions = mockPlugin.Object.CreateBillingTransactions(customerCodeIPCollection, rows)
				.Select(billingTransaction => new TimeStampedTransaction(billingTransaction.ServiceOccuredUTC, billingTransaction)).ToList();

			Assert.AreEqual(4, transactions.ToArray().Length);
			AssertBillingTransaction(transactions[0], 20, "MO6???PRD", "180.235.156.174", "210.61.48.84", "2023-05-24T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[1], 50, "aaa???AAA", "1.1.1.2", "10.3.64.160", "2023-05-24T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[2], 70, "bbb???BBB", "2.2.2.2", "10.2.64.132", "2023-05-24T08:21:00.0000000Z");
			AssertBillingTransaction(transactions[3], 30, "aaa???AAA", "10.4.64.134", "1.1.1.1", "2023-05-24T08:21:00.0000000Z");
			mockLog.VerifyAll();
		}

				[Test]
		public void TestCreateBillingTransactions_AryakaPrivateIPShouldBeOmitted()
		{
			var mockLog = new Mock<ILogger>(MockBehavior.Strict);

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(MockIInformationServiceClient().Object);

			SetLogger(mockPlugin, mockLog.Object);

			UpdateSettings(ref mockPlugin,
				refFilePath: csvpath,
				serverUser: "bennelong",
				serverPass: "southern-cross",
				serverDomain: "CORP",
				swUrl: "https://blahblahblah:1010/SolarWinds/InformationService/v3/OrionBasic",
				firewall: new []{"FROU-22"},
				masks:  new []{"Internet-Hot"},
				user: "ReportAPIReadOnly",
				pass: "99",
				ExcludingIPRegex : "^10\\.[0-9]{1,2}\\.[0-9]{1,3}\\.[0-9]{1,3}",
				ExcludingIPWhiteList : "10.2.64.132;10.3.64.160;10.4.64.134",
				AryakaPrivateIPs: "206.201.196.224/28;159.100.192.192/28;147.75.235.208/28");

			var path = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, "SampleFilterIP.csv");

			var xmlResultFromSolarwindsAmbiguous = @"<queryResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""" + Helper.SolarwindsNamespace + @""">
  <template>
	<resultset>
	  <column name=""SourceIP"" type=""String"" ordinal=""0"" />
	  <column name = ""DestinationIP"" type=""String"" ordinal=""1"" />
	  <column name = ""EgressBytes"" type=""Single"" ordinal=""2"" />
	  <column name = ""TimeStamp"" type=""String"" ordinal=""3"" />
	</resultset>
  </template>
  <data>
	<row>
	  <c0>206.201.196.224</c0>
	  <c1>2.2.2.2</c1>
	  <c2>20000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
<row>
	  <c0>159.100.192.192</c0>
	  <c1>159.100.192.195</c1>
	  <c2>20000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>147.75.235.208</c0>
	  <c1>147.75.235.220</c1>
	  <c2>50000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
	<row>
	  <c0>206.201.196.226</c0>
	  <c1>2.2.2.2</c1>
	  <c2>70000000</c2>
	  <c3>2023-05-24T08:21:00.0000000Z</c3>
	</row>
 </data>
</queryResult>";

			var customerCodeIPCollection = WiseCloudSQLAccessReferenceFileProcessor.ProcessReferenceFile(path, mockErrorReportingClient.Object, mockLog.Object);

			var rows = GetRowsFromSWResultXml(xmlResultFromSolarwindsAmbiguous);
			var transactions = mockPlugin.Object.CreateBillingTransactions(customerCodeIPCollection, rows)
				.Select(billingTransaction => new TimeStampedTransaction(billingTransaction.ServiceOccuredUTC, billingTransaction)).ToList();

			Assert.AreEqual(0, transactions.ToArray().Length);
			mockLog.VerifyAll();
		}

		#endregion

		#region Test Query Records

		[TestCase("-5", "offset by 5 hours in early")]
		[TestCase("10", "offset by 10 hours later")]
		[TestCase("-1", "offset by 1 hour in early")]
		[TestCase("-0.5", "offset fraction not accepted")]
		[TestCase("0.5", "offset fraction not accepted")]
		public void TestGetTransactionsWithGivenOffsetHours(string bufferOffsetString, string description)
		{
			var utcEnd = DateTime.UtcNow.AddDays(-1);
			var utcStart = utcEnd.AddDays(-2);
			const string sourceIP = "212.170.34.88";
			const string destinationIP = "203.62.208.56";

			var startHour = new DateTime(utcStart.Year, utcStart.Month, utcStart.Day, utcStart.Hour, 0, 0, DateTimeKind.Utc);
			var endHour = new DateTime(utcEnd.Year, utcEnd.Month, utcEnd.Day, utcEnd.Hour, 0, 0, DateTimeKind.Utc);
			var offsetHours = 0;
			if (int.TryParse(bufferOffsetString, out int offsetValue))
			{
				offsetHours = offsetValue;
			}

			var expectedStartHour = startHour.AddHours(offsetHours);
			var expectedEndHour = endHour.AddHours(offsetHours);

			var listQueriedDateRanges = GetQueryRangeOnQueryGetTransactions(utcStart, utcEnd, sourceIP, destinationIP, bufferOffsetString);
			Assert.That(listQueriedDateRanges, Has.Exactly(1).Matches<Tuple<DateTime, DateTime>>(x => x.Item1 == expectedStartHour), description);
			Assert.That(listQueriedDateRanges, Has.Exactly(1).Matches<Tuple<DateTime, DateTime>>(x => x.Item2 == expectedEndHour), description);
		}

		[Test]
		public void TestUtcToUniversalTime_Produces_WrongDateTime()
		{
			using (MockTimeZone("en-AU"))
			{
				const string utcTime = "2019-08-22T00:00:00Z";
				var utcNow = DateTime.Parse(utcTime);
				var utcNowToUniversalTime = utcNow.ToUniversalTime();
				var utcNowWithDateTimeKind = DateTime.Parse(utcTime, null, DateTimeStyles.AdjustToUniversal);

				Assert.That(utcNowToUniversalTime, Is.Not.EqualTo(utcNow), "result from DateTime.Parse produces local time");
				Assert.That(utcNowToUniversalTime, Is.EqualTo(utcNowWithDateTimeKind));
				Assert.That(utcNow.Kind, Is.Not.EqualTo(DateTimeKind.Utc), "local time and utc time does not equal");
				Assert.That(utcNowWithDateTimeKind.Kind, Is.EqualTo(DateTimeKind.Utc), "string contains UTC info is adjusted to UTC");
			}
		}

		[Test]
		public void TestBufferOffsetInHours()
		{
			var end = DateTime.UtcNow.AddDays(-1);
			var start = end.AddDays(-2);

			var mockLog = new Mock<ILogger>(MockBehavior.Default);
			mockLog.Setup(m => m.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Start processing WiseCloudSQLAccessBilling."), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));
			mockLog.Setup(m => m.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Finished processing WiseCloudSQLAccessBilling."), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(MockIInformationServiceClient().Object);

			SetLogger(mockPlugin, mockLog.Object);

			UpdateSettings(ref mockPlugin,
							refFilePath: csvpath,
							serverUser: "bennelong",
							serverPass: "southern-cross",
							serverDomain: "CORP",
							swUrl: "https://blahblahblah:1010/SolarWinds/InformationService/v3/OrionBasic",
							firewall: new []{"FROU-22"},
							masks:  new []{"Internet-Hot"},
							user: "ReportAPIReadOnly",
							pass: "99");

			var actualBillingTransactions = mockPlugin.Object.GetTransactions(start, end).ToArray();
			mockLog.VerifyAll();
			mockPlugin.VerifyAll();
		}

		List<Tuple<DateTime, DateTime>> GetQueryRangeOnQueryGetTransactions(DateTime utcStart, DateTime utcEnd, string sourceIP, string destinationIP, string bufferOffsetString = "")
		{
			Assert.That(utcEnd, Is.GreaterThan(utcStart), "The provided UTC end time must be greater than the start time");
			Assert.That(utcStart.Kind, Is.EqualTo(DateTimeKind.Utc), "Only UTC date time is accepted");
			Assert.That(utcEnd.Kind, Is.EqualTo(DateTimeKind.Utc), "Only UTC date time is accepted");
			Assert.That(string.IsNullOrEmpty(sourceIP), Is.False, $"Please provide a valid {nameof(sourceIP)}");
			Assert.That(string.IsNullOrEmpty(destinationIP), Is.False, $"Please provide a valid {nameof(destinationIP)}");

			// prepare test data
			var listQueriedDateRanges = new List<Tuple<DateTime, DateTime>>() { };

			var templateDocument = TestHelper.LoadFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, SolarWindQueryResultTemplate);
			var mockServiceClient = MockIInformationServiceClient();
			mockServiceClient
				.Setup(x => x.QueryXml(It.IsAny<string>(), It.IsAny<schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary>()))
				.Returns((string query, schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary args) =>
				{
					var xdoc = new XmlDocument();
					xdoc.LoadXml(templateDocument.OuterXml);

					var match = Regex.Match(query, TimeStampPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						var start = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcStart].Value);
						var end = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcEnd].Value);

						listQueriedDateRanges.Add(new Tuple<DateTime, DateTime>(start, end));
					}

					return xdoc.DocumentElement;
				});

			// configure test data
			OverwriteReferenceFileWith($"Customer IP Address,Firewall Object Name{Environment.NewLine}101.231.62.210,Customer-A5ISHA-01");

			// invoke and verify
			var mockLog = new Mock<ILogger>();
			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(mockServiceClient.Object);

			SetLogger(mockPlugin, mockLog.Object);

			UpdateSettings(ref mockPlugin,
							refFilePath: csvpath,
							serverUser: "bennelong",
							serverPass: "southern-cross",
							serverDomain: "CORP",
							swUrl: "https://blahblahblah:1010/SolarWinds/InformationService/v3/OrionBasic",
							firewall: new []{"FROU-22"},
							masks:  new []{"Internet-Hot"},
							user: "ReportAPIReadOnly",
							pass: "99",
							offset: bufferOffsetString);

			_ = mockPlugin.Object.GetTransactions(utcStart, utcEnd).ToList();
			return listQueriedDateRanges;
		}
		[TestCaseSource(nameof(SolarWindQueryResult))]
		public void TestSolarWindsQuery(string startTime, string endTime)
		{
			var utcStart = Helper.ISO8601StringToDateTimeUtc(startTime);
			var utcEnd = Helper.ISO8601StringToDateTimeUtc(endTime);
			var listQueriedDateRanges = new List<Tuple<DateTime, DateTime>>() { };
			var queryResult = string.Empty;

			var templateDocument = TestHelper.LoadFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, SolarWindQueryResultTemplate);
			var mockServiceClient = MockIInformationServiceClient();
			mockServiceClient
				.Setup(x => x.QueryXml(It.IsAny<string>(), It.IsAny<schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary>()))
				.Returns((string query, schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary args) =>
				{
					var xdoc = new XmlDocument();
					xdoc.LoadXml(templateDocument.OuterXml);

					var match = Regex.Match(query, TimeStampPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						var start = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcStart].Value);
						var end = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcEnd].Value);
						queryResult = query;
						listQueriedDateRanges.Add(new Tuple<DateTime, DateTime>(start, end));
					}
					
					return xdoc.DocumentElement;
				});
			// configure test data
			OverwriteReferenceFileWith($"Customer IP Address,Firewall Object Name{Environment.NewLine}101.231.62.210,Customer-A5ISHA-01");
			// invoke and verify
			var mockLog = new Mock<ILogger>();
			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(mockServiceClient.Object);
			SetLogger(mockPlugin, mockLog.Object);
			UpdateSettings(ref mockPlugin,
							refFilePath: csvpath,
							serverUser: "bennelong",
							serverPass: "southern-cross",
							serverDomain: "CORP",
							swUrl: "https://blahblahblah:1010/SolarWinds/InformationService/v3/OrionBasic",
							firewall: new []{"FROU-22"},
							masks:  new []{"Internet-Hot", "ethernet1/5 ", "ethernet1/6 ", "ethernet1/7 ", "ethernet1/24.300 ", "ethernet1/23.200 "},
							user: "ReportAPIReadOnly",
							portNumber: "1433" ,
							pass: "99",
							offset: "");

			_ = mockPlugin.Object.GetTransactions(utcStart, utcEnd).ToList();
			Assert.AreEqual($@"SELECT
				F.SourceIP,
				F.DestinationIP,
				SUM(F.EgressBytes) AS[EgressBytes],
				MIN(F.TimeStamp) AS[TimeStamp],
				MAX(F.TimeStamp) AS[MaxTimeStamp]
			FROM
				Orion.Nodes AS N
				INNER JOIN Orion.NPM.Interfaces AS I ON I.NodeID = N.NodeID
				INNER JOIN Orion.Netflow.FlowsByConversation  AS F ON F.NodeID = N.NodeID
			WHERE
				(N.Caption like '%FROU-22%')
				AND (I.Caption like '%Internet-Hot%' OR I.Caption like '%ethernet1/5 %' OR I.Caption like '%ethernet1/6 %' OR I.Caption like '%ethernet1/7 %' OR I.Caption like '%ethernet1/24.300 %' OR I.Caption like '%ethernet1/23.200 %')
				AND
				(F.Port = 1433)
				AND
				F.TimeStamp > '2018-09-02T05:00:00Z'
				AND
				F.TimeStamp <= '2018-09-02T06:00:00Z'
			Group by F.SourceIP, F.DestinationIP", queryResult
			);
		}

		[Test]
		public void TestSolarWindsQueryWithDifferentDataCenter()
		{
			var utcStart = Helper.ISO8601StringToDateTimeUtc("2018-09-02T05:01:00Z");
			var utcEnd = Helper.ISO8601StringToDateTimeUtc("2018-09-02T06:00:00Z");
			var listQueriedDateRanges = new List<Tuple<DateTime, DateTime>>() { };
			var queryResult = string.Empty;

			var templateDocument = TestHelper.LoadFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, SolarWindQueryResultTemplate);
			var mockServiceClient = MockIInformationServiceClient();
			mockServiceClient
				.Setup(x => x.QueryXml(It.IsAny<string>(), It.IsAny<schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary>()))
				.Returns((string query, schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary args) =>
				{
					var xdoc = new XmlDocument();
					xdoc.LoadXml(templateDocument.OuterXml);

					var match = Regex.Match(query, TimeStampPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						var start = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcStart].Value);
						var end = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcEnd].Value);
						queryResult = query;
						listQueriedDateRanges.Add(new Tuple<DateTime, DateTime>(start, end));
					}

					return xdoc.DocumentElement;
				});
			// configure test data
			OverwriteReferenceFileWith($"Customer IP Address,Firewall Object Name{Environment.NewLine}101.231.62.210,Customer-A5ISHA-01");
			// invoke and verify
			var mockLog = new Mock<ILogger>();
			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(mockServiceClient.Object);
			SetLogger(mockPlugin, mockLog.Object);
			UpdateSettings(ref mockPlugin,
							refFilePath: csvpath,
							serverUser: "bennelong",
							serverPass: "southern-cross",
							serverDomain: "CORP",
							swUrl: "https://blahblahblah:1010/SolarWinds/InformationService/v3/OrionBasic",
							firewall: new[] { "US2%FROU-22", "AU2%FROU-22", "DE1%FROU-22" },
							masks: new[] { "loopback.10"},
							user: "ReportAPIReadOnly",
							portNumberRange: new string[] { "1051-1052", "1123-1132", "4000-4999", "1062-1120" },
							pass: "99",
							offset: "");

			_ = mockPlugin.Object.GetTransactions(utcStart, utcEnd).ToList();
			Assert.AreEqual($@"SELECT
				F.SourceIP,
				F.DestinationIP,
				SUM(F.EgressBytes) AS[EgressBytes],
				MIN(F.TimeStamp) AS[TimeStamp],
				MAX(F.TimeStamp) AS[MaxTimeStamp]
			FROM
				Orion.Nodes AS N
				INNER JOIN Orion.NPM.Interfaces AS I ON I.NodeID = N.NodeID
				INNER JOIN Orion.Netflow.FlowsByConversation  AS F ON F.NodeID = N.NodeID
			WHERE
				(N.Caption like '%US2%FROU-22%' OR N.Caption like '%AU2%FROU-22%' OR N.Caption like '%DE1%FROU-22%')
				AND (I.Caption like '%loopback.10%')
				AND
				((F.Port >= 1051 AND F.Port <= 1052) OR (F.Port >= 1123 AND F.Port <= 1132) OR (F.Port >= 4000 AND F.Port <= 4999) OR (F.Port >= 1062 AND F.Port <= 1120))
				AND
				F.TimeStamp > '2018-09-02T05:00:00Z'
				AND
				F.TimeStamp <= '2018-09-02T06:00:00Z'
			Group by F.SourceIP, F.DestinationIP", queryResult
			);
		}

		[Test]
		public void TestSolarWindsQueryWithPortsAndPortRanges()
		{
			var utcStart = Helper.ISO8601StringToDateTimeUtc("2018-09-02T05:01:00Z");
			var utcEnd = Helper.ISO8601StringToDateTimeUtc("2018-09-02T06:00:00Z");
			var listQueriedDateRanges = new List<Tuple<DateTime, DateTime>>() { };
			var queryResult = string.Empty;

			var templateDocument = TestHelper.LoadFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, SolarWindQueryResultTemplate);
			var mockServiceClient = MockIInformationServiceClient();
			mockServiceClient
				.Setup(x => x.QueryXml(It.IsAny<string>(), It.IsAny<schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary>()))
				.Returns((string query, schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary args) =>
				{
					var xdoc = new XmlDocument();
					xdoc.LoadXml(templateDocument.OuterXml);

					var match = Regex.Match(query, TimeStampPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						var start = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcStart].Value);
						var end = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcEnd].Value);
						queryResult = query;
						listQueriedDateRanges.Add(new Tuple<DateTime, DateTime>(start, end));
					}

					return xdoc.DocumentElement;
				});
			// configure test data
			OverwriteReferenceFileWith($"Customer IP Address,Firewall Object Name{Environment.NewLine}101.231.62.210,Customer-A5ISHA-01");
			// invoke and verify
			var mockLog = new Mock<ILogger>();
			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(mockServiceClient.Object);
			SetLogger(mockPlugin, mockLog.Object);
			UpdateSettings(ref mockPlugin,
							refFilePath: csvpath,
							serverUser: "bennelong",
							serverPass: "southern-cross",
							serverDomain: "CORP",
							swUrl: "https://blahblahblah:1010/SolarWinds/InformationService/v3/OrionBasic",
							firewall: new []{"FROU-22"},
							masks: new[] { "Internet-Hot", "ethernet1/5 ", "ethernet1/6 ", "ethernet1/7 ", "ethernet1/24.300 ", "ethernet1/23.200 " },
							user: "ReportAPIReadOnly",
							portNumber: "1120;1433",
							portNumberRange: new string[] { "1051-1052", "1123-1129", "4000-4999" },
							pass: "99",
							offset: "");

			_ = mockPlugin.Object.GetTransactions(utcStart, utcEnd).ToList();
			Assert.AreEqual($@"SELECT
				F.SourceIP,
				F.DestinationIP,
				SUM(F.EgressBytes) AS[EgressBytes],
				MIN(F.TimeStamp) AS[TimeStamp],
				MAX(F.TimeStamp) AS[MaxTimeStamp]
			FROM
				Orion.Nodes AS N
				INNER JOIN Orion.NPM.Interfaces AS I ON I.NodeID = N.NodeID
				INNER JOIN Orion.Netflow.FlowsByConversation  AS F ON F.NodeID = N.NodeID
			WHERE
				(N.Caption like '%FROU-22%')
				AND (I.Caption like '%Internet-Hot%' OR I.Caption like '%ethernet1/5 %' OR I.Caption like '%ethernet1/6 %' OR I.Caption like '%ethernet1/7 %' OR I.Caption like '%ethernet1/24.300 %' OR I.Caption like '%ethernet1/23.200 %')
				AND
				(F.Port = 1120 OR F.Port = 1433 OR (F.Port >= 1051 AND F.Port <= 1052) OR (F.Port >= 1123 AND F.Port <= 1129) OR (F.Port >= 4000 AND F.Port <= 4999))
				AND
				F.TimeStamp > '2018-09-02T05:00:00Z'
				AND
				F.TimeStamp <= '2018-09-02T06:00:00Z'
			Group by F.SourceIP, F.DestinationIP", queryResult
			);
		}

		[TestCase("2019-08-22T00:01:20Z", "2019-08-23T23:02:38Z", "212.170.34.88", "203.62.208.56", 47, Description = "2-days span")]
		[TestCase("2019-07-11T03:04:05Z", "2019-07-11T22:21:20Z", "212.170.34.88", "203.62.208.56", 19, Description = "single day span")]
		[TestCase("2019-07-11T03:04:05Z", "2019-07-11T03:34:05Z", "212.170.34.88", "203.62.208.56", 0, Description = "30 min span within the hour")]
		[TestCase("2019-07-11T02:50:00Z", "2019-07-11T03:10:00Z", "212.170.34.88", "203.62.208.56", 1, Description = "20 min span cross the hour")]
		[TestCase("2019-07-11T06:00:00Z", "2019-07-11T07:00:00Z", "212.170.34.88", "203.62.208.56", 1, Description = "an hour span sharp on hours")]
		[TestCase("2019-07-11T06:00:00Z", "2019-07-11T06:59:59Z", "212.170.34.88", "203.62.208.56", 0, Description = "an hour span sharp on hours short by a second")]
		[TestCase("2019-07-11T06:00:01Z", "2019-07-11T07:00:00Z", "212.170.34.88", "203.62.208.56", 1, Description = "an hour span sharp on hours short by a second")]
		public void TestGetTransactionsSplitTimeRangeToWholeHours(string startTime, string endTime, string sourceIP, string destinationIP, int expectedQueries)
		{
			var utcStart = Helper.ISO8601StringToDateTimeUtc(startTime);
			var utcEnd = Helper.ISO8601StringToDateTimeUtc(endTime);

			var listQueriedDateRanges = GetQueryRangeOnQueryGetTransactions(utcStart, utcEnd, sourceIP, destinationIP);

			Assert.That(listQueriedDateRanges, Is.Not.Null);
			Assert.That(listQueriedDateRanges, Has.Count.EqualTo(expectedQueries));

			if (expectedQueries > 0)
			{
				Assert.That(listQueriedDateRanges.Select(x => x.Item1).ToArray(), Has.One.LessThanOrEqualTo(utcStart));
				Assert.That(listQueriedDateRanges.Select(x => x.Item2).ToArray(), Has.Some.GreaterThanOrEqualTo(utcStart));
				Assert.That(listQueriedDateRanges.Select(x => x.Item1).ToArray(), Has.All.LessThanOrEqualTo(utcEnd));
				Assert.That(listQueriedDateRanges.Select(x => x.Item2).ToArray(), Has.All.LessThanOrEqualTo(utcEnd));
			}
		}

		[TestCaseSource(nameof(ExecuteWholeHoursExpectedQueryRange))]
		[TestCaseSource(nameof(Executed59MinutesExpectedQueryRange))]
		[TestCaseSource(nameof(ExecutedLongThanHourExpectedQueryRange))]
		public void TestGetTransactionsExecutedDateTimeRangeResultQueryRanges(
			string startTime,
			string endTime,
			string expectedQueryStartTime,
			string expectedQueryEndTime,
			string description)
		{
			const string sourceIP = "212.170.34.88";
			const string destinationIP = "203.62.208.56";
			var utcStart = Helper.ISO8601StringToDateTimeUtc(startTime);
			var utcEnd = Helper.ISO8601StringToDateTimeUtc(endTime);

			var expectedQueryUtcStart = Helper.ISO8601StringToDateTimeUtc(expectedQueryStartTime);
			var expectedQueryUtcEnd = Helper.ISO8601StringToDateTimeUtc(expectedQueryEndTime);

			var listQueriedDateRanges = GetQueryRangeOnQueryGetTransactions(utcStart, utcEnd, sourceIP, destinationIP);
			if (expectedQueryUtcStart == DateTime.MinValue || expectedQueryUtcEnd == DateTime.MinValue)
			{
				Assert.That(listQueriedDateRanges, Has.Count.LessThanOrEqualTo(0));
			}
			else
			{
				Assert.That(
					listQueriedDateRanges,
					Has.Exactly(1).Matches<Tuple<DateTime, DateTime>>(x =>
						x.Item1 == expectedQueryUtcStart && x.Item2 == expectedQueryUtcEnd), description);
			}
		}

		// sharp hours
		// plugin range = query range
		// 06:00 - 07:00 = 06:00 - 07:00
		// 07:00 - 08:00 = 07:00 - 08:00
		// 08:00 - 09:00 = 08:00 - 09:00
		static readonly IList<TestCaseData> ExecuteWholeHoursExpectedQueryRange = new[]
		{
			new TestCaseData("2018-09-02T06:00:00Z", "2018-09-02T07:00:00Z","2018-09-02T06:00:00Z", "2018-09-02T07:00:00Z", "start whole hour end whole hour result whole hour"),
			new TestCaseData("2018-09-02T07:00:00Z", "2018-09-02T08:00:00Z","2018-09-02T07:00:00Z", "2018-09-02T08:00:00Z", "start whole hour end whole hour result whole hour"),
			new TestCaseData("2018-09-02T08:00:00Z", "2018-09-02T09:00:00Z","2018-09-02T08:00:00Z", "2018-09-02T09:00:00Z", "start whole hour end whole hour result whole hour"),
		};

		// fractioned short to whole hour extend to whole hours to the left
		// plugin range = query range
		// 05:01 - 06:00 = 05:00 - 06:00
		// 06:00 - 06:59 = NA
		// 06:59 - 07:58 = 06:00 - 07:00
		// 07:58 - 08:57 = 07:00 - 08:00
		static readonly IList<TestCaseData> Executed59MinutesExpectedQueryRange = new[]
		{
			new TestCaseData("2018-09-02T05:01:00Z", "2018-09-02T06:00:00Z","2018-09-02T05:00:00Z", "2018-09-02T06:00:00Z", "within hour start fraction end whole result whole hour"),
			new TestCaseData("2018-09-02T06:00:00Z", "2018-09-02T06:59:00Z","NA", "NA", "within hour start whole end fraction result none"),
			new TestCaseData("2018-09-02T06:59:00Z", "2018-09-02T07:58:00Z","2018-09-02T06:00:00Z", "2018-09-02T07:00:00Z", "within hour start fraction end fraction result whole early hour"),
			new TestCaseData("2018-09-02T07:58:00Z", "2018-09-02T08:57:00Z","2018-09-02T07:00:00Z", "2018-09-02T08:00:00Z", "within hour start fraction end fraction result whole early hour"),
		};

		// fractioned longer than an hour extend to whole hours to the left
		// plugin range = query range
		// 05:01 - 06:30 = 05:00 - 06:00
		// 06:00 - 07:01 = 06:00 - 07:00
		// 06:59 - 07:59 = 06:00 - 07:00
		// 07:58 - 08:59 = 07:00 - 08:00
		static readonly IList<TestCaseData> ExecutedLongThanHourExpectedQueryRange = new[]
		{
			new TestCaseData("2018-09-02T05:01:00Z", "2018-09-02T06:30:00Z", "2018-09-02T05:00:00Z", "2018-09-02T06:00:00Z", "over an hour start fraction end fraction result whole hour"),
			new TestCaseData("2018-09-02T06:00:00Z", "2018-09-02T07:01:00Z", "2018-09-02T06:00:00Z", "2018-09-02T07:00:00Z", "over an hour start fraction end fraction result whole hour"),
			new TestCaseData("2018-09-02T06:59:00Z", "2018-09-02T07:59:00Z", "2018-09-02T06:00:00Z", "2018-09-02T07:00:00Z", "over an hour start fraction end fraction result whole hour"),
			new TestCaseData("2018-09-02T07:58:00Z", "2018-09-02T08:59:00Z", "2018-09-02T07:00:00Z", "2018-09-02T08:00:00Z", "over an hour start fraction end fraction result whole hour"),
		};

		static readonly IList<TestCaseData> SolarWindQueryResult = new[]
		{
			new TestCaseData("2018-09-02T05:01:00Z", "2018-09-02T06:00:00Z"),
		};

		[TestCase("2019-08-22T00:00:00Z", "2019-08-23T23:59:59Z", "212.170.34.88", "203.62.208.56", Description = "2-days span")]
		[TestCase("2019-07-11T00:00:00Z", "2019-07-11T23:59:59Z", "212.170.34.88", "203.62.208.56", Description = "single day")]
		public void TestGetTransactionsForSameDateRangeProduceNoDuplicates(string startTime, string endTime, string sourceIP, string destinationIP)
		{
			var utcStart = Helper.ISO8601StringToDateTimeUtc(startTime);
			var utcEnd = Helper.ISO8601StringToDateTimeUtc(endTime);

			Assert.That(utcEnd, Is.GreaterThan(utcStart), "The provided UTC end time must be greater than the start time");
			Assert.That(string.IsNullOrEmpty(sourceIP), Is.False, $"Please provide a valid {nameof(sourceIP)}");
			Assert.That(string.IsNullOrEmpty(destinationIP), Is.False, $"Please provide a valid {nameof(destinationIP)}");

			var xmlSerializer = new XmlSerializer(typeof(WiseCloudSQLAccessRecord), Helper.SolarwindsNamespace);
			var xml = string.Empty;

			// prepare test data
			var records = new List<WiseCloudSQLAccessRecord>();
			var diffTimeRange = (utcEnd - utcStart);
			var random = new Random();
			var listQueriedDateRange = new List<string>();

			const int Repeates = 2;
			for (var i = 0; i < Repeates; i++)
			{
				var randomBytes = (long)((decimal)(100 * (1 + random.NextDouble())) * WiseCloudSQLAccessRecord.Mega);
				records.Add(new WiseCloudSQLAccessRecord(sourceIP, sourceIP, sourceIP, destinationIP, 0, utcStart.AddTicks((long)(random.NextDouble() * diffTimeRange.Ticks)))
				{
					TotalBytesConsumedString = randomBytes.ToString(CultureInfo.InvariantCulture)
				});

				if ((int)diffTimeRange.TotalDays > 1)
				{
					records.Add(new WiseCloudSQLAccessRecord(sourceIP, sourceIP, sourceIP, destinationIP, 0, utcStart.AddDays(1).AddTicks((long)(random.NextDouble() * diffTimeRange.Ticks / 2)))
					{
						TotalBytesConsumedString = randomBytes.ToString(CultureInfo.InvariantCulture)
					});
				}
			}

			var templateDocument = TestHelper.LoadFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, SolarWindQueryResultTemplate);
			var mockServiceClient = MockIInformationServiceClient();
			mockServiceClient
				.Setup(x => x.QueryXml(It.IsAny<string>(), It.IsAny<schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary>()))
				.Returns((string query, schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary args) =>
				{
					var xdoc = new XmlDocument();
					xdoc.LoadXml(templateDocument.OuterXml);

					var match = Regex.Match(query, TimeStampPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						listQueriedDateRange.Add($"{match.Groups[UtcStart].Value}-{match.Groups[UtcEnd].Value}");

						var start = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcStart].Value);
						var end = Helper.ISO8601StringToDateTimeUtc(match.Groups[UtcEnd].Value);

						var recordsOverTheTimeRange = records.Where(x => x.MinTimeStamp >= start && x.MinTimeStamp < end).ToArray();

						foreach (var record in recordsOverTheTimeRange)
						{
							AddRecordToDataElement(record, xdoc);
						}
					}

					return xdoc.DocumentElement;
				});


			// configure test data
			var customerIPs = records.Select(x => x.SourceIp).Distinct().Select(x => $"{x},Customer-A5ISHA-01").ToArray();
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Customer IP Address,Firewall Object Name");
			for (var i = 0; i < customerIPs.Count(); i++)
			{
				stringBuilder.AppendLine($"{customerIPs[i]},Customer-xxx-{i:2}");
			}
			OverwriteReferenceFileWith(stringBuilder.ToString());

			// invoke and verify
			var mockLog = new Mock<ILogger>();
			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(mockServiceClient.Object);

			SetLogger(mockPlugin, mockLog.Object);

			UpdateSettings(ref mockPlugin,
							refFilePath: csvpath,
							serverUser: "bennelong",
							serverPass: "southern-cross",
							serverDomain: "CORP",
							swUrl: "https://blahblahblah:1010/SolarWinds/InformationService/v3/OrionBasic",
							firewall: new []{"FROU-22"},
							masks:  new []{"Internet-Hot"},
							user: "ReportAPIReadOnly",
							pass: "99");

			var billingTransactions = mockPlugin.Object.GetTransactions(utcStart, utcEnd).ToArray();
			var duplicates = from item in billingTransactions
							 group item by new
							 {
								 item.BillingTransaction.ServiceOccuredUTC,
								 item.BillingTransaction.ClientID,
								 item.BillingTransaction.Reference1,
								 item.BillingTransaction.Reference2,
								 item.BillingTransaction.BillableCount
							 } into grouping
							 where grouping.Count() > 1
							 select grouping.Count();

			Assert.That(duplicates.Count(), Is.EqualTo(0), "No duplicates expected");
		}

		[TestCaseSource(nameof(QueryResultXmlFiles))]
		public void TestGetTransactionsFromQueryResults(string resourceFileName)
		{
			// prepare test data
			var resultsDocument = TestHelper.LoadFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, resourceFileName);
			var rows = resultsDocument.GetElementsByTagName("row");
			Assert.That(rows, Is.Not.Null);

			var records = new List<WiseCloudSQLAccessRecord>();
			var xmlSerializer = new XmlSerializer(typeof(WiseCloudSQLAccessRecord));
			foreach (XmlElement row in rows)
			{
				using (var reader = new StringReader(row.OuterXml))
				{
					records.Add((WiseCloudSQLAccessRecord)xmlSerializer.Deserialize(reader));
				}
			}

			var templateDocument = TestHelper.LoadFromEmbeddedResource(typeof(WiseCloudSQLAccessPluginTest).Assembly, SolarWindQueryResultTemplate);
			var mockServiceClient = MockIInformationServiceClient();
			mockServiceClient
				.Setup(x => x.QueryXml(It.IsAny<string>(), It.IsAny<schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary>()))
				.Returns((string query, schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary args) =>
				{
					var xdoc = new XmlDocument();
					xdoc.LoadXml(templateDocument.OuterXml);

					var match = Regex.Match(query, TimeStampPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						var startTime = DateTime.Parse(match.Groups[UtcStart].Value);
						var endTime = DateTime.Parse(match.Groups[UtcEnd].Value);
						var recordsOverTheTimeRange = records.Where(x => x.MinTimeStamp >= startTime && x.MinTimeStamp < endTime).ToArray();

						foreach (var record in recordsOverTheTimeRange)
						{
							AddRecordToDataElement(record, xdoc);
						}
					}

					return xdoc.DocumentElement;
				});


			// configure test data
			var utcStart = records.Min(x => x.MinTimeStamp);
			var utcEnd = records.Max(x => x.MinTimeStamp);

			var customerIPs = records.Select(x => x.SourceIp).Distinct().Select(x => $"{x},Customer-A5ISHA-01").ToArray();
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Customer IP Address,Firewall Object Name");
			for (var i = 0; i < customerIPs.Count(); i++)
			{
				stringBuilder.AppendLine($"{customerIPs[i]},Customer-xxx-{i:2}");
			}
			OverwriteReferenceFileWith(stringBuilder.ToString());

			// invoke and verify
			var mockLog = new Mock<ILogger>();
			var mockPlugin = new Mock<Plugin> { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(mockServiceClient.Object);

			SetLogger(mockPlugin, mockLog.Object);

			UpdateSettings(ref mockPlugin,
							refFilePath: csvpath,
							serverUser: "bennelong",
							serverPass: "southern-cross",
							serverDomain: "CORP",
							swUrl: "https://blahblahblah:1010/SolarWinds/InformationService/v3/OrionBasic",
							firewall: new []{"FROU-22"},
							masks:  new []{"Internet-Hot"},
							user: "ReportAPIReadOnly",
							pass: "99");

			var resultTransactions = mockPlugin.Object.GetTransactions(utcStart, utcEnd).ToList();

			var countBillingTransactions = resultTransactions.Count();
			var countDistinctRecordsResulted = resultTransactions.Select(x => x.GetHashCode()).Distinct().Count();

			var duplicates = from item in resultTransactions
							 group item by new
							 {
								 item.BillingTransaction.ServiceOccuredUTC,
								 item.BillingTransaction.ClientID,
								 item.BillingTransaction.Reference1,
								 item.BillingTransaction.Reference2,
								 item.BillingTransaction.BillableCount
							 } into grouping
							 where grouping.Count() > 1
							 select grouping.Count();

			Assert.That(countDistinctRecordsResulted, Is.EqualTo(countBillingTransactions));
			Assert.That(duplicates.Count(), Is.EqualTo(0), "No duplicates expected");
		}

		static void AddRecordToDataElement(WiseCloudSQLAccessRecord record, XmlDocument xdoc)
		{
			var dataElement = xdoc.GetElementsByTagName(WiseCloudSQLAccessRecord.Element.Data)[0];

			var row = xdoc.CreateElement(WiseCloudSQLAccessRecord.Element.Row, Helper.SolarwindsNamespace);
			var c0 = xdoc.CreateElement(WiseCloudSQLAccessRecord.Element.SourceIp, Helper.SolarwindsNamespace);
			var c1 = xdoc.CreateElement(WiseCloudSQLAccessRecord.Element.DestinationIp, Helper.SolarwindsNamespace);
			var c2 = xdoc.CreateElement(WiseCloudSQLAccessRecord.Element.TotalBytesConsumedString, Helper.SolarwindsNamespace);
			var c3 = xdoc.CreateElement(WiseCloudSQLAccessRecord.Element.MinTimeStampString, Helper.SolarwindsNamespace);

			c0.InnerText = record.SourceIp;
			c1.InnerText = record.DestinationIp;
			c2.InnerText = record.TotalBytesConsumedString;
			c3.InnerText = record.MinTimeStamp.ToString("o", CultureInfo.InvariantCulture);

			row.AppendChild(c0);
			row.AppendChild(c1);
			row.AppendChild(c2);
			row.AppendChild(c3);

			dataElement.AppendChild(row);
		}

		static readonly IList<TestCaseData> QueryResultXmlFiles = new[]
{
			new TestCaseData("SolarWindQueryResult-1.xml")
		};

		#endregion

		const string UtcStart = "utcStart";
		const string UtcEnd = "utcEnd";
		const string TimeStampPattern = @"TimeStamp > '(?<" + UtcStart + @">.*)'\s+AND\s+.*TimeStamp <= '(?<" + UtcEnd + ">.*)'";
		const string SolarWindQueryResultTemplate = "SolarWindQueryResultTemplate.xml";
		string csvpath = Path.ChangeExtension(Path.GetTempFileName(), "csv");

		#region TestInputDates

		[Test]
		public void TestStartDateExactly1DayBeforeEndDate()
		{
			var customerIPsWithHeader = $"Customer IP Address,Firewall Object Name{Environment.NewLine}101.231.62.210,Customer-A5ISHA-01";
			OverwriteReferenceFileWith(customerIPsWithHeader);

			var start = new DateTime(2018, 10, 1, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2018, 10, 2, 0, 0, 0, DateTimeKind.Utc);
			var mockLog = new Mock<ILogger>(MockBehavior.Strict);

			var startHour = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0, DateTimeKind.Utc);
			var endHour = new DateTime(end.Year, end.Month, end.Day, end.Hour, 0, 0, DateTimeKind.Utc);

			mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Information, "Start processing WiseCloudSQLAccessBilling."));
			mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Information, $"Input dates from '{start:u}' to '{end:u}'."));
			mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Information, "CustomerCodeIP collection has 1 records"));
			mockLog.Setup(LoggerTestHelper.GetLogSetupExpression(LogLevel.Information, $"Query hours from '{startHour}' inclusive to '{endHour} exclusive'."));

			var mockPlugin = new Mock<Plugin>() { CallBase = true };
			SetLogger(mockPlugin, mockLog.Object);

			UpdateSettings(ref mockPlugin,
				refFilePath: csvpath,
				swUrl: "xttd://doesnotexist.gov");

			try
			{
				mockPlugin.Object.GetTransactions(start, end).ToArray();
			}
			catch (ArgumentException exception)
			{
				Assert.That(exception.Message, Is.EqualTo("The provided URI scheme 'xttd' is invalid; expected 'https'.\r\nParameter name: via"));
			}
			mockLog.VerifyAll();
		}

		#endregion

		#region Test updated reference file gets processed

		[Test]
		public void TestUpdatedReferenceFile_ProcessedBy_NextExecution()
		{
			var customerIPsWithHeader_1 = $"Customer IP Address,Firewall Object Name{Environment.NewLine}101.231.62.210,Customer-A5ISHA-01";
			var customerIPsWithHeader_2 = $"Customer IP Address,Firewall Object Name{Environment.NewLine}101.231.62.210,Customer-A5ISHA-01{Environment.NewLine}110.143.21.225,Customer-CSLTST-01";

			OverwriteReferenceFileWith(customerIPsWithHeader_1);

			var start = new DateTime(2018, 10, 1, 0, 0, 0);
			var end = new DateTime(2018, 10, 2, 0, 0, 0);
			var mockLog = new Mock<ILogger>();

			var mockPlugin = new Mock<Plugin>() { CallBase = true };
			mockPlugin.Setup(x => x.GetInformationServiceClient()).Returns(MockIInformationServiceClient().Object);

			SetLogger(mockPlugin, mockLog.Object);

			UpdateSettings(ref mockPlugin,
							refFilePath: csvpath,
							serverUser: "user1",
							serverPass: "pass1",
							serverDomain: "domain1",
							firewall: new []{""},
							masks:  new []{"Internet-Hot"},
							user: "ReportAPIReadOnly",
							pass: "pa$$");

			_ = mockPlugin.Object.GetTransactions(start, end).ToArray();

			OverwriteReferenceFileWith(customerIPsWithHeader_2);
			_ = mockPlugin.Object.GetTransactions(start, end).ToArray();
			mockLog.Verify(LoggerTestHelper.GetLogSetupExpression(LogLevel.Information, "CustomerCodeIP collection has 1 records"), Times.Once);
			mockLog.Verify(LoggerTestHelper.GetLogSetupExpression(LogLevel.Information, "CustomerCodeIP collection has 2 records"), Times.Once);
		}

		#endregion

		#region Setup
		[SetUp]
		public void SetUp()
		{
			mockErrorReportingClient = new Mock<IErrorReportingClient>();
			mockErrorReportingClient.Setup(x => x.ServiceUri).Returns(new Uri("", UriKind.Relative));
			mockLogger = new Mock<ILogger>();
			mockLoggerFactory = new Mock<ILoggerFactory>();
			mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
		}
		#endregion

		#region Helpers

		IDisposable MockTimeZone(string name)
		{
			var currentCulture = Thread.CurrentThread.CurrentCulture;
			var currentUICulture = Thread.CurrentThread.CurrentUICulture;

			var cultureInfo = new CultureInfo(name);
			Thread.CurrentThread.CurrentCulture = cultureInfo;
			Thread.CurrentThread.CurrentUICulture = cultureInfo;

			return Disposable.Create(() =>
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			});
		}

		Mock<IInformationServiceClient> MockIInformationServiceClient()
		{
			const string queryResultXml = @"<queryResult xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""" + Helper.SolarwindsNamespace + @""">
  <template>
	<resultset>
	  <column name=""SourceIP"" type=""String"" ordinal=""0"" />
	  <column name=""DestinationIP"" type=""String"" ordinal=""1"" />
	  <column name=""EgressBytes"" type=""Double"" ordinal=""2"" />
	  <column name=""TimeStamp"" type=""DateTime"" ordinal=""3"" />
	</resultset>
  </template>
</queryResult>";
			var doc = new XmlDocument();
			doc.LoadXml(queryResultXml);

			var mockServiceClient = new Mock<IInformationServiceClient>();
			mockServiceClient.Setup(x => x.ClientCredentials).Returns(new ClientCredentials());
			mockServiceClient.Setup(x => x.QueryXml(It.IsAny<string>(), It.IsAny<schemas.solarwinds.com._2007._08.informationservice.propertybag.dictionary>()))
				.Returns(doc.DocumentElement);

			return mockServiceClient;
		}

		#endregion

		Mock<IErrorReportingClient> mockErrorReportingClient;
		Mock<ILoggerFactory> mockLoggerFactory;
		Mock<ILogger> mockLogger;
	}
}
