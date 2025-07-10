using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class CusEntryLineFetchStrategy : Customs.Business.FetchStrategies.CusEntryLineFetchStrategy
	{
		public CusEntryLineFetchStrategy(Customs.Business.CusEntryLine line) : base(line)
		{
		}

		protected new CusEntryLine BusinessObject
		{
			get { return (CusEntryLine)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusDispositionSchema.CDI_ParentID, BusinessObject.PK);
		}
	}
}
