using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Test
{
	[TestedType(typeof(CusStorageDocPivot))]
	sealed class CusStorageDocPivotTest : Customs.Business.Testing.BaseCusStorageDocPivotTest
	{
		public void TestCSD_DocType()
		{
			AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(typeof(CusStorageDocPivot), nameof(CusStorageDocPivot.CSD_DocType)).Caption);
		}

		public void TestCSD_StorageDocReference()
		{
			AssertEquals("eDoc", DataBoundResourceStrings.GetDataForProperty(typeof(CusStorageDocPivot), nameof(CusStorageDocPivot.CSD_StorageDocReference)).Caption);
		}

		public void TestUniqueIdentifier()
		{
			var docPivot = oceanBill.EDocPivotCollection.AddNew();
			var eDoc1 = oceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "CIV");
			docPivot.CSD_StorageDocReference = eDoc1.UniqueKey;

			AssertEquals(eDoc1.UniqueKey, ((ITSWAttachment)docPivot).UniqueIdentifier);
		}

		public void TestFileName()
		{
			var docPivot = oceanBill.EDocPivotCollection.AddNew();
			var eDoc1 = oceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "CIV");
			docPivot.CSD_StorageDocReference = eDoc1.UniqueKey;

			AssertEquals("CIV-Test1.pdf", ((ITSWAttachment)docPivot).FileName);
		}

		public void TestDocType()
		{
			var docPivot = oceanBill.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "OTH";

			AssertEquals("OTH", ((ITSWAttachment)docPivot).DocType);
		}

		public void TestFileTooBig()
		{
			using (NZCustomsDataRegistry.Instance.MaxMessageAttachmentSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var docPivot1 = oceanBill.EDocPivotCollection.AddNew();
				var eDoc1 = oceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "CIV");
				docPivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
				AssertEquals(false, ((ITSWAttachment)docPivot1).FileTooBig);

				var docPivot2 = oceanBill.EDocPivotCollection.AddNew();
				var eDoc2 = oceanBill.DocManagerInfo.AddFileOrDocument(new byte[11], "Test2.pdf", "CIV");
				docPivot2.CSD_StorageDocReference = eDoc2.UniqueKey;
				AssertEquals(true, ((ITSWAttachment)docPivot2).FileTooBig);
			}
		}

		public override void TestParent()
		{
			var docPivot = Factory.New<CusStorageDocPivot>();
			var scaOceanBill = Factory.New<CusSCAOceanBill>();
			docPivot.Parent = scaOceanBill;
			AssertType<CusSCAOceanBill>(docPivot.Parent);
			AssertEquals(scaOceanBill, docPivot.Parent);

			var scaHouseBill = scaOceanBill.HouseBills.AddNew();
			docPivot.Parent = scaHouseBill;
			AssertType<CusSCAHouse>(docPivot.Parent);
			AssertEquals(scaHouseBill, docPivot.Parent);

			var cusMAWB = Factory.New<CusMAWB>();
			docPivot.Parent = cusMAWB;
			AssertType<CusMAWB>(docPivot.Parent);
			AssertEquals(cusMAWB, docPivot.Parent);

			var childBill = cusMAWB.ChildBills.AddNew();
			docPivot.Parent = childBill;
			AssertType<CusHAWB>(docPivot.Parent);
			AssertEquals(childBill, docPivot.Parent);
		}

		public void TestValidation() => CombineAssertions(() =>
		{
			var oceanBillDoc = oceanBill.EDocPivotCollection.AddNew();
			AssertType<CusStorageDocPivotSCAOceanBillValidation>("OceanBill document", oceanBillDoc.Validation);

			var houseBill = oceanBill.HouseBills.AddNew();
			var houseBillDoc = houseBill.EDocPivotCollection.AddNew();
			AssertType<CusStorageDocPivotSCAHouseValidation>("HouseBill document", houseBillDoc.Validation);

			var mawb = Factory.New<CusMAWB>();
			var mawbDoc = mawb.EDocPivotCollection.AddNew();
			AssertType<CusStorageDocPivotMAWBValidation>("MAWB document", mawbDoc.Validation);

			var hawb = mawb.ChildBills.AddNew();
			var hawbDoc = hawb.EDocPivotCollection.AddNew();
			AssertType<CusStorageDocPivotHAWBValidation>("HAWB document", hawbDoc.Validation);
		});

		protected override void SetUp()
		{
			base.SetUp();

			oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
		}
		CusSCAOceanBill oceanBill;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var docPivot = oceanBill.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = AttachmentTypeList.Codes.OTH;
			return docPivot;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var docPivot = oceanBill.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = AttachmentTypeList.Codes.OTH;
			return docPivot;
		}
	}
}
