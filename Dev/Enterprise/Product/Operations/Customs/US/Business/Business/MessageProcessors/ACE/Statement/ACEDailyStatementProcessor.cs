using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business.Customs.US;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public class ACEDailyStatementProcessor : CommonDailyStatementProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override string StatementReportSubject
		{
			get { return "ACE Daily Statement Reports"; }
		}

		protected override ZDateTime GetProcessDate()
		{
			return A?.TransmissionDate ?? ZDateTime.Now;
		}

		protected override CusStatementLine ProcessMessageBlockCore(CusStatementHeader statementHeader, CusStatementLine statementLine, MessageBlock block, Dictionary<ZString, HtmlTableCreator> listOfDeclarationJobsRequiredAII)
		{
			if (block is ADSTQ1)
			{
				var dstq1 = (ADSTQ1)block;

				//calculate for previous DSTQ1
				CalculateTotalAmountForStatementLine(statementLine);
				statementLine = statementHeader.StatementLines.GetStatementLineFor(dstq1.EntryFilerCode, dstq1.EntryNumber);
				if (statementLine == null)
				{
					statementLine = statementHeader.StatementLines.AddNew();
					statementLine.B3_EntryFilerCode = dstq1.EntryFilerCode;
					statementLine.B3_EntryNum = dstq1.EntryNumber;
					statementLine.B3_EntryProcessPort = dstq1.DistrictPortOfEntrySummary;

					var declaration = statementLine.Declaration;

					var brokerReference = declaration != null ? declaration.JE_DeclarationReference : dstq1.BrokerReferenceNumber.Trim();
					statementLine.B3_BrokerReference = brokerReference;
				}

				statementLine.B3_EntryType = dstq1.EntryType.ToString();

				statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Duty, dstq1.EstimatedDutyAmount);

				string chargeType = dstq1.DeferredTaxIndicator == "Y" ? Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred : Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable;
				statementLine.Charges.UpdateLineChargeFor(chargeType, dstq1.EstimatedTaxAmount);
			}
			else if (block is ADSTQ2)
			{
				var dstq2 = (ADSTQ2)block;
				statementLine.B3_EntryStatus = StatementEntryStatus.Codes.Paperless;
				statementLine.B3_Team = dstq2.TeamNumber;

				var declaration = statementLine.Declaration;
				if (declaration != null)
				{
					var formalDeclaration = declaration as JobDeclaration;
					if (formalDeclaration != null)
					{
						if (!statementLine.B3_Team.IsEmpty && formalDeclaration.US_TeamNo != statementLine.B3_Team)
						{
							formalDeclaration.US_TeamNo = statementLine.B3_Team;
						}
					}
				}

				statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, dstq2.CountervailingDutyAmount);
				statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, dstq2.AntidumpingDutyAmount);
				statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, dstq2.InterestAmountForReconciliationSummary);
			}
			else if (block is ADSTQ4)
			{
				if (!IsFinal)
				{
					var dstq4 = (ADSTQ4)block;

					statementHeader.B2_StatementAmount = dstq4.TotalAmountDue;
				}
			}
			else if (block is ADSTQ6)
			{
				if (IsFinal)
				{
					var dstq6 = (ADSTQ6)block;

					statementHeader.B2_StatementAmount = dstq6.TotalAmountDue;
				}
			}
			else if (block is ADSTQ7)
			{
				var dstq7 = (ADSTQ7)block;

				ProcessDeletedEntry(statementHeader, dstq7.EntryNumber, dstq7.DeleteSource);
				ProcessDeletedEntry(statementHeader, dstq7.EntryNumber1, dstq7.DeleteSource1);
				ProcessDeletedEntry(statementHeader, dstq7.EntryNumber2, dstq7.DeleteSource2);
				ProcessDeletedEntry(statementHeader, dstq7.EntryNumber3, dstq7.DeleteSource3);
			}
			else if (block is ADSTQA)
			{
				var dstqA = (ADSTQA)block;

				ProcessFees(statementLine, dstqA.FirstFeeClassCode, dstqA.FirstFeeAmount);
				ProcessFees(statementLine, dstqA.SecondFeeClassCode, dstqA.SecondFeeAmount);
				ProcessFees(statementLine, dstqA.ThirdFeeClassCode, dstqA.ThirdFeeAmount);
				ProcessFees(statementLine, dstqA.FourthFeeClassCode, dstqA.FourthFeeAmount);
				ProcessFees(statementLine, dstqA.FifthFeeClassCode, dstqA.FifthFeeAmount);
			}

			return statementLine;
		}

		void ProcessFees(CusStatementLine statementLine, ZString feeClassCode, ZDecimal feeAmount)
		{
			if (statementLine != null && EntryChargeTypeList.IsFeeType(feeClassCode))
			{
				statementLine.Charges.UpdateLineChargeFor(feeClassCode, feeAmount);
			}
		}
	}
}
