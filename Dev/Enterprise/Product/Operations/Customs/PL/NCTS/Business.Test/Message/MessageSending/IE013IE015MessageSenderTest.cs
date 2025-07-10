using Enterprise.Customs.Common;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

abstract class IE013IE015MessageSenderTest<T> : MessageSenderAbstractTest<T>
	where T : IE013IE015MessageSender
{
	public void TestPreSend_AssignsDeclarationGoodsItemNumbersAsNaturalNumberSequence()
	{
		(var nctsHeader, var sender) = CreateMessageSender();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var bill2 = nctsHeader.Bills.AddNew();
		bill2.SequenceNumber = 2;
		var bill2goodsItem2 = bill2.GoodsItems.AddNew();
		bill2goodsItem2.BY_LineNo = 2;
		bill2goodsItem2.BY_DeclarationGoodsItemNumber = 5;
		var bill2goodsItem1 = bill2.GoodsItems.AddNew();
		bill2goodsItem1.BY_LineNo = 1;

		var bill1 = nctsHeader.Bills.AddNew();
		bill1.SequenceNumber = 1;
		var bill1goodsItem2 = bill1.GoodsItems.AddNew();
		bill1goodsItem2.BY_LineNo = 2;
		var bill1goodsItem1 = bill1.GoodsItems.AddNew();
		bill1goodsItem1.BY_LineNo = 1;
		bill1goodsItem1.BY_DeclarationGoodsItemNumber = 3;

		sender.Send();

		CombineAssertions(() =>
		{
			AssertEquals("Bill #1, Goods Item #1", 1, bill1goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill #1, Goods Item #2", 2, bill1goodsItem2.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill #2, Goods Item #1", 3, bill2goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill #2, Goods Item #2", 4, bill2goodsItem2.BY_DeclarationGoodsItemNumber);
		});
	}
}
