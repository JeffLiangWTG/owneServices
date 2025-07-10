using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public SPTSHeaderFetchStrategy(SPTSHeader header)
			: base(header)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			AddCommonFetch();
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			AddCommonFetch();
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			AddCommonFetch();
		}

		void AddCommonFetch()
		{
			Factory.AddFetchHint(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
		}
	}
}
