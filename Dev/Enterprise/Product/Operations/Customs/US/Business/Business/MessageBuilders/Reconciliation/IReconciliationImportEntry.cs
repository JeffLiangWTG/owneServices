using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IReconciliationImportEntry
	{
		ZString ImportEntryFilerCodeNumber { get; }
		ZString EntryFilerCode { get; }
		ZString EntryNumber { get; }

		ZString Port { get; }

		/// <summary>
		/// Message spec says zero should be sent for aggregated entries, however it should be message layer manipulation as R90 needs this value.
		/// </summary>
		ZDecimal OriginalDuty { get; }

		/// <summary>
		/// Message spec says zero should be sent for aggregated entries, however it should be message layer manipulation as R90 needs this value.
		/// </summary>
		ZDecimal EstimatedReconciliationDuty { get; }

		/// <summary>
		/// Message spec says zero should be sent for aggregated entries, however it should be message layer manipulation as R90 needs this value.
		/// </summary>
		ZDecimal OriginalTax { get; }

		/// <summary>
		/// Message spec says zero should be sent for aggregated entries, however it should be message layer manipulation as R90 needs this value.
		/// </summary>
		ZDecimal EstimatedReconciliationTax { get; }

		/// <summary>
		/// Message spec says zero should be sent for aggregated entries, however it should be message layer manipulation as R90 needs this value.
		/// </summary>
		ZDecimal EstimatedReconciliationInterest { get; }

		IEnumerable<IReconciliationImportEntryFee> Fees { get; }

		//ARECRP1
		ZString ProtestID { get; }

		//ARECRQ1
		ZString PendingActionIDType { get; }
		ZString PendingActionID { get; }
	}
}
