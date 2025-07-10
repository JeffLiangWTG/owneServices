using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ReconChangedLine : AutoReconChangedLine, IReconEntryLine, IReconSecondaryLine
	{
		public ReconChangedLine(BusinessObjectFactory factory)
			: base(factory)
		{
			fees = new List<IReconciliationImportEntryFee>();
			origEntryLines = new List<IReconOriginalEntryLine>();
		}

		public ZDecimal CustomsValueChange
		{
			get { return US_CustomsValue - US_OrigCustomsValue; }
		}

		public ZDecimal DutyChange
		{
			get { return US_Duty - US_OrigDuty; }
		}

		public ZString US_SecondarySPI { get; set; }

		public ZDecimal US_OrigMPF => new ZDecimal(fees.Where(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).Sum(x => x.OriginalFee)).Round(2);

		public ZDecimal US_MPF => new ZDecimal(fees.Where(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).Sum(x => x.EstimatedReconciliationFee)).Round(2);

		public ZDecimal US_OrigHMF => new ZDecimal(fees.Where(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.HMF).Sum(x => x.OriginalFee)).Round(2);

		public ZDecimal US_HMF => new ZDecimal(fees.Where(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.HMF).Sum(x => x.EstimatedReconciliationFee)).Round(2);

		public ZDecimal US_OrigOtherFee => new ZDecimal(fees.Where(x => x.FeeClass != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing && x.FeeClass != Core.Constants.USCustoms.FeeCodes.HMF).Sum(x => x.OriginalFee)).Round(2);

		public ZDecimal US_OtherFee => new ZDecimal(fees.Where(x => x.FeeClass != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing && x.FeeClass != Core.Constants.USCustoms.FeeCodes.HMF).Sum(x => x.EstimatedReconciliationFee)).Round(2);

		public ZDecimal MPFChange
		{
			get;
			internal set;
		}

		public ZDecimal HMFChange
		{
			get { return US_HMF - US_OrigHMF; }
		}

		public ZDecimal OtherFeeChange
		{
			get { return US_OtherFee - US_OrigOtherFee; }
		}

		public ZDecimal OrigDutyRate
		{
			get { return US_OrigCustomsValue != 0m ? (ZDecimal)(US_OrigDuty / US_OrigCustomsValue) : ZDecimal.Zero; }
		}

		public ZString OrigDutyRateDesc
		{
			get { return (HasRateChanged || US_OrigDutyRateDesc.IsEmpty) ? (ZString)((OrigDutyRate * 100).ToString("0.00", CultureInfo.CurrentCulture) + "%") : US_OrigDutyRateDesc; }
		}

		public ZDecimal DutyRate
		{
			get { return US_CustomsValue != 0m ? (ZDecimal)(US_Duty / US_CustomsValue) : ZDecimal.Zero; }
		}

		public ZString DutyRateDesc
		{
			get { return (HasRateChanged || US_OrigDutyRateDesc.IsEmpty) ? (ZString)((DutyRate * 100).ToString("0.00", CultureInfo.CurrentCulture) + "%") : US_OrigDutyRateDesc; }
		}

		public bool HasRateChanged
		{
			get { return (US_OrigTariff != US_Tariff) || (US_OrigTariff.IsEmpty && US_Tariff.IsEmpty); }
		}

		public ReconChangedLineLookups Lookups
		{
			get { return lookups ?? (lookups = new ReconChangedLineLookups(this)); }
		}
		ReconChangedLineLookups lookups;

		internal void RoundValues()
		{
			US_CustomsValue = US_CustomsValue.Round(0);
			US_OrigCustomsValue = US_OrigCustomsValue.Round(0);
		}

		readonly List<IReconciliationImportEntryFee> fees;

		readonly List<IReconOriginalEntryLine> origEntryLines;

		public void AddOrigEntryLine(IReconOriginalEntryLine origEntryLine)
		{
			origEntryLines.Add(origEntryLine);
		}

		public void AddFees(IEnumerable<IReconciliationImportEntryFee> newFees)
		{
			fees.AddRange(newFees);
		}

		public ZString GroupLineNumber => !US_ReconLineNumber.IsEmpty ? US_ReconLineNumber.Left(US_ReconLineNumber.Length - SecondaryLineNumber.Length) : ZString.Empty;

		public ZString SecondaryLineNumber
		{
			get
			{
				ZString secondaryLineNumberRange = "ABCDEFGHI";
				ZString result = ZString.Empty;
				if (!US_ReconLineNumber.IsEmpty)
				{
					var secondaryLineNumber = US_ReconLineNumber.SubstringSafe(US_ReconLineNumber.Length - 1);
					if (secondaryLineNumberRange.Contains(secondaryLineNumber, StringComparison.OrdinalIgnoreCase))
					{
						result = secondaryLineNumber;
					}
				}
				return result;
			}
		}

		#region IReconEntryLine  Members
		ZString IReconEntryLine.OriginalCoutryOfOrigin => US_UC_NKCountryOfOrigin;
		ZString IReconEntryLine.OriginalSPI => US_OrigSPI;
		ZDate IReconEntryLine.OriginalHTSEffectiveDate => US_OriginalHTSEffectiveDate.Date;
		ZString IReconEntryLine.ReconReason => US_ReconReasonText;
		ZString IReconEntryLine.OriginalHTS => US_OrigTariff;
		ZString IReconEntryLine.ReconHTS => US_Tariff;
		ZDecimal IReconEntryLine.ReconCustomsValue => US_CustomsValue;
		ZDecimal IReconEntryLine.ReconDuty => US_Duty;
		ZString IReconEntryLine.ReconSPI => US_SPI;
		ZBool IReconEntryLine.HTSChangedDueToValueIndicator => US_HTSChangedDueToValueIndicator;
		ZBool IReconEntryLine.IsCottonFeeMandatory => US_CottonFeeMandatory;
		IEnumerable<IReconciliationImportEntryFee> IReconEntryLine.Fees
		{
			get
			{
				List<IReconciliationImportEntryFee> totalFees = new List<IReconciliationImportEntryFee>();
				totalFees.Add(new ReconEntryFeeIReconciliationImportEntryFee()
				{
					FeeType = ReconDeclaration.Constants.DutyAccountingClassCode,
					OriginalFee = US_OrigDuty,
					ReconFee = US_Duty
				});
				var sumFees = fees.GroupBy(x => x.FeeClass).Select(x => new ReconEntryFeeIReconciliationImportEntryFee()
				{
					FeeType = x.Key,
					ReconFee = new ZDecimal(x.Sum(fee => fee.EstimatedReconciliationFee)).Round(2),
					OriginalFee = new ZDecimal(x.Sum(fee => fee.OriginalFee)).Round(2)
				});
				totalFees.AddRange(sumFees);
				if (!totalFees.Any(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing))
				{
					totalFees.Add(new ReconEntryFeeIReconciliationImportEntryFee()
					{
						FeeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing,
						OriginalFee = 0m,
						ReconFee = 0m
					});
				}

				totalFees.Sort(new ReconEntryFeeComparer());
				return totalFees;
			}
		}

		IEnumerable<IReconOriginalEntryLine> IReconEntryLine.OriginalEntryLines => origEntryLines;
		ZString IReconEntryLine.GroupLineNumber => GroupLineNumber;
		ZString IReconEntryLine.SecondaryLineNumber => SecondaryLineNumber;
		ZBool IReconEntryLine.IsNAFTARecon => US_NAFTAReconIndicator;
		ZDecimal IReconEntryLine.OriginalCustomsValue => US_OrigCustomsValue;
		ZDecimal IReconEntryLine.OriginalDuty => US_OrigDuty;
		ZString IReconEntryLine.CalculateYear => US_Year;

		#endregion

		#region IReconSecondaryLine  Members
		ZString IReconSecondaryLine.OriginalHTS => US_OrigTariff;
		ZString IReconSecondaryLine.ReconHTS => US_Tariff;
		ZDecimal IReconSecondaryLine.ReconCustomsValue => US_CustomsValue;
		ZDecimal IReconSecondaryLine.ReconDuty => US_Duty;
		ZDecimal IReconSecondaryLine.OriginalCustomsValue => US_OrigCustomsValue;
		ZDecimal IReconSecondaryLine.OriginalDuty => US_OrigDuty;
		#endregion
	}
}
