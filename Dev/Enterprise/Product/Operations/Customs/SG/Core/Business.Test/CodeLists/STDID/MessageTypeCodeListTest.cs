using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class MessageTypeCodeListTest : TestCaseWithFactory
	{
		public void TestGetListWithAdvanceShippingNotice()
		{
			var messageTypeCodeList1 = MessageTypeCodeList.GetListWithAdvanceShippingNotice(Factory);
			Assert("ASN should be added", messageTypeCodeList1.ContainsCode("ASN"));
			var messageTypeCodeList2 = MessageTypeCodeList.GetListWithAdvanceShippingNotice(Factory);
			Assert("ASN should be added", messageTypeCodeList2.ContainsCode("ASN"));
			AssertEquals("MessageTypeCodeList should be cached", messageTypeCodeList1, messageTypeCodeList2);
		}
	}
}
