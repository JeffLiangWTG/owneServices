using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestMasterWayBillNumberIsNotImportedIntoJE_AddInfo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declarationDataObject = SetupDeclaration(null, "HB1", new WayBillType() { Code = WayBillTypeList.Codes.House }, AddInfoCollectionCreator.CreateCollection(Constants.AddInfoKeys.Declaration.MasterWayBillNumber + "=MB2*ECCN=DD"));
				var masterBill1DataObject = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master }, NoOfPacks = 10m };
				var masterBill2DataObject = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master }, NoOfPacks = 15m };
				var masterBill3DataObject = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB3", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master }, NoOfPacks = 5m };
				var masterBill2HouseBill1DataObject = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, NoOfPacks = 5m, ParentBillNumber = "MB3" };
				var masterBill1HouseBill1DataObject = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, NoOfPacks = 15m, ParentBillNumber = "MB2" };
				var masterBill3HouseBill1DataObject = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, NoOfPacks = 10m, ParentBillNumber = "MB1" };
				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBill3DataObject, masterBill3HouseBill1DataObject, masterBill1DataObject, masterBill1HouseBill1DataObject, masterBill2DataObject, masterBill2HouseBill1DataObject }));

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("MB2", declarationBO.JE_MasterBill);
				AssertEquals("HB1", declarationBO.JE_HouseBill);
				declarationBO.Bills.Load();
				AssertEquals(6, declarationBO.Bills.Count);
				var masterBill = declarationBO.PrimaryMasterBill;
				AssertEquals(15m, masterBill.CU_NoOfPacks);
				var hosueBill = declarationBO.PrimaryMasterBill;
				AssertEquals(15m, hosueBill.CU_NoOfPacks);
				AssertEquals("ECCN=DD", declarationBO.JE_AddInfo);
			}
		}
	}
}
