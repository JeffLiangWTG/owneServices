using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business
{
	class JobDeclarationConsolidatedEntryProvider : Customs.Business.JobDeclarationConsolidatedEntryProvider
	{
		public JobDeclarationConsolidatedEntryProvider(BaseJobDeclaration declaration, IConsolidatedEntryDeclarationRemover consolidatedEntryDeclarationRemover = null) : base(declaration, consolidatedEntryDeclarationRemover)
		{
		}

		public override ZString ConsolidationStatus
		{
			get => base.ConsolidationStatus;
			protected set
			{
				if (value.IsEmpty || ConsolidationStatusList.ContainsCode(value))
				{
					declaration.JE_MessageStatus = value;
					declaration.JE_EntryStatus = value.IsEmpty ? new ZString(FormalEntryStatusList.Codes.NotSentToCustoms) : value;
				}
			}
		}
	}
}
