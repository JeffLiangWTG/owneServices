using CargoWise.RefDbRepo.CHReferenceData.Services;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	internal class KeyStructureImport : KeyStructure
	{
		internal KeyStructureImport(DownloadResult keyStructureDownload, bool isPrefaceDictionary) : base(keyStructureDownload, isPrefaceDictionary)
		{
		}

		protected override string VTyp => "VLSI";
		protected override string STyp => "STI";
	}
}
