using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class UserAccountReportTest : TestCaseWithFactory
	{
		public void TestUserAccountReportToXMLString()
		{
			SystemDataRegistry.Instance.IsFirstTimeSendingStaffReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			PrepareTestData();
			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			regKey.DatabaseNumberForTest = 1234;
			var lastRunTime = SystemDataRegistry.Instance.UserAccountLastReportTime.Value;
			var report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			var xml = report.XML;
			var expectedXml = ExpectedXml(report.ReportTimeUtc);
			AssertEquals(expectedXml, xml.ToString());
		}

		public void TestUserAccountReportToXMLStringWhenStaffCountFeatureIsEnabled()
		{
			SystemDataRegistry.Instance.IsFirstTimeSendingStaffReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UARCounts, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
			PrepareTestData();
			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			regKey.DatabaseNumberForTest = 1234;
			var lastRunTime = SystemDataRegistry.Instance.UserAccountLastReportTime.Value;
			var report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			var xml = report.XML;
			var expectedXml = ExpectedXml(report.ReportTimeUtc, true, true);
			AssertEquals(expectedXml, xml.ToString());
		}

		public void TestUserAccountReportHasCorrectStaffCount()
		{
			PrepareTestData();
			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			regKey.DatabaseNumberForTest = 1234;
			var lastRunTime = SystemDataRegistry.Instance.UserAccountLastReportTime.Value;
			var report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			AssertEquals("2", report.ActiveStaffCount);

			var staff1 = Factory.Load<GlbStaff>(staff1PK);
			staff1.GS_IsActive = false;
			Factory.Save();

			report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			AssertEquals("1", report.ActiveStaffCount);

			var staff2 = Factory.Load<GlbStaff>(staff2PK);
			staff2.GS_IsRobot = true;
			Factory.Save();

			report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			AssertEquals("0", report.ActiveStaffCount);
		}

		public void TestUserAccountReportRetrievesCorrectStaffCountWhenOutputtingDelta()
		{
			PrepareTestData();
			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			regKey.DatabaseNumberForTest = 1234;
			var lastRunTime = SystemDataRegistry.Instance.UserAccountLastReportTime.Value;
			var report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			// Full report
			AssertEquals("2", report.ActiveStaffCount);
			AssertEquals(2, report.StaffList.Count);
			lastRunTime = ZDateTime.UtcNow.ToDateTime();
			var staff1 = Factory.Load<GlbStaff>(staff1PK);
			staff1.GS_FullName = "changedStaff";
			Factory.Save();
			report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			AssertEquals("2", report.ActiveStaffCount);
			AssertEquals(1, report.StaffList.Count);
		}

		public void TestUserAccountReportAlwaysRetrievesStaffRecordsFromDatabase()
		{
			PrepareTestData();
			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			regKey.DatabaseNumberForTest = 1234;
			var lastRunTime = SystemDataRegistry.Instance.UserAccountLastReportTime.Value;
			var report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			lastRunTime = ZDateTime.UtcNow.ToDateTime();
			AssertEquals(2, report.StaffList.Count);

			// update record in the database to simulate it being updated by CW1 i.e. in a different process from the service task that UserAccountReport is running in
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var staff1 = factory.Load<GlbStaff>(staff1PK);
			staff1.GS_IsActive = false;
			factory.Save();

			report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			AssertEquals(1, report.StaffList.Count);
		}

		public void TestUserAccountReport_IsFirstTimeSendingStaffReport()
		{
			PrepareTestData();

			var testStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			testStaff3.GS_FullName = "staff3";
			testStaff3.GS_Code = "GS3";
			testStaff3.GS_EmailAddress = "staff3@test.com";
			testStaff3.GS_WorkPhone = "333333";
			testStaff3.GS_WorkExtension = "www";
			testStaff3.GS_Title = "wise";
			testStaff3.GS_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-5);
			testStaff3.GS_SystemLastEditTimeUtc = DateTime.UtcNow.AddDays(-5);
			Factory.Save();

			using (TestConnection.SuspendAuditTriggers())
			{
				TestConnection.ExecuteNonQuery("UPDATE GlbStaff SET GS_SystemLastEditTimeUtc = DATEADD(day, -5, GetUtcDate()), GS_SystemLastEditUser = 'E' WHERE GS_Code = 'GS3'");
			}

			SystemDataRegistry.Instance.IsFirstTimeSendingStaffReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var regKey1 = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			regKey1.DatabaseNumberForTest = 1234;
			var report1 = new UserAccountReport.UserAccountReport(regKey1, DateTime.UtcNow.AddDays(-1));
			var expectedXml1 = ExpectedXml(report1.ReportTimeUtc);
			AssertEquals("Should not contain old staff", expectedXml1, report1.XML.ToString());

			SystemDataRegistry.Instance.IsFirstTimeSendingStaffReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var report2 = new UserAccountReport.UserAccountReport(regKey1, DateTime.UtcNow);
			var expectedStaffs = @"<StaffList>
    <IsFullStaffList>1</IsFullStaffList>
    <Staff>
      <IsActive>1</IsActive>
      <IsRobot>0</IsRobot>
      <Code>GS1</Code>
      <Name>staff1</Name>
      <Email>staff1@test.com</Email>
      <Language>EN</Language>
      <Branch>DEM</Branch>
      <WorkPhone>111111</WorkPhone>
      <Extension>xxx</Extension>
      <JobTitle>boss</JobTitle>
    </Staff>
    <Staff>
      <IsActive>1</IsActive>
      <IsRobot>0</IsRobot>
      <Code>GS2</Code>
      <Name>staff2</Name>
      <Email>staff2@test.com</Email>
      <Language>EN</Language>
      <Branch>DEM</Branch>
      <WorkPhone>222222</WorkPhone>
      <Extension>yyy</Extension>
      <JobTitle>master</JobTitle>
    </Staff>
    <Staff>
      <IsActive>1</IsActive>
      <IsRobot>0</IsRobot>
      <Code>GS3</Code>
      <Name>staff3</Name>
      <Email>staff3@test.com</Email>
      <Language>EN</Language>
      <Branch>DEM</Branch>
      <WorkPhone>333333</WorkPhone>
      <Extension>www</Extension>
      <JobTitle>wise</JobTitle>
    </Staff>
  </StaffList>";
			AssertEquals("Should contain all staffs", true, report2.XML.Contains(expectedStaffs));
		}

		public void TestUserAccountReport_BranchCount()
		{
			PrepareTestData();

			var regKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			regKey.DatabaseNumberForTest = 1234;
			var lastRunTime = SystemDataRegistry.Instance.UserAccountLastReportTime.Value;

			var expectedBranches = GetBranchCountWithoutDemoCompany();

			var report = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			AssertEquals(expectedBranches.ToString(), report.TotalNonDemoBranchCount);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_PostCode = "003078";
			branch1.GB_Address1 = "Address 11";
			branch1.GB_Address2 = "Address 22";
			branch1.GB_City = "City 11";
			branch1.GB_State = "State 11";
			Factory.Save();

			var report2 = new UserAccountReport.UserAccountReport(regKey, lastRunTime);
			AssertEquals("Should ignore demo branches", (expectedBranches + 1).ToString(), report2.TotalNonDemoBranchCount);
		}

		int GetBranchCountWithoutDemoCompany()
		{
			return Factory.GetDatabaseCount(typeof(GlbBranch), new ZQuery(GlbBranchSchema.GB_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode));
		}

		string ExpectedXml(ZDateTime reportTimeUtc, bool hasStaffCount = false, bool hasBranchCount = false)
		{
			return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UserAccountReport>
  <DatabaseNumber>1234</DatabaseNumber>
  <ReportTimeUtc>{reportTimeUtc.ToString("yyyyMMdd_HHmm")}</ReportTimeUtc>{(hasStaffCount ? "\r\n  <ActiveStaffCount>2</ActiveStaffCount>" : "")}{(hasBranchCount ? "\r\n  <TotalNonDemoBranchCount>" + GetBranchCountWithoutDemoCompany() + "</TotalNonDemoBranchCount>" : "")}
  <BranchList>
    <Branch>
      <IsActive>1</IsActive>
      <PK>{XmlConvert.ToString(TestBranch1.PK.ToGuid())}</PK>
      <Code>7VQ</Code>
      <BranchName>Test Branch</BranchName>
      <CompanyName>Eagle Datamation International</CompanyName>
      <CompanyCode>EDI</CompanyCode>
      <Address1>Address 1</Address1>
      <Address2>Address 2</Address2>
      <City>City 1</City>
      <State>State 1</State>
      <PostCode>002068</PostCode>
      <CountryCode>AU</CountryCode>
      <Unloco />
      <ValidationStatus>NYV</ValidationStatus>
    </Branch>
  </BranchList>
  <StaffList>
    <IsFullStaffList>0</IsFullStaffList>
    <Staff>
      <IsActive>1</IsActive>
      <IsRobot>0</IsRobot>
      <Code>GS1</Code>
      <Name>staff1</Name>
      <Email>staff1@test.com</Email>
      <Language>EN</Language>
      <Branch>DEM</Branch>
      <WorkPhone>111111</WorkPhone>
      <Extension>xxx</Extension>
      <JobTitle>boss</JobTitle>
    </Staff>
    <Staff>
      <IsActive>1</IsActive>
      <IsRobot>0</IsRobot>
      <Code>GS2</Code>
      <Name>staff2</Name>
      <Email>staff2@test.com</Email>
      <Language>EN</Language>
      <Branch>DEM</Branch>
      <WorkPhone>222222</WorkPhone>
      <Extension>yyy</Extension>
      <JobTitle>master</JobTitle>
    </Staff>
  </StaffList>
</UserAccountReport>";//, xml.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			mockFeatureManager = new Mock<IFeatureControlManager>();
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UARCounts, CancellationToken.None)).Returns(Task.FromResult(null as IFeatureData));
			ObjectFactory.Substitute(mockFeatureManager.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ObjectFactory.DisposeSubstitutions();
		}

		void PrepareTestData(bool clearBranches = false)
		{
			TestBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			TestBranch1.GB_GC = GlbCompany.CurrentCompany.PK;
			TestBranch1.GB_PostCode = "002068";
			TestBranch1.GB_Address1 = "Address 1";
			TestBranch1.GB_Address2 = "Address 2";
			TestBranch1.GB_City = "City 1";
			TestBranch1.GB_State = "State 1";

			TestStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			TestStaff1.GS_FullName = "staff1";
			TestStaff1.GS_Code = "GS1";
			TestStaff1.GS_EmailAddress = "staff1@test.com";
			TestStaff1.GS_WorkPhone = "111111";
			TestStaff1.GS_WorkExtension = "xxx";
			TestStaff1.GS_Title = "boss";
			staff1PK = TestStaff1.PK;

			TestStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			TestStaff2.GS_FullName = "staff2";
			TestStaff2.GS_Code = "GS2";
			TestStaff2.GS_EmailAddress = "staff2@test.com";
			TestStaff2.GS_WorkPhone = "222222";
			TestStaff2.GS_WorkExtension = "yyy";
			TestStaff2.GS_Title = "master";
			staff2PK = TestStaff2.PK;
			Factory.Save();
		}

		GlbBranch TestBranch1;
		GlbStaff TestStaff1;
		GlbStaff TestStaff2;
		ZGuid staff1PK;
		ZGuid staff2PK;
		Mock<IFeatureControlManager> mockFeatureManager;
	}
}
