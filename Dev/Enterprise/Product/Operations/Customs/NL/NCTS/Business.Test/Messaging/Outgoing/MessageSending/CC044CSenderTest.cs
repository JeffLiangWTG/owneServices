using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC044CSenderTest : NCTSMessageSenderTest<CC044CSender, ICC044C>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CC044CSender(null));
	}

	public void TestFillDeclarationGoodsItemNumberOnSending()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = header.Bills.AddNew();
		bill.B0_Weight = 1;
		bill.B0_WeightUQ = "KG";
		var goodsItem = bill.GoodsItems.AddNew();
		goodsItem.BY_Description = "Test";
		CombineAssertions(() =>
		{
			AssertEquals("Initial situation: no Declaration Goods Item Number generated.", 0, goodsItem.BY_DeclarationGoodsItemNumber);
			var declarationSenderProvider = new CC044CSender(new MessageSendingAction(header.MovementHeader));
			AssertEquals("After creation of DeclarationMessageSenderProvider: Declaration Goods Item Number is generated", 1, goodsItem.BY_DeclarationGoodsItemNumber);
		});
	}

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override ZString EntryType => NctsMessageTypeListNL.Codes.UnloadingRemarks;

	protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;

	protected override ZString ParentTableName => Customs.Business.AutoCusInBondHeader.Schema.TableName;
}
