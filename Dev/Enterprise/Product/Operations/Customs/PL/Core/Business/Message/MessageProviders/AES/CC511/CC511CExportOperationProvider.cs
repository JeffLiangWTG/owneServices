using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CC511CExportOperationProvider : ICC511CExportOperation
{
	public CC511CExportOperationProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		jobDeclaration = Argument.NotNull(entryHeader.Declaration, $"{nameof(entryHeader)}.{nameof(CusEntryHeader.Declaration)}");
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, $"{nameof(entryHeader)}.{nameof(CusEntryHeader.EntryInstruction)}");
	}

	readonly CusEntryInstruction entryInstruction;
	readonly JobDeclaration jobDeclaration;
	readonly CusEntryHeader entryHeader;

	public string LRN => entryHeader.CH_BGMReference;

	public bool? StoringFlag => !jobDeclaration.JE_OfficeOfEntryExit.IsEmpty && jobDeclaration.JE_OfficeOfEntryExit == jobDeclaration.JE_CustomsOffice
		? entryInstruction.ZG_ExportManifest
		: null;
}
