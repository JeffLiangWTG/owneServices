using System;

namespace Enterprise.Customs.PL.Business;

public class ExportOperationProvider_CC515_CC513(BaseMessageSendingObject sendingObject) : AESExportOperationProvider(sendingObject)
{
	protected override byte? GetEadPrint() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? null
		: base.GetEadPrint();

	protected override bool? GetStorage() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? null
		: base.GetStorage();

	protected override DateTime? GetPresentationOfTheGoodsDateAndTime() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? null
		: base.GetPresentationOfTheGoodsDateAndTime();

	protected override string GetSpecificCircumstanceIndicator() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? null
		: base.GetSpecificCircumstanceIndicator();
}
