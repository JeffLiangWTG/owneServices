using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusPackageJob : Customs.Business.CusPackageJob, Integration.Customs.TW.ICusPackageJob
	{
		public CusPackageJob(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ChildEditable]
		public new CusPackageCollection Packages => (CusPackageCollection)base.Packages;

		protected override PkgPackageCollection GetPackageCollectionCore() => new CusPackageCollection(this);

		public bool IsCalculatePackQtyFromPack { get; set; }
	}
}
