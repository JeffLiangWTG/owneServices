using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USCPSCLabReportAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLabReportInformationTypeList()
		{
			var labReportInformationTypeList = lookups.LabReportInformationTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "CP1, CP2, CP3", labReportInformationTypeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<LabReportInformationTypeList>(), labReportInformationTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var report = Factory.New<CPSCReport>();
			var addInfo = new CPSCReportAddInfo(report.B7_AddInfoDataInfo);
			lookups = new USCPSCLabReportAddInfoLookups(addInfo);
		}
		USCPSCLabReportAddInfoLookups lookups;
	}
}
