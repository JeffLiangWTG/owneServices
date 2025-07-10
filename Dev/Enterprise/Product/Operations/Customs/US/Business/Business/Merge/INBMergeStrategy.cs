
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class INBMergeStrategy : ImportEntryCreationStrategy
	{
		public INBMergeStrategy(JobDeclaration declaration)
			: base(declaration, CusEntryHeaderMessageTypeList.Codes.InBond)
		{
		}

		protected override bool IsActiveCore
		{
			get
			{
				var declaration = (JobDeclaration)Declaration;
				return declaration.IsFormalImport && declaration.US_EnableINB;
			}
		}

		protected override Customs.Business.MergeKey GetKeyForHeaderCore(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			//for inbond, there should only one entry per job. Otherwise, short-formed bills which dont have to have invoice lines are orphaned.
			return new Customs.Business.MergeKey(0);
		}

		/// <summary>
		/// For INB, MergeBy option does not count. Therefore no need to call base.
		/// </summary>
		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)baseInvoiceLine;

			Customs.Business.MergeKey result = new Customs.Business.MergeKey(2);

			new INBLineKeyGenerator().AddKey(result, invoiceLine);

			return result;
		}

		protected override Customs.Business.CusEntryHeader GetExistingEntryHeader(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			CusEntryHeader result = (CusEntryHeader)base.GetExistingEntryHeader(invoiceLine);

			if (result == null)
			{
				//An inbond entry can be created detached from invoices and invoice lines as short-formed inbonds do not require invoice lines
				CusEntryHeader[] inBondEntries = (CusEntryHeader[])Declaration.ActiveEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.InBond));

				if (inBondEntries.Length > 0)
				{
					result = inBondEntries[0];//there is always only one inbond entry
				}
			}

			return result;
		}
	}

	class INBLineKeyGenerator : ILineKeyGenerator
	{
		public void AddKey(Customs.Business.MergeKey result, JobComInvoiceLine invoiceLine)
		{
			result.Add(invoiceLine.JI_Tariff);

			//If this is secondary tariff, then it does not get merged with other lines. If this behaviour changes, 
			//then IInBondTariffLineDetails.AdditionalTariffLines in CusEntryLine.cs should change 
			//as it makes an assumption it is one-to-one for secondary tariffs or parent tariffs to CusEntryLine
			result.Add(ImportEntryCreationStrategy.ShouldNotMergeThisLineWithOtherLines(invoiceLine) ? invoiceLine.PK : ZGuid.Empty);
		}
	}
}
