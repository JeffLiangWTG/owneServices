using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Status()
	{
		var reportItem = Factory.New<CusExitReportItem>();
		var additionalInfo = reportItem.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		additionalInfo.CSI_Status = string.Empty;
		AssertNoNotifications("When CSI_SubType is INF", additionalInfo.CSI_StatusInfo);

		additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
		additionalInfo.CSI_Status = string.Empty;
		AssertHasNotifications("When CSI_SubType is TRA", additionalInfo.CSI_StatusInfo);
	}
}
