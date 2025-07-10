using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.PL.Business;

public sealed class CustomsDeclarationMessageSendingObject(CusEntryHeader header) : BaseMessageSendingObject(header)
{
	public override CodeDescriptionPairList ActionList => Header.IsExport
		? GetExportActionList()
		: GetImportActionList();

	protected override int GetExpectedEntryNumberLengthCore() => Action == Constants.MessageSendingObjectActionCodes.CC513 ? 18 : base.EntryNumberMaxLength;

	protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() =>
		new CustomsDeclarationMessageSendingObjectValidation(this);

	CodeDescriptionPairList GetExportActionList() =>
		Factory.GetCachedValue($"PL.{nameof(ExportMessageSendingObjectActionList)}|{Header.CH_EntryStatus}", () =>
		{
			var list = new ExportMessageSendingObjectActionList();
			if (Header.CH_EntryStatus != AESEntryStatusList.Codes.ControlledForExport)
			{
				list.RemoveCode(ExportMessageSendingObjectActionList.Codes.CC566);
			}

			return list;
		});

	CodeDescriptionPairList GetImportActionList() =>
		Factory.GetCachedValue<ImportMessageSendingObjectActionList>();
}
