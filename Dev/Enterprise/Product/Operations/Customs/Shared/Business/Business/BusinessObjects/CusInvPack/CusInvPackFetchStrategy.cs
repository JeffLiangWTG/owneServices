using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusInvPackFetchStrategy<T> : EnterpriseBusinessObjectFetchStrategy
		where T : BusinessObject
	{
		public CusInvPackFetchStrategy(CusInvPack pivot)
			: base(pivot)
		{
		}

		CusInvPack Pivot
		{
			get { return (CusInvPack)BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(T), Pivot.B5_ParentID);
		}
	}
}
