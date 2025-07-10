using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using WTG.DevTools.Definitions;

namespace Enterprise.MasterFiles.Business.VersionReport.Testing
{
	sealed class VersionReportTest : TestCaseWithFactory
	{
		const int NumberOfExpectedLinesInXmlExcludingDbFiles = 131;

		#region All Fields Tested

		public void TestNewFieldsTested()
		{
			FieldInfo[] allFields = typeof(VersionReport).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

			var newFields = new StringBuilder();
			foreach (var field in allFields)
			{
				if (!ExpectedFieldsOnVersionReport.Contains(field.Name))
				{
					newFields.AppendLine(field.Name);
				}
			}

			if (newFields.Length > 0)
			{
				Fail("The following fields are new. Please add them to the ExpectedFieldsOnVersionReport collection, AND update the XML tests for the new field:\r\n\r\n" + newFields.ToString());
			}

			AssertEquals("No fields have been removed - if this fails, please remove the field from the ExpectedFieldsOnVersionReport collection and update the XML tests for the removed fields", ExpectedFieldsOnVersionReport.Count, allFields.Length);
		}

		StringCollectionX expectedFieldsOnVersionReport;
		StringCollectionX ExpectedFieldsOnVersionReport
		{
			get
			{
				if (expectedFieldsOnVersionReport == null)
				{
					expectedFieldsOnVersionReport = new StringCollectionX();
					expectedFieldsOnVersionReport.AddRange(
						new string[] {
							"factory",
							"reportXml",
							"databaseNumber",
							"enterpriseCode",
							"physicalServerID",
							"dbServerName",
							"dbName",
							"currentVersion",
							"currentDate",
							"currentRelease",
							"preferredUpgradeMethod",
							"dbServerSecurityMode",
							"sqlServerName",
							"sqlServerInstanceName",
							"internalPOP3UserName",
							"internalPOP3MailServer",
							"internalMailServerPort",
							"internalSMTPMailServer",
							"internalSMTPPort",
							"logAndDataFiles",
							"companyList",
							"databaseBackupPath",
							"encryptedSystemExpirationKey",
							"encryptedRegistrationKey",
							"internalPOP3EmailAddress",
							"activePrintersCount",
							"sqlServerEdition",
							"sqlServerFullVersionText",
							"sqlServerVersion",
							"additionalDatabaseSystemInfoList",
							"licenceUsage",
							"isBasic",
							"outboundEAdaptorUrl",
							"nextRunTimeUtcUPG",
							"nextRunTimeUtcMUG",
							"scheduleStateUPG",
							"scheduleStateMUG",
							"tokenAuthenticationEnabled",
							"featureControlRule",
							"supportedFeatureCodes",
						}
					);
				}

				return expectedFieldsOnVersionReport;
			}
		}

		#endregion

		#region Valid XML

		public void TestLoadCompanies()
		{
			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			var report = new VersionReport(regKey, true, "server", "db", "1.2.3.4", "ALP", ZDateTime.UtcToday, null, "");
			var actual = report.CompanyList.OrderBy(x => x.Code).ToArray();
			var expected = new GlbCompanyCollection(Factory, new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode))
				.OrderBy(x => x.GC_Code).ToArray();
			AssertEquals(expected.Length, actual.Length);
			for (int i = 0; i < actual.Length; ++i)
			{
				AssertCompany(expected[i], actual[i]);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:EAdaptorNamingRule", Justification = "Testing")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1057:DoNotHardcodeMailserverName", Justification = "Testing")]
		public void TestVersionReportWithIndividualParameters()
		{
			using var nextRunTimeSubstitue = PrepareTestUpgradeScheduleInfo("NextRunTime");
			using var scheduleStateSubstitute = PrepareTestUpgradeScheduleInfo("ScheduleState");
			PrepareTokenAuthentication();
			CreateTestCompanies();
			WebDataRegistry.Instance.FeatureControlRuleContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, FeatureControlRule);
			Factory.Save();

			int dbFileCount = AssertOperationalDatabasesAndReturnFileCount();

			ZDateTime currentDate = ZDateTime.Now;

			List<String> additionalInfoList = new List<string>();
			additionalInfoList.Add("Host Name:                 SYD-WSCW-1");
			additionalInfoList.Add("OS Name:                   Microsoft Windows 7 Enterprise");
			additionalInfoList.Add("OS Version:                6.1.7600 N/A Build 7600");
			additionalInfoList.Add("System Manufacturer:       Gigabyte Technology Co., Ltd.");
			additionalInfoList.Add("BIOS Version:              Award Software International, Inc. F5, 18/06/2008");
			additionalInfoList.Add("System Locale:             en-us;English (United States)");
			additionalInfoList.Add("Total Physical Memory:     8,190 MB");
			additionalInfoList.Add("Time Zone:                 (UTC+10:00) Canberra, Melbourne, Sydney");
			additionalInfoList.Add("Processor(s):              1 Processor(s) Installed.");
			additionalInfoList.Add("                           [01]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~3000 Mhz");
			additionalInfoList.Add("System Model:              EP45-DS3P");

			string licenceUsage = "SomeUsage";

			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			var report = new VersionReportForTesting(regKey, true, "DBS\\SRV", "DB", "1.2.3.0", ReleaseRings.Codes.ALP, currentDate, additionalInfoList, licenceUsage);
			report.AddCompaniesForTest(new GlbCompany[] { TestCompany1, TestCompany2 });

			AssertEquals(
				"IF THIS FAILS, THEN YOU HAVE NOT ADDED YOUR NEW FIELD TO THIS TEST.\n\nPlease add your new field on the version report to this test, and increment the count of NumberOfExpectedFieldsInXML.",
				NumberOfExpectedLinesInXmlExcludingDbFiles + dbFileCount,
				report.XML.Occurrences(System.Environment.NewLine));

			AssertEquals("EnterpriseCode", "ENT", report.EnterpriseCode);
			AssertEquals("PhysicalServerID", "SRV", report.PhysicalServerID);
			AssertEquals("DBServerName", "DBS\\SRV", report.DBServerName);
			AssertEquals("DBName", "DB", report.DBName);
			AssertEquals("CurrentVersion", "1.2.3.0", report.CurrentVersion);
			AssertEquals("CurrentDate", currentDate, report.CurrentDate);
			AssertEquals("CurrentRelease", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.ALP, new VersionNumber(1, 4, 3, 0)), report.CurrentRelease);
			AssertEquals("PreferredUpgradeMethod", "DEF", report.PreferredUpgradeMethod);
			AssertEquals("DBServerSecurityMode", "O12", report.DBServerSecurityMode);
			AssertEquals("SQLServerName", "DBS", report.SQLServerName);
			AssertEquals("SQLServerInstanceName", "SRV", report.SQLServerInstanceName);
			AssertEquals("InternalPOP3EmailAddress", "POP3EmailAddress@edi.com.au", report.InternalPOP3EmailAddress);
			AssertEquals("InternalPOP3UserName", "POP3UserName", report.InternalPOP3UserName);
			AssertEquals("InternalPOP3MailServer", "POP3MailServer", report.InternalPOP3MailServer);
			AssertEquals("InternalMailServerPort", 8080, report.InternalMailServerPort);
			AssertEquals("InternalSMTPMailServer", "XCH", report.InternalSMTPMailServer);
			AssertEquals("InternalSMTPPort", 9090, report.InternalSMTPPort);
			AssertEquals("Four database files listed", dbFileCount, report.LogAndDataFiles.Count);
			AssertLogAndDataFile(report, Db.DatabaseName, "MDF");
			AssertLogAndDataFile(report, Db.DatabaseName, "LDF");
			AssertEquals("Database Backup Path", Env.Registry.BackupDirectoryPath, report.DatabaseBackupPath);
			AssertEquals("Encrypted Licence Key", "{test legacy key}", report.EncryptedSystemExpirationKey);
			AssertEquals("Active Printer Count", 3, report.ActivePrintersCount);
			AssertEquals("Twelve database system info records", 11, report.AdditionalDatabaseSystemInfoList.Count);
			AssertEquals("Licence Usage", "SomeUsage", report.LicenceUsage);
			AssertEquals("Outboud eAdapter Url", "https://antongorlin.com", report.OutboundEAdaptorUrl);
			AssertEquals("Next RunTimeUtc UPG", NextRunTimeUtcUPG, report.NextRunTimeUtcUPG);
			AssertEquals("Next RunTimeUtc UPG DateTimeKind", DateTimeKind.Utc, report.NextRunTimeUtcUPG.Kind);
			AssertEquals("Next RunTimeUtc MUG", NextRunTimeUtcMUG, report.NextRunTimeUtcMUG);
			AssertEquals("Next RunTimeUtc MUG DateTimeKind", DateTimeKind.Utc, report.NextRunTimeUtcMUG.Kind);
			AssertEquals("ScheduleState UPG", ScheduleStateUPG, report.ScheduleStateUPG);
			AssertEquals("ScheduleState MUG", ScheduleStateMUG, report.ScheduleStateMUG);
			AssertEquals("TokenAuthenticationEnabled", true, report.TokenAuthenticationEnabled);
			AssertEquals("FeatureControlRule", FeatureControlRule, report.FeatureControlRule);

			AssertEquals("Number of test companies", 2, report.CompanyList.Count);
			AssertCompany(TestCompany1, report.CompanyList[0]);
			AssertCompany(TestCompany2, report.CompanyList[1]);
		}

		IDisposable PrepareTestUpgradeScheduleInfo(string scheduleInfoType)
		{
			if (scheduleInfoType.Equals("NextRunTime"))
			{
				var mockServiceManagerQuerier = new Mock<IServiceManagerQuerier>();
				var upgNextRunTime = NextRunTimeUtcUPG.ToNullableDateTimeOffset();
				var mugNextRunTime = NextRunTimeUtcMUG.ToNullableDateTimeOffset();
				mockServiceManagerQuerier
					.Setup(q => q.TryGetServiceTaskNextRunTime("UPG", out upgNextRunTime))
					.Returns(true);
				mockServiceManagerQuerier
					.Setup(q => q.TryGetServiceTaskNextRunTime("MUG", out mugNextRunTime))
					.Returns(true);

				return ObjectFactory.Substitute(mockServiceManagerQuerier.Object);
			}

			var mockScheduleStateQuerier = new Mock<IServiceTaskScheduleStateQuerier>();
			var upgScheduleState = ScheduleStateUPG;
			var mugScheduleState = ScheduleStateMUG;
			mockScheduleStateQuerier
				.Setup(q => q.TryGetServiceTaskScheduleState("UPG", out upgScheduleState))
				.Returns(true);
			mockScheduleStateQuerier
				.Setup(q => q.TryGetServiceTaskScheduleState("MUG", out mugScheduleState))
				.Returns(true);

			return ObjectFactory.Substitute(mockScheduleStateQuerier.Object);
		}

		void PrepareTokenAuthentication()
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = "https://test.com",
				ClientIdentifier = "testid",
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "testname", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
			oidcConfig.IsVerified = true;

			SystemDataRegistry.Instance.OIDCConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig);
		}

		void AssertCompany(GlbCompany a, CompanyReport b)
		{
			AssertEquals(a.GC_Address1, b.Address1);
			AssertEquals(a.GC_Address2, b.Address2);
			AssertEquals(a.GC_BusinessRegNo, b.BusinessRegNo);
			AssertEquals(a.GC_BusinessRegNo2, b.BusinessRegNo2);
			AssertEquals(a.GC_City, b.City);
			AssertEquals(a.GC_Code, b.Code);
			AssertEquals(a.GC_RN_NKCountryCode, b.CountryCode);
			AssertEquals(a.GC_RX_NKLocalCurrency, b.CurrencyCode);
			AssertEquals(a.GC_CustomsRegistrationNo, b.CustomsRegistrationNo);
			AssertEquals(a.GC_IsActive, b.IsActive);
			AssertEquals(a.GC_IsGSTCashBasis, b.IsGSTCashBasis);
			AssertEquals(a.GC_IsGSTRegistered, b.IsGSTRegistered);
			AssertEquals(a.GC_IsReciprocal, b.IsReciprocal);
			AssertEquals(a.GC_IsWHTCashBasis, b.IsWHTCashBasis);
			AssertEquals(a.GC_IsWHTRegistered, b.IsWHTRegistered);
			AssertEquals(a.GC_Name, b.Name);
			AssertEquals(a.GC_Phone, b.Phone);
			AssertEquals(a.GC_PostCode, b.PostCode);
			AssertEquals(a.GC_State, b.State);
			AssertEquals(a.GC_WebAddress, b.WebAddress);
		}

		/// <summary>
		/// Expected operational databases are 2: 1 main + 1 SD.
		/// If more DBs are added in the test run, it will impact in the number of DB files.
		/// </summary>
		/// <returns></returns>
		int AssertOperationalDatabasesAndReturnFileCount()
		{
			var operationalDbs = TestConnection.GetDatabases(DatabaseType.Operational);

			if (TestingState.IsRunningOnDAT)
			{
				var dbNames = string.Join(", ", operationalDbs);
				AssertEquals($"Number of Operation Databases should be 2. If more are added it will impact in the number of DB files. Now we found [{dbNames}]", 2, operationalDbs.Count());
			}

			int dbFileCount = TestConnection.GetDbFiles(operationalDbs.First()).Length + TestConnection.GetDbFiles(operationalDbs.Last()).Length;
			return dbFileCount;
		}

		void AssertLogAndDataFile(VersionReport report, string dbName, string fileExtension)
		{
			bool found = false;

			var fileRegex = new Regex(String.Format(@"{0}[^.]*\.{1}", dbName, fileExtension), RegexOptions.IgnoreCase);

			foreach (string filePath in report.LogAndDataFiles)
			{
				if (fileRegex.IsMatch(filePath))
				{
					found = true;
				}
			}

			Assert("The (" + fileExtension + ") database file should have been found", found);
		}

		#endregion

		#region XML Generation

		public void TestXML()
		{
			using var nextRunTimeSubstitue = PrepareTestUpgradeScheduleInfo("NextRunTime");
			using var scheduleStateSubstitute = PrepareTestUpgradeScheduleInfo("ScheduleState");
			PrepareTokenAuthentication();
			WebDataRegistry.Instance.FeatureControlRuleContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, FeatureControlRule);

			string actualXml = TestVersionReport.XML;

			string msg = "If this fails on a developer machine its because you dont have the Scanned Docs database installed as part of the standard test configuration. These tests only run on Auto-testers, so dont change anything unless you know what you're doing.";

			// This has a more friendly displace of differences, but removes \r
			AssertMultilineASCIIEquals(msg, XmlDataForTestingNewFormat, actualXml);

			// Ensure an exact match
			AssertEquals(msg, XmlDataForTestingNewFormat, actualXml);
		}

		public static string StringToHex(string input)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(input);
			return string.Concat(bytes.Select(b => b.ToString("X2")));
		}

		public static byte[] HexStringToByteArray(string hex)
		{
			if (hex.StartsWith("0x"))
			{
				hex = hex.Substring(2);
			}

			int length = hex.Length / 2;
			byte[] result = new byte[length];

			for (int i = 0; i < length; i++)
			{
				string byteValue = hex.Substring(i * 2, 2);
				if (!byte.TryParse(byteValue, System.Globalization.NumberStyles.HexNumber, null, out byte byteResult))
				{
					throw new ArgumentException("Invalid hexadecimal string.");
				}
				result[i] = byteResult;
			}

			return result;
		}

		VersionReportForTesting TestVersionReport
		{
			get
			{
				CreateTestCompanies();
				Factory.Save();

				var additionalInfoList = new List<string>
				{
					"Host Name:                 SYD-WSCW-1",
					"OS Name:                   Microsoft Windows 7 Enterprise",
					"OS Version:                6.1.7600 N/A Build 7600",
					"System Manufacturer:       Gigabyte Technology Co., Ltd.",
					"BIOS Version:              Award Software International, Inc. F5, 18/06/2008",
					"System Locale:             en-us;English (United States)",
					"Total Physical Memory:     8,190 MB",
					"Time Zone:                 (UTC+10:00) Canberra, Melbourne, Sydney",
					"Processor(s):              1 Processor(s) Installed.",
					"                           [01]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~3000 Mhz",
					"System Model:              EP45-DS3P"
				};
				var result = new VersionReportForTesting(ObjectFactory.Get<IProductRegistration>().KeyForTest, true,
					"NTSQLSRV\\TESTDB", "OdysseyTest",
					"1.1.1000.10000", ReleaseRings.Codes.GPR, CurrentDate,
					additionalInfoList, "SomeUsage");
				result.AddCompaniesForTest(new GlbCompany[] { TestCompany1, TestCompany2 });
				return result;
			}
		}

		#endregion

		#region Basic Version Only Report

		[TestDate(2015, 7, 6, 15, 28, 0)]
		public void TestCreateBasic()
		{
			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			var reportTimeUtc = ZDateTime.UtcNow;
			var report = VersionReport.CreateBasic(regKey, "NTSQLSRV\\TESTDB", "OdysseyTest", "1.2.3.4", reportTimeUtc);
			AssertEquals(TestDateAttribute.Date, report.CurrentDate);
			AssertEquals("1.2.3.4", report.CurrentVersion);
			var xml = report.XML;
			const string expectedTemplateXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<VersionReport>
  <DatabaseNumber>1234</DatabaseNumber>
  <EnterpriseCode>ENT</EnterpriseCode>
  <PhysicalServerID>SRV</PhysicalServerID>
  <DBServerName>NTSQLSRV\TESTDB</DBServerName>
  <DBName>OdysseyTest</DBName>
  <CurrentVersion>1.2.3.4</CurrentVersion>
  <CurrentDate>{CurrentDate}</CurrentDate>
  <SQLServerName>NTSQLSRV</SQLServerName>
  <SQLServerInstanceName>TESTDB</SQLServerInstanceName>
  <RegistrationData>{test reg key}</RegistrationData>
  <OutboundEAdaptorUrl>https://antongorlin.com</OutboundEAdaptorUrl>
</VersionReport>";
			var expectedXml = expectedTemplateXml
				.Replace("{CurrentDate}", reportTimeUtc.ToString("o").ToUpper());

			AssertMultilineASCIIEquals("XML", expectedXml, xml);
			AssertEquals(expectedXml, xml);
		}

		#endregion

		#region Implementation

		void CreateTestCompanies()
		{
			if (TestCompany1 != null)
			{
				return;
			}

			TestCompany1 = Factory.New<GlbCompany>();
			TestCompany2 = Factory.New<GlbCompany>();

			var orgProxy1 = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy1.OH_RL_NKClosestPort = "AUSYD";

			var orgProxy2 = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy2.OH_RL_NKClosestPort = "NZAKL";

			TestCompany1.GC_Address1 = "5/123 中文百强网 <need xml escaping>";
			TestCompany1.GC_Address2 = @"6\7";
			TestCompany1.GC_BusinessRegNo = "123-456";
			TestCompany1.GC_BusinessRegNo2 = "AA";
			TestCompany1.GC_City = "City1";
			TestCompany1.GC_Code = "111";
			TestCompany1.GC_RN_NKCountryCode = "AU";
			TestCompany1.GC_RX_NKLocalCurrency = "AUD";
			TestCompany1.GC_CustomsRegistrationNo = "789";
			TestCompany1.GC_IsActive = false;
			TestCompany1.GC_IsGSTCashBasis = false;
			TestCompany1.GC_IsGSTRegistered = false;
			TestCompany1.GC_IsReciprocal = false;
			TestCompany1.GC_IsWHTCashBasis = false;
			TestCompany1.GC_IsWHTRegistered = false;
			TestCompany1.GC_Name = "Company 1";
			TestCompany1.GC_Phone = "(1) 234";
			TestCompany1.GC_PostCode = "2000";
			TestCompany1.GC_State = "NSW";
			TestCompany1.GC_WebAddress = "http://www.wisetechglobal.com/index.html?a=2#anchor";
			TestCompany1.GC_OH_OrgProxy = orgProxy1.PK;

			TestCompany2.GC_Address1 = "Addr 1";
			TestCompany2.GC_Address2 = @"Addr 2\2";
			TestCompany2.GC_BusinessRegNo = "2";
			TestCompany2.GC_BusinessRegNo2 = "2b";
			TestCompany2.GC_City = "City2";
			TestCompany2.GC_Code = "222";
			TestCompany2.GC_RN_NKCountryCode = "NZ";
			TestCompany2.GC_RX_NKLocalCurrency = "NZD";
			TestCompany2.GC_CustomsRegistrationNo = "2222";
			TestCompany2.GC_IsActive = true;
			TestCompany2.GC_IsGSTCashBasis = true;
			TestCompany2.GC_IsGSTRegistered = true;
			TestCompany2.GC_IsReciprocal = true;
			TestCompany2.GC_IsWHTCashBasis = true;
			TestCompany2.GC_IsWHTRegistered = true;
			TestCompany2.GC_Name = "Company 2";
			TestCompany2.GC_Phone = "(2) 567";
			TestCompany2.GC_PostCode = "1000";
			TestCompany2.GC_State = "CHC";
			TestCompany2.GC_WebAddress = "https://www.cargowise.com";
			TestCompany2.GC_OH_OrgProxy = orgProxy2.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			EnvProxy.SetHostedLocationForTest("SYD"); // force DB security to Open
			Env.Registry.MailboxEmailAddress = "POP3EmailAddress@edi.com.au";
			Env.Registry.MailboxUserName = "POP3UserName";
			Env.Registry.MailServer = "POP3MailServer";
			Env.Registry.BackupDirectoryPath = Env.TempPath;
			Env.Registry.MailServerPort = 8080;
			Env.Registry.SMTPPort = 9090;
			Env.Registry.LegacyEncryptedSystemRegistrationKey = "{test legacy key}";
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://antongorlin.com");
			CreateDocEngineTestPrinters();

			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			regKey.DatabaseNumberForTest = 1234;
			regKey.EnterpriseCodeForTest = "ENT";
			regKey.ServerCodeForTest = "SRV";

			Env.Registry.RawRegistry.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "{test reg key}");
		}

		GlbCompany TestCompany1;
		GlbCompany TestCompany2;

		void CreateDocEngineTestPrinters()
		{
			var serverPK = CreateTestPrinterServer();

			CreateTestPrinter(ZBool.True, "printer1", serverPK);
			CreateTestPrinter(ZBool.True, "printer2", serverPK);
			CreateTestPrinter(ZBool.False, "printer3", serverPK);
			CreateTestPrinter(ZBool.False, "printer4", serverPK);
			CreateTestPrinter(ZBool.True, "printer5", serverPK);
		}

		void CreateTestPrinter(ZBool isActive, string printerName, Guid serverPK)
		{
			string sQL = String.Format("INSERT INTO dbo.StmPrintQueue (SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) VALUES (NEWID(), {0}, '{1}', '{1}', '{2}')", isActive ? "1" : "0", printerName, serverPK);
			TestConnection.ExecuteNonQuery(sQL);
		}

		Guid CreateTestPrinterServer()
		{
			var serverPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"insert into dbo.StmPrintServer(SPS_PK, SPS_ServerName) values('{serverPK}', 'MyServer')");
			return serverPK;
		}

		#endregion

		#region Properties

		readonly ZDateTime CurrentDate = ZDateTime.Now;
		readonly ZDateTime NextRunTimeUtcUPG = new ZDateTime(2023, 6, 8, 15, 0, 0, DateTimeKind.Utc);
		readonly ZDateTime NextRunTimeUtcMUG = new ZDateTime(2023, 6, 8, 16, 0, 0, DateTimeKind.Utc);
		const string ScheduleStateUPG = "<HostedServiceSerializableSettings><ConfigString>TestScheduleStateUPG</ConfigString><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>";
		const string ScheduleStateMUG = "<HostedServiceSerializableSettings><ConfigString>TestScheduleStateMUG</ConfigString><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>";
		const string FeatureControlRule = "H4sIAAAAAAAEAIWR0WqDMBSG7wd7B/Fek2hbOrH2wrVQaGGoZaM3JehhDWiUJK3d2y9aKUYYg1ycnHzfT3ISru9Vad1ASFbzlU1cbK+j15dwC1RdBcQ1V6IuLQ1xGdxlsbIvSjUBQm3buq3v1uIbeRgT9HXYp/kFKmo/YfY/7DAuFeU5DJZhQMHcvK5cekUbrkA0gklIQdxYDsjWt7SsMGMV6ISqOao88rA3c4jnkLcML4P5LCDEnfv+KUQG1ovJtQTZl8NmqPVuGx/O5vvjuoAoTubJJv3c6bg/iFFCcu4ys59Ge/tdZ4w6BpcqKtQ7VWC8YJlhHPTr9JANzAjY8GKq+2SqjyBD/qCCVqCHK6PFYqE/Hk26w4TQc0SPsjvQrDGD6BfcxIckTQIAAA==";

		string XmlDataForTestingNewFormat
		{
			get
			{
				if (xmlDataForTestingNewFormat == null)
				{
					CreateTestCompanies();
					Factory.Save();

					var result = new StringBuilder();

					result.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
					result.AppendLine("<VersionReport>");
					result.AppendLine("  <DatabaseNumber>1234</DatabaseNumber>");
					result.AppendLine("  <EnterpriseCode>ENT</EnterpriseCode>");
					result.AppendLine("  <PhysicalServerID>SRV</PhysicalServerID>");
					result.AppendLine("  <DBServerName>NTSQLSRV\\TESTDB</DBServerName>");
					result.AppendLine("  <DBName>OdysseyTest</DBName>");
					result.AppendLine("  <CurrentVersion>1.1.1000.10000</CurrentVersion>");
					result.AppendFormat("  <CurrentDate>{0}</CurrentDate>\r\n", CurrentDate.ToString("o").ToUpper());
					result.AppendFormat("  <CurrentRelease>{0}</CurrentRelease>\r\n", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.GPR, new VersionNumber(1, 4, 1000, 10000)));
					result.AppendLine("  <PreferredUpgradeMethod>DEF</PreferredUpgradeMethod>");
					result.AppendLine("  <DBServerSecurityMode>O12</DBServerSecurityMode>");
					result.AppendLine("  <SQLServerName>NTSQLSRV</SQLServerName>");
					result.AppendLine("  <SQLServerInstanceName>TESTDB</SQLServerInstanceName>");
					result.AppendLine("  <POP3>");
					result.AppendLine("    <EmailAddress>POP3EmailAddress@edi.com.au</EmailAddress>");
					result.AppendLine("    <UserName>POP3UserName</UserName>");
					result.AppendLine("    <MailServer>POP3MailServer</MailServer>");
					result.AppendLine("    <Port>8080</Port>");
					result.AppendLine("  </POP3>");
					result.AppendLine("  <SMTP>");
					result.AppendLine("    <MailServer>XCH</MailServer>");
					result.AppendLine("    <Port>9090</Port>");
					result.AppendLine("  </SMTP>");
					result.AppendLine("  <DBFileNameList>");
					AppendDatabaseFilesToTestVersionReportXml(result);
					result.AppendLine("  </DBFileNameList>");
					result.AppendLine(
$@"  <CompanyList>
    <Company>
      <Code>111</Code>
      <Name>Company 1</Name>
      <CountryCode>AU</CountryCode>
      <CurrencyCode>AUD</CurrencyCode>
      <Address1>5/123 中文百强网 &lt;need xml escaping&gt;</Address1>
      <Address2>6\7</Address2>
      <City>City1</City>
      <State>NSW</State>
      <PostCode>2000</PostCode>
      <Phone>(1) 234</Phone>
      <BusinessRegNo>123-456</BusinessRegNo>
      <BusinessRegNo2>AA</BusinessRegNo2>
      <CustomsRegistrationNo>789</CustomsRegistrationNo>
      <WebAddress>http://www.wisetechglobal.com/index.html?a=2#anchor</WebAddress>
      <Email />
      <IsActive>0</IsActive>
      <IsGSTRegistered>0</IsGSTRegistered>
      <IsGSTCashBasis>0</IsGSTCashBasis>
      <IsWHTRegistered>0</IsWHTRegistered>
      <IsWHTCashBasis>0</IsWHTCashBasis>
      <IsReciprocal>0</IsReciprocal>
      <PK>{XmlConvert.ToString(TestCompany1.PK.ToGuid())}</PK>
    </Company>
    <Company>
      <Code>222</Code>
      <Name>Company 2</Name>
      <CountryCode>NZ</CountryCode>
      <CurrencyCode>NZD</CurrencyCode>
      <Address1>Addr 1</Address1>
      <Address2>Addr 2\2</Address2>
      <City>City2</City>
      <State>CHC</State>
      <PostCode>1000</PostCode>
      <Phone>(2) 567</Phone>
      <BusinessRegNo>2</BusinessRegNo>
      <BusinessRegNo2>2b</BusinessRegNo2>
      <CustomsRegistrationNo>2222</CustomsRegistrationNo>
      <WebAddress>https://www.cargowise.com</WebAddress>
      <Email />
      <IsActive>1</IsActive>
      <IsGSTRegistered>1</IsGSTRegistered>
      <IsGSTCashBasis>1</IsGSTCashBasis>
      <IsWHTRegistered>1</IsWHTRegistered>
      <IsWHTCashBasis>1</IsWHTCashBasis>
      <IsReciprocal>1</IsReciprocal>
      <PK>{XmlConvert.ToString(TestCompany2.PK.ToGuid())}</PK>
    </Company>
  </CompanyList>");
					result.AppendFormat("  <DatabaseBackupPath>{0}</DatabaseBackupPath>\r\n", Env.Registry.BackupDirectoryPath);
					result.AppendFormat("  <SystemHealthData>{0}</SystemHealthData>\r\n", "{test legacy key}");
					result.AppendLine("  <RegistrationData>{test reg key}</RegistrationData>");
					result.AppendLine("  <DocEngine>");
					result.AppendLine("    <ActivePrintersCount>3</ActivePrintersCount>");
					result.AppendLine("  </DocEngine>");
					result.AppendLine("  <SqlServerVersionDetails>");
					result.AppendLine("    <SqlServerCpuArchitecture>X64</SqlServerCpuArchitecture>");
					result.AppendFormat("    <SqlServerEdition>{0}</SqlServerEdition>\r\n", TestConnection.ServerEdition);
					result.AppendFormat("    <SqlServerVersion>{0}</SqlServerVersion>\r\n", TestConnection.ServerVersionNumber.SqlServerGeneration);
					result.AppendFormat("    <SqlServerFullVersionText>{0}</SqlServerFullVersionText>\r\n", SecurityElement.Escape(TestConnection.ServerFullVersionText));
					result.AppendLine("  </SqlServerVersionDetails>");
					result.AppendLine("  <AdditionalDatabaseSystemInfoList>");
					result.AppendLine("    <SystemInfo>Host Name:                 SYD-WSCW-1</SystemInfo>");
					result.AppendLine("    <SystemInfo>OS Name:                   Microsoft Windows 7 Enterprise</SystemInfo>");
					result.AppendLine("    <SystemInfo>OS Version:                6.1.7600 N/A Build 7600</SystemInfo>");
					result.AppendLine("    <SystemInfo>System Manufacturer:       Gigabyte Technology Co., Ltd.</SystemInfo>");
					result.AppendLine("    <SystemInfo>BIOS Version:              Award Software International, Inc. F5, 18/06/2008</SystemInfo>");
					result.AppendLine("    <SystemInfo>System Locale:             en-us;English (United States)</SystemInfo>");
					result.AppendLine("    <SystemInfo>Total Physical Memory:     8,190 MB</SystemInfo>");
					result.AppendLine("    <SystemInfo>Time Zone:                 (UTC+10:00) Canberra, Melbourne, Sydney</SystemInfo>");
					result.AppendLine("    <SystemInfo>Processor(s):              1 Processor(s) Installed.</SystemInfo>");
					result.AppendLine("    <SystemInfo>                           [01]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~3000 Mhz</SystemInfo>");
					result.AppendLine("    <SystemInfo>System Model:              EP45-DS3P</SystemInfo>");
					result.AppendLine("  </AdditionalDatabaseSystemInfoList>");
					result.AppendLine("  <LicenceUsage>SomeUsage</LicenceUsage>");
					result.AppendLine("  <OutboundEAdaptorUrl>https://antongorlin.com</OutboundEAdaptorUrl>");
					result.AppendFormat($"  <NextRunTimeUtcUPG>{NextRunTimeUtcUPG.ToString("o")}</NextRunTimeUtcUPG>\r\n");
					result.AppendFormat($"  <NextRunTimeUtcMUG>{NextRunTimeUtcMUG.ToString("o")}</NextRunTimeUtcMUG>\r\n");
					result.AppendLine("  <ScheduleStateUPG>&lt;HostedServiceSerializableSettings&gt;&lt;ConfigString&gt;TestScheduleStateUPG&lt;/ConfigString&gt;&lt;SecondaryProcessesMaxCount&gt;0&lt;/SecondaryProcessesMaxCount&gt;&lt;/HostedServiceSerializableSettings&gt;</ScheduleStateUPG>");
					result.AppendLine("  <ScheduleStateMUG>&lt;HostedServiceSerializableSettings&gt;&lt;ConfigString&gt;TestScheduleStateMUG&lt;/ConfigString&gt;&lt;SecondaryProcessesMaxCount&gt;0&lt;/SecondaryProcessesMaxCount&gt;&lt;/HostedServiceSerializableSettings&gt;</ScheduleStateMUG>");
					result.AppendLine("  <TokenAuthenticationEnabled>Y</TokenAuthenticationEnabled>");
					result.AppendLine($"  <FeatureControlRule>{FeatureControlRule}</FeatureControlRule>");
					result.AppendLine("  <SupportedFeatureCodes>");
					result.AppendLine("    <FeatureCodePair>");
					result.AppendLine("      <FeatureCode>CR5RESWIZ</FeatureCode>");
					result.AppendLine("      <FeatureDescription>CR5 Resolution Wizard</FeatureDescription>");
					result.AppendLine("      <FeatureStage>Active</FeatureStage>");
					result.AppendLine("    </FeatureCodePair>");
					result.AppendLine("    <FeatureCodePair>");
					result.AppendLine("      <FeatureCode>ACCRBKFTR</FeatureCode>");
					result.AppendLine("      <FeatureDescription>Accounting Reporting Book Feature</FeatureDescription>");
					result.AppendLine("      <FeatureStage>Development</FeatureStage>");
					result.AppendLine("    </FeatureCodePair>");
					result.AppendLine("    <FeatureCodePair>");
					result.AppendLine("      <FeatureCode>ACCGLDFTR</FeatureCode>");
					result.AppendLine("      <FeatureDescription>Accounting General Ledger Data Feature Control</FeatureDescription>");
					result.AppendLine("      <FeatureStage>Development</FeatureStage>");
					result.AppendLine("    </FeatureCodePair>");
					result.AppendLine("    <FeatureCodePair>");
					result.AppendLine("      <FeatureCode>CWNext</FeatureCode>");
					result.AppendLine("      <FeatureDescription>CargoWise Next</FeatureDescription>");
					result.AppendLine("      <FeatureStage>Active</FeatureStage>");
					result.AppendLine("    </FeatureCodePair>");
					result.AppendLine("  </SupportedFeatureCodes>");
					result.Append("</VersionReport>");

					xmlDataForTestingNewFormat = result.ToString();
				}

				return xmlDataForTestingNewFormat;
			}
		}

		void AppendDatabaseFilesToTestVersionReportXml(StringBuilder versionReportBuilder)
		{
			foreach (string dbName in TestConnection.GetDatabases(DatabaseType.Operational))
			{
				foreach (string dbFilePath in TestConnection.GetDbFiles(dbName))
				{
					versionReportBuilder.AppendFormat("    <DBFileName>{0}</DBFileName>\r\n", dbFilePath);
				}
			}
		}

		string xmlDataForTestingNewFormat;

		#endregion
	}
}
