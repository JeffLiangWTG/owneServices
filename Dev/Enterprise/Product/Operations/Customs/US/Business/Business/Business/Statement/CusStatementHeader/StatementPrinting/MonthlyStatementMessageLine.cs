using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Registry.Business.Customs.US;

namespace Enterprise.Customs.US.Business
{
	class MonthlyStatementMessageLine : StatementMessageLine, IObsoleteValidation
	{
		public MonthlyStatementMessageLine(PMSQ1 block)
		{
			this.blockQ1 = block;
		}
		readonly PMSQ1 blockQ1;

		public PMSQ2 blockQ2 { private get; set; }

		public ZString B2_StatementNumber
		{
			get { return blockQ1.PeriodicDailyStatementNumber; }
		}

		public ZDateTime B2_PrintDate
		{
			get { return blockQ1.PreliminaryPeriodicDailyStatementPrintDate; }
		}

		public ZDateTime B2_ProcessDate
		{
			get { return blockQ1.EntrySummaryPresentationDate; }
		}

		public ZDecimal FinalTotalDuty
		{
			get { return blockQ1.TotalDuty; }
		}

		public ZDecimal FinalTotalPayableTax
		{
			get { return blockQ1.TotalTax; }
		}

		public ZDecimal FinalTotalCVD
		{
			get { return blockQ2 != null ? blockQ2.TotalCountervailingDuty : ZDecimal.Zero; }
		}

		public ZDecimal FinalTotalADD
		{
			get { return blockQ2 != null ? blockQ2.TotalAntidumpingDuty : ZDecimal.Zero; }
		}

		public ZDecimal FinalTotalUserFees
		{
			get
			{
				if (!fFinalTotalUserFees.HasValue)
				{
					fFinalTotalUserFees = ZDecimal.Zero;
					foreach (var fee in fees)
					{
						if (EntryChargeTypeList.IsFeeType(fee.Key))
						{
							fFinalTotalUserFees += fee.Value;
						}
					}
					fFinalTotalUserFees += GetFee(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest);
				}
				return fFinalTotalUserFees.Value;
			}
		}
		ZDecimal? fFinalTotalUserFees;
	}
}
