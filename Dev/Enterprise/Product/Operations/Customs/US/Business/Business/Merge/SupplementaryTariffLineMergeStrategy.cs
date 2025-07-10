using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	interface ILineKeyGenerator
	{
		void AddKey(Customs.Business.MergeKey mergeKey, JobComInvoiceLine invoiceLine);
	}

	class SupplementaryTariffLineMergeStrategy : ImportEntryCreationStrategy
	{
		public delegate ILineKeyGenerator GetLineKeyGenerator();

		public SupplementaryTariffLineMergeStrategy(JobDeclaration declaration, ZString messageType, GetLineKeyGenerator getLineKeyGenerator)
			: base(declaration, messageType)
		{
			this.getLineKeyGenerator = getLineKeyGenerator;
		}

		/// <summary>
		/// This strategy only creates additional lines. Entry Headers are created through normal strategies.
		/// </summary>
		readonly GetLineKeyGenerator getLineKeyGenerator;

		protected override void AfterCreateOrGetEntryLine(Customs.Business.CusEntryLine rateEntryLine, Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryLine(rateEntryLine, baseInvoiceLine);

			CusEntryLine entryLine = (CusEntryLine)rateEntryLine;

			entryLine.US_SupLine = true;
		}

		protected override Customs.Business.AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(Customs.Business.CusEntryLine entryLine, Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			return invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
		}

		protected override Customs.Business.CusEntryHeader GetExistingEntryHeader(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			CusEntryHeader[] result = (CusEntryHeader[])Declaration.ActiveEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CH_MessageTypeToNewEntryHeader));

			return result.Length > 0 ? result[0] : null;
		}

		protected override Customs.Business.CusEntryLine GetExistingEntryLine(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			return GetMatchedSupplementaryEntryLine((JobComInvoiceLine)invoiceLine);
		}

		protected override void ClearReferenceToEntryLineWhenLineIsNotValidForMerge(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var entryline = GetMatchedSupplementaryEntryLine(invoiceLine);
			if (entryline != null)
			{
				invoiceLine.AdditionalEntryLineLinks.DeleteLinkIfExistsFor(entryline);
			}
		}

		protected virtual CusEntryLine GetMatchedSupplementaryEntryLine(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.GetEntryLineFor(CH_MessageTypeToNewEntryHeader, true);
		}

		public override bool LineIsValidForMerge(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			return !((JobComInvoiceLine)baseInvoiceLine).HasEmptySupTariff;
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			Customs.Business.MergeKey result = base.GetKeyForLine(invoiceLine);

			JobComInvoiceLine usInvoiceLine = (JobComInvoiceLine)invoiceLine;

			ILineKeyGenerator lineKeyGenerator = getLineKeyGenerator();
			lineKeyGenerator.AddKey(result, usInvoiceLine);

			result.Add(GetSupplementaryTariff(usInvoiceLine));

			return result;
		}

		protected virtual ZString GetSupplementaryTariff(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.US_SupTariff;
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			return System.Array.Empty<CusEntryHeader>();
		}

		protected override IEnumerable<Customs.Business.CusEntryLine> GetExistingEntryLinesCreatedThroughThisStrategy()
		{
			CusEntryHeader[] entries = (CusEntryHeader[])Declaration.ActiveEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CH_MessageTypeToNewEntryHeader));

			foreach (CusEntryHeader entry in entries)
			{
				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					if (IsExistingEntryLineCreatedThroughThisStrategyMatching(entryLine))
					{
						yield return entryLine;
					}
				}
			}
		}

		protected virtual ZBool IsExistingEntryLineCreatedThroughThisStrategyMatching(CusEntryLine entryLine)
		{
			return entryLine.US_SupLine && !entryLine.US_SupAdditionalLine && !entryLine.US_SupAdditionalLine2 && !entryLine.US_SupAdditionalLine3 && !entryLine.US_SupAdditionalLine4 && !entryLine.US_SupAdditionalLine5;
		}

		protected override bool IsActiveCore
		{
			get
			{
				bool result = base.IsActiveCore && Declaration.IsImport;

				if (result)
				{
					result = ((JobDeclaration)Declaration).IsRelevantFor(CH_MessageTypeToNewEntryHeader);
				}

				return result;
			}
		}
	}
}
