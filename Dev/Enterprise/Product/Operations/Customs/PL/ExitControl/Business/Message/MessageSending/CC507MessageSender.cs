using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;
using PLEntryStatus = Enterprise.Customs.PL.Business.Declaration.PLEntryStatusList.Codes;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC507MessageSender(ExitControlMessageSendingObject sendingObject) : ExitControlMessageSender(sendingObject)
{
	protected internal override ZString MessageSubType => ExitControlMessageCodes.Descriptions.CC507;

	protected override IXmlMessageBuilder GetXmlMessageBuilder() => new CC507CMessageBuilder(new CC507RootProvider(ExitReport));

	public override void AfterSendCore()
	{
		base.AfterSendCore();

		if (EntryHeader?.CH_EntryStatus.ToString() == PLEntryStatus.ReleasedForExport)
		{
			ExitReport.CER_MessageStatus = LogicalStatusList.Codes.Sent;
		}
	}
}
