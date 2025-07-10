
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AggregateReconEntryDocumentData : AutoAggregateReconEntryDocumentData, IObsoleteValidation
	{
		/// <param name="entry1">this should not be null</param>
		/// <param name="entry2">this can be null</param>
		public AggregateReconEntryDocumentData(ReconOriginalEntryHeader entry1, ReconOriginalEntryHeader entry2)
			: base(entry1.Factory)
		{
			this.entry1 = entry1;
			this.entry2 = entry2;

			PopulateDataFromEntry1And2();
		}

		readonly ReconOriginalEntryHeader entry1;
		readonly ReconOriginalEntryHeader entry2;

		void PopulateDataFromEntry1And2()
		{
			US_EntryNumber1 = entry1.CH_OrigEntryReference.Left(US_EntryNumber1Info.MaxLength);
			US_PortCode1 = entry1.US_SchDEntry.Left(US_PortCode1Info.MaxLength);

			if (entry2 != null)
			{
				US_EntryNumber2 = entry2.CH_OrigEntryReference.Left(US_EntryNumber2Info.MaxLength);
				US_PortCode2 = entry2.US_SchDEntry.Left(US_PortCode2Info.MaxLength);
			}
		}
	}
}
