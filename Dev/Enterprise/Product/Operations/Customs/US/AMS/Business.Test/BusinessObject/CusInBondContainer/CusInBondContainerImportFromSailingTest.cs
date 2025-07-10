using System.Linq;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondContainerImportFromSailingTest : LinkedSailingBillsImportedTest
	{
		public void TestImporting()
		{
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var container = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "AAA").MovementDetail.Containers[0];
			AssertEquals("ABCD1111", container.BC_ContainerNum);
			AssertEquals(rContainer.PK, container.BC_RC);
			AssertEquals("123", container.BC_Seal1);
			AssertEquals("321", container.BC_Seal2);
		}
	}
}
