using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Registry.Business.Customs.US;

namespace Enterprise.Customs.US.Business
{
	class DailyStatementMessageLine : StatementMessageLine, IObsoleteValidation
	{
		public DailyStatementMessageLine(CusStatementHeader header, IDailyStatementDutyAndTax block)
		{
			this.blockQ1 = block;
			this.header = header;
		}
		readonly IDailyStatementDutyAndTax blockQ1;
		readonly CusStatementHeader header;

		public IDailyStatementDutyAndTaxContinued blockQ2 { private get; set; }

		public ZString FormattedEntryNumber
		{
			get { return blockQ1.EntryFilerCode + "-" + blockQ1.EntryNumber.SubstringSafe(0, 7) + "-" + blockQ1.EntryNumber.SubstringSafe(7, 1); }
		}

		public ZString B3_BrokerReference
		{
			get
			{
				if (!fB3_BrokerReference.HasValue)
				{
					var statementLine = StatementLine;
					if (statementLine != null)
					{
						fB3_BrokerReference = statementLine.B3_BrokerReference;
					}

					if (!fB3_BrokerReference.HasValue)
					{
						fB3_BrokerReference = blockQ1.BrokerReferenceNumber;
					}
				}
				return fB3_BrokerReference.Value;
			}
		}
		ZString? fB3_BrokerReference;

		CusStatementLine StatementLine
		{
			get
			{
				if (line == null)
				{
					line = header.ActiveLines.Cast<CusStatementLine>().FirstOrDefault(x => x.B3_EntryNum == blockQ1.EntryNumber && x.B3_EntryFilerCode == blockQ1.EntryFilerCode);
				}
				return line;
			}
		}
		CusStatementLine line;

		public ZString B3_EntryType
		{
			get { return blockQ1.EntryType; }
		}

		public ZString B3_Team
		{
			get { return blockQ2 != null ? blockQ2.TeamNumber : ZString.Empty; }
		}

		public ZString B3_EntryStatus
		{
			get
			{
				var result = ZString.Empty;
				if (blockQ2 != null)
				{
					if (blockQ2.CensusWarningIndicator == StatementCensusWarningList.Codes.Census)
					{
						result = StatementEntryStatus.Codes.Census;
					}
					else if (blockQ2.ACEIndicator == "Y")
					{
						result = StatementEntryStatus.Codes.ACE;
					}
					else if (blockQ2.PaperlessElectronicIndicator == "P")
					{
						result = StatementEntryStatus.Codes.Paperless;
					}
				}
				return result;
			}
		}

		public ZDecimal EstimatedDuty
		{
			get { return blockQ1.EstimatedDutyAmount; }
		}

		public ZDecimal EstimatedTax
		{
			get { return blockQ1.EstimatedTaxAmount; }
		}

		public ZString IsDeferredTaxIndicator
		{
			get { return blockQ1.DeferredTaxIndicator; }
		}

		public ZDecimal EstimatedCVD
		{
			get { return IsACSDailyStatement ? blockQ1.CountervailingDutyAmount : blockQ2.CountervailingDutyAmount; }
		}

		bool IsACSDailyStatement
		{
			get { return blockQ1.MessageType == ApplicationIdentifierCodeList.Codes.DailyStatement; }
		}

		public ZDecimal EstimatedADD
		{
			get { return IsACSDailyStatement ? blockQ1.AntidumpingDutyAmount : blockQ2.AntidumpingDutyAmount; }
		}

		public ZString B3_EIIndicator
		{
			get
			{
				var result = ZString.Empty;
				if (blockQ2 != null)
				{
					if (IsACSDailyStatement)
					{
						result = blockQ2.ElectronicInvoiceIndicator == "I" ? YesNoDefaultList.Codes.Yes : string.Empty;
					}
					else
					{
						result = blockQ2.ElectronicInvoiceIndicator == "I" ? "Z" : string.Empty;
					}
				}
				return result;
			}
		}

		public ZString B2_PaymentType
		{
			get { return blockQ2 != null ? blockQ2.PaymentTypeIndicator : ZString.Empty; }
		}

		public ZDecimal UserFees
		{
			get
			{
				if (!fUserFees.HasValue)
				{
					fUserFees = ZDecimal.Zero;
					foreach (var fee in fees)
					{
						if (EntryChargeTypeList.IsFeeType(fee.Key))
						{
							fUserFees += fee.Value;
						}
					}
					fUserFees += InterestAmountForReconciliationSummary;
				}
				return fUserFees.Value;
			}
		}
		ZDecimal? fUserFees;

		public ZDecimal InterestAmountForReconciliationSummary
		{
			get
			{
				return StatementLine?.GetPayableAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest) ?? ZDecimal.Zero;
			}
		}

		public ZString B3_EntryProcessPort
		{
			get { return blockQ1.DistrictPortOfEntrySummary; }
		}

		public ZString InterestForReconciliationIndicator
		{
			get { return InterestAmountForReconciliationSummary > 0 ? "I" : string.Empty; }
		}

		public ZDecimal B3_CustomsFeesTotal
		{
			get
			{
				return EstimatedDuty + UserFees + (blockQ1.DeferredTaxIndicator == "Y" ? ZDecimal.Zero : EstimatedTax) +
					(blockQ2 != null && blockQ2.CountervailingIndicator == "Y" ? EstimatedCVD : ZDecimal.Zero) +
					(blockQ2 != null && blockQ2.AntidumpingIndicator == "Y" ? EstimatedADD : ZDecimal.Zero);
			}
		}
	}
}
