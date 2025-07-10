using System.Collections.Generic;

namespace Enterprise.Customs.US.Business.Testing
{
	public static class ReconTestHelper
	{
		public static void ImportLines(this ReconDeclaration reconDeclaration, IEnumerable<JobDeclaration> importDeclarations)
		{
			var retriever = new ReconImportEntryRetriever(reconDeclaration);
			foreach (JobDeclaration importDeclaration in importDeclarations)
			{
				foreach (CusEntryHeader importEntry in importDeclaration.ActiveEntryHeaders)
				{
					if (importEntry.IsFormalEntry)
					{
						var entry = reconDeclaration.OriginalEntries.FindEntryBy(importEntry.EntryFilerCode + importEntry.EntryNumber);

						if (entry == null)
						{
							entry = reconDeclaration.OriginalEntries.AddNew();
							entry.CH_OrigEntryReference = importEntry.EntryFilerCode + importEntry.EntryNumber;
						}

						retriever.ImportLinesFromImportEntry(entry);
					}
				}
			}
		}
	}
}
