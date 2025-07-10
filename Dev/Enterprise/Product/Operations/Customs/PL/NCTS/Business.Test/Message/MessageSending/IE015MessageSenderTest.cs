using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(IE015MessageSender))]
sealed class IE015MessageSenderTest : IE013IE015MessageSenderTest<IE015MessageSender>
{
	public void TestSend()
	{
		(var nctsHeader, var sender) = CreateMessageSender();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		sender.Send();

		var message = (EDIMessage)nctsHeader.MovementHeader.Messages.First();
		CombineAssertions(() =>
		{
			var expectedMessageType = EUJobMessageTypeList.Codes.NctsDeparture;
			AssertEquals("EM_MessageType", expectedMessageType, message.EM_MessageType);
			var expectedMessageSubType = Constants.MessageSubTypeCodes.IE015;
			AssertEquals("EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
			var expectedFragmentOfContent = "IE015PL";
			Assert("EM_MessageText", message.EM_MessageText.Contains(expectedFragmentOfContent));
			var expectedPhaseCode = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			AssertEquals($"BM_Phase should be updated with MessageType:{expectedMessageType}", expectedPhaseCode, nctsHeader.MovementHeader.BM_Phase);
		});
	}

	public void TestDeclarationGoodsItemNumberAfterSend()
	{
		const int DeclarationGoodsItemNumberBeforeSend = 0;
		const int DeclarationGoodsItemNumberAfterSend = 1;

		(var nctsHeader, var sender) = CreateMessageSender();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("BY_DeclarationGoodsItemNumber should be set 0 as default before sending", DeclarationGoodsItemNumberBeforeSend, goodsItem.BY_DeclarationGoodsItemNumber);

			sender.Send();

			AssertEquals("BY_DeclarationGoodsItemNumber should be set 1 after sending", DeclarationGoodsItemNumberAfterSend, goodsItem.BY_DeclarationGoodsItemNumber);
		});
	}

	[TestDate]
	public void TestSettingValuationDate()
	{
		(var nctsHeader, var sender) = CreateMessageSender();

		nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;

		sender.Send();

		AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);
	}

	protected override ZString HeaderType => NctsMovementType.Codes.Departure;
}
