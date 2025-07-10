using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	class DA66DA63DocumentWrapper : DA63DocumentWrapper
	{
		public DA66DA63DocumentWrapper(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public ZString MRN => Declaration.InvoiceLines[0].JI_PreviousEntryNumber;

		public ZString RefundDrawbackItemNumber => DA63EntryLines.Cast<DA63LineDetailWrapper>().FirstOrDefault()?.RefundDrawbackItemNumber ?? ZString.Empty;

		public ZDecimal B3CustomsDuty => DA63EntryLines.Cast<DA63LineDetailWrapper>().Sum(x => x.DA63CustomsDutyExcluding12B);
		public ZDecimal B3ExciseDuty => DA63EntryLines.Cast<DA63LineDetailWrapper>().Sum(x => x.Others.Where(x => ChargeTypeHelper.IsExciseDutyCode(x.Code)).Select(x => x.Value).Sum(x => x));
		public ZDecimal B3AntiDumpingDuty => DA63EntryLines.Cast<DA63LineDetailWrapper>().Sum(x => x.Others.Where(x => ChargeTypeHelper.IsAntiDumpingDutyCode(x.Code)).Select(x => x.Value).Sum(x => x));
		public ZDecimal B3DutySch1P2B => DA63EntryLines.Cast<DA63LineDetailWrapper>().Sum(x => x.DA63S1P2BDuty);
		public ZDecimal B3VAT => DA63EntryLines.Cast<DA63LineDetailWrapper>().Sum(x => x.DA63ValueAddedTax);
		public ZDecimal B3Other => DA63EntryLines.Cast<DA63LineDetailWrapper>().Sum(
			x => x.Others.Where(x => !ChargeTypeHelper.IsExciseDutyCode(x.Code) && !ChargeTypeHelper.IsAntiDumpingDutyCode(x.Code)).Select(x => x.Value).Sum(x => x));

		public BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> DetailsOfAmounts => detailsOfAmounts ??= PopulateDetailsOfAmounts();
		BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> detailsOfAmounts;

		BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> PopulateDetailsOfAmounts()
		{
			var result = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>();

			foreach (var entryLine in EntryHeader.MergedLines?.OfType<CusEntryLine>()?.Where(x => x.RandomLine?.IsDA63 ?? false))
			{
				foreach (var invoiceLine in entryLine.InvoiceLines.OfType<JobComInvoiceLine>())
				{
					result.AddRange(invoiceLine.DA63AdditionalDuties.Select(x => new DutyFeeInformationDocWrapper(x.CY_Code, x.CY_Value)));
				}
			}

			return result;
		}

		protected override BusinessObjectCollectionWrapper<DA63LineDetailWrapper> PopulateDA63EntryLines()
		{
			var result = base.PopulateDA63EntryLines();
			foreach (var line in result.Where(line => !line.LineNumber.IsEmpty).Select((line, i) => (line, i)))
			{
				line.line.DA63LineNumber = (line.i + 1).ToString();
			}
			return result;
		}
	}
}
