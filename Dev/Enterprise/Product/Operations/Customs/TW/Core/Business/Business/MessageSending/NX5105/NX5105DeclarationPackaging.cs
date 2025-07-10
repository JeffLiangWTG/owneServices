using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105Declaration_DeclarationPackaging : IDeclarationPackaging
	{
		readonly CusEntryHeader entryHeader;
		readonly CusEntryInstruction entryInstruction;

		public NX5105Declaration_DeclarationPackaging(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "entryHeader");
			entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, "entryInstruction");
		}

		ZString IDeclarationPackaging.MarksNumbers => entryHeader.MarksAndNumbers;

		ZString IDeclarationPackaging.PackagingMaterialDescription => entryInstruction?.CEI_PackageDescription ?? ZString.Empty;

		ZString IDeclarationPackaging.Combination => entryInstruction.CEI_IsCoPackaged.ConvertBoolToString(YesNoList.Codes.Yes);

		ZString IDeclarationPackaging.TypeCode => entryHeader.Declaration?.JE_TotalNoOfPacksPackType ?? ZString.Empty;
	}
}
