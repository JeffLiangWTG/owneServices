using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AESExportOperationProvider(BaseMessageSendingObject sendingObject) : IExportOperation
{
	protected readonly BaseMessageSendingObject SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	protected readonly CusEntryHeader EntryHeader = Argument.NotNull(sendingObject.Header, $"{nameof(SendingObject)}.{nameof(BaseMessageSendingObject.Header)}");
	protected readonly JobDeclaration Declaration = Argument.NotNull(sendingObject.Header.Declaration, $"{nameof(EntryHeader)}.{nameof(CusEntryHeader.Declaration)}");
	protected readonly CusEntryInstruction EntryInstruction = Argument.NotNull(sendingObject.Header.EntryInstruction, $"{nameof(EntryHeader)}.{nameof(CusEntryHeader.EntryInstruction)}");

	public string DeclarationType => Declaration.JE_MessageSubType;

	public string AdditionalDeclarationType => EntryInstruction.CEI_SubStyle;

	public DateTime? PresentationOfTheGoodsDateAndTime => CachedValueHelper.GetValue(ref presentationOfTheGoodsDateAndTime, GetPresentationOfTheGoodsDateAndTime);
	CachedValue<DateTime?> presentationOfTheGoodsDateAndTime;

	public string Security => SendingObject.Security;

	public string SpecificCircumstanceIndicator => CachedValueHelper.GetValue(ref specificCircumstanceIndicator, GetSpecificCircumstanceIndicator);
	CachedValue<string> specificCircumstanceIndicator;

	public bool? Storage => CachedValueHelper.GetValue(ref storage, GetStorage);
	CachedValue<bool?> storage;

	public byte? EadPrint => CachedValueHelper.GetValue(ref eadPrint, GetEadPrint);
	CachedValue<byte?> eadPrint;

	protected virtual DateTime? GetPresentationOfTheGoodsDateAndTime() => !Declaration.ZG_PresentationStartDate.IsEmpty
		? Declaration.ZG_PresentationStartDate.ToDateTime()
		: null;

	protected virtual string GetSpecificCircumstanceIndicator() => MessageProviderHelper.ReturnNullIfEmpty(Declaration.ZG_SpecificCircumstanceIndicator);

	protected virtual bool? GetStorage() => !Declaration.JE_OfficeOfEntryExit.IsEmpty && Declaration.JE_OfficeOfEntryExit == Declaration.JE_CustomsOffice
		? EntryInstruction.ZG_ExportManifest
		: null;

	protected virtual byte? GetEadPrint()
	{
		var parsed = ZByte.ParseSafe(EntryInstruction.ZG_EADPrintOut, ZByte.Zero);
		return parsed.IsEmpty ? null : parsed;
	}
}
