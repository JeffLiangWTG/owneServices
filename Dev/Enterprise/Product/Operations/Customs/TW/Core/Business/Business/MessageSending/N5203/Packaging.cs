using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class Packaging : IDeclarationPackaging
	{
		public Packaging(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
			entryInstruction = entryHeader.EntryInstruction;
		}

		readonly CusEntryHeader entryHeader;

		readonly CusEntryInstruction entryInstruction;

		public ZString MarksNumbers => entryHeader.MarksAndNumbers;

		public ZString PackagingMaterialDescription => entryInstruction?.CEI_PackageDescription ?? ZString.Empty;

		public ZString Combination => (entryInstruction?.CEI_IsCoPackaged ?? false) ? YesNoList.Codes.Yes : string.Empty;

		public ZString TypeCode => entryHeader?.Declaration?.JE_TotalNoOfPacksPackType ?? ZString.Empty;
	}
}
