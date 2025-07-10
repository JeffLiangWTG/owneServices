using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageJobPackageHeaderPivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public PkgPackageJobPackageHeaderPivotFetchStrategy(PkgPackageJobPackageHeaderPivot pivot)
			: base(pivot)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(PkgPackageHeaderSchema.Constants.TableName, Pivot.KPJ_KPH_PackageHeader); // tested in PkgPackageJobFetchStrategy
		}

		PkgPackageJobPackageHeaderPivot Pivot
		{
			get { return (PkgPackageJobPackageHeaderPivot)BusinessObject; }
		}
	}
}
