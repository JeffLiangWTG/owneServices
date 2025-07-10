using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AggregateReconEntryDocumentDataCollection : NonPersistentBusinessObjectCollection<AggregateReconEntryDocumentData>
	{
		public AggregateReconEntryDocumentDataCollection(ReconDeclaration reconDeclaration)
			: base(reconDeclaration.Factory)
		{
			this.reconDeclaration = reconDeclaration;
			PopulateCollection();
		}

		readonly ReconDeclaration reconDeclaration;

		#region Implementation

		void PopulateCollection()
		{
			List<ReconOriginalEntryHeader> originalEntries = new List<ReconOriginalEntryHeader>(new TypedEnumerable<ReconOriginalEntryHeader>(reconDeclaration.OriginalEntries));
			originalEntries.Sort(new ReconOriginalEntryHeaderDocumentComparer());

			int count = originalEntries.Count;
			int midPoint = (count % 2 == 1 ? count + 1 : count) / 2;

			for (int index = 0; index < midPoint; index++)
			{
				ReconOriginalEntryHeader entry1 = originalEntries[index];
				ReconOriginalEntryHeader entry2 = (index + midPoint) < originalEntries.Count ? originalEntries[index + midPoint] : null;

				Add(new AggregateReconEntryDocumentData(entry1, entry2));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("AllowNew is false and users do not create a new element in a grid. this is for documents only");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}
