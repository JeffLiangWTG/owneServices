using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public abstract class CargoReleaseMergeStrategy : ImportEntryCreationStrategy
	{
		protected CargoReleaseMergeStrategy(JobDeclaration declaration, ZString messageType)
			: base(declaration, messageType)
		{
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForLine(invoiceLine);

			var line = (JobComInvoiceLine)invoiceLine;

			new CargoReleaseLineMergeKeyGenerator().AddKey(result, line);

			return result;
		}

		protected override Customs.Business.CusEntryHeader GetExistingEntryHeader(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			return ((JobDeclaration)Declaration).ActiveEntryHeaders.CargoReleaseEntry ?? base.GetExistingEntryHeader(invoiceLine);
		}

		protected override void AfterCreateOrGetEntryHeader(Customs.Business.CusEntryHeader entryHeader, Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryHeader(entryHeader, baseInvoiceLine);

			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			Customs.Business.CusEntryLine ensEntryLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			Customs.Business.CusEntryHeader ensEntry = ensEntryLine != null ? ensEntryLine.Header : null;
			entryHeader.CH_CH_PrimeEntry = (ensEntry != null) ? ensEntry.PK : ZGuid.Empty;
		}
	}

	class CargoReleaseLineMergeKeyGenerator : ILineKeyGenerator
	{
		public virtual void AddKey(Customs.Business.MergeKey mergeKey, JobComInvoiceLine line)
		{
			mergeKey.Add(line.US_UC_NKCountryOfOrigin);
			mergeKey.Add(line.JI_Tariff);
			mergeKey.Add(line.US_SupTariff);//lines without 98/99 should not be merged with lines with 98/99 even though lines do not get reported as a child of 98/99 lines
			mergeKey.Add(line.US_SupAdditionalTariff1);
			mergeKey.Add(line.US_SupAdditionalTariff2);
			mergeKey.Add(line.US_SupAdditionalTariff3);
			mergeKey.Add(line.US_SupAdditionalTariff4);
			mergeKey.Add(line.US_SupAdditionalTariff5);
			mergeKey.Add(line.ManufacturerFallBackToSupplierNumber);
			mergeKey.Add(line.JI_OA_ConsigneeAddress);

			ZGuid additionalTariffDetailsKey = ImportEntryCreationStrategy.ShouldNotMergeThisLineWithOtherLines(line) ? line.PK : ZGuid.Empty;
			mergeKey.Add(additionalTariffDetailsKey);
		}
	}
}
