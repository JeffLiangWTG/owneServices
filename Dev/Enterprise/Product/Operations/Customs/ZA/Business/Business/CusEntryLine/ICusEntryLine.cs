using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public interface ICusEntryLine : Customs.Business.ICusEntryLine, Integration.Customs.ZA.ICusEntryLine
	{
		#region Questionable - potentially obsolete

		ZGuid PK { get; }
		CusEntryHeader Header { get; }

		#endregion

		CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees { get; }
		ZDecimal VAT { get; }
		ZDecimal CustomsDuty { get; }
		ZString ValuationCode { get; }

		#region Proxied from Declaration

		bool IsImport { get; }

		#endregion

		#region Proxied property from RandomLine

		ZInt PreviousEntryLineNumber { get; }
		ZDecimal ImportDutyPaid { get; }
		ZDecimal ImportDutySch1P2BPaid { get; }
		ZDecimal ImportVATPaid { get; }
		ZDecimal ImportCustomsValue { get; }

		ZString PrimaryPreference { get; }
		ZBool JI_TakeUpInTradeStatistics { get; }
		ZString ValueDeterminationNumber { get; }

		#endregion

		ZString ProcedureCategory { get; }
		ZString CustomsProcedureCode { get; }
		ZString PreviousProcedureCode { get; }

		CurrencyConverter CurrencyConverter { get; }
	}
}
