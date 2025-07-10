using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryHeaderENS7501Bill))]
	sealed class EntryHeaderENS7501BillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEntrySummary7501BillMembers()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var declaration = GetDeclaration();
			var houseBill = declaration.PrimaryMasterBill.ChildBills.AddNew();
			houseBill.CU_BillNum = "HAWB1643453";
			houseBill.US_UI_NKBillIssuerSCAC = "SCA2";

			var subHousebill = houseBill.ChildBills.AddNew();
			subHousebill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHousebill.CU_BillNum = "SubBill12342";
			subHousebill.US_UI_NKBillIssuerSCAC = "SCA3";
			subHousebill.CU_PackType = ABIUnitOfMeasureList.Codes.DozenPairs;
			var itNo = subHousebill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "ITN1231";
			subHousebill.CU_NoOfPacks = 65;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryHeaderENS7501Bill = new EntryHeaderENS7501Bill(entry.PK, subHousebill.ITAndSplitDetails[0]);

			AssertEquals("InBondNumber", "ITN1231", entryHeaderENS7501Bill.ITNO);
			AssertEquals("InBondDate", new ZDateTime(2008, 3, 1), entryHeaderENS7501Bill.ITDate);
			AssertEquals("MasterBillNumber", "MAWB123212", entryHeaderENS7501Bill.MasterBill);
			AssertEquals("IssuerCodeOfMasterBillNumber", "SCA1", entryHeaderENS7501Bill.EffectiveMasterBillIssuerSCAC);
			AssertEquals("HouseBillNumber", "HAWB1643453", entryHeaderENS7501Bill.HouseBill);
			AssertEquals("IssuerCodeOfHouseBillNumber", "SCA2", entryHeaderENS7501Bill.EffectiveHouseBillIssuerSCAC);
			AssertEquals("SubHouseBillNumber", "SubBill12342", entryHeaderENS7501Bill.SubHouseBill);
			AssertEquals("IssuerCodeOfSubHouseBillNumber", "SCA3", entryHeaderENS7501Bill.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("PackageQuantity", 65, entryHeaderENS7501Bill.PkgQty);
			AssertEquals("PackageType", ABIUnitOfMeasureList.Codes.DozenPairs, entryHeaderENS7501Bill.PkgType);

			IParentDocManagerSupport supporter = entryHeaderENS7501Bill;
			AssertEquals(Core.Constants.DocManagerCodes.CustomsEntry, supporter.DocManagerInfo.DocManagerCode);
			AssertEquals("ParentGuid", entry.PK, supporter.ParentGuid);
			AssertEquals("ParentTableName", entry.TableName, supporter.ParentTableName);

			IDocumentDeliveredLogSupporter logSupporter = entryHeaderENS7501Bill;
			AssertEquals("BusinessObjectTypeToLogAgainst", typeof(CusEntryHeader), logSupporter.BusinessObjectTypeToLogAgainst);
			AssertEquals("Identifier", entry.PK, logSupporter.Identifier);
		}

		protected override BusinessObject GetNewBusinessObject() => new EntryHeaderENS7501Bill(Factory.New<CusEntryHeader>().PK, GetDeclaration().PrimaryMasterBill.ITAndSplitDetails.AddNew());

		JobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_ITDate = new ZDateTime(2008, 3, 1);

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillNum = "MAWB123212";
			masterBill.US_UI_NKBillIssuerSCAC = "SCA1";
			return declaration;
		}
	}
}
