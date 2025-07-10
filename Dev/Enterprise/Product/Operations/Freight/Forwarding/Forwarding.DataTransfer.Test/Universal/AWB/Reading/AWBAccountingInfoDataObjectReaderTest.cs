using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class AWBAccountingInfoDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestReadAccountingInfo()
		{
			var accountingInfoDO = new AWBAccountingInfo();

			accountingInfoDO.Information = "Blah";
			accountingInfoDO.Type = new CodeDescriptionPair { Code = "BLA" };

			var reader = new AWBAccountingInfoDataObjectReader(accountingInfoDO, Logger, Factory);
			var accountingInfoBO = reader.ReadIntoBusinessObject();

			AssertEquals("Blah", accountingInfoBO.EA_Information);
			AssertEquals("BLA", accountingInfoBO.EA_InformationID);
		}
	}
}
