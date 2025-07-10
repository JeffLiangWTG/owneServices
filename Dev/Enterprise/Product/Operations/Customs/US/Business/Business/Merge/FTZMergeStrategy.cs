using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	class FTZMergeStrategy : ImportEntryCreationStrategy
	{
		public FTZMergeStrategy(JobDeclaration declaration)
			: base(declaration, CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone)
		{
		}

		new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override bool IsActiveCore
		{
			get { return Declaration.IsFTZAdmission; }
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			return new MergeKey();
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var result = base.GetKeyForLine(baseInvoiceLine);

			new FTZLineKeyGenerator().AddKey(result, (JobComInvoiceLine)baseInvoiceLine);

			return result;
		}

		protected override IComparer<BaseJobComInvoiceLine> GetInvoiceLineComparerForMerge(ReadOnlyBusinessObjectFactory cleanFactory)
		{
			return new InvoiceLineComparerForMerge(this, cleanFactory);
		}

		protected override AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_CL = entryLine.PK;
			return null;
		}
	}
}
