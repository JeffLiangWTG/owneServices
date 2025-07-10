using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Test
{
	sealed class CusStorageDocPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAttachmentTypes()
		{
			var scaOceanBill = Factory.New<CusSCAOceanBill>();
			scaOceanBill.CB_RL_NKPortOfLoading = "ZAJNB";
			scaOceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			var docPivot = scaOceanBill.EDocPivotCollection.AddNew();
			AssertEquals("Import attachment types", Factory.GetCachedValue<AttachmentTypeList>(), docPivot.Lookups.AttachmentTypes);

			scaOceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			scaOceanBill.CB_RL_NKPortOfDischarge = "ZAJNB";
			AssertEquals("Export attachment types", Factory.GetCachedValue<AttachmentTypeListPerMsg.AttachmentTypeListForCRE>(), docPivot.Lookups.AttachmentTypes);

			scaOceanBill.CB_RL_NKPortOfLoading = "ZAJNB";
			scaOceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			var scaHouseBill = scaOceanBill.HouseBills.AddNew();
			docPivot = scaHouseBill.EDocPivotCollection.AddNew();
			AssertEquals("Import attachment types", Factory.GetCachedValue<AttachmentTypeList>(), docPivot.Lookups.AttachmentTypes);

			scaOceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			scaOceanBill.CB_RL_NKPortOfDischarge = "ZAJNB";
			AssertEquals("Export attachment types", Factory.GetCachedValue<AttachmentTypeListPerMsg.AttachmentTypeListForCRE>(), docPivot.Lookups.AttachmentTypes);

			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_RL_NKLoadPort = "ZAJNB";
			cusMAWB.CM_RL_NKDischargePort = "NZAKL";
			docPivot = cusMAWB.EDocPivotCollection.AddNew();
			AssertEquals("Import attachment types", Factory.GetCachedValue<AttachmentTypeList>(), docPivot.Lookups.AttachmentTypes);

			cusMAWB.CM_RL_NKLoadPort = "NZAKL";
			cusMAWB.CM_RL_NKDischargePort = "ZAJNB";
			AssertEquals("Export attachment types", Factory.GetCachedValue<AttachmentTypeListPerMsg.AttachmentTypeListForCRE>(), docPivot.Lookups.AttachmentTypes);

			cusMAWB.CM_RL_NKLoadPort = "ZAJNB";
			cusMAWB.CM_RL_NKDischargePort = "NZAKL";
			var childBill = cusMAWB.ChildBills.AddNew();
			docPivot = childBill.EDocPivotCollection.AddNew();
			AssertEquals("Import attachment types", Factory.GetCachedValue<AttachmentTypeList>(), docPivot.Lookups.AttachmentTypes);

			cusMAWB.CM_RL_NKLoadPort = "NZAKL";
			cusMAWB.CM_RL_NKDischargePort = "ZAJNB";
			AssertEquals("Export attachment types", Factory.GetCachedValue<AttachmentTypeListPerMsg.AttachmentTypeListForCRE>(), docPivot.Lookups.AttachmentTypes);
		}

		public void TestAvailableEDocs()
		{
			var scaOceanBill = Factory.New<CusSCAOceanBill>();
			var scaHouseBill = scaOceanBill.HouseBills.AddNew();
			var eDoc1 = scaOceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "CIV");
			var eDoc2 = scaOceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "Test2.pdf", "CIV");

			var scaOceanBillDocPivot = scaOceanBill.EDocPivotCollection.AddNew();
			var scaHouseBillDocPivot = scaHouseBill.EDocPivotCollection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Available EDocs count for SCA ocean bill", 2, scaOceanBillDocPivot.Lookups.AvailableEDocs.Count);
				AssertEquals("Available EDocs count for SCA house bill", 2, scaHouseBillDocPivot.Lookups.AvailableEDocs.Count);

				AssertEquals("First edoc for SCA ocean bill", eDoc1.UniqueKey, scaOceanBillDocPivot.Lookups.AvailableEDocs[0].PK);
				AssertEquals("First edoc for SCA house bill", eDoc1.UniqueKey, scaHouseBillDocPivot.Lookups.AvailableEDocs[0].PK);
				AssertEquals("Second edoc for SCA ocean bill", eDoc2.UniqueKey, scaOceanBillDocPivot.Lookups.AvailableEDocs[1].PK);
				AssertEquals("Second edoc for SCA house bill", eDoc2.UniqueKey, scaHouseBillDocPivot.Lookups.AvailableEDocs[1].PK);
			});
		}
	}
}
