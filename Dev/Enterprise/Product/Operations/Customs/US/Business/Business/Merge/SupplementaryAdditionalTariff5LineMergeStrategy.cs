using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class SupplementaryAdditionalTariff5LineMergeStrategy : SupplementaryTariffLineMergeStrategy
	{
		public SupplementaryAdditionalTariff5LineMergeStrategy(JobDeclaration declaration, ZString messageType, GetLineKeyGenerator getLineKeyGenerator)
			: base(declaration, messageType, getLineKeyGenerator)
		{
		}

		public override bool LineIsValidForMerge(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			var tariff = ((JobComInvoiceLine)baseInvoiceLine).US_SupAdditionalTariff5;
			return !tariff.IsEmpty && tariff != TariffViewAsCodeDescription.NotApplicableCode;
		}

		protected override ZString GetSupplementaryTariff(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.US_SupAdditionalTariff5;
		}

		protected override CusEntryLine GetMatchedSupplementaryEntryLine(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.GetEntryLineFor(CH_MessageTypeToNewEntryHeader, true, (x) => x.US_SupAdditionalLine5);
		}

		protected override void AfterCreateOrGetEntryLine(Customs.Business.CusEntryLine rateEntryLine, Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryLine(rateEntryLine, baseInvoiceLine);

			var entryLine = (CusEntryLine)rateEntryLine;
			entryLine.US_SupAdditionalLine5 = true;
		}

		protected override ZBool IsExistingEntryLineCreatedThroughThisStrategyMatching(CusEntryLine entryLine)
		{
			return entryLine.US_SupLine && entryLine.US_SupAdditionalLine5;
		}
	}
}
