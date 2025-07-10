using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Business
{
	class DailyStatementMessageHeader : StatementMessageHeader
	{
		public DailyStatementMessageHeader(CusStatementHeader header, MQEDIMessage dailyStatementMessage)
			: base(header, dailyStatementMessage)
		{
		}

		#region Preliminary figures

		public ZString TotalNumberRevenueProducingEntriesForPrint
		{
			get { return GetPreliminaryFee(Constants.TotalNumberRevenueProducingEntries).ToString(); }
		}

		public ZString TotalNumberNonRevenueProducingEntriesForPrint
		{
			get { return GetPreliminaryFee(Constants.TotalNumberNonRevenueProducingEntries).ToString(); }
		}

		#endregion

		#region Final figures

		public ZString FinalTotalNumberRevenueProducingEntriesForPrint
		{
			get { return IsFinal ? (ZString)GetFinalFee(Constants.TotalNumberRevenueProducingEntries).ToString() : TotalNumberRevenueProducingEntriesForPrint; }
		}

		public ZString FinalTotalNumberNonRevenueProducingEntriesForPrint
		{
			get { return IsFinal ? (ZString)GetFinalFee(Constants.TotalNumberNonRevenueProducingEntries).ToString() : TotalNumberNonRevenueProducingEntriesForPrint; }
		}

		#endregion

		protected override void ProcessMessageBlocks(out IEnumerable<KeyValuePair<ZString, ZDecimal>> preliminaryFees,
			out IEnumerable<KeyValuePair<ZString, ZDecimal>> finalFees)
		{
			var preliminaryFees1 = new List<KeyValuePair<ZString, ZDecimal>>();
			var finalFees1 = new List<KeyValuePair<ZString, ZDecimal>>();

			bool isFinal = IsFinal;
			DailyStatementMessageLine lastDailyStatementDetail = null;
			var messageType = message.EM_MessageType;
			foreach (MessageBlock block in message.MessageBlock.MessageBlocks)
			{
				var blockType = block.MandatoryCharacters;
				switch (blockType)
				{
					case "Q1":
						{
							lastDailyStatementDetail = new DailyStatementMessageLine(header, (IDailyStatementDutyAndTax)block);
							AllOrActiveLines.Add(lastDailyStatementDetail);
							break;
						}

					case "Q2":
						{
							if (lastDailyStatementDetail != null)
							{
								lastDailyStatementDetail.blockQ2 = (IDailyStatementDutyAndTaxContinued)block;
								if (messageType == ApplicationIdentifierCodeList.Codes.DailyStatement)
								{
									lastDailyStatementDetail.AddFees((DSTQ2)block);
								}
							}
							break;
						}

					case "QA":
					case "QB":
					case "QC":
						{
							if (lastDailyStatementDetail != null)
							{
								lastDailyStatementDetail.AddFees((IStatementFees)block);
							}
							break;
						}

					case "Q3":
					case "Q4":
					case "QE":
					case "QF":
					case "QG":
						{
							if (messageType == ACEApplicationIdentifierCodeList.Codes.DailyStatement)
							{
								preliminaryFees1.AddRange(((IStatementFees)block).Fees);
							}
							else
							{
								if (isFinal)
								{
									finalFees1.AddRange(((IStatementFees)block).Fees);
								}
								else
								{
									preliminaryFees1.AddRange(((IStatementFees)block).Fees);
								}
							}
							break;
						}

					case "Q5":
					case "Q6":
					case "QJ":
					case "QK":
					case "QL":
						{
							if (messageType == ACEApplicationIdentifierCodeList.Codes.DailyStatement)
							{
								finalFees1.AddRange(((IStatementFees)block).Fees);
							}
							else
							{
								preliminaryFees1.AddRange(((IStatementFees)block).Fees);
							}

							break;
						}

					case "Q7":
						{
							var deletedEntries = (IStatementDeletedEntries)block;
							foreach (var deletedEntry in deletedEntries.DeletedEntries)
							{
								DailyDeletedLines.Add(new StatementDeletedEntry(deletedEntries.StatementNumber, deletedEntry.Key, deletedEntry.Value));
							}
							break;
						}
				}
			}

			preliminaryFees = preliminaryFees1;
			finalFees = finalFees1;
		}

		public StatementDeletedEntryCollection DailyDeletedLines
		{
			get { return fDailyDeletedLines ?? (fDailyDeletedLines = new StatementDeletedEntryCollection()); }
		}
		StatementDeletedEntryCollection fDailyDeletedLines;

		public StatementMessageLineCollection AllOrActiveLines
		{
			get { return fAllOrActiveLines ?? (fAllOrActiveLines = new StatementMessageLineCollection()); }
		}
		StatementMessageLineCollection fAllOrActiveLines;
	}
}
