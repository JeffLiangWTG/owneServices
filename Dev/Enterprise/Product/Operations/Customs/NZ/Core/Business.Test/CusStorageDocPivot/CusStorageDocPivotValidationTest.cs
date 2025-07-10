using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Test
{
	sealed class CusStorageDocPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSD_DocType()
		{
			var scaOceanBill = Factory.New<CusSCAOceanBill>();
			scaOceanBill.CB_RL_NKPortOfLoading = "ZAJNB";
			scaOceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			var docPivot = scaOceanBill.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "XXX";
			docPivot.Validation.ValidateCSD_DocType();
			AssertHasErrorContaining(docPivot.CSD_DocTypeInfo, ListValidation.InvalidCodeError);

			docPivot.CSD_DocType = AttachmentTypeList.Codes.OTH;
			docPivot.Validation.ValidateCSD_DocType();
			AssertNoErrorContaining(docPivot.CSD_DocTypeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCSD_StorageDocReference()
		{
			using (NZCustomsDataRegistry.Instance.MaxMessageAttachmentSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var scaOceanBill = Factory.New<CusSCAOceanBill>();
				var eDoc1 = scaOceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "CIV");
				var eDoc2 = scaOceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "你好.pdf", "CIV");
				var eDoc3 = scaOceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "再见.pdf", "CIV");
				var eDoc4 = scaOceanBill.DocManagerInfo.AddFileOrDocument(new byte[11], "Test2.pdf", "CIV");
				var docPivot1 = scaOceanBill.EDocPivotCollection.AddNew();
				docPivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
				AssertNoErrors(docPivot1.CSD_StorageDocReferenceInfo);

				var docPivot2 = scaOceanBill.EDocPivotCollection.AddNew();
				docPivot2.CSD_StorageDocReference = eDoc2.UniqueKey;
				var docPivot3 = scaOceanBill.EDocPivotCollection.AddNew();
				docPivot3.CSD_StorageDocReference = eDoc3.UniqueKey;
				docPivot2.Validation.ValidateCSD_StorageDocReference();
				docPivot3.Validation.ValidateCSD_StorageDocReference();
				AssertHasError(docPivot2.CSD_StorageDocReferenceInfo, "Filename contains Invalid character(s).");
				AssertHasError(docPivot3.CSD_StorageDocReferenceInfo, "Filename contains Invalid character(s).");

				var docPivot4 = scaOceanBill.EDocPivotCollection.AddNew();
				docPivot4.CSD_StorageDocReference = eDoc4.UniqueKey;
				AssertHasErrorContaining(docPivot4.CSD_StorageDocReferenceInfo, "The supporting document selected exceeds the NZ Customs system limitation file size maximum");

				var docPivot5 = scaOceanBill.EDocPivotCollection.AddNew();
				docPivot5.CSD_StorageDocReference = ZGuid.Invalid;
				AssertHasErrorContaining(docPivot5.CSD_StorageDocReferenceInfo, "Enter a valid eDoc");
			}
		}
	}
}
