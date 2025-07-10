using System.Collections.Generic;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Customs.Business.ArchiveManager;

namespace Enterprise.Customs.US.Business.ArchiveManager
{
	class ArchiveableJobDeclaration : ArchiveableJobDeclaration<Integration.Customs.US.IJobDeclaration, JobDeclaration>
	{
		public ArchiveableJobDeclaration(Integration.Customs.US.IJobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override IEnumerable<ArchiveReferenceKey> CountrySpecificAdditionalKeys
		{
			get
			{
				if (Declaration.IsExport)
				{
					foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
					{
						yield return new ArchiveReferenceKey(CustomsArchiveReferenceKeys.CustomsReferenceNumber, entryHeader.EntryNumber);
					}
				}
				else
				{
					yield return new ArchiveReferenceKey(CustomsArchiveReferenceKeys.CustomsReferenceNumber, Declaration.EntryFilerCode + Declaration.ImportEntryNumber);
				}
			}
		}
	}
}
