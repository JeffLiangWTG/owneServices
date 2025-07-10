using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public abstract class ReconDutyDataLineHeader : IDutyDataLineHeader
	{
		protected ReconDutyDataLineHeader(ReconOriginalEntryHeader entry)
		{
			this.entry = entry;
		}

		public readonly ReconOriginalEntryHeader entry;

		#region IDutyDataLineHeader Members

		public virtual void OnCalculating()
		{
		}

		public bool CalculateChangedLinesOnly => entry.US_R_ChangedLinesOnly;

		public ZDecimal OriginalTotalCV => entry.US_R_OrigCV;

		public bool IsHMFApplicable
		{
			get { return entry.US_R_IsHMFApplicable == YesNoDefaultList.Codes.Yes; }
		}

		public bool IsInformalFeeApplicable
		{
			get { return false; }
		}

		public bool IsDutiableMailFeeApplicable
		{
			get { return false; }
		}

		public bool IsHMFDeMinimisApplicable
		{
			get { return IsHMFDeminisApplicableCore; }
		}

		protected abstract bool IsHMFDeminisApplicableCore { get; }

		public ZDateTime DateForMPFCalculation
		{
			get { return entry.US_R_DateForMPFCalc; }
		}

		public ZDateTime DateForFeeCalculation
		{
			get { return entry.US_R_DutyRateDate; }
		}

		public BusinessObjectFactory Factory
		{
			get { return entry.Factory; }
		}

		public void DeleteDetachedEntryLines()
		{
			//this is only for normal jobs
		}

		public IEnumerable<IEntryLineOrInvoiceLineDutyData> DutyDataLines
		{
			get { return DutyDataLinesCore; }
		}

		protected abstract IEnumerable<IEntryLineOrInvoiceLineDutyData> DutyDataLinesCore { get; }

		public IFees FeeAndCharges
		{
			get { return FeeAndChargesCore; }
		}

		protected abstract IFees FeeAndChargesCore { get; }

		public abstract void UpdateAfterHMFDeMinimusRuleApplied();

		bool IDutyDataLineHeader.DoesMPFSurchargeApply
		{
			get { return false; }
		}

		bool IDutyDataLineHeader.IsCottonFeeDeMinimusApplicable
		{
			get
			{
				return entry.IsCottonFeeDeMinimusApplicable();
			}
		}

		ZDecimal? IDutyDataLineHeader.OverridenTotalMPFPayable => null;

		public abstract bool AreDutyFeeKnownAndImported { get; }

		#endregion

	}

	public class ReconCurrentDutyDataLineHeader : ReconDutyDataLineHeader, IDutyDataLineHeader
	{
		public ReconCurrentDutyDataLineHeader(ReconOriginalEntryHeader entry)
			: base(entry)
		{
		}

		protected override IEnumerable<IEntryLineOrInvoiceLineDutyData> DutyDataLinesCore
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in entry.Invoice)
				{
					foreach (JobComInvoiceLine line in invoice.JobComInvoiceLines)
					{
						if (!line.HasEmptySupTariff)
						{
							yield return new ReconSupDutyData(line, entry.US_R_DateForMPFCalc);
						}

						yield return new ReconCurrentDutyData(line, entry.US_R_DateForMPFCalc);
					}
				}
			}
		}

		protected override IFees FeeAndChargesCore
		{
			get { return entry.ReconCharges; }
		}

		protected override bool IsHMFDeminisApplicableCore
		{
			get { return entry.ReconDuty == 0m && entry.ReconTax == 0m; }
		}

		public override void UpdateAfterHMFDeMinimusRuleApplied()
		{
			if (entry.IsACE)
			{
				foreach (JobComInvoiceHeader invoice in entry.Invoice)
				{
					foreach (JobComInvoiceLine line in invoice.JobComInvoiceLines)
					{
						line.FeeCusCodes.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.HMF, 0m);
					}
				}
			}
		}

		public override bool AreDutyFeeKnownAndImported
		{
			get { return false; }
		}

		ZDecimal? IDutyDataLineHeader.OverridenTotalMPFPayable
		{
			get
			{
				if (entry.US_R_MonthlyFiling)
				{
					return entry.ReconMPF;
				}

				return null;
			}
		}

		public override void OnCalculating()
		{
			base.OnCalculating();
			if (CalculateChangedLinesOnly)
			{
				foreach (ReconEntryOriginalCharge originalCharge in entry.OriginalCharges)
				{
					if (originalCharge.CY_Code != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
					{
						var code = originalCharge.CY_Code == Core.Constants.USCustoms.FeeCodes.MPC ? Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing : originalCharge.CY_Code.ToString();
						entry.ReconCharges.AddNew(code, originalCharge.CY_Amount);
					}
				}
				if (IsHMFApplicable && entry.OriginalHMF == 0)
				{
					var hmfRate = new FeeCalculationHelper(entry.Factory, DateForFeeCalculation).HMFRatePercentage;
					entry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, new ZDecimal(OriginalTotalCV * hmfRate / 100).Round(2));
				}
			}
		}
	}

	public class ReconOriginalDutyDataLineHeader : ReconDutyDataLineHeader, IDutyDataLineHeader
	{
		public ReconOriginalDutyDataLineHeader(ReconOriginalEntryHeader entry)
			: base(entry)
		{
		}

		protected override IEnumerable<IEntryLineOrInvoiceLineDutyData> DutyDataLinesCore
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in entry.Invoice)
				{
					foreach (JobComInvoiceLine line in invoice.JobComInvoiceLines)
					{
						if (!line.US_R_OrigSupTariff.IsEmpty)
						{
							yield return new ReconOriginalSupDutyData(line, entry.US_R_DateForMPFCalc);
						}

						yield return new ReconOriginalDutyData(line, entry.US_R_DateForMPFCalc);
					}
				}
			}
		}

		protected override IFees FeeAndChargesCore
		{
			get { return entry.OriginalCharges; }
		}

		protected override bool IsHMFDeminisApplicableCore
		{
			get { return entry.OriginalDuty == 0m && entry.OriginalTax == 0m; }
		}

		public override void UpdateAfterHMFDeMinimusRuleApplied()
		{
			if (entry.IsACE)
			{
				foreach (JobComInvoiceHeader invoice in entry.Invoice)
				{
					foreach (JobComInvoiceLine line in invoice.JobComInvoiceLines)
					{
						line.ReconOriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.HMF, 0m);
					}
				}
			}
		}

		public override bool AreDutyFeeKnownAndImported
		{
			get { return !entry.US_R_CalcOrigDuty; }
		}

		ZDecimal? IDutyDataLineHeader.OverridenTotalMPFPayable
		{
			get
			{
				if (entry.US_R_MonthlyFiling)
				{
					return entry.OriginalMPF;
				}

				return null;
			}
		}
	}
}
