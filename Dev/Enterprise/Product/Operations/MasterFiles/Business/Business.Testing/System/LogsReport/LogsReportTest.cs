using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.LogsReport.Testing
{
	sealed class LogsReportTest : TestCaseWithFactory
	{
		public void TestToXMLStreamAndBack()
		{
			LogsReport report = new LogsReport("CS01234567", "ABC", new byte[] { 0x01, 0x23, 0x45 });
			AssertEquals("CS01234567", report.IncidentNumber);
			AssertEquals("ABC", report.ServiceTaskCode);
			AssertEquals((byte)0x01, report.LogFilesZip[0]);
			AssertEquals((byte)0x23, report.LogFilesZip[1]);
			AssertEquals((byte)0x45, report.LogFilesZip[2]);
			AssertEquals(3, report.LogFilesZip.Length);
			ZString xml = report.XML;
			LogsReport reportFromXml = new LogsReport(xml);
			AssertEquals(report.IncidentNumber, reportFromXml.IncidentNumber);
			AssertEquals(report.ServiceTaskCode, reportFromXml.ServiceTaskCode);
			AssertEquals(report.LogFilesZip[0], reportFromXml.LogFilesZip[0]);
			AssertEquals(report.LogFilesZip[1], reportFromXml.LogFilesZip[1]);
			AssertEquals(report.LogFilesZip[2], reportFromXml.LogFilesZip[2]);
			AssertEquals(report.LogFilesZip.Length, reportFromXml.LogFilesZip.Length);
		}
	}
}
