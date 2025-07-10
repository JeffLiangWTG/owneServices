using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	/// <summary>
	/// Group these lines per OriginalCountryOfOrigin/OriginalSPI/HTSEffectiveDate/OriginalHTS and all of SecondaryLines.OriginalHTS in that order
	/// Report them in 50/51
	/// 52 is to be populated by IReconEntryLine.EntryNumber and EntryLineNumber
	/// 53 is to be populated by IReconEntryLine.ReconHTS, ReconCustomsValue and ReconDuty, ReconSPI
	/// 54 is to be populated by IReconEntryLine.SecondaryLines.ReconHTS, ReconCustomsValue, ReconDuty
	/// 55 is to be populated by IReconEntryLine.Fees
	/// </summary>
	public interface IReconEntryLine
	{
		ZString OriginalCoutryOfOrigin { get; }
		ZString OriginalSPI { get; }
		ZDate OriginalHTSEffectiveDate { get; }
		ZString ReconReason { get; }
		ZString OriginalHTS { get; }
		ZString ReconHTS { get; }
		ZDecimal ReconCustomsValue { get; }
		ZDecimal ReconDuty { get; }
		ZString ReconSPI { get; }
		ZBool HTSChangedDueToValueIndicator { get; }
		IEnumerable<IReconciliationImportEntryFee> Fees { get; }
		IEnumerable<IReconOriginalEntryLine> OriginalEntryLines { get; }
		ZString GroupLineNumber { get; }
		ZString SecondaryLineNumber { get; }
		ZBool IsCottonFeeMandatory { get; }
		ZBool IsNAFTARecon { get; }
		ZDecimal OriginalCustomsValue { get; }
		ZDecimal OriginalDuty { get; }
		ZString CalculateYear { get; }
	}

	public interface IReconOriginalEntryLine
	{
		ZString EntryNumber { get; }
		ZString EntryFilerCode { get; }
		ZString EntryLineNumber { get; }
	}
}
