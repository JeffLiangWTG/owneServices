using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryMessageENS7501Bill))]
	sealed class EntryMessageENS7501BillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEntrySummary7501Bills()
		{
			var entry = Factory.New<CusEntryHeader>();
			var billDetail = new BillDetails() { ITNo = "123457789", MasterBillNumber = "198121115", IssuerCodeOfMasterBillNumber = "APLU", HouseBillNumber = "387000796006", IssuerCodeOfHouseBillNumber = "HLMU", Quantity = 500, Unit = "PAL" };
			var bill = new EntryMessageENS7501Bill(entry.PK, entry.Factory, ZDate.Empty, billDetail);

			IParentDocManagerSupport supporter = bill;
			AssertEquals(Core.Constants.DocManagerCodes.CustomsEntry, supporter.DocManagerInfo.DocManagerCode);
			AssertEquals("ParentGuid", entry.PK, supporter.ParentGuid);
			AssertEquals("ParentTableName", entry.TableName, supporter.ParentTableName);

			IDocumentDeliveredLogSupporter logSupporter = bill;
			AssertEquals("BusinessObjectTypeToLogAgainst", typeof(CusEntryHeader), logSupporter.BusinessObjectTypeToLogAgainst);
			AssertEquals("Identifier", entry.PK, logSupporter.Identifier);

			AssertEquals("ITDate", ZDateTime.Empty, bill.ITDate);
			AssertEquals("ITNO", "123457789", bill.ITNO);
			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", bill.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "198121115", bill.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "HLMU", bill.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "387000796006", bill.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", bill.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "", bill.SubHouseBill);
			AssertEquals("PkgQty", 500, bill.PkgQty);
			AssertEquals("PkgType", "PAL", bill.PkgType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<CusEntryHeader>();
			var billDetail = new BillDetails() { ITNo = "V123457789", MasterBillNumber = "Master1", IssuerCodeOfMasterBillNumber = "APLU", HouseBillNumber = "House1", IssuerCodeOfHouseBillNumber = "HLMU", SubHouseBillNumber = "SubHouse1", Quantity = 1, Unit = "PK" };
			return new EntryMessageENS7501Bill(entry.PK, Factory, ZDate.Empty, billDetail);
		}
	}
}
