using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class CusStatementLineCollection : DependentBusinessObjectCollection<CusStatementLine, CusStatementHeader>
	{
		public CusStatementLineCollection(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		public new CusStatementHeader Master
		{
			get { return base.Master; }
		}

		public CusStatementLine GetStatementLineFor(ZString entryfilerCode, ZString entryNumber)
		{
			ZQuery query = new ZQuery(CusStatementLineSchema.B3_EntryNum, entryNumber);
			query.AddToFilter(CusStatementLineSchema.B3_EntryFilerCode, entryfilerCode);

			CusStatementLine[] lines = (CusStatementLine[])Find(query);
			return lines.Length > 0 ? lines[0] : null;
		}

		public void RefreshAP_ARInvoiceDetails()
		{
			foreach (CusStatementLine statementLine in this)
			{
				statementLine.RefreshAccountingAP_ARInvoiceQueryResult();
			}
		}

		public bool HasLinesWithDeletionPending
		{
			get { return Find(new ZQuery(CusStatementLineSchema.B3_Status, StatementLineStatusList.Codes.DeletionPending)).Length > 0; }
		}

		public bool AreAllLinesDeleted
		{
			get
			{
				bool result = false;

				if (Count > 0)
				{
					int deletedLinesCount = Find(new ZQuery(CusStatementLineSchema.B3_Status, StatementLineStatusList.Codes.Deleted)).Length;
					result = (Count - deletedLinesCount) == 0;
				}

				return result;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
