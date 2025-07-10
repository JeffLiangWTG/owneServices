using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IDrawbackEntryLine : IBaseDrawbackEntryLine, IEntryLine
	{
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		IDrawbackEntryLine[] AllDrawbackEntryLines { get; }

		ZDateTime EntryDate { get; }
		ZString EntryPort { get; }

		ZString DutyRateDescription { get; }
		ZString InvoiceNumber { get; }

		ZString PartNo { get; }
		ZString SecondCustomsUnitQty { get; }
		ZString ThirdCustomsUnitQty { get; }

		ZDecimal HMFAmountForEntry { get; }
		ZBool US_HasMPF { get; }
		ZDecimal MPFAmountForEntry { get; }
		ZDecimal TotalEnteredValueForEntry { get; }

		ZDecimal SecondCustomsQuantity { get; }
		ZDecimal ThirdCustomsQuantity { get; }

		ZBool IsSecondaryTariffLine { get; }

		new IDrawbackEntryLine ParentLine { get; }
		IEnumerable<IFee> Fees { get; }
		new IEnumerable<IDrawbackEntryLine> ChildSecondaryEntryLines { get; }
		ZDecimal GetFeeAmount(ZString feeTypeCode);

		ZDecimal TotalCustomsValueIncludingSecondaryLines { get; }
		ZDecimal TotalCustomsValueIncludingSecondaryLinesFromInvoiceLines { get; }
		ZDecimal TotalTaxIncludingSecondaryLines { get; }
		ZDecimal TotalFeeAmountIncludingSecondaryLines { get; }
		ZDecimal TotalMPFIncludingSecondaryLines { get; }
		ZDecimal TotalHMFIncludingSecondaryLines { get; }
		ZDecimal TotalDutyIncludingSecondaryLines { get; }
		ZDecimal TotalPayableMPFIncludingSecondaryLines { get; }
	}
}
