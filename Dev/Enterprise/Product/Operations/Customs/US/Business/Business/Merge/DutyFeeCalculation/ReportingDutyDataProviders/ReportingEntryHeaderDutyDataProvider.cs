using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class ReportingEntryHeaderDutyDataProvider : IEntryHeaderDutyDataProvider
	{
		public ReportingEntryHeaderDutyDataProvider(CusEntryHeader entry, ReportingDeclarationDutyDataProvider reportingDeclaration)
		{
			this.entry = Argument.NotNull(entry, nameof(entry));
			this.reportingDeclaration = Argument.NotNull(reportingDeclaration, nameof(reportingDeclaration));
		}
		internal ReportingFeesDutyDataProvider Charges => charges ?? (charges = new ReportingFeesDutyDataProvider());
		ReportingFeesDutyDataProvider charges;

		readonly CusEntryHeader entry;
		readonly ReportingDeclarationDutyDataProvider reportingDeclaration;
		ZString IEntryHeaderDutyDataProvider.EntryType => entry.EntryType;

		ZDecimal IEntryHeaderDutyDataProvider.InformalFee => GetChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal);

		ZDecimal IEntryHeaderDutyDataProvider.MPFAmountForEntry => GetChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);

		ZDecimal GetChargeAmount(ZString chargeType) => Charges.GetFeeOrChargeAmount(chargeType);

		bool IDutyDataLineHeader.CalculateChangedLinesOnly => false;

		ZDecimal IDutyDataLineHeader.OriginalTotalCV => 0m;

		void IDutyDataLineHeader.OnCalculating()
		{
		}

		IDutyDataLineHeader dutyDataLineHeader => entry;
		void IDutyDataLineHeader.DeleteDetachedEntryLines()
		{
		}

		void IDutyDataLineHeader.UpdateAfterHMFDeMinimusRuleApplied()
		{
			if (entry.IsACE)
			{
				foreach (var entryLine in EntryLines)
				{
					entryLine.Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.HMF, 0m);
				}
			}
		}

		IEnumerable<IInvoiceLineDutyDataProvider> IEntryHeaderDutyDataProvider.InvoiceLines => InvoiceLines;
		ReportingInvoiceLineDutyDataProvider[] InvoiceLines => invoiceLines ?? (invoiceLines = entry.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => reportingDeclaration.GetOrCreate(x)).ToArray());
		ReportingInvoiceLineDutyDataProvider[] invoiceLines;

		IEnumerable<IEntryLineDutyDataProvider> IEntryHeaderDutyDataProvider.EntryLines => EntryLines;
		ReportingEntryLineDutyDataProvider[] EntryLines => entryLines ?? (entryLines = entry.MergedLines.Cast<CusEntryLine>().Select(x => reportingDeclaration.GetOrCreate(x)).ToArray());
		ReportingEntryLineDutyDataProvider[] entryLines;

		bool IDutyDataLineHeader.AreDutyFeeKnownAndImported => dutyDataLineHeader.AreDutyFeeKnownAndImported;

		bool IDutyDataLineHeader.IsHMFApplicable => dutyDataLineHeader.IsHMFApplicable;

		bool IDutyDataLineHeader.IsInformalFeeApplicable => dutyDataLineHeader.IsInformalFeeApplicable;

		bool IDutyDataLineHeader.IsDutiableMailFeeApplicable => dutyDataLineHeader.IsDutiableMailFeeApplicable;

		bool IDutyDataLineHeader.IsHMFDeMinimisApplicable => dutyDataLineHeader.IsHMFDeMinimisApplicable;

		ZDecimal? IDutyDataLineHeader.OverridenTotalMPFPayable => dutyDataLineHeader.OverridenTotalMPFPayable;

		ZDateTime IDutyDataLineHeader.DateForMPFCalculation => dutyDataLineHeader.DateForMPFCalculation;

		ZDateTime IDutyDataLineHeader.DateForFeeCalculation => dutyDataLineHeader.DateForFeeCalculation;

		bool IDutyDataLineHeader.IsCottonFeeDeMinimusApplicable => dutyDataLineHeader.IsCottonFeeDeMinimusApplicable;
		IEnumerable<IEntryLineOrInvoiceLineDutyData> IDutyDataLineHeader.DutyDataLines => EntryLines;
		IFees IDutyDataLineHeader.FeeAndCharges => Charges;
		bool IDutyDataLineHeader.DoesMPFSurchargeApply => dutyDataLineHeader.DoesMPFSurchargeApply;
		BusinessObjectFactory IDutyDataLineHeader.Factory => entry.Factory;
	}
}
