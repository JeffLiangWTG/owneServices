using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IUSCustomsChargeEntry
	{
		ZString EntryFilerCode { get; }
		ZString EntryNumber { get; }
		ZString PaymentType { get; }
		ZDate DueDate { get; }

		bool IsPaidByImporter { get; }
	}

	public static class IUSCustomsChargeEntryExtensions
	{
		public static ZString GetFormattedEntryReferenceForHeader(this IUSCustomsChargeEntry chargeEntry)
		{
			return GetFormattedEntryReferenceForHeader(chargeEntry.EntryFilerCode, chargeEntry.EntryNumber, chargeEntry.IsPaidByImporter, chargeEntry.PaymentType, chargeEntry.DueDate);
		}

		// XXX-NNNNNNN-N (ACH Pay Type 3 by dd-MMM-yyyy) 
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "This is a preferred format and ToShortDateString() has two digits for month instead.")]
		public static ZString GetFormattedEntryReferenceForHeader(ZString entryFilerCode, ZString entryNumber, bool isPaidByImporter, ZString paymentType, ZDate dueDate)
		{
			ZString formattedEntryNumber = CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(entryFilerCode, entryNumber);

			ZString additionalInfo = ZString.Empty;

			if (isPaidByImporter)
			{
				string dueDateDetails = "";

				if (!dueDate.IsEmpty)
				{
					dueDateDetails = " by " + dueDate.ToString("dd-MMM-yyyy");// This is a preferred format and ToShortDateString() has two digits for month instead.
				}

				additionalInfo = string.Format(AdditionalEntryReferenceHeaderTemplate, paymentType, dueDateDetails);
			}

			return formattedEntryNumber + additionalInfo;
		}

		public const string AdditionalEntryReferenceHeaderTemplate = " (ACH Pay Type {0}{1})";
	}
}
