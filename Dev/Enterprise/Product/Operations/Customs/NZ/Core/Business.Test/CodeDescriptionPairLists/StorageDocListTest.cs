using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.Test
{
	sealed class StorageDocListTest : TestCaseWithFactory
	{
		public void TestSetupDocList()
		{
			using (NZCustomsDataRegistry.Instance.MaxMessageAttachmentSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var scaOceanBill = Factory.New<CusSCAOceanBill>();
				var eDoc1 = scaOceanBill.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "CIV");
				var eDoc2 = scaOceanBill.DocManagerInfo.AddFileOrDocument(new byte[11], "Test2.pdf", "CIV");
				var eDoc3 = scaOceanBill.DocManagerInfo.AddFileOrDocument(new byte[11], "Test2.xxx", "CIV");
				var docPivot = scaOceanBill.EDocPivotCollection.AddNew();
				var availableEDocs = docPivot.Lookups.AvailableEDocs;

				AssertNotContains("Normal attachment description", StorageDocList.FileTooBigIndicator, availableEDocs[eDoc1.UniqueKey].Description);
				AssertContains("Too big attachment description", StorageDocList.FileTooBigIndicator, availableEDocs[eDoc2.UniqueKey].Description);
				AssertNull("Attachment with invalid file extension", availableEDocs[eDoc3.UniqueKey]);
			}
		}
	}
}
