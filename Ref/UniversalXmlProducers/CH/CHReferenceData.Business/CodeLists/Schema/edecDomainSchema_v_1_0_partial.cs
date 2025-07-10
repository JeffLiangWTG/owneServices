using System;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.DomainSchema
{
	partial class domains : IInputDoc
	{
		public DateTime Created => created;
	}

	partial class domainsDomainEntry : IInputEntry
	{
		public string MeaningDe => meaningDe;
		public string MeaningFr => meaningFr;
		public string MeaningIt => meaningIt;
		public string MeaningEn => meaningEn;
	}
}
