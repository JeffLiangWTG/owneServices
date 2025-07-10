using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestParentMasterBillNumberIsNotImportedIntoCU_AddInfo()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var additionalBillDataObject = SetupAdditionalBill("HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 4, 3), "MB1", SetupAddInfos(string.Format("{0}*{1}=MB32423*", USBillAddInfoString, Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber)), 11, Core.Constants.PkgUnit.Box, "Box");
				var reader = new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, CurrentCompanyHelper, new AdditionalBillDataProvider<Bill>(declaration), new BillDetail() { BillNumber = "MB1" }, new BillDetail() { BillNumber = "HB1" });
				var billBO = reader.ReadIntoBusinessObject();
				AssertContents(billBO, "HB1", BillTypeList.Codes.HouseBill, ZGuid.Empty, new ZDateTime(2011, 4, 3), 11, Core.Constants.PkgUnit.Box, USBillAddInfoString);
			}
		}
	}
}
