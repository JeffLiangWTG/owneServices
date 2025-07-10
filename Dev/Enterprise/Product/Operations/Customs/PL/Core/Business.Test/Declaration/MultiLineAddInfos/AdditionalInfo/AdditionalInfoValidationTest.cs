using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Status()
	{
		var declarataion = Factory.New<JobDeclaration>();
		var additionalInfo = declarataion.AdditionalInfos.AddNew();

		CombineAssertions(() =>
		{
			declarataion.JE_MessageType = MessageTypeList.Codes.Export;
			additionalInfo.Validation.ValidateCSI_Status();
			AssertNoNotifications("Export", additionalInfo.CSI_StatusInfo);

			declarataion.JE_MessageType = MessageTypeList.Codes.Import;
			additionalInfo.Validation.ValidateCSI_Status();
			AssertNoNotifications("Import", additionalInfo.CSI_StatusInfo);
		});
	}
}
