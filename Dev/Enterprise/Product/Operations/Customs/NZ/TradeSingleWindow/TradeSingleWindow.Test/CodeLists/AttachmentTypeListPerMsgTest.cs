using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.TradeSingleWindow.Testing
{
	class AttachmentTypeListPerMsgTest : TestCaseWithFactory
	{
		public void TestListHasRightNumberOfCodesAndDescendsFromBase()
		{
			var attachmentList = new AttachmentTypeList();
			AssertEquals("Full List.Count", 21, attachmentList.Count);
			var creList = new AttachmentTypeListPerMsg.AttachmentTypeListForCRE();
			AssertEquals("CRE List should only have OTH code", 1, creList.Count);
			AssertEquals(true, creList.ContainsCode("OTH"));
			var im1List = new AttachmentTypeListPerMsg.AttachmentTypeListForIM1();
			AssertEquals("IM1 List should have 9 codes", 9, im1List.Count);
			AssertEquals(true, im1List.ContainsCode("OTH"));
			AssertEquals(true, im1List.ContainsCode("UBD"));
			AssertEquals(false, im1List.ContainsCode("SSR"));
			var ex1List = new AttachmentTypeListPerMsg.AttachmentTypeListForEX1();
			AssertEquals("ex1 List should have 6 codes", 6, ex1List.Count);
			AssertEquals(true, ex1List.ContainsCode("OTH"));
			AssertEquals(false, ex1List.ContainsCode("UBD"));
			AssertEquals(false, ex1List.ContainsCode("SSR"));
			AssertEquals(true, ex1List.ContainsCode("PAC"));
			var ocrList = new AttachmentTypeListPerMsg.AttachmentTypeListForOCR();
			AssertEquals("CRE List should only have OTH code", 1, ocrList.Count);
			AssertEquals(true, ocrList.ContainsCode("OTH"));
		}
	}
}
