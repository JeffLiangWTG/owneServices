using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business
{
	class MonthlyStatementMessageHeader : StatementMessageHeader
	{
		public MonthlyStatementMessageHeader(CusStatementHeader header, MQEDIMessage monthlyStatementMessage)
			: base(header, monthlyStatementMessage)
		{
		}

		public StatementDeletedEntryCollection MonthlyDeletedLines
		{
			get { return fMonthlyDeletedLines ?? (fMonthlyDeletedLines = new StatementDeletedEntryCollection()); }
		}
		StatementDeletedEntryCollection fMonthlyDeletedLines;

		public StatementMessageLineCollection DailyStatements
		{
			get { return fDailyStatements ?? (fDailyStatements = new StatementMessageLineCollection()); }
		}
		StatementMessageLineCollection fDailyStatements;

		protected override void ProcessMessageBlocks(out IEnumerable<KeyValuePair<ZString, ZDecimal>> preliminaryFees,
			out IEnumerable<KeyValuePair<ZString, ZDecimal>> finalFees)
		{
			var preliminaryFees1 = new List<KeyValuePair<ZString, ZDecimal>>();
			var finalFees1 = new List<KeyValuePair<ZString, ZDecimal>>();

			var isFinal = IsFinal;

			var easternTime = message.EM_SystemCreateTimeUtc.IsValid
				? EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("MIAMI", message.EM_SystemCreateTimeUtc.ToDateTime())
				: ZDateTime.Invalid;

			var isACEMessage = header.B2_StatementType == StatementTypeList.Codes.ACE || (easternTime.IsValid && easternTime >= ValidationConstants.ACEDeploymentDate);

			MonthlyStatementMessageLine lastDailyStatement = null;

			foreach (MessageBlock block in message.MessageBlock.MessageBlocks)
			{
				var blockType = block.MandatoryCharacters;
				switch (blockType)
				{
					case "Q1":
						{
							lastDailyStatement = new MonthlyStatementMessageLine((PMSQ1)block);
							DailyStatements.Add(lastDailyStatement);
							break;
						}

					case "Q2":
						{
							if (lastDailyStatement != null)
							{
								lastDailyStatement.blockQ2 = (PMSQ2)block;
							}
							break;
						}

					case "QA":
						{
							if (lastDailyStatement != null)
							{
								lastDailyStatement.AddFees((IStatementFees)block);
							}
							break;
						}

					case "Q3":
					case "Q4":
					case "QE":
						{
							if (isACEMessage)
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
						{
							if (isACEMessage)
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
								MonthlyDeletedLines.Add(new StatementDeletedEntry(deletedEntries.StatementNumber, deletedEntry.Key, deletedEntry.Value));
							}
							break;
						}
				}
			}

			preliminaryFees = preliminaryFees1;
			finalFees = finalFees1;
		}
	}
}
