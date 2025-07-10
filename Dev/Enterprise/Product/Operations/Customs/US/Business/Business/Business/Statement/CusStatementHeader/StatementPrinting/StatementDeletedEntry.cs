using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class StatementDeletedEntry : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StatementDeletedEntry(ZString dailyStatementNumber, ZString entryNumber, ZString deletedByParty)
		{
			this.StatementNumber = dailyStatementNumber;
			this.B3_DeletedByParty = deletedByParty;
			this.FormattedEntryNumber = entryNumber.SubstringSafe(0, 7) + "-" + entryNumber.SubstringSafe(7);
		}

		public ZString FormattedEntryNumber
		{
			get;
			private set;
		}

		public ZString B3_DeletedByParty
		{
			get;
			private set;
		}

		public ZString StatementNumber
		{
			get;
			private set;
		}
	}
}
