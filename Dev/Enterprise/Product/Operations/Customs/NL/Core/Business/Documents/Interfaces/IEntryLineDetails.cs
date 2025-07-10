
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public interface IEntryLineDetails
{
	ZInt ArticleSequenceNumber { get; }
	ZString MovementReferenceNumber { get; }
	ZDecimal TotalAmountDuties { get; }
	ZDecimal TotalAmountTax { get; }
	ZDecimal TotalAmountDutiesAndTax { get; }
	IEnumerable<IEntryLineFee> LineFeesSummary { get; }
	ZBool IsLastFeeOfEntryLine { get; }
}
