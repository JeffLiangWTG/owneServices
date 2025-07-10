using System;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.Schema.PermitItemDetails
{
	partial class permitItemDetails : IInputDoc
	{
		public DateTime Created => created;
	}

	partial class permitItemDetailsPermitItemDetailEntry : IInputEntry
	{
		public string MeaningDe => meaningDe;
		public string MeaningFr => meaningFr;
		public string MeaningIt => meaningIt;
		public string MeaningEn => meaningEn;
	}
}
