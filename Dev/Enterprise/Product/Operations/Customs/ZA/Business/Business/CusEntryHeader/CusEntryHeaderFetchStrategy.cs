using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.FetchStrategy
{
	public class CusEntryHeaderFetchStrategy : Customs.Business.FetchStrategies.CusEntryHeaderFetchStrategy
	{
		public CusEntryHeaderFetchStrategy(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected override void FetchForLoadCore()
		{
			Factory.AddFetchHint(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			base.FetchForLoadCore();
		}

		protected override void FetchForValidateCore()
		{
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			base.FetchForValidateCore();
		}
	}
}
