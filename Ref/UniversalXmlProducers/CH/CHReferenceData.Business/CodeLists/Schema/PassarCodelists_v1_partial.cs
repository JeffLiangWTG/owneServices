using System;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.PassarCodeListsSchema
{
	partial class codeLists : IInputDoc
	{
		public DateTime Created => created;
	}

	partial class codeListsCodeListCode : IInputEntry
	{
		public string MeaningDe => textDe;
		public string MeaningFr => textFr;
		public string MeaningIt => textIt;
		public string MeaningEn => textEn;
	}
}
