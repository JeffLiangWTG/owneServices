using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class AWBAccountingInfoDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteAccountingInfo()
		{
			var accountingInfo = Factory.New<ExportAWBAccountingInformation>();
			accountingInfo.EA_Information = "Some Information";
			accountingInfo.EA_InformationID = Core.Constants.AWB.AccountingCodes.MCO;

			var writer = new AWBAccountingInfoDataObjectWriter(new DataWritingManager(new ActionInfo(null, accountingInfo)));
			var accountingInfoDataObject = writer.GetDataObject(accountingInfo);

			AssertEquals("Some Information", accountingInfoDataObject.Information);
			AssertEquals(Core.Constants.AWB.AccountingCodes.MCO, accountingInfoDataObject.Type.Code);
			AssertEquals("Miscellaneous Charges Order", accountingInfoDataObject.Type.Description);
		}
	}
}
